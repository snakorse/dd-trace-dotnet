using System;
using Datadog.RuntimeMetrics;

namespace ConsoleApp1
{
    public class Program
    {
        public static void Main()
        {
            var listener = new SimpleEventListener();
            Console.ReadLine();
        }
    }
}