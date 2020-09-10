using System.Collections.Generic;
using Datadog.Trace.ClrProfiler.CallTarget.DuckTyping;
using Xunit;

#pragma warning disable SA1201 // Elements must appear in the correct order

namespace Datadog.Trace.ClrProfiler.Managed.Tests.CallTarget.DuckTyping
{
    public class DuckTypeValueTypePropertyTests
    {
        public static IEnumerable<object[]> Data()
        {
            return new[]
            {
                new object[] { ObscureObject.GetPropertyPublicObject() },
                new object[] { ObscureObject.GetPropertyInternalObject() },
                new object[] { ObscureObject.GetPropertyPrivateObject() },
            };
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticPropertyOnlySetException(object obscureObject)
        {
            Assert.Throws<DuckTypePropertyCantBeWrittenException>(() =>
            {
                obscureObject.As<IObscureStaticErrorDuckType>();
            });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void PropertyOnlySetException(object obscureObject)
        {
            Assert.Throws<DuckTypePropertyCantBeWrittenException>(() =>
            {
                obscureObject.As<IObscureErrorDuckType>();
            });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticOnlyGetProperties(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // *
            Assert.Equal(10, duckInterface.PublicStaticGetValueType);
            Assert.Equal(10, duckAbstract.PublicStaticGetValueType);
            Assert.Equal(10, duckVirtual.PublicStaticGetValueType);

            // *
            Assert.Equal(11, duckInterface.InternalStaticGetValueType);
            Assert.Equal(11, duckAbstract.InternalStaticGetValueType);
            Assert.Equal(11, duckVirtual.InternalStaticGetValueType);

            // *
            Assert.Equal(12, duckInterface.ProtectedStaticGetValueType);
            Assert.Equal(12, duckAbstract.ProtectedStaticGetValueType);
            Assert.Equal(12, duckVirtual.ProtectedStaticGetValueType);

            // *
            Assert.Equal(13, duckInterface.PrivateStaticGetValueType);
            Assert.Equal(13, duckAbstract.PrivateStaticGetValueType);
            Assert.Equal(13, duckVirtual.PrivateStaticGetValueType);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticProperties(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            Assert.Equal(20, duckInterface.PublicStaticGetSetValueType);
            Assert.Equal(20, duckAbstract.PublicStaticGetSetValueType);
            Assert.Equal(20, duckVirtual.PublicStaticGetSetValueType);

            duckInterface.PublicStaticGetSetValueType = 42;
            Assert.Equal(42, duckInterface.PublicStaticGetSetValueType);
            Assert.Equal(42, duckAbstract.PublicStaticGetSetValueType);
            Assert.Equal(42, duckVirtual.PublicStaticGetSetValueType);

            duckAbstract.PublicStaticGetSetValueType = 50;
            Assert.Equal(50, duckInterface.PublicStaticGetSetValueType);
            Assert.Equal(50, duckAbstract.PublicStaticGetSetValueType);
            Assert.Equal(50, duckVirtual.PublicStaticGetSetValueType);

            duckVirtual.PublicStaticGetSetValueType = 60;
            Assert.Equal(60, duckInterface.PublicStaticGetSetValueType);
            Assert.Equal(60, duckAbstract.PublicStaticGetSetValueType);
            Assert.Equal(60, duckVirtual.PublicStaticGetSetValueType);

            // *

            Assert.Equal(21, duckInterface.InternalStaticGetSetValueType);
            Assert.Equal(21, duckAbstract.InternalStaticGetSetValueType);
            Assert.Equal(21, duckVirtual.InternalStaticGetSetValueType);

            duckInterface.InternalStaticGetSetValueType = 42;
            Assert.Equal(42, duckInterface.InternalStaticGetSetValueType);
            Assert.Equal(42, duckAbstract.InternalStaticGetSetValueType);
            Assert.Equal(42, duckVirtual.InternalStaticGetSetValueType);

            duckAbstract.InternalStaticGetSetValueType = 50;
            Assert.Equal(50, duckInterface.InternalStaticGetSetValueType);
            Assert.Equal(50, duckAbstract.InternalStaticGetSetValueType);
            Assert.Equal(50, duckVirtual.InternalStaticGetSetValueType);

            duckVirtual.InternalStaticGetSetValueType = 60;
            Assert.Equal(60, duckInterface.InternalStaticGetSetValueType);
            Assert.Equal(60, duckAbstract.InternalStaticGetSetValueType);
            Assert.Equal(60, duckVirtual.InternalStaticGetSetValueType);

            // *

            Assert.Equal(22, duckInterface.ProtectedStaticGetSetValueType);
            Assert.Equal(22, duckAbstract.ProtectedStaticGetSetValueType);
            Assert.Equal(22, duckVirtual.ProtectedStaticGetSetValueType);

            duckInterface.ProtectedStaticGetSetValueType = 42;
            Assert.Equal(42, duckInterface.ProtectedStaticGetSetValueType);
            Assert.Equal(42, duckAbstract.ProtectedStaticGetSetValueType);
            Assert.Equal(42, duckVirtual.ProtectedStaticGetSetValueType);

            duckAbstract.ProtectedStaticGetSetValueType = 50;
            Assert.Equal(50, duckInterface.ProtectedStaticGetSetValueType);
            Assert.Equal(50, duckAbstract.ProtectedStaticGetSetValueType);
            Assert.Equal(50, duckVirtual.ProtectedStaticGetSetValueType);

            duckVirtual.ProtectedStaticGetSetValueType = 60;
            Assert.Equal(60, duckInterface.ProtectedStaticGetSetValueType);
            Assert.Equal(60, duckAbstract.ProtectedStaticGetSetValueType);
            Assert.Equal(60, duckVirtual.ProtectedStaticGetSetValueType);

            // *

            Assert.Equal(23, duckInterface.PrivateStaticGetSetValueType);
            Assert.Equal(23, duckAbstract.PrivateStaticGetSetValueType);
            Assert.Equal(23, duckVirtual.PrivateStaticGetSetValueType);

            duckInterface.PrivateStaticGetSetValueType = 42;
            Assert.Equal(42, duckInterface.PrivateStaticGetSetValueType);
            Assert.Equal(42, duckAbstract.PrivateStaticGetSetValueType);
            Assert.Equal(42, duckVirtual.PrivateStaticGetSetValueType);

            duckAbstract.PrivateStaticGetSetValueType = 50;
            Assert.Equal(50, duckInterface.PrivateStaticGetSetValueType);
            Assert.Equal(50, duckAbstract.PrivateStaticGetSetValueType);
            Assert.Equal(50, duckVirtual.PrivateStaticGetSetValueType);

            duckVirtual.PrivateStaticGetSetValueType = 60;
            Assert.Equal(60, duckInterface.PrivateStaticGetSetValueType);
            Assert.Equal(60, duckAbstract.PrivateStaticGetSetValueType);
            Assert.Equal(60, duckVirtual.PrivateStaticGetSetValueType);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void OnlyGetProperties(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // *
            Assert.Equal(30, duckInterface.PublicGetValueType);
            Assert.Equal(30, duckAbstract.PublicGetValueType);
            Assert.Equal(30, duckVirtual.PublicGetValueType);

            // *
            Assert.Equal(31, duckInterface.InternalGetValueType);
            Assert.Equal(31, duckAbstract.InternalGetValueType);
            Assert.Equal(31, duckVirtual.InternalGetValueType);

            // *
            Assert.Equal(32, duckInterface.ProtectedGetValueType);
            Assert.Equal(32, duckAbstract.ProtectedGetValueType);
            Assert.Equal(32, duckVirtual.ProtectedGetValueType);

            // *
            Assert.Equal(33, duckInterface.PrivateGetValueType);
            Assert.Equal(33, duckAbstract.PrivateGetValueType);
            Assert.Equal(33, duckVirtual.PrivateGetValueType);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void Properties(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            Assert.Equal(40, duckInterface.PublicGetSetValueType);
            Assert.Equal(40, duckAbstract.PublicGetSetValueType);
            Assert.Equal(40, duckVirtual.PublicGetSetValueType);

            duckInterface.PublicGetSetValueType = 42;
            Assert.Equal(42, duckInterface.PublicGetSetValueType);
            Assert.Equal(42, duckAbstract.PublicGetSetValueType);
            Assert.Equal(42, duckVirtual.PublicGetSetValueType);

            duckAbstract.PublicGetSetValueType = 50;
            Assert.Equal(50, duckInterface.PublicGetSetValueType);
            Assert.Equal(50, duckAbstract.PublicGetSetValueType);
            Assert.Equal(50, duckVirtual.PublicGetSetValueType);

            duckVirtual.PublicGetSetValueType = 60;
            Assert.Equal(60, duckInterface.PublicGetSetValueType);
            Assert.Equal(60, duckAbstract.PublicGetSetValueType);
            Assert.Equal(60, duckVirtual.PublicGetSetValueType);

            // *

            Assert.Equal(41, duckInterface.InternalGetSetValueType);
            Assert.Equal(41, duckAbstract.InternalGetSetValueType);
            Assert.Equal(41, duckVirtual.InternalGetSetValueType);

            duckInterface.InternalGetSetValueType = 42;
            Assert.Equal(42, duckInterface.InternalGetSetValueType);
            Assert.Equal(42, duckAbstract.InternalGetSetValueType);
            Assert.Equal(42, duckVirtual.InternalGetSetValueType);

            duckAbstract.InternalGetSetValueType = 50;
            Assert.Equal(50, duckInterface.InternalGetSetValueType);
            Assert.Equal(50, duckAbstract.InternalGetSetValueType);
            Assert.Equal(50, duckVirtual.InternalGetSetValueType);

            duckVirtual.InternalGetSetValueType = 60;
            Assert.Equal(60, duckInterface.InternalGetSetValueType);
            Assert.Equal(60, duckAbstract.InternalGetSetValueType);
            Assert.Equal(60, duckVirtual.InternalGetSetValueType);

            // *

            Assert.Equal(42, duckInterface.ProtectedGetSetValueType);
            Assert.Equal(42, duckAbstract.ProtectedGetSetValueType);
            Assert.Equal(42, duckVirtual.ProtectedGetSetValueType);

            duckInterface.ProtectedGetSetValueType = 45;
            Assert.Equal(45, duckInterface.ProtectedGetSetValueType);
            Assert.Equal(45, duckAbstract.ProtectedGetSetValueType);
            Assert.Equal(45, duckVirtual.ProtectedGetSetValueType);

            duckAbstract.ProtectedGetSetValueType = 50;
            Assert.Equal(50, duckInterface.ProtectedGetSetValueType);
            Assert.Equal(50, duckAbstract.ProtectedGetSetValueType);
            Assert.Equal(50, duckVirtual.ProtectedGetSetValueType);

            duckVirtual.ProtectedGetSetValueType = 60;
            Assert.Equal(60, duckInterface.ProtectedGetSetValueType);
            Assert.Equal(60, duckAbstract.ProtectedGetSetValueType);
            Assert.Equal(60, duckVirtual.ProtectedGetSetValueType);

            // *

            Assert.Equal(43, duckInterface.PrivateGetSetValueType);
            Assert.Equal(43, duckAbstract.PrivateGetSetValueType);
            Assert.Equal(43, duckVirtual.PrivateGetSetValueType);

            duckInterface.PrivateGetSetValueType = 42;
            Assert.Equal(42, duckInterface.PrivateGetSetValueType);
            Assert.Equal(42, duckAbstract.PrivateGetSetValueType);
            Assert.Equal(42, duckVirtual.PrivateGetSetValueType);

            duckAbstract.PrivateGetSetValueType = 50;
            Assert.Equal(50, duckInterface.PrivateGetSetValueType);
            Assert.Equal(50, duckAbstract.PrivateGetSetValueType);
            Assert.Equal(50, duckVirtual.PrivateGetSetValueType);

            duckVirtual.PrivateGetSetValueType = 60;
            Assert.Equal(60, duckInterface.PrivateGetSetValueType);
            Assert.Equal(60, duckAbstract.PrivateGetSetValueType);
            Assert.Equal(60, duckVirtual.PrivateGetSetValueType);
        }

        public interface IObscureDuckType
        {
            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int PublicStaticGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int InternalStaticGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int ProtectedStaticGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int PrivateStaticGetValueType { get; }

            // *

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int PublicStaticGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int InternalStaticGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int ProtectedStaticGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int PrivateStaticGetSetValueType { get; set; }

            // *

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int PublicGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int InternalGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int ProtectedGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int PrivateGetValueType { get; }

            // *

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int PublicGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int InternalGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int ProtectedGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int PrivateGetSetValueType { get; set; }
        }

        public interface IObscureStaticErrorDuckType
        {
            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int PublicStaticGetValueType { get; set; }
        }

        public interface IObscureErrorDuckType
        {
            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            int PublicGetValueType { get; set; }
        }

        public abstract class ObscureDuckTypeAbstractClass
        {
            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PublicStaticGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int InternalStaticGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int ProtectedStaticGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PrivateStaticGetValueType { get; }

            // *

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PublicStaticGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int InternalStaticGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int ProtectedStaticGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PrivateStaticGetSetValueType { get; set; }

            // *

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PublicGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int InternalGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int ProtectedGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PrivateGetValueType { get; }

            // *

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PublicGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int InternalGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int ProtectedGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PrivateGetSetValueType { get; set; }
        }

        public class ObscureDuckType
        {
            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PublicStaticGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int InternalStaticGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int ProtectedStaticGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PrivateStaticGetValueType { get; }

            // *

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PublicStaticGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int InternalStaticGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int ProtectedStaticGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PrivateStaticGetSetValueType { get; set; }

            // *

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PublicGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int InternalGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int ProtectedGetValueType { get; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PrivateGetValueType { get; }

            // *

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PublicGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int InternalGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int ProtectedGetSetValueType { get; set; }

            [Duck(BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PrivateGetSetValueType { get; set; }
        }
    }
}
