using System.Collections.Generic;
using Datadog.Trace.ClrProfiler.CallTarget.DuckTyping;
using Xunit;

#pragma warning disable SA1201 // Elements must appear in the correct order

namespace Datadog.Trace.ClrProfiler.Managed.Tests.CallTarget.DuckTyping
{
    public class DuckTypeSelfTypeFieldTests
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

            /*

            // *
            Assert.NotEqual(duckInterface, duckInterface.PublicStaticReadonlySelfTypeField);
            Assert.NotEqual(duckAbstract, duckAbstract.PublicStaticReadonlySelfTypeField);
            Assert.NotEqual(duckVirtual, duckVirtual.PublicStaticReadonlySelfTypeField);

            Assert.NotNull(((IDuckType)duckInterface.PublicStaticReadonlySelfTypeField));

            Assert.Equal(((IDuckType)duckInterface).Instance, ((IDuckType)duckInterface.PublicStaticReadonlySelfTypeField).Instance);
            Assert.Equal(((IDuckType)duckAbstract).Instance, ((IDuckType)duckAbstract.PublicStaticReadonlySelfTypeField).Instance);
            Assert.Equal(((IDuckType)duckVirtual).Instance, ((IDuckType)duckVirtual.PublicStaticReadonlySelfTypeField).Instance);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.PublicStaticReadonlySelfTypeField = null;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.PublicStaticReadonlySelfTypeField = null;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.PublicStaticReadonlySelfTypeField = null;
            });

            // *
            Assert.NotEqual(duckInterface, duckInterface.InternalStaticReadonlySelfTypeField);
            Assert.NotEqual(duckAbstract, duckAbstract.InternalStaticReadonlySelfTypeField);
            Assert.NotEqual(duckVirtual, duckVirtual.InternalStaticReadonlySelfTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.InternalStaticReadonlySelfTypeField = null;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.InternalStaticReadonlySelfTypeField = null;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.InternalStaticReadonlySelfTypeField = null;
            });

            // *
            Assert.NotEqual(duckInterface, duckInterface.ProtectedStaticReadonlySelfTypeField);
            Assert.NotEqual(duckAbstract, duckAbstract.ProtectedStaticReadonlySelfTypeField);
            Assert.NotEqual(duckVirtual, duckVirtual.ProtectedStaticReadonlySelfTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.ProtectedStaticReadonlySelfTypeField = null;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.ProtectedStaticReadonlySelfTypeField = null;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.ProtectedStaticReadonlySelfTypeField = null;
            });

            // *
            Assert.NotEqual(duckInterface, duckInterface.PrivateStaticReadonlySelfTypeField);
            Assert.NotEqual(duckAbstract, duckAbstract.PrivateStaticReadonlySelfTypeField);
            Assert.NotEqual(duckVirtual, duckVirtual.PrivateStaticReadonlySelfTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.PrivateStaticReadonlySelfTypeField = null;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.PrivateStaticReadonlySelfTypeField = null;
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.PrivateStaticReadonlySelfTypeField = null;
            });*/
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void StaticFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

/*
            Assert.NotEqual(duckInterface, duckInterface.PublicStaticSelfTypeField);
            Assert.NotEqual(duckAbstract, duckAbstract.PublicStaticSelfTypeField);
            Assert.NotEqual(duckVirtual, duckVirtual.PublicStaticSelfTypeField);

            duckInterface.PublicStaticSelfTypeField = "42";
            Assert.Equal("42", duckInterface.PublicStaticSelfTypeField);
            Assert.Equal("42", duckAbstract.PublicStaticSelfTypeField);
            Assert.Equal("42", duckVirtual.PublicStaticSelfTypeField);

            duckAbstract.PublicStaticSelfTypeField = "50";
            Assert.Equal("50", duckInterface.PublicStaticSelfTypeField);
            Assert.Equal("50", duckAbstract.PublicStaticSelfTypeField);
            Assert.Equal("50", duckVirtual.PublicStaticSelfTypeField);

            duckVirtual.PublicStaticSelfTypeField = "60";
            Assert.Equal("60", duckInterface.PublicStaticSelfTypeField);
            Assert.Equal("60", duckAbstract.PublicStaticSelfTypeField);
            Assert.Equal("60", duckVirtual.PublicStaticSelfTypeField);

            // *

            Assert.Equal("21", duckInterface.InternalStaticSelfTypeField);
            Assert.Equal("21", duckAbstract.InternalStaticSelfTypeField);
            Assert.Equal("21", duckVirtual.InternalStaticSelfTypeField);

            duckInterface.InternalStaticSelfTypeField = "42";
            Assert.Equal("42", duckInterface.InternalStaticSelfTypeField);
            Assert.Equal("42", duckAbstract.InternalStaticSelfTypeField);
            Assert.Equal("42", duckVirtual.InternalStaticSelfTypeField);

            duckAbstract.InternalStaticSelfTypeField = "50";
            Assert.Equal("50", duckInterface.InternalStaticSelfTypeField);
            Assert.Equal("50", duckAbstract.InternalStaticSelfTypeField);
            Assert.Equal("50", duckVirtual.InternalStaticSelfTypeField);

            duckVirtual.InternalStaticSelfTypeField = "60";
            Assert.Equal("60", duckInterface.InternalStaticSelfTypeField);
            Assert.Equal("60", duckAbstract.InternalStaticSelfTypeField);
            Assert.Equal("60", duckVirtual.InternalStaticSelfTypeField);

            // *

            Assert.Equal("22", duckInterface.ProtectedStaticSelfTypeField);
            Assert.Equal("22", duckAbstract.ProtectedStaticSelfTypeField);
            Assert.Equal("22", duckVirtual.ProtectedStaticSelfTypeField);

            duckInterface.ProtectedStaticSelfTypeField = "42";
            Assert.Equal("42", duckInterface.ProtectedStaticSelfTypeField);
            Assert.Equal("42", duckAbstract.ProtectedStaticSelfTypeField);
            Assert.Equal("42", duckVirtual.ProtectedStaticSelfTypeField);

            duckAbstract.ProtectedStaticSelfTypeField = "50";
            Assert.Equal("50", duckInterface.ProtectedStaticSelfTypeField);
            Assert.Equal("50", duckAbstract.ProtectedStaticSelfTypeField);
            Assert.Equal("50", duckVirtual.ProtectedStaticSelfTypeField);

            duckVirtual.ProtectedStaticSelfTypeField = "60";
            Assert.Equal("60", duckInterface.ProtectedStaticSelfTypeField);
            Assert.Equal("60", duckAbstract.ProtectedStaticSelfTypeField);
            Assert.Equal("60", duckVirtual.ProtectedStaticSelfTypeField);

            // *

            Assert.Equal("23", duckInterface.PrivateStaticSelfTypeField);
            Assert.Equal("23", duckAbstract.PrivateStaticSelfTypeField);
            Assert.Equal("23", duckVirtual.PrivateStaticSelfTypeField);

            duckInterface.PrivateStaticSelfTypeField = "42";
            Assert.Equal("42", duckInterface.PrivateStaticSelfTypeField);
            Assert.Equal("42", duckAbstract.PrivateStaticSelfTypeField);
            Assert.Equal("42", duckVirtual.PrivateStaticSelfTypeField);

            duckAbstract.PrivateStaticSelfTypeField = "50";
            Assert.Equal("50", duckInterface.PrivateStaticSelfTypeField);
            Assert.Equal("50", duckAbstract.PrivateStaticSelfTypeField);
            Assert.Equal("50", duckVirtual.PrivateStaticSelfTypeField);

            duckVirtual.PrivateStaticSelfTypeField = "60";
            Assert.Equal("60", duckInterface.PrivateStaticSelfTypeField);
            Assert.Equal("60", duckAbstract.PrivateStaticSelfTypeField);
            Assert.Equal("60", duckVirtual.PrivateStaticSelfTypeField);*/
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void ReadonlyFields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();
/*
            // *
            Assert.Equal("30", duckInterface.PublicReadonlySelfTypeField);
            Assert.Equal("30", duckAbstract.PublicReadonlySelfTypeField);
            Assert.Equal("30", duckVirtual.PublicReadonlySelfTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.PublicReadonlySelfTypeField = "99";
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.PublicReadonlySelfTypeField = "99";
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.PublicReadonlySelfTypeField = "99";
            });

            // *
            Assert.Equal("31", duckInterface.InternalReadonlySelfTypeField);
            Assert.Equal("31", duckAbstract.InternalReadonlySelfTypeField);
            Assert.Equal("31", duckVirtual.InternalReadonlySelfTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.InternalReadonlySelfTypeField = "99";
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.InternalReadonlySelfTypeField = "99";
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.InternalReadonlySelfTypeField = "99";
            });

            // *
            Assert.Equal("32", duckInterface.ProtectedReadonlySelfTypeField);
            Assert.Equal("32", duckAbstract.ProtectedReadonlySelfTypeField);
            Assert.Equal("32", duckVirtual.ProtectedReadonlySelfTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.ProtectedReadonlySelfTypeField = "99";
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.ProtectedReadonlySelfTypeField = "99";
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.ProtectedReadonlySelfTypeField = "99";
            });

            // *
            Assert.Equal("33", duckInterface.PrivateReadonlySelfTypeField);
            Assert.Equal("33", duckAbstract.PrivateReadonlySelfTypeField);
            Assert.Equal("33", duckVirtual.PrivateReadonlySelfTypeField);

            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckInterface.PrivateReadonlySelfTypeField = "99";
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckAbstract.PrivateReadonlySelfTypeField = "99";
            });
            Assert.Throws<DuckTypeFieldIsReadonlyException>(() =>
            {
                duckVirtual.PrivateReadonlySelfTypeField = "99";
            });*/
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void Fields(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();
            /*

            Assert.Equal("40", duckInterface.PublicSelfTypeField);
            Assert.Equal("40", duckAbstract.PublicSelfTypeField);
            Assert.Equal("40", duckVirtual.PublicSelfTypeField);

            duckInterface.PublicSelfTypeField = "42";
            Assert.Equal("42", duckInterface.PublicSelfTypeField);
            Assert.Equal("42", duckAbstract.PublicSelfTypeField);
            Assert.Equal("42", duckVirtual.PublicSelfTypeField);

            duckAbstract.PublicSelfTypeField = "50";
            Assert.Equal("50", duckInterface.PublicSelfTypeField);
            Assert.Equal("50", duckAbstract.PublicSelfTypeField);
            Assert.Equal("50", duckVirtual.PublicSelfTypeField);

            duckVirtual.PublicSelfTypeField = "60";
            Assert.Equal("60", duckInterface.PublicSelfTypeField);
            Assert.Equal("60", duckAbstract.PublicSelfTypeField);
            Assert.Equal("60", duckVirtual.PublicSelfTypeField);

            // *

            Assert.Equal("41", duckInterface.InternalSelfTypeField);
            Assert.Equal("41", duckAbstract.InternalSelfTypeField);
            Assert.Equal("41", duckVirtual.InternalSelfTypeField);

            duckInterface.InternalSelfTypeField = "42";
            Assert.Equal("42", duckInterface.InternalSelfTypeField);
            Assert.Equal("42", duckAbstract.InternalSelfTypeField);
            Assert.Equal("42", duckVirtual.InternalSelfTypeField);

            duckAbstract.InternalSelfTypeField = "50";
            Assert.Equal("50", duckInterface.InternalSelfTypeField);
            Assert.Equal("50", duckAbstract.InternalSelfTypeField);
            Assert.Equal("50", duckVirtual.InternalSelfTypeField);

            duckVirtual.InternalSelfTypeField = "60";
            Assert.Equal("60", duckInterface.InternalSelfTypeField);
            Assert.Equal("60", duckAbstract.InternalSelfTypeField);
            Assert.Equal("60", duckVirtual.InternalSelfTypeField);

            // *

            Assert.Equal("42", duckInterface.ProtectedSelfTypeField);
            Assert.Equal("42", duckAbstract.ProtectedSelfTypeField);
            Assert.Equal("42", duckVirtual.ProtectedSelfTypeField);

            duckInterface.ProtectedSelfTypeField = "45";
            Assert.Equal("45", duckInterface.ProtectedSelfTypeField);
            Assert.Equal("45", duckAbstract.ProtectedSelfTypeField);
            Assert.Equal("45", duckVirtual.ProtectedSelfTypeField);

            duckAbstract.ProtectedSelfTypeField = "50";
            Assert.Equal("50", duckInterface.ProtectedSelfTypeField);
            Assert.Equal("50", duckAbstract.ProtectedSelfTypeField);
            Assert.Equal("50", duckVirtual.ProtectedSelfTypeField);

            duckVirtual.ProtectedSelfTypeField = "60";
            Assert.Equal("60", duckInterface.ProtectedSelfTypeField);
            Assert.Equal("60", duckAbstract.ProtectedSelfTypeField);
            Assert.Equal("60", duckVirtual.ProtectedSelfTypeField);

            // *

            Assert.Equal("43", duckInterface.PrivateSelfTypeField);
            Assert.Equal("43", duckAbstract.PrivateSelfTypeField);
            Assert.Equal("43", duckVirtual.PrivateSelfTypeField);

            duckInterface.PrivateSelfTypeField = "42";
            Assert.Equal("42", duckInterface.PrivateSelfTypeField);
            Assert.Equal("42", duckAbstract.PrivateSelfTypeField);
            Assert.Equal("42", duckVirtual.PrivateSelfTypeField);

            duckAbstract.PrivateSelfTypeField = "50";
            Assert.Equal("50", duckInterface.PrivateSelfTypeField);
            Assert.Equal("50", duckAbstract.PrivateSelfTypeField);
            Assert.Equal("50", duckVirtual.PrivateSelfTypeField);

            duckVirtual.PrivateSelfTypeField = "60";
            Assert.Equal("60", duckInterface.PrivateSelfTypeField);
            Assert.Equal("60", duckAbstract.PrivateSelfTypeField);
            Assert.Equal("60", duckVirtual.PrivateSelfTypeField);*/
        }

        public interface IObscureDuckType
        {
            [Duck(Name = "_publicStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject PublicStaticReadonlySelfTypeField { get; set; }

            [Duck(Name = "_internalStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject InternalStaticReadonlySelfTypeField { get; set; }

            [Duck(Name = "_protectedStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject ProtectedStaticReadonlySelfTypeField { get; set; }

            [Duck(Name = "_privateStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject PrivateStaticReadonlySelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject PublicStaticSelfTypeField { get; set; }

            [Duck(Name = "_internalStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject InternalStaticSelfTypeField { get; set; }

            [Duck(Name = "_protectedStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject ProtectedStaticSelfTypeField { get; set; }

            [Duck(Name = "_privateStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject PrivateStaticSelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject PublicReadonlySelfTypeField { get; set; }

            [Duck(Name = "_internalReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject InternalReadonlySelfTypeField { get; set; }

            [Duck(Name = "_protectedReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject ProtectedReadonlySelfTypeField { get; set; }

            [Duck(Name = "_privateReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject PrivateReadonlySelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject PublicSelfTypeField { get; set; }

            [Duck(Name = "_internalSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject InternalSelfTypeField { get; set; }

            [Duck(Name = "_protectedSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject ProtectedSelfTypeField { get; set; }

            [Duck(Name = "_privateSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            IDummyFieldObject PrivateSelfTypeField { get; set; }
        }

        public abstract class ObscureDuckTypeAbstractClass
        {
            [Duck(Name = "_publicStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject PublicStaticReadonlySelfTypeField { get; set; }

            [Duck(Name = "_internalStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject InternalStaticReadonlySelfTypeField { get; set; }

            [Duck(Name = "_protectedStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject ProtectedStaticReadonlySelfTypeField { get; set; }

            [Duck(Name = "_privateStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject PrivateStaticReadonlySelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject PublicStaticSelfTypeField { get; set; }

            [Duck(Name = "_internalStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject InternalStaticSelfTypeField { get; set; }

            [Duck(Name = "_protectedStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject ProtectedStaticSelfTypeField { get; set; }

            [Duck(Name = "_privateStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject PrivateStaticSelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject PublicReadonlySelfTypeField { get; set; }

            [Duck(Name = "_internalReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject InternalReadonlySelfTypeField { get; set; }

            [Duck(Name = "_protectedReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject ProtectedReadonlySelfTypeField { get; set; }

            [Duck(Name = "_privateReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject PrivateReadonlySelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject PublicSelfTypeField { get; set; }

            [Duck(Name = "_internalSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject InternalSelfTypeField { get; set; }

            [Duck(Name = "_protectedSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject ProtectedSelfTypeField { get; set; }

            [Duck(Name = "_privateSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public abstract IDummyFieldObject PrivateSelfTypeField { get; set; }
        }

        public class ObscureDuckType
        {
            [Duck(Name = "_publicStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject PublicStaticReadonlySelfTypeField { get; set; }

            [Duck(Name = "_internalStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject InternalStaticReadonlySelfTypeField { get; set; }

            [Duck(Name = "_protectedStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject ProtectedStaticReadonlySelfTypeField { get; set; }

            [Duck(Name = "_privateStaticReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject PrivateStaticReadonlySelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject PublicStaticSelfTypeField { get; set; }

            [Duck(Name = "_internalStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject InternalStaticSelfTypeField { get; set; }

            [Duck(Name = "_protectedStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject ProtectedStaticSelfTypeField { get; set; }

            [Duck(Name = "_privateStaticSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject PrivateStaticSelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject PublicReadonlySelfTypeField { get; set; }

            [Duck(Name = "_internalReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject InternalReadonlySelfTypeField { get; set; }

            [Duck(Name = "_protectedReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject ProtectedReadonlySelfTypeField { get; set; }

            [Duck(Name = "_privateReadonlySelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject PrivateReadonlySelfTypeField { get; set; }

            // *

            [Duck(Name = "_publicSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject PublicSelfTypeField { get; set; }

            [Duck(Name = "_internalSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject InternalSelfTypeField { get; set; }

            [Duck(Name = "_protectedSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject ProtectedSelfTypeField { get; set; }

            [Duck(Name = "_privateSelfTypeField", Kind = DuckKind.Field, BindingFlags = DuckAttribute.AllFlags)]
            public virtual IDummyFieldObject PrivateSelfTypeField { get; set; }
        }

        public interface IDummyFieldObject
        {
            [Duck(Kind = DuckKind.Field)]
            int MagicNumber { get; set; }
        }
    }
}
