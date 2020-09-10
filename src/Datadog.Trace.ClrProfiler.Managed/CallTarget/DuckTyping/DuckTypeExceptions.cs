using System;
using System.Reflection;
#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// DuckType property can't be read
    /// </summary>
    public class DuckTypePropertyCantBeReadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuckTypePropertyCantBeReadException"/> class.
        /// </summary>
        /// <param name="property">Property info</param>
        internal DuckTypePropertyCantBeReadException(PropertyInfo property)
            : base($"The property '{property.Name}' can't be read, you should remove the getter from the base type class or interface.")
        {
        }
    }

    /// <summary>
    /// DuckType property can't be written
    /// </summary>
    public class DuckTypePropertyCantBeWrittenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuckTypePropertyCantBeWrittenException"/> class.
        /// </summary>
        /// <param name="property">Property info</param>
        internal DuckTypePropertyCantBeWrittenException(PropertyInfo property)
            : base($"The property '{property.Name}' can't be written, you should remove the setter from the base type class or interface.")
        {
        }
    }

    /// <summary>
    /// DuckType field is readonly
    /// </summary>
    public class DuckTypeFieldIsReadonlyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuckTypeFieldIsReadonlyException"/> class.
        /// </summary>
        /// <param name="field">Field info</param>
        internal DuckTypeFieldIsReadonlyException(FieldInfo field)
            : base($"The field '{field.Name}' is marked as readonly, you should remove the setter from the base type class or interface.")
        {
        }
    }

    /// <summary>
    /// DuckType property or field not found
    /// </summary>
    public class DuckTypePropertyOrFieldNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuckTypePropertyOrFieldNotFoundException"/> class.
        /// </summary>
        /// <param name="name">Property or field name</param>
        public DuckTypePropertyOrFieldNotFoundException(string name)
            : base($"The property or field  for '{name}' was not found in the instance.")
        {
        }
    }

    /// <summary>
    /// DuckType type is not an interface exception
    /// </summary>
    public class DuckTypeTypeIsNotValidException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuckTypeTypeIsNotValidException"/> class.
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="argumentName">Name of the argument</param>
        public DuckTypeTypeIsNotValidException(Type type, string argumentName)
            : base($"The type '{type.FullName}' is not a valid type, argument: '{argumentName}'")
        {
        }
    }

    /// <summary>
    /// DuckType type is not public exception
    /// </summary>
    public class DuckTypeTypeIsNotPublicException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuckTypeTypeIsNotPublicException"/> class.
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="argumentName">Name of the argument</param>
        public DuckTypeTypeIsNotPublicException(Type type, string argumentName)
            : base($"The type '{type.FullName}' must be public, argument: '{argumentName}'")
        {
        }
    }

    /// <summary>
    /// DuckType struct members cannot be changed exception
    /// </summary>
    public class DuckTypeStructMembersCannotBeChangedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuckTypeStructMembersCannotBeChangedException"/> class.
        /// </summary>
        /// <param name="type">Type</param>
        internal DuckTypeStructMembersCannotBeChangedException(Type type)
            : base($"Modifying struct members is not supported. [{type.FullName}]")
        {
        }
    }
}
