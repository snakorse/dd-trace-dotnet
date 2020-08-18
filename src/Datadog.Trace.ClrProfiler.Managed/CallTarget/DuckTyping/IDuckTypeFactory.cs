namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type factory
    /// </summary>
    /// <typeparam name="TDuckBase">Base type of the ducktype object</typeparam>
    public interface IDuckTypeFactory<TDuckBase>
        where TDuckBase : class
    {
        /// <summary>
        /// Create duck type proxy instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Duck type proxy instance</returns>
        TDuckBase Create(object instance);
    }

    /// <summary>
    /// Duck Type factory
    /// </summary>
    public interface IDuckTypeFactory
    {
        /// <summary>
        /// Create duck type proxy instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Duck type proxy instance</returns>
        IDuckType Create(object instance);
    }
}
