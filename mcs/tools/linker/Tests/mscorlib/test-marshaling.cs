using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace marshalertest
{
    struct MarshalTest : ICustomMarshaler
    {
        public static ICustomMarshaler GetInstance (string key) {
            return new MarshalTest();
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

        public static unsafe int Main(string[] args)
        {
            // Avoid beeing linked out (need something better)
            MarshalTest.GetInstance (null);
            MarshalerNoInterface.GetInstance (null);
            MarshalerWrongArgumentType.GetInstance (0);
            MarshalerWrongArgumentCount.GetInstance ("", "");
            MarshalerNullInstance.GetInstance (null);
            MarshalerWrongReturnType.GetInstance (null);

            var param = "1";
            TestMarshalling(ref param);
            param = "2";
            TestMarshalling(ref param);
            param = "3";
            TestMarshalling2(ref param);

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

            return errorCount;
        }
    }
}
