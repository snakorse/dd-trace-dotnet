using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datadog.Trace.ClrProfiler.CallTarget.DuckTyping;
using Xunit;

#pragma warning disable SA1201 // Elements must appear in the correct order

namespace Datadog.Trace.ClrProfiler.Managed.Tests.CallTarget.DuckTyping
{
    public class DuckTypeFieldTests
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
        public void StaticReadonlyValueTypeField(object obscureObject)
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
        public void StaticValueTypeField(object obscureObject)
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
        }
    }
}
