using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Datadog.Trace.ClrProfiler.DuckTyping;
using Xunit;
using static Datadog.Trace.ClrProfiler.Managed.Tests.DuckTyping.TypeChainingFieldTests;

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
        public void ReturnMethods(object obscureObject)
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
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void VoidMethods(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // Void with object
            duckInterface.Add("Key01", new ObscureObject.DummyFieldObject());
            duckAbstract.Add("Key02", new ObscureObject.DummyFieldObject());
            duckVirtual.Add("Key03", new ObscureObject.DummyFieldObject());

            // Void with int
            duckInterface.Add("KeyInt01", 42);
            duckAbstract.Add("KeyInt02", 42);
            duckVirtual.Add("KeyInt03", 42);

            // Void with string
            duckInterface.Add("KeyString01", "Value01");
            duckAbstract.Add("KeyString02", "Value02");
            duckVirtual.Add("KeyString03", "Value03");
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void RefParametersMethods(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // Ref parameter
            int value = 4;
            duckInterface.Pow2(ref value);
            duckAbstract.Pow2(ref value);
            duckVirtual.Pow2(ref value);
            Assert.Equal(65536, value);

            value = 4;
            duckInterface.GetReference(ref value);
            duckAbstract.GetReference(ref value);
            duckVirtual.GetReference(ref value);
            Assert.Equal(65536, value);

            // Ref object parameter
            object objValue = 4;
            object objValue2 = objValue;
            duckInterface.GetReferenceObject(ref objValue);
            duckAbstract.GetReferenceObject(ref objValue);
            duckVirtual.GetReferenceObject(ref objValue);
            Assert.Equal(65536, (int)objValue);

            // Ref DuckType
            IDummyFieldObject refDuckType;
            refDuckType = null;
            Assert.True(duckInterface.TryGetReference(ref refDuckType));
            Assert.Equal(100, refDuckType.MagicNumber);
            Assert.True(duckAbstract.TryGetReference(ref refDuckType));
            Assert.Equal(101, refDuckType.MagicNumber);
            Assert.True(duckVirtual.TryGetReference(ref refDuckType));
            Assert.Equal(102, refDuckType.MagicNumber);

            // Ref object
            object refObject;
            refObject = null;
            Assert.True(duckInterface.TryGetReferenceObject(ref refObject));
            Assert.Equal(100, refObject.As<IDummyFieldObject>().MagicNumber);
            Assert.True(duckAbstract.TryGetReferenceObject(ref refObject));
            Assert.Equal(101, refObject.As<IDummyFieldObject>().MagicNumber);
            Assert.True(duckVirtual.TryGetReferenceObject(ref refObject));
            Assert.Equal(102, refObject.As<IDummyFieldObject>().MagicNumber);

            // Private internal parameter type with duck type output
            refDuckType = null;
            Assert.True(duckInterface.TryGetPrivateReference(ref refDuckType));
            Assert.Equal(100, refDuckType.MagicNumber);
            Assert.True(duckAbstract.TryGetPrivateReference(ref refDuckType));
            Assert.Equal(101, refDuckType.MagicNumber);
            Assert.True(duckVirtual.TryGetPrivateReference(ref refDuckType));
            Assert.Equal(102, refDuckType.MagicNumber);

            // Private internal parameter type object output
            refObject = null;
            Assert.True(duckInterface.TryGetPrivateReferenceObject(ref refObject));
            Assert.Equal(100, refObject.As<IDummyFieldObject>().MagicNumber);
            Assert.True(duckAbstract.TryGetPrivateReferenceObject(ref refObject));
            Assert.Equal(101, refObject.As<IDummyFieldObject>().MagicNumber);
            Assert.True(duckVirtual.TryGetPrivateReferenceObject(ref refObject));
            Assert.Equal(102, refObject.As<IDummyFieldObject>().MagicNumber);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void OutParametersMethods(object obscureObject)
        {
            var duckInterface = obscureObject.As<IObscureDuckType>();
            var duckAbstract = obscureObject.As<ObscureDuckTypeAbstractClass>();
            var duckVirtual = obscureObject.As<ObscureDuckType>();

            // Out parameter
            int outValue;
            duckInterface.GetOutput(out outValue);
            Assert.Equal(42, outValue);
            duckAbstract.GetOutput(out outValue);
            Assert.Equal(42, outValue);
            duckVirtual.GetOutput(out outValue);
            Assert.Equal(42, outValue);

            // Out object parameter
            object outObjectValue;
            duckInterface.GetOutputObject(out outObjectValue);
            Assert.Equal(42, (int)outObjectValue);
            duckAbstract.GetOutputObject(out outObjectValue);
            Assert.Equal(42, (int)outObjectValue);
            duckVirtual.GetOutputObject(out outObjectValue);
            Assert.Equal(42, (int)outObjectValue);

            // Duck type output
            IDummyFieldObject outDuckType;
            Assert.True(duckInterface.TryGetObscure(out outDuckType));
            Assert.NotNull(outDuckType);
            Assert.Equal(99, outDuckType.MagicNumber);

            Assert.True(duckAbstract.TryGetObscure(out outDuckType));
            Assert.NotNull(outDuckType);
            Assert.Equal(99, outDuckType.MagicNumber);

            Assert.True(duckVirtual.TryGetObscure(out outDuckType));
            Assert.NotNull(outDuckType);
            Assert.Equal(99, outDuckType.MagicNumber);

            // Object output
            object outObject;
            Assert.True(duckInterface.TryGetObscureObject(out outObject));
            Assert.NotNull(outObject);
            Assert.Equal(99, outObject.As<IDummyFieldObject>().MagicNumber);

            Assert.True(duckAbstract.TryGetObscureObject(out outObject));
            Assert.NotNull(outObject);
            Assert.Equal(99, outObject.As<IDummyFieldObject>().MagicNumber);

            Assert.True(duckVirtual.TryGetObscureObject(out outObject));
            Assert.NotNull(outObject);
            Assert.Equal(99, outObject.As<IDummyFieldObject>().MagicNumber);

            // Private internal parameter type with duck type output
            Assert.True(duckInterface.TryGetPrivateObscure(out outDuckType));
            Assert.NotNull(outDuckType);
            Assert.Equal(99, outDuckType.MagicNumber);

            Assert.True(duckAbstract.TryGetPrivateObscure(out outDuckType));
            Assert.NotNull(outDuckType);
            Assert.Equal(99, outDuckType.MagicNumber);

            Assert.True(duckVirtual.TryGetPrivateObscure(out outDuckType));
            Assert.NotNull(outDuckType);
            Assert.Equal(99, outDuckType.MagicNumber);

            // Private internal parameter type object output
            Assert.True(duckInterface.TryGetPrivateObscureObject(out outObject));
            Assert.NotNull(outObject);
            Assert.Equal(99, outObject.As<IDummyFieldObject>().MagicNumber);

            Assert.True(duckAbstract.TryGetPrivateObscureObject(out outObject));
            Assert.NotNull(outObject);
            Assert.Equal(99, outObject.As<IDummyFieldObject>().MagicNumber);

            Assert.True(duckVirtual.TryGetPrivateObscureObject(out outObject));
            Assert.NotNull(outObject);
            Assert.Equal(99, outObject.As<IDummyFieldObject>().MagicNumber);
        }

        [Fact]
        public void DictionaryDuckTypeExample()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            var duckInterface = dictionary.As<IDictioDuckType>();

            duckInterface.Add("Key01", "Value01");
            duckInterface.Add("Key02", "Value02");
            duckInterface.Add("K", "V");

            Assert.True(duckInterface.ContainsKey("K"));
            if (duckInterface.ContainsKey("K"))
            {
                Assert.True(duckInterface.Remove("K"));
            }

            if (duckInterface.TryGetValue("Key01", out string value))
            {
                Assert.Equal("Value01", value);
            }

            Assert.Equal("Value02", duckInterface["Key02"]);

            Assert.Equal(2, duckInterface.Count);

            foreach (KeyValuePair<string, string> val in duckInterface)
            {
                Assert.NotNull(val.Key);
            }

            if (duckInterface.TryGetValueInObject("Key02", out object objValue))
            {
                Assert.NotNull(objValue);
            }

            if (duckInterface.TryGetValueInDuckChaining("Key02", out IDictioValue dictioValue))
            {
                Assert.NotNull(dictioValue);
            }
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void DefaultGenericsMethods(object obscureObject)
        {
            if (obscureObject.GetType().IsPublic || obscureObject.GetType().IsNestedPublic)
            {
                var duckInterface = obscureObject.As<IDefaultGenericMethodDuckType>();
                var duckAbstract = obscureObject.As<DefaultGenericMethodDuckTypeAbstractClass>();
                var duckVirtual = obscureObject.As<DefaultGenericMethodDuckType>();

                // GetDefault int
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
                Assert.Null(duckVirtual.GetDefault<string>());

                // Wrap ints
                Tuple<int, int> wrapper = duckInterface.Wrap(10, 20);
                Assert.Equal(10, wrapper.Item1);
                Assert.Equal(20, wrapper.Item2);

                // Wrap string
                Tuple<string, string> wrapper2 = duckAbstract.Wrap("Hello", "World");
                Assert.Equal("Hello", wrapper2.Item1);
                Assert.Equal("World", wrapper2.Item2);

                // Wrap object
                Tuple<object, string> wrapper3 = duckAbstract.Wrap<object, string>(null, "World");
                Assert.Null(wrapper3.Item1);
                Assert.Equal("World", wrapper3.Item2);
            }
            else
            {
                Assert.Throws<DuckTypeProxyMethodsWithGenericParametersNotSupportedInNonPublicInstancesException>(
                    () =>
                    {
                        obscureObject.As<IDefaultGenericMethodDuckType>();
                    });
                Assert.Throws<DuckTypeProxyMethodsWithGenericParametersNotSupportedInNonPublicInstancesException>(
                    () =>
                    {
                        obscureObject.As<DefaultGenericMethodDuckTypeAbstractClass>();
                    });
                Assert.Throws<DuckTypeProxyMethodsWithGenericParametersNotSupportedInNonPublicInstancesException>(
                    () =>
                    {
                        obscureObject.As<DefaultGenericMethodDuckType>();
                    });
            }
        }

        // ...

        public interface IObscureDuckType
        {
            int Sum(int a, int b);

            float Sum(float a, float b);

            double Sum(double a, double b);

            short Sum(short a, short b);

            TestEnum2 ShowEnum(TestEnum2 val);

            [Duck(BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic)]
            object InternalSum(int a, int b);

            void Add(string name, object obj);

            void Add(string name, int obj);

            void Add(string name, string obj = "none");

            void Pow2(ref int value);

            void GetOutput(out int value);

            [Duck(Name = "GetOutput")]
            void GetOutputObject(out object value);

            bool TryGetObscure(out IDummyFieldObject obj);

            [Duck(Name = "TryGetObscure")]
            bool TryGetObscureObject(out object obj);

            void GetReference(ref int value);

            [Duck(Name = "GetReference")]
            void GetReferenceObject(ref object value);

            bool TryGetReference(ref IDummyFieldObject obj);

            [Duck(Name = "TryGetReference")]
            bool TryGetReferenceObject(ref object obj);

            bool TryGetPrivateObscure(out IDummyFieldObject obj);

            [Duck(Name = "TryGetPrivateObscure")]
            bool TryGetPrivateObscureObject(out object obj);

            bool TryGetPrivateReference(ref IDummyFieldObject obj);

            [Duck(Name = "TryGetPrivateReference")]
            bool TryGetPrivateReferenceObject(ref object obj);
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

            public abstract void Add(string name, object obj);

            public abstract void Add(string name, int obj);

            public abstract void Add(string name, string obj = "none");

            public abstract void Pow2(ref int value);

            public abstract void GetOutput(out int value);

            [Duck(Name = "GetOutput")]
            public abstract void GetOutputObject(out object value);

            public abstract bool TryGetObscure(out IDummyFieldObject obj);

            [Duck(Name = "TryGetObscure")]
            public abstract bool TryGetObscureObject(out object obj);

            public abstract void GetReference(ref int value);

            [Duck(Name = "GetReference")]
            public abstract void GetReferenceObject(ref object value);

            public abstract bool TryGetReference(ref IDummyFieldObject obj);

            [Duck(Name = "TryGetReference")]
            public abstract bool TryGetReferenceObject(ref object obj);

            public abstract bool TryGetPrivateObscure(out IDummyFieldObject obj);

            [Duck(Name = "TryGetPrivateObscure")]
            public abstract bool TryGetPrivateObscureObject(out object obj);

            public abstract bool TryGetPrivateReference(ref IDummyFieldObject obj);

            [Duck(Name = "TryGetPrivateReference")]
            public abstract bool TryGetPrivateReferenceObject(ref object obj);
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

            public virtual void Add(string name, object obj)
            {
            }

            public virtual void Add(string name, int obj)
            {
            }

            public virtual void Add(string name, string obj = "none")
            {
            }

            public virtual void Pow2(ref int value)
            {
            }

            public virtual void GetOutput(out int value)
            {
                value = default;
            }

            [Duck(Name = "GetOutput")]
            public virtual void GetOutputObject(out object value)
            {
                value = default;
            }

            public virtual bool TryGetObscure(out IDummyFieldObject obj)
            {
                obj = default;
                return false;
            }

            [Duck(Name = "TryGetObscure")]
            public virtual bool TryGetObscureObject(out object obj)
            {
                obj = default;
                return false;
            }

            public virtual void GetReference(ref int value)
            {
            }

            [Duck(Name = "GetReference")]
            public virtual void GetReferenceObject(ref object value)
            {
            }

            public virtual bool TryGetReference(ref IDummyFieldObject obj)
            {
                return false;
            }

            [Duck(Name = "TryGetReference")]
            public virtual bool TryGetReferenceObject(ref object obj)
            {
                return false;
            }

            public virtual bool TryGetPrivateObscure(out IDummyFieldObject obj)
            {
                obj = default;
                return false;
            }

            [Duck(Name = "TryGetPrivateObscure")]
            public virtual bool TryGetPrivateObscureObject(out object obj)
            {
                obj = default;
                return false;
            }

            public virtual bool TryGetPrivateReference(ref IDummyFieldObject obj)
            {
                return false;
            }

            [Duck(Name = "TryGetPrivateReference")]
            public virtual bool TryGetPrivateReferenceObject(ref object obj)
            {
                return false;
            }
        }

        public enum TestEnum2
        {
            Primero,
            Segundo
        }

        // ...

        public interface IDefaultGenericMethodDuckType
        {
            T GetDefault<T>();

            Tuple<T1, T2> Wrap<T1, T2>(T1 a, T2 b);
        }

        public abstract class DefaultGenericMethodDuckTypeAbstractClass
        {
            public abstract T GetDefault<T>();

            public abstract Tuple<T1, T2> Wrap<T1, T2>(T1 a, T2 b);
        }

        public class DefaultGenericMethodDuckType
        {
            public virtual T GetDefault<T>() => default;

            public virtual Tuple<T1, T2> Wrap<T1, T2>(T1 a, T2 b) => null;
        }

        // ...

        public interface IDictioDuckType
        {
            public string this[string key] { get; set; }

            public ICollection<string> Keys { get; }

            public ICollection<string> Values { get; }

            int Count { get; }

            void Add(string key, string value);

            bool ContainsKey(string key);

            bool Remove(string key);

            bool TryGetValue(string key, out string value);

            [Duck(Name = "TryGetValue")]
            bool TryGetValueInObject(string key, out object value);

            [Duck(Name = "TryGetValue")]
            bool TryGetValueInDuckChaining(string key, out IDictioValue value);

            IEnumerator<KeyValuePair<string, string>> GetEnumerator();
        }

        public interface IDictioValue
        {
        }
    }
}
