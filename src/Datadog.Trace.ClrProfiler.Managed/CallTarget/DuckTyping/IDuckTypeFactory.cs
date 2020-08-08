namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type factory
    /// </summary>
    /// <typeparam name="TInterface">Type of the ducktype object</typeparam>
    public interface IDuckTypeFactory<TInterface>
        where TInterface : class
    {
        /// <summary>
        /// Create duck type proxy instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Duck type proxy instance</returns>
        TInterface Create(object instance);
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
