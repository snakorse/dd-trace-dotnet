using System.Collections.Generic;
using Datadog.Trace.ClrProfiler.CallTarget.DuckTyping;
using Xunit;

#pragma warning disable SA1201 // Elements must appear in the correct order

namespace Datadog.Trace.ClrProfiler.Managed.Tests.CallTarget.DuckTyping
{
    public class DuckTypeValueTypeFieldTests
    {
        public static IEnumerable<object[]> Data()
        {
            return new[]
            {
                new object[] { ObscureObject.GetFieldPublicObject() },
                new object[] { ObscureObject.GetFieldInternalObject() },
                new object[] { ObscureObject.GetFieldPrivateObject() },
            };
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticReadonlyFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // *
            Assert.Equal(10, duckInterface.PublicStaticReadonlyValueTypeField);
            Assert.Equal(10, duckAbstract.PublicStaticReadonlyValueTypeField);
            Assert.Equal(10, duckVirtual.PublicStaticReadonlyValueTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.PublicStaticReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.PublicStaticReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.PublicStaticReadonlyValueTypeField = 99;
            });

            // *
            Assert.Equal(11, duckInterface.InternalStaticReadonlyValueTypeField);
            Assert.Equal(11, duckAbstract.InternalStaticReadonlyValueTypeField);
            Assert.Equal(11, duckVirtual.InternalStaticReadonlyValueTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.InternalStaticReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.InternalStaticReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.InternalStaticReadonlyValueTypeField = 99;
            });

            // *
            Assert.Equal(12, duckInterface.ProtectedStaticReadonlyValueTypeField);
            Assert.Equal(12, duckAbstract.ProtectedStaticReadonlyValueTypeField);
            Assert.Equal(12, duckVirtual.ProtectedStaticReadonlyValueTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.ProtectedStaticReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.ProtectedStaticReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.ProtectedStaticReadonlyValueTypeField = 99;
            });

            // *
            Assert.Equal(13, duckInterface.PrivateStaticReadonlyValueTypeField);
            Assert.Equal(13, duckAbstract.PrivateStaticReadonlyValueTypeField);
            Assert.Equal(13, duckVirtual.PrivateStaticReadonlyValueTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.PrivateStaticReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.PrivateStaticReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.PrivateStaticReadonlyValueTypeField = 99;
            });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            Assert.Equal(20, duckInterface.PublicStaticValueTypeField);
            Assert.Equal(20, duckAbstract.PublicStaticValueTypeField);
            Assert.Equal(20, duckVirtual.PublicStaticValueTypeField);

            duckInterface.PublicStaticValueTypeField = 42;
            Assert.Equal(42, duckInterface.PublicStaticValueTypeField);
            Assert.Equal(42, duckAbstract.PublicStaticValueTypeField);
            Assert.Equal(42, duckVirtual.PublicStaticValueTypeField);

            duckAbstract.PublicStaticValueTypeField = 50;
            Assert.Equal(50, duckInterface.PublicStaticValueTypeField);
            Assert.Equal(50, duckAbstract.PublicStaticValueTypeField);
            Assert.Equal(50, duckVirtual.PublicStaticValueTypeField);

            duckVirtual.PublicStaticValueTypeField = 60;
            Assert.Equal(60, duckInterface.PublicStaticValueTypeField);
            Assert.Equal(60, duckAbstract.PublicStaticValueTypeField);
            Assert.Equal(60, duckVirtual.PublicStaticValueTypeField);

            // *

            Assert.Equal(21, duckInterface.InternalStaticValueTypeField);
            Assert.Equal(21, duckAbstract.InternalStaticValueTypeField);
            Assert.Equal(21, duckVirtual.InternalStaticValueTypeField);

            duckInterface.InternalStaticValueTypeField = 42;
            Assert.Equal(42, duckInterface.InternalStaticValueTypeField);
            Assert.Equal(42, duckAbstract.InternalStaticValueTypeField);
            Assert.Equal(42, duckVirtual.InternalStaticValueTypeField);

            duckAbstract.InternalStaticValueTypeField = 50;
            Assert.Equal(50, duckInterface.InternalStaticValueTypeField);
            Assert.Equal(50, duckAbstract.InternalStaticValueTypeField);
            Assert.Equal(50, duckVirtual.InternalStaticValueTypeField);

            duckVirtual.InternalStaticValueTypeField = 60;
            Assert.Equal(60, duckInterface.InternalStaticValueTypeField);
            Assert.Equal(60, duckAbstract.InternalStaticValueTypeField);
            Assert.Equal(60, duckVirtual.InternalStaticValueTypeField);

            // *

            Assert.Equal(22, duckInterface.ProtectedStaticValueTypeField);
            Assert.Equal(22, duckAbstract.ProtectedStaticValueTypeField);
            Assert.Equal(22, duckVirtual.ProtectedStaticValueTypeField);

            duckInterface.ProtectedStaticValueTypeField = 42;
            Assert.Equal(42, duckInterface.ProtectedStaticValueTypeField);
            Assert.Equal(42, duckAbstract.ProtectedStaticValueTypeField);
            Assert.Equal(42, duckVirtual.ProtectedStaticValueTypeField);

            duckAbstract.ProtectedStaticValueTypeField = 50;
            Assert.Equal(50, duckInterface.ProtectedStaticValueTypeField);
            Assert.Equal(50, duckAbstract.ProtectedStaticValueTypeField);
            Assert.Equal(50, duckVirtual.ProtectedStaticValueTypeField);

            duckVirtual.ProtectedStaticValueTypeField = 60;
            Assert.Equal(60, duckInterface.ProtectedStaticValueTypeField);
            Assert.Equal(60, duckAbstract.ProtectedStaticValueTypeField);
            Assert.Equal(60, duckVirtual.ProtectedStaticValueTypeField);

            // *

            Assert.Equal(23, duckInterface.PrivateStaticValueTypeField);
            Assert.Equal(23, duckAbstract.PrivateStaticValueTypeField);
            Assert.Equal(23, duckVirtual.PrivateStaticValueTypeField);

            duckInterface.PrivateStaticValueTypeField = 42;
            Assert.Equal(42, duckInterface.PrivateStaticValueTypeField);
            Assert.Equal(42, duckAbstract.PrivateStaticValueTypeField);
            Assert.Equal(42, duckVirtual.PrivateStaticValueTypeField);

            duckAbstract.PrivateStaticValueTypeField = 50;
            Assert.Equal(50, duckInterface.PrivateStaticValueTypeField);
            Assert.Equal(50, duckAbstract.PrivateStaticValueTypeField);
            Assert.Equal(50, duckVirtual.PrivateStaticValueTypeField);

            duckVirtual.PrivateStaticValueTypeField = 60;
            Assert.Equal(60, duckInterface.PrivateStaticValueTypeField);
            Assert.Equal(60, duckAbstract.PrivateStaticValueTypeField);
            Assert.Equal(60, duckVirtual.PrivateStaticValueTypeField);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void ReadonlyFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // *
            Assert.Equal(30, duckInterface.PublicReadonlyValueTypeField);
            Assert.Equal(30, duckAbstract.PublicReadonlyValueTypeField);
            Assert.Equal(30, duckVirtual.PublicReadonlyValueTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.PublicReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.PublicReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.PublicReadonlyValueTypeField = 99;
            });

            // *
            Assert.Equal(31, duckInterface.InternalReadonlyValueTypeField);
            Assert.Equal(31, duckAbstract.InternalReadonlyValueTypeField);
            Assert.Equal(31, duckVirtual.InternalReadonlyValueTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.InternalReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.InternalReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.InternalReadonlyValueTypeField = 99;
            });

            // *
            Assert.Equal(32, duckInterface.ProtectedReadonlyValueTypeField);
            Assert.Equal(32, duckAbstract.ProtectedReadonlyValueTypeField);
            Assert.Equal(32, duckVirtual.ProtectedReadonlyValueTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.ProtectedReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.ProtectedReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.ProtectedReadonlyValueTypeField = 99;
            });

            // *
            Assert.Equal(33, duckInterface.PrivateReadonlyValueTypeField);
            Assert.Equal(33, duckAbstract.PrivateReadonlyValueTypeField);
            Assert.Equal(33, duckVirtual.PrivateReadonlyValueTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.PrivateReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.PrivateReadonlyValueTypeField = 99;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.PrivateReadonlyValueTypeField = 99;
            });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void Fields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            Assert.Equal(40, duckInterface.PublicValueTypeField);
            Assert.Equal(40, duckAbstract.PublicValueTypeField);
            Assert.Equal(40, duckVirtual.PublicValueTypeField);

            duckInterface.PublicValueTypeField = 42;
            Assert.Equal(42, duckInterface.PublicValueTypeField);
            Assert.Equal(42, duckAbstract.PublicValueTypeField);
            Assert.Equal(42, duckVirtual.PublicValueTypeField);

            duckAbstract.PublicValueTypeField = 50;
            Assert.Equal(50, duckInterface.PublicValueTypeField);
            Assert.Equal(50, duckAbstract.PublicValueTypeField);
            Assert.Equal(50, duckVirtual.PublicValueTypeField);

            duckVirtual.PublicValueTypeField = 60;
            Assert.Equal(60, duckInterface.PublicValueTypeField);
            Assert.Equal(60, duckAbstract.PublicValueTypeField);
            Assert.Equal(60, duckVirtual.PublicValueTypeField);

            // *

            Assert.Equal(41, duckInterface.InternalValueTypeField);
            Assert.Equal(41, duckAbstract.InternalValueTypeField);
            Assert.Equal(41, duckVirtual.InternalValueTypeField);

            duckInterface.InternalValueTypeField = 42;
            Assert.Equal(42, duckInterface.InternalValueTypeField);
            Assert.Equal(42, duckAbstract.InternalValueTypeField);
            Assert.Equal(42, duckVirtual.InternalValueTypeField);

            duckAbstract.InternalValueTypeField = 50;
            Assert.Equal(50, duckInterface.InternalValueTypeField);
            Assert.Equal(50, duckAbstract.InternalValueTypeField);
            Assert.Equal(50, duckVirtual.InternalValueTypeField);

            duckVirtual.InternalValueTypeField = 60;
            Assert.Equal(60, duckInterface.InternalValueTypeField);
            Assert.Equal(60, duckAbstract.InternalValueTypeField);
            Assert.Equal(60, duckVirtual.InternalValueTypeField);

            // *

            Assert.Equal(42, duckInterface.ProtectedValueTypeField);
            Assert.Equal(42, duckAbstract.ProtectedValueTypeField);
            Assert.Equal(42, duckVirtual.ProtectedValueTypeField);

            duckInterface.ProtectedValueTypeField = 45;
            Assert.Equal(45, duckInterface.ProtectedValueTypeField);
            Assert.Equal(45, duckAbstract.ProtectedValueTypeField);
            Assert.Equal(45, duckVirtual.ProtectedValueTypeField);

            duckAbstract.ProtectedValueTypeField = 50;
            Assert.Equal(50, duckInterface.ProtectedValueTypeField);
            Assert.Equal(50, duckAbstract.ProtectedValueTypeField);
            Assert.Equal(50, duckVirtual.ProtectedValueTypeField);

            duckVirtual.ProtectedValueTypeField = 60;
            Assert.Equal(60, duckInterface.ProtectedValueTypeField);
            Assert.Equal(60, duckAbstract.ProtectedValueTypeField);
            Assert.Equal(60, duckVirtual.ProtectedValueTypeField);

            // *

            Assert.Equal(43, duckInterface.PrivateValueTypeField);
            Assert.Equal(43, duckAbstract.PrivateValueTypeField);
            Assert.Equal(43, duckVirtual.PrivateValueTypeField);

            duckInterface.PrivateValueTypeField = 42;
            Assert.Equal(42, duckInterface.PrivateValueTypeField);
            Assert.Equal(42, duckAbstract.PrivateValueTypeField);
            Assert.Equal(42, duckVirtual.PrivateValueTypeField);

            duckAbstract.PrivateValueTypeField = 50;
            Assert.Equal(50, duckInterface.PrivateValueTypeField);
            Assert.Equal(50, duckAbstract.PrivateValueTypeField);
            Assert.Equal(50, duckVirtual.PrivateValueTypeField);

            duckVirtual.PrivateValueTypeField = 60;
            Assert.Equal(60, duckInterface.PrivateValueTypeField);
            Assert.Equal(60, duckAbstract.PrivateValueTypeField);
            Assert.Equal(60, duckVirtual.PrivateValueTypeField);
        }

        public interface IObscureDuckType
        {
            [Duck(Name = "_publicStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int PublicStaticReadonlyValueTypeField { get; set; }

            [Duck(Name = "_internalStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int InternalStaticReadonlyValueTypeField { get; set; }

            [Duck(Name = "_protectedStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int ProtectedStaticReadonlyValueTypeField { get; set; }

            [Duck(Name = "_privateStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int PrivateStaticReadonlyValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int PublicStaticValueTypeField { get; set; }

            [Duck(Name = "_internalStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int InternalStaticValueTypeField { get; set; }

            [Duck(Name = "_protectedStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int ProtectedStaticValueTypeField { get; set; }

            [Duck(Name = "_privateStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int PrivateStaticValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int PublicReadonlyValueTypeField { get; set; }

            [Duck(Name = "_internalReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int InternalReadonlyValueTypeField { get; set; }

            [Duck(Name = "_protectedReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int ProtectedReadonlyValueTypeField { get; set; }

            [Duck(Name = "_privateReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int PrivateReadonlyValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int PublicValueTypeField { get; set; }

            [Duck(Name = "_internalValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int InternalValueTypeField { get; set; }

            [Duck(Name = "_protectedValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int ProtectedValueTypeField { get; set; }

            [Duck(Name = "_privateValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            int PrivateValueTypeField { get; set; }
        }

        public abstract class ObscureDuckTypeAbstractClass
        {
            [Duck(Name = "_publicStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PublicStaticReadonlyValueTypeField { get; set; }

            [Duck(Name = "_internalStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int InternalStaticReadonlyValueTypeField { get; set; }

            [Duck(Name = "_protectedStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int ProtectedStaticReadonlyValueTypeField { get; set; }

            [Duck(Name = "_privateStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PrivateStaticReadonlyValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PublicStaticValueTypeField { get; set; }

            [Duck(Name = "_internalStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int InternalStaticValueTypeField { get; set; }

            [Duck(Name = "_protectedStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int ProtectedStaticValueTypeField { get; set; }

            [Duck(Name = "_privateStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PrivateStaticValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PublicReadonlyValueTypeField { get; set; }

            [Duck(Name = "_internalReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int InternalReadonlyValueTypeField { get; set; }

            [Duck(Name = "_protectedReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int ProtectedReadonlyValueTypeField { get; set; }

            [Duck(Name = "_privateReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PrivateReadonlyValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PublicValueTypeField { get; set; }

            [Duck(Name = "_internalValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int InternalValueTypeField { get; set; }

            [Duck(Name = "_protectedValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int ProtectedValueTypeField { get; set; }

            [Duck(Name = "_privateValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract int PrivateValueTypeField { get; set; }
        }

        public class ObscureDuckType
        {
            [Duck(Name = "_publicStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PublicStaticReadonlyValueTypeField { get; set; }

            [Duck(Name = "_internalStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int InternalStaticReadonlyValueTypeField { get; set; }

            [Duck(Name = "_protectedStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int ProtectedStaticReadonlyValueTypeField { get; set; }

            [Duck(Name = "_privateStaticReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PrivateStaticReadonlyValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PublicStaticValueTypeField { get; set; }

            [Duck(Name = "_internalStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int InternalStaticValueTypeField { get; set; }

            [Duck(Name = "_protectedStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int ProtectedStaticValueTypeField { get; set; }

            [Duck(Name = "_privateStaticValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PrivateStaticValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PublicReadonlyValueTypeField { get; set; }

            [Duck(Name = "_internalReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int InternalReadonlyValueTypeField { get; set; }

            [Duck(Name = "_protectedReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int ProtectedReadonlyValueTypeField { get; set; }

            [Duck(Name = "_privateReadonlyValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PrivateReadonlyValueTypeField { get; set; }

            // *

            [Duck(Name = "_publicValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PublicValueTypeField { get; set; }

            [Duck(Name = "_internalValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int InternalValueTypeField { get; set; }

            [Duck(Name = "_protectedValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int ProtectedValueTypeField { get; set; }

            [Duck(Name = "_privateValueTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual int PrivateValueTypeField { get; set; }
        }
    }
}
