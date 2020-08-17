namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Ignores access checks, is the reverse InternalsVisibleTo
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        private string _assemblyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoresAccessChecksToAttribute"/> class.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        /// <summary>
        /// Gets the assembly name
        /// </summary>
        public string AssemblyName
        {
            get { return _assemblyName; }
        }
    }
}
