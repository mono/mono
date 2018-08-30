using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace marshalertest
{
    struct MarshalTest : ICustomMarshaler
    {
        public static readonly List<string> Log = new List<string>();

        public static int NextId = 1;

        public int Id;
        public string Key;

        public static ICustomMarshaler GetInstance (string key) {
            return new MarshalTest() { Key = key, Id = NextId++};            
        }

        public void CleanUpManagedData (object ManagedObj) {
        }

        public void CleanUpNativeData(IntPtr pNativeData) {
            Log.Add($"{this}.CleanUpNativeData");
        }

        public int GetNativeDataSize () {
            return -1;
        }

        public IntPtr MarshalManagedToNative (object ManagedObj) {
            Log.Add($"{this}.MarshalManagedToNative");
            return IntPtr.Zero;
        }

        public object MarshalNativeToManaged (IntPtr pNativeData) {
            Log.Add($"{this}.MarshalNativeToManaged");
            return null;
        }

        public override string ToString () {
            return $"(id:{Id}, key:{Key})";
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

        public delegate void TestFn(ref string p);

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

        public static unsafe void ExpectThrow<T> (string message, TestFn func, ref int errorCount) 
            where T: Exception {

            try {
                string param = "1";
                func(ref param);
            } catch (Exception exc) {
                if (exc.GetType() != typeof(T)) {
                    Console.Error.WriteLine($"Expected {func.Method.Name} to throw {typeof(T)} but it threw {exc.GetType()}.");
                    Console.Error.WriteLine(exc);
                    Console.Error.WriteLine();
                    errorCount++;
                } else if (!exc.Message.Contains(message)) {
                    Console.Error.WriteLine($"Expected {func.Method.Name} to throw {typeof(T)} with '{message}' in its message.");
                    Console.Error.WriteLine(exc);
                    Console.Error.WriteLine();
                    errorCount++;
                }
            }
        }

        public static unsafe int Main(string[] args) {
            var param = "1";
            TestMarshalling(ref param);
            param = "2";
            TestMarshalling(ref param);
            param = "3";
            TestMarshalling2(ref param);

            var expected = new string[] {
                "(id:1, key:1).MarshalManagedToNative",
                "(id:1, key:1).MarshalNativeToManaged",
                "(id:1, key:1).CleanUpNativeData",
                "(id:1, key:1).MarshalManagedToNative",
                "(id:1, key:1).MarshalNativeToManaged",
                "(id:1, key:1).CleanUpNativeData",
                "(id:2, key:2).MarshalManagedToNative",
                "(id:2, key:2).MarshalNativeToManaged",
                "(id:2, key:2).CleanUpNativeData"
            };

            if (!expected.SequenceEqual(MarshalTest.Log)) {
                Console.Error.WriteLine("Log does not match expected sequence. Log follows:");
                foreach (var entry in MarshalTest.Log)
                    Console.Error.WriteLine(entry);

                return 1;
            }

            int errorCount = 0;

            ExpectThrow<ApplicationException>("returned null, which is not allowed", 
                TestNoInterface, ref errorCount);
            ExpectThrow<ApplicationException>("does not implement a static GetInstance method", 
                TestNoGetInstance, ref errorCount);
            ExpectThrow<ApplicationException>("does not implement a static GetInstance method", 
                TestWrongArgumentType, ref errorCount);
            ExpectThrow<ApplicationException>("does not implement a static GetInstance method", 
                TestWrongArgumentCount, ref errorCount);
            ExpectThrow<ApplicationException>("returned null, which is not allowed", 
                TestNullInstance, ref errorCount);
            ExpectThrow<ApplicationException>("does not implement a static GetInstance method", 
                TestWrongReturnType, ref errorCount);
            ExpectThrow<Exception>("Custom GetInstance exception", 
                TestGetInstanceThrows, ref errorCount);
            ExpectThrow<Exception>("Inner exception", 
                TestGetInstanceNestedThrow, ref errorCount);

            return errorCount;
        }
    }
}
