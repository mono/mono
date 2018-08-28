using System;
using System.Runtime.InteropServices;

namespace marshalertest
{
    struct MarshalTest : ICustomMarshaler
    {
        public static int NextId = 1;

        public int Id;
        public string Key;
        private IntPtr _stored;

        public static ICustomMarshaler GetInstance (string key) {
            return new MarshalTest() { Key = key, Id = NextId++};            
        }

        public void CleanUpManagedData (object ManagedObj) {
        }

        public void CleanUpNativeData(IntPtr pNativeData) {
            Console.WriteLine($"CleanUpNativeData: {this}, pNativeData = {pNativeData}");
        }

        public int GetNativeDataSize () {
            return -1;
        }

        public IntPtr MarshalManagedToNative (object ManagedObj) {
            _stored = (IntPtr)int.Parse((string)ManagedObj);
            Console.WriteLine($"MarshalManagedToNative: {this}, returning = {_stored}");
            return _stored;
        }

        public object MarshalNativeToManaged (IntPtr pNativeData) {
            Console.WriteLine($"MarshalNativeToManaged: {this}, {pNativeData}");
            return pNativeData.ToString();
        }

        public override string ToString () {
            return $"Id={Id}, Key={Key}, Stored={_stored}";
        }
    }

    struct MarshalerNoInterface {
        public static ICustomMarshaler GetInstance (string key) {
            return null;
        }
    }

    struct MarshalerNoMethod : ICustomMarshaler
    {
        public void CleanUpManagedData (object ManagedObj) {
        }

        public void CleanUpNativeData(IntPtr pNativeData) {
        }

        public int GetNativeDataSize () {
            return -1;
        }

        public IntPtr MarshalManagedToNative (object ManagedObj) {
            return IntPtr.Zero;
        }

        public object MarshalNativeToManaged (IntPtr pNativeData) {
            return null;
        }
    }

    struct MarshalerWrongArgumentType : ICustomMarshaler
    {
        public static ICustomMarshaler GetInstance (int key) {
            return null;
        }

        public void CleanUpManagedData (object ManagedObj) {
        }

        public void CleanUpNativeData(IntPtr pNativeData) {
        }

        public int GetNativeDataSize () {
            return -1;
        }

        public IntPtr MarshalManagedToNative (object ManagedObj) {
            return IntPtr.Zero;
        }

        public object MarshalNativeToManaged (IntPtr pNativeData) {
            return null;
        }
    }

    struct MarshalerWrongArgumentCount : ICustomMarshaler
    {
        public static ICustomMarshaler GetInstance (string a, string b) {
            return null;
        }

        public void CleanUpManagedData (object ManagedObj) {
        }

        public void CleanUpNativeData(IntPtr pNativeData) {
        }

        public int GetNativeDataSize () {
            return -1;
        }

        public IntPtr MarshalManagedToNative (object ManagedObj) {
            return IntPtr.Zero;
        }

        public object MarshalNativeToManaged (IntPtr pNativeData) {
            return null;
        }
    }

    struct MarshalerNullInstance : ICustomMarshaler
    {
        public static ICustomMarshaler GetInstance (string key) {
            return null;
        }

        public void CleanUpManagedData (object ManagedObj) {
        }

        public void CleanUpNativeData(IntPtr pNativeData) {
        }

        public int GetNativeDataSize () {
            return -1;
        }

        public IntPtr MarshalManagedToNative (object ManagedObj) {
            return IntPtr.Zero;
        }

        public object MarshalNativeToManaged (IntPtr pNativeData) {
            return null;
        }
    }

    struct MarshalerWrongReturnType
    {
        public static object GetInstance (string key) {
            return new MarshalerWrongReturnType();
        }
    }

    struct MarshalerGetInstanceThrows : ICustomMarshaler
    {
        public static ICustomMarshaler GetInstance (string key) {
            throw new Exception("Custom GetInstance exception");
        }

        public void CleanUpManagedData (object ManagedObj) {
        }

        public void CleanUpNativeData(IntPtr pNativeData) {
        }

        public int GetNativeDataSize () {
            return -1;
        }

        public IntPtr MarshalManagedToNative (object ManagedObj) {
            return IntPtr.Zero;
        }

        public object MarshalNativeToManaged (IntPtr pNativeData) {
            return null;
        }
    }

    struct MarshalerGetInstanceNestedThrow : ICustomMarshaler
    {
        public static ICustomMarshaler GetInstance (string key) {
            return InnerMethod1 (key);
        }

        static ICustomMarshaler InnerMethod1 (string key) {
            return InnerMethod2 (key);
        }

        static ICustomMarshaler InnerMethod2 (string key) {
            throw new Exception("Inner exception");
        }

        public void CleanUpManagedData (object ManagedObj) {
        }

        public void CleanUpNativeData(IntPtr pNativeData) {
        }

        public int GetNativeDataSize () {
            return -1;
        }

        public IntPtr MarshalManagedToNative (object ManagedObj) {
            return IntPtr.Zero;
        }

        public object MarshalNativeToManaged (IntPtr pNativeData) {
            return null;
        }
    }

    static class Program {
        const string fileName = "./test-marshaling-native.so";

        [DllImport(fileName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TestMarshalling")]
        static extern void TestMarshalling(
            [param: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalTest), MarshalCookie = "1")]
            ref string p
        );

        [DllImport(fileName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TestMarshalling")]
        static extern void TestMarshalling2(
            [param: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalTest), MarshalCookie = "2")]
            ref string p
        );

        [DllImport(fileName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TestMarshalling")]
        static extern void TestNoInterface(
            [param: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalerNoInterface))]
            ref string p
        );

        [DllImport(fileName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TestMarshalling")]
        static extern void TestNoGetInstance(
            [param: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalerNoMethod))]
            ref string p
        );

        [DllImport(fileName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TestMarshalling")]
        static extern void TestWrongArgumentType(
            [param: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalerWrongArgumentType))]
            ref string p
        );

        [DllImport(fileName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TestMarshalling")]
        static extern void TestWrongArgumentCount(
            [param: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalerWrongArgumentCount))]
            ref string p
        );

        [DllImport(fileName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TestMarshalling")]
        static extern void TestNullInstance(
            [param: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalerNullInstance))]
            ref string p
        );

        [DllImport(fileName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TestMarshalling")]
        static extern void TestWrongReturnType(
            [param: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalerWrongReturnType))]
            ref string p
        );

        [DllImport(fileName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TestMarshalling")]
        static extern void TestGetInstanceThrows(
            [param: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalerGetInstanceThrows))]
            ref string p
        );

        [DllImport(fileName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "TestMarshalling")]
        static extern void TestGetInstanceNestedThrow(
            [param: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalerGetInstanceNestedThrow))]
            ref string p
        );

        public static unsafe void Main(string[] args) {
            var param = "1";
            TestMarshalling(ref param);
            param = "2";
            TestMarshalling(ref param);
            param = "3";
            TestMarshalling2(ref param);

            try {
                Console.WriteLine("No interface");
                TestNoInterface(ref param);
            } catch (Exception exc) {
                Console.Error.WriteLine(exc);
            }
            try {
                Console.WriteLine("No getinstance");
                TestNoGetInstance(ref param);
            } catch (Exception exc) {
                Console.Error.WriteLine(exc);
            }
            try {
                Console.WriteLine("Wrong argtype");
                TestWrongArgumentType(ref param);
            } catch (Exception exc) {
                Console.Error.WriteLine(exc);
            }
            try {
                Console.WriteLine("Wrong argcount");
                TestWrongArgumentCount(ref param);
            } catch (Exception exc) {
                Console.Error.WriteLine(exc);
            }
            try {
                Console.WriteLine("Null instance");
                TestNullInstance(ref param);
            } catch (Exception exc) {
                Console.Error.WriteLine(exc);
            }
            try {
                Console.WriteLine("Wrong getinstance return type");
                TestWrongReturnType(ref param);
            } catch (Exception exc) {
                Console.Error.WriteLine(exc);
            }
            try {
                Console.WriteLine("Getinstance throws");
                TestGetInstanceThrows(ref param);
            } catch (Exception exc) {
                Console.Error.WriteLine(exc);
            }
            try {
                Console.WriteLine("Getinstance throws nested");
                TestGetInstanceNestedThrow(ref param);
            } catch (Exception exc) {
                Console.Error.WriteLine(exc);
            }
        }
    }
}
