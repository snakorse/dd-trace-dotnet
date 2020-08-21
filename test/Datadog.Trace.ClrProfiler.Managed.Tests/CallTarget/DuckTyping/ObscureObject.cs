using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SA1202 // Elements must be ordered by access
#pragma warning disable SA1401 // Fields must be private
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable 414

namespace Datadog.Trace.ClrProfiler.Managed.Tests.CallTarget.DuckTyping
{
    public class ObscureObject
    {
        private static FieldPublicObject fieldPublicObject = new FieldPublicObject();
        private static FieldInternalObject fieldInternalObject = new FieldInternalObject();
        private static FieldPrivateObject fieldPrivateObject = new FieldPrivateObject();

        public static object GetFieldPublicObject() => fieldPublicObject;

        public static object GetFieldInternalObject() => fieldInternalObject;

        public static object GetFieldPrivateObject() => fieldPrivateObject;

        public class DummyFieldObject
        {
            public int MagicNumber = 42;
        }

        public class FieldPublicObject
        {
            public static readonly int _publicStaticReadonlyValueTypeField = 10;
            internal static readonly int _internalStaticReadonlyValueTypeField = 11;
            protected static readonly int _protectedStaticReadonlyValueTypeField = 12;
            private static readonly int _privateStaticReadonlyValueTypeField = 13;

            public static int _publicStaticValueTypeField = 20;
            internal static int _internalStaticValueTypeField = 21;
            protected static int _protectedStaticValueTypeField = 22;
            private static int _privateStaticValueTypeField = 23;

            public readonly int _publicReadonlyValueTypeField = 30;
            internal readonly int _internalReadonlyValueTypeField = 31;
            protected readonly int _protectedReadonlyValueTypeField = 32;
            private readonly int _privateReadonlyValueTypeField = 33;

            public int _publicValueTypeField = 40;
            internal int _internalValueTypeField = 41;
            protected int _protectedValueTypeField = 42;
            private int _privateValueTypeField = 43;

            // ***

            public static readonly string _publicStaticReadonlyReferenceTypeField = "10";
            internal static readonly string _internalStaticReadonlyReferenceTypeField = "11";
            protected static readonly string _protectedStaticReadonlyReferenceTypeField = "12";
            private static readonly string _privateStaticReadonlyReferenceTypeField = "13";

            public static string _publicStaticReferenceTypeField = "20";
            internal static string _internalStaticReferenceTypeField = "21";
            protected static string _protectedStaticReferenceTypeField = "22";
            private static string _privateStaticReferenceTypeField = "23";

            public readonly string _publicReadonlyReferenceTypeField = "30";
            internal readonly string _internalReadonlyReferenceTypeField = "31";
            protected readonly string _protectedReadonlyReferenceTypeField = "32";
            private readonly string _privateReadonlyReferenceTypeField = "33";

            public string _publicReferenceTypeField = "40";
            internal string _internalReferenceTypeField = "41";
            protected string _protectedReferenceTypeField = "42";
            private string _privateReferenceTypeField = "43";

            // ***

            public static readonly DummyFieldObject _publicStaticReadonlySelfTypeField = new DummyFieldObject();
            internal static readonly DummyFieldObject _internalStaticReadonlySelfTypeField = new DummyFieldObject();
            protected static readonly DummyFieldObject _protectedStaticReadonlySelfTypeField = new DummyFieldObject();
            private static readonly DummyFieldObject _privateStaticReadonlySelfTypeField = new DummyFieldObject();

            public static DummyFieldObject _publicStaticSelfTypeField = new DummyFieldObject();
            internal static DummyFieldObject _internalStaticSelfTypeField = new DummyFieldObject();
            protected static DummyFieldObject _protectedStaticSelfTypeField = new DummyFieldObject();
            private static DummyFieldObject _privateStaticSelfTypeField = new DummyFieldObject();

            public readonly DummyFieldObject _publicReadonlySelfTypeField = new DummyFieldObject();
            internal readonly DummyFieldObject _internalReadonlySelfTypeField = new DummyFieldObject();
            protected readonly DummyFieldObject _protectedReadonlySelfTypeField = new DummyFieldObject();
            private readonly DummyFieldObject _privateReadonlySelfTypeField = new DummyFieldObject();

            public DummyFieldObject _publicSelfTypeField = new DummyFieldObject();
            internal DummyFieldObject _internalSelfTypeField = new DummyFieldObject();
            protected DummyFieldObject _protectedSelfTypeField = new DummyFieldObject();
            private DummyFieldObject _privateSelfTypeField = new DummyFieldObject();
        }

        internal class FieldInternalObject
        {
            public static readonly int _publicStaticReadonlyValueTypeField = 10;
            internal static readonly int _internalStaticReadonlyValueTypeField = 11;
            protected static readonly int _protectedStaticReadonlyValueTypeField = 12;
            private static readonly int _privateStaticReadonlyValueTypeField = 13;

            public static int _publicStaticValueTypeField = 20;
            internal static int _internalStaticValueTypeField = 21;
            protected static int _protectedStaticValueTypeField = 22;
            private static int _privateStaticValueTypeField = 23;

            public readonly int _publicReadonlyValueTypeField = 30;
            internal readonly int _internalReadonlyValueTypeField = 31;
            protected readonly int _protectedReadonlyValueTypeField = 32;
            private readonly int _privateReadonlyValueTypeField = 33;

            public int _publicValueTypeField = 40;
            internal int _internalValueTypeField = 41;
            protected int _protectedValueTypeField = 42;
            private int _privateValueTypeField = 43;

            // ***

            public static readonly string _publicStaticReadonlyReferenceTypeField = "10";
            internal static readonly string _internalStaticReadonlyReferenceTypeField = "11";
            protected static readonly string _protectedStaticReadonlyReferenceTypeField = "12";
            private static readonly string _privateStaticReadonlyReferenceTypeField = "13";

            public static string _publicStaticReferenceTypeField = "20";
            internal static string _internalStaticReferenceTypeField = "21";
            protected static string _protectedStaticReferenceTypeField = "22";
            private static string _privateStaticReferenceTypeField = "23";

            public readonly string _publicReadonlyReferenceTypeField = "30";
            internal readonly string _internalReadonlyReferenceTypeField = "31";
            protected readonly string _protectedReadonlyReferenceTypeField = "32";
            private readonly string _privateReadonlyReferenceTypeField = "33";

            public string _publicReferenceTypeField = "40";
            internal string _internalReferenceTypeField = "41";
            protected string _protectedReferenceTypeField = "42";
            private string _privateReferenceTypeField = "43";

            // ***

            public static readonly DummyFieldObject _publicStaticReadonlySelfTypeField = new DummyFieldObject();
            internal static readonly DummyFieldObject _internalStaticReadonlySelfTypeField = new DummyFieldObject();
            protected static readonly DummyFieldObject _protectedStaticReadonlySelfTypeField = new DummyFieldObject();
            private static readonly DummyFieldObject _privateStaticReadonlySelfTypeField = new DummyFieldObject();

            public static DummyFieldObject _publicStaticSelfTypeField = new DummyFieldObject();
            internal static DummyFieldObject _internalStaticSelfTypeField = new DummyFieldObject();
            protected static DummyFieldObject _protectedStaticSelfTypeField = new DummyFieldObject();
            private static DummyFieldObject _privateStaticSelfTypeField = new DummyFieldObject();

            public readonly DummyFieldObject _publicReadonlySelfTypeField = new DummyFieldObject();
            internal readonly DummyFieldObject _internalReadonlySelfTypeField = new DummyFieldObject();
            protected readonly DummyFieldObject _protectedReadonlySelfTypeField = new DummyFieldObject();
            private readonly DummyFieldObject _privateReadonlySelfTypeField = new DummyFieldObject();

            public DummyFieldObject _publicSelfTypeField = new DummyFieldObject();
            internal DummyFieldObject _internalSelfTypeField = new DummyFieldObject();
            protected DummyFieldObject _protectedSelfTypeField = new DummyFieldObject();
            private DummyFieldObject _privateSelfTypeField = new DummyFieldObject();
        }

        private class FieldPrivateObject
        {
            public static readonly int _publicStaticReadonlyValueTypeField = 10;
            internal static readonly int _internalStaticReadonlyValueTypeField = 11;
            protected static readonly int _protectedStaticReadonlyValueTypeField = 12;
            private static readonly int _privateStaticReadonlyValueTypeField = 13;

            public static int _publicStaticValueTypeField = 20;
            internal static int _internalStaticValueTypeField = 21;
            protected static int _protectedStaticValueTypeField = 22;
            private static int _privateStaticValueTypeField = 23;

            public readonly int _publicReadonlyValueTypeField = 30;
            internal readonly int _internalReadonlyValueTypeField = 31;
            protected readonly int _protectedReadonlyValueTypeField = 32;
            private readonly int _privateReadonlyValueTypeField = 33;

            public int _publicValueTypeField = 40;
            internal int _internalValueTypeField = 41;
            protected int _protectedValueTypeField = 42;
            private int _privateValueTypeField = 43;

            // ***

            public static readonly string _publicStaticReadonlyReferenceTypeField = "10";
            internal static readonly string _internalStaticReadonlyReferenceTypeField = "11";
            protected static readonly string _protectedStaticReadonlyReferenceTypeField = "12";
            private static readonly string _privateStaticReadonlyReferenceTypeField = "13";

            public static string _publicStaticReferenceTypeField = "20";
            internal static string _internalStaticReferenceTypeField = "21";
            protected static string _protectedStaticReferenceTypeField = "22";
            private static string _privateStaticReferenceTypeField = "23";

            public readonly string _publicReadonlyReferenceTypeField = "30";
            internal readonly string _internalReadonlyReferenceTypeField = "31";
            protected readonly string _protectedReadonlyReferenceTypeField = "32";
            private readonly string _privateReadonlyReferenceTypeField = "33";

            public string _publicReferenceTypeField = "40";
            internal string _internalReferenceTypeField = "41";
            protected string _protectedReferenceTypeField = "42";
            private string _privateReferenceTypeField = "43";

            // ***

            public static readonly DummyFieldObject _publicStaticReadonlySelfTypeField = new DummyFieldObject();
            internal static readonly DummyFieldObject _internalStaticReadonlySelfTypeField = new DummyFieldObject();
            protected static readonly DummyFieldObject _protectedStaticReadonlySelfTypeField = new DummyFieldObject();
            private static readonly DummyFieldObject _privateStaticReadonlySelfTypeField = new DummyFieldObject();

            public static DummyFieldObject _publicStaticSelfTypeField = new DummyFieldObject();
            internal static DummyFieldObject _internalStaticSelfTypeField = new DummyFieldObject();
            protected static DummyFieldObject _protectedStaticSelfTypeField = new DummyFieldObject();
            private static DummyFieldObject _privateStaticSelfTypeField = new DummyFieldObject();

            public readonly DummyFieldObject _publicReadonlySelfTypeField = new DummyFieldObject();
            internal readonly DummyFieldObject _internalReadonlySelfTypeField = new DummyFieldObject();
            protected readonly DummyFieldObject _protectedReadonlySelfTypeField = new DummyFieldObject();
            private readonly DummyFieldObject _privateReadonlySelfTypeField = new DummyFieldObject();

            public DummyFieldObject _publicSelfTypeField = new DummyFieldObject();
            internal DummyFieldObject _internalSelfTypeField = new DummyFieldObject();
            protected DummyFieldObject _protectedSelfTypeField = new DummyFieldObject();
            private DummyFieldObject _privateSelfTypeField = new DummyFieldObject();
        }
    }
}
