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

            public static readonly FieldPublicObject _publicStaticReadonlySelfTypeField = fieldPublicObject;
            internal static readonly FieldPublicObject _internalStaticReadonlySelfTypeField = fieldPublicObject;
            protected static readonly FieldPublicObject _protectedStaticReadonlySelfTypeField = fieldPublicObject;
            private static readonly FieldPublicObject _privateStaticReadonlySelfTypeField = fieldPublicObject;

            public static FieldPublicObject _publicStaticSelfTypeField = fieldPublicObject;
            internal static FieldPublicObject _internalStaticSelfTypeField = fieldPublicObject;
            protected static FieldPublicObject _protectedStaticSelfTypeField = fieldPublicObject;
            private static FieldPublicObject _privateStaticSelfTypeField = fieldPublicObject;

            public readonly FieldPublicObject _publicReadonlySelfTypeField = fieldPublicObject;
            internal readonly FieldPublicObject _internalReadonlySelfTypeField = fieldPublicObject;
            protected readonly FieldPublicObject _protectedReadonlySelfTypeField = fieldPublicObject;
            private readonly FieldPublicObject _privateReadonlySelfTypeField = fieldPublicObject;

            public FieldPublicObject _publicSelfTypeField = fieldPublicObject;
            internal FieldPublicObject _internalSelfTypeField = fieldPublicObject;
            protected FieldPublicObject _protectedSelfTypeField = fieldPublicObject;
            private FieldPublicObject _privateSelfTypeField = fieldPublicObject;
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

            public static readonly FieldInternalObject _publicStaticReadonlySelfTypeField = fieldInternalObject;
            internal static readonly FieldInternalObject _internalStaticReadonlySelfTypeField = fieldInternalObject;
            protected static readonly FieldInternalObject _protectedStaticReadonlySelfTypeField = fieldInternalObject;
            private static readonly FieldInternalObject _privateStaticReadonlySelfTypeField = fieldInternalObject;

            public static FieldInternalObject _publicStaticSelfTypeField = fieldInternalObject;
            internal static FieldInternalObject _internalStaticSelfTypeField = fieldInternalObject;
            protected static FieldInternalObject _protectedStaticSelfTypeField = fieldInternalObject;
            private static FieldInternalObject _privateStaticSelfTypeField = fieldInternalObject;

            public readonly FieldInternalObject _publicReadonlySelfTypeField = fieldInternalObject;
            internal readonly FieldInternalObject _internalReadonlySelfTypeField = fieldInternalObject;
            protected readonly FieldInternalObject _protectedReadonlySelfTypeField = fieldInternalObject;
            private readonly FieldInternalObject _privateReadonlySelfTypeField = fieldInternalObject;

            public FieldInternalObject _publicSelfTypeField = fieldInternalObject;
            internal FieldInternalObject _internalSelfTypeField = fieldInternalObject;
            protected FieldInternalObject _protectedSelfTypeField = fieldInternalObject;
            private FieldInternalObject _privateSelfTypeField = fieldInternalObject;
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

            public static readonly FieldPrivateObject _publicStaticReadonlySelfTypeField = fieldPrivateObject;
            internal static readonly FieldPrivateObject _internalStaticReadonlySelfTypeField = fieldPrivateObject;
            protected static readonly FieldPrivateObject _protectedStaticReadonlySelfTypeField = fieldPrivateObject;
            private static readonly FieldPrivateObject _privateStaticReadonlySelfTypeField = fieldPrivateObject;

            public static FieldPrivateObject _publicStaticSelfTypeField = fieldPrivateObject;
            internal static FieldPrivateObject _internalStaticSelfTypeField = fieldPrivateObject;
            protected static FieldPrivateObject _protectedStaticSelfTypeField = fieldPrivateObject;
            private static FieldPrivateObject _privateStaticSelfTypeField = fieldPrivateObject;

            public readonly FieldPrivateObject _publicReadonlySelfTypeField = fieldPrivateObject;
            internal readonly FieldPrivateObject _internalReadonlySelfTypeField = fieldPrivateObject;
            protected readonly FieldPrivateObject _protectedReadonlySelfTypeField = fieldPrivateObject;
            private readonly FieldPrivateObject _privateReadonlySelfTypeField = fieldPrivateObject;

            public FieldPrivateObject _publicSelfTypeField = fieldPrivateObject;
            internal FieldPrivateObject _internalSelfTypeField = fieldPrivateObject;
            protected FieldPrivateObject _protectedSelfTypeField = fieldPrivateObject;
            private FieldPrivateObject _privateSelfTypeField = fieldPrivateObject;
        }
    }
}
