using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Datadog.Trace.HttpOverStreams.HttpContent;
using Datadog.Trace.Logging;

namespace Datadog.Trace.HttpOverStreams
{
    internal class DatadogHttpClient
    {
        /// <summary>
        /// Typical response from the agent is ~148 bytes.
        /// Allow enough room for failure messages and future expanding.
        /// </summary>
        public const int MaxResponseBufferSize = 5120;

        /// <summary>
        /// Typical headers sent to the agent are small.
        /// Allow enough room for future expansion of headers.
        /// </summary>
        public const int MaxRequestHeadersBufferSize = 2560;

        private static readonly Vendors.Serilog.ILogger Logger = DatadogLogging.For<DatadogHttpClient>();

        public async Task<HttpResponse> SendAsync(HttpRequest request, Stream requestStream, Stream responseStream)
        {
            await SendRequestAsync(request, requestStream).ConfigureAwait(false);
            return await ReadResponseAsync(responseStream).ConfigureAwait(false);
        }

        private async Task SendRequestAsync(HttpRequest request, Stream requestStream)
        {
            // TODO: Determine if it's always ASCII
            using (var writer = new StreamWriter(requestStream, Encoding.ASCII, MaxRequestHeadersBufferSize, leaveOpen: true))
            {
                await DatadogHttpHeaderHelper.WriteLeadingHeaders(request, writer).ConfigureAwait(false);

                foreach (var header in request.Headers)
                {
                    await DatadogHttpHeaderHelper.WriteHeader(writer, header).ConfigureAwait(false);
                }

                await DatadogHttpHeaderHelper.WriteEndOfHeaders(writer).ConfigureAwait(false);
            }

            await request.Content.CopyToAsync(requestStream).ConfigureAwait(false);
            Logger.Debug("Datadog HTTP: Flushing stream.");
            await requestStream.FlushAsync().ConfigureAwait(false);
        }

        private async Task<HttpResponse> ReadResponseAsync(Stream responseStream)
        {
            var headers = new HttpHeaders();
            int statusCode = 0;
            string responseMessage = null;

            // hack: buffer the entire response so we can seek
            var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            memoryStream.Position = 0;
            int streamPosition = 0;

            using (var reader = new StreamReader(memoryStream, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, MaxResponseBufferSize, leaveOpen: true))
            {
                // https://tools.ietf.org/html/rfc2616#section-4.2
                // HTTP/1.1 200 OK
                // HTTP/1.1 XXX MESSAGE
                string line = reader.ReadLine();
                streamPosition += reader.CurrentEncoding.GetByteCount(line) + DatadogHttpHeaderHelper.CrLfLength;

                const int statusCodeStart = 9;
                const int statusCodeLength = 3;
                const int startOfMessage = 13;

                if (!int.TryParse(line.Substring(statusCodeStart, statusCodeLength), out statusCode))
                {
                    throw new DatadogHttpRequestException("Invalid response, can't parse status code. Line was:" + line);
                }

                responseMessage = line.Substring(startOfMessage);

                // TODO: Get these from a StringBuilderCache
                var keyBuilder = new StringBuilder();
                var valueBuilder = new StringBuilder();

                // read headers
                while (true)
                {
                    var headerChar = Convert.ToChar(reader.Read());
                    streamPosition++;

                    if (headerChar.Equals(DatadogHttpHeaderHelper.CarriageReturn))
                    {
                        // end of headers
                        DatadogHttpHeaderHelper.SkipFeed(reader);
                        streamPosition++;
                        break;
                    }

                    do
                    {
                        if (headerChar.Equals(':'))
                        {
                            break;
                        }

                        keyBuilder.Append(headerChar);
                        headerChar = Convert.ToChar(reader.Read());
                        streamPosition++;
                    }
                    while (true);

                    do
                    {
                        var valueChar = Convert.ToChar(reader.Read());
                        streamPosition++;

                        if (valueChar.Equals(DatadogHttpHeaderHelper.CarriageReturn))
                        {
                            break;
                        }

                        valueBuilder.Append(valueChar);
                    }
                    while (true);

                    DatadogHttpHeaderHelper.SkipFeed(reader);
                    streamPosition++;

                    var name = keyBuilder.ToString().Trim();
                    var value = valueBuilder.ToString().Trim();

                    headers.Add(name, value);

                    keyBuilder.Clear();
                    valueBuilder.Clear();
                }
            }

            memoryStream.Position = streamPosition;
            long bytesLeft = memoryStream.Length - memoryStream.Position;
            var length = long.TryParse(headers.GetValue("Content-Length"), out var headerValue) ? headerValue : (long?)null;

            if (length == null)
            {
                length = bytesLeft;
            }
            else if (length != bytesLeft)
            {
                throw new DatadogHttpRequestException("Content length from http headers does not match content's actual length.");
            }

            return new HttpResponse(statusCode, responseMessage, headers, new StreamContent(memoryStream, length));
        }
    }
}
