using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Datadog.Trace.ClrProfiler.DuckTyping;
using Xunit;

#pragma warning disable SA1201 // Elements must appear in the correct order

namespace Datadog.Trace.ClrProfiler.Managed.Tests.DuckTyping
{
    public class MethodTests
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
        public void BasicMethods(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // Integers
            Assert.Equal(20, duckInterface.Sum(10, 10));
            Assert.Equal(20, duckAbstract.Sum(10, 10));
            Assert.Equal(20, duckVirtual.Sum(10, 10));

            // Float
            Assert.Equal(20f, duckInterface.Sum(10f, 10f));
            Assert.Equal(20f, duckAbstract.Sum(10f, 10f));
            Assert.Equal(20f, duckVirtual.Sum(10f, 10f));

            // Double
            Assert.Equal(20d, duckInterface.Sum(10d, 10d));
            Assert.Equal(20d, duckAbstract.Sum(10d, 10d));
            Assert.Equal(20d, duckVirtual.Sum(10d, 10d));

            // Short
            Assert.Equal((short)20, duckInterface.Sum((short)10, (short)10));
            Assert.Equal((short)20, duckAbstract.Sum((short)10, (short)10));
            Assert.Equal((short)20, duckVirtual.Sum((short)10, (short)10));

            // Enum
            Assert.Equal(TestEnum2.Segundo, duckInterface.ShowEnum(TestEnum2.Segundo));
            Assert.Equal(TestEnum2.Segundo, duckAbstract.ShowEnum(TestEnum2.Segundo));
            Assert.Equal(TestEnum2.Segundo, duckVirtual.ShowEnum(TestEnum2.Segundo));

            // Internal Sum
            Assert.Equal(20, duckInterface.InternalSum(10, 10));
            Assert.Equal(20, duckAbstract.InternalSum(10, 10));
            Assert.Equal(20, duckVirtual.InternalSum(10, 10));

            /*// GetDefault int
            Assert.Equal(0, duckInterface.GetDefault<int>());
            Assert.Equal(0, duckAbstract.GetDefault<int>());
            Assert.Equal(0, duckVirtual.GetDefault<int>());

            // GetDefault double
            Assert.Equal(0d, duckInterface.GetDefault<double>());
            Assert.Equal(0d, duckAbstract.GetDefault<double>());
            Assert.Equal(0d, duckVirtual.GetDefault<double>());

            // GetDefault string
            Assert.Null(duckInterface.GetDefault<string>());
            Assert.Null(duckAbstract.GetDefault<string>());
            Assert.Null(duckVirtual.GetDefault<string>());*/

            duckInterface.Add("Key01", new ObscureObject.DummyFieldObject());
            duckAbstract.Add("Key02", new ObscureObject.DummyFieldObject());
            duckVirtual.Add("Key03", new ObscureObject.DummyFieldObject());

            duckInterface.Add("KeyInt01", 42);
            duckAbstract.Add("KeyInt02", 42);
            duckVirtual.Add("KeyInt03", 42);

            duckInterface.Add("KeyString01", "Value01");
            duckAbstract.Add("KeyString02", "Value02");
            duckVirtual.Add("KeyString03", "Value03");
        }

        public interface IObscureDuckType
        {
            int Sum(int a, int b);

            float Sum(float a, float b);

            double Sum(double a, double b);

            short Sum(short a, short b);

            TestEnum2 ShowEnum(TestEnum2 val);

            [Duck(BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic)]
            object InternalSum(int a, int b);

            // T GetDefault<T>();

            void Add(string name, object obj);

            void Add(string name, int obj);

            void Add(string name, string obj = "none");
        }

        public abstract class ObscureDuckTypeAbstractClass
        {
            public abstract int Sum(int a, int b);

            public abstract float Sum(float a, float b);

            public abstract double Sum(double a, double b);

            public abstract short Sum(short a, short b);

            public abstract TestEnum2 ShowEnum(TestEnum2 val);

            [Duck(BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic)]
            public abstract object InternalSum(int a, int b);

            // public abstract T GetDefault<T>();

            public abstract void Add(string name, object obj);

            public abstract void Add(string name, int obj);

            public abstract void Add(string name, string obj = "none");
        }

        public class ObscureDuckType
        {
            public virtual int Sum(int a, int b) => default;

            public virtual float Sum(float a, float b) => default;

            public virtual double Sum(double a, double b) => default;

            public virtual short Sum(short a, short b) => default;

            public virtual TestEnum2 ShowEnum(TestEnum2 val) => default;

            [Duck(BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic)]
            public virtual object InternalSum(int a, int b) => default;

            // public virtual T GetDefault<T>() => default;

            public virtual void Add(string name, object obj)
            {
            }

            public virtual void Add(string name, int obj)
            {
            }

            public virtual void Add(string name, string obj = "none")
            {
            }
        }

        public enum TestEnum2
        {
            Primero,
            Segundo
        }
    }
}
