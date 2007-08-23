//
// System.Runtime.InteropServices.Marshal Test Cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
//
#if !TARGET_JVM
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace MonoTests.System.Runtime.InteropServices
{
	[TestFixture]
	public class MarshalTest
	{
		[StructLayout (LayoutKind.Sequential)]
		class ClsSequential {
			public int field;
		}

		class ClsNoLayout {
			public int field;
		}

		[StructLayout (LayoutKind.Explicit)]
		class ClsExplicit {
			[FieldOffset (0)] public int field;
		}

		[StructLayout (LayoutKind.Sequential)]
		struct StrSequential {
			public int field;
		}

		struct StrNoLayout {
			public int field;
		}

		[StructLayout (LayoutKind.Explicit)]
		struct StrExplicit {
			[FieldOffset (0)] public int field;
		}

		[Test]
		public void ClassSequential ()
		{
			Marshal.SizeOf (typeof (ClsSequential));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ClassNoLayout ()
		{
			Marshal.SizeOf (typeof (ClsNoLayout));
		}

		[Test]
		public void ClassExplicit ()
		{
			Marshal.SizeOf (typeof (ClsExplicit));
		}

		[Test]
		public void StructSequential ()
		{
			Marshal.SizeOf (typeof (StrSequential));
		}

		[Test]
		public void StructNoLayout ()
		{
			Marshal.SizeOf (typeof (StrNoLayout));
		}

		[Test]
		public void StructExplicit ()
		{
			Marshal.SizeOf (typeof (StrExplicit));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArrayType ()
		{
			Marshal.SizeOf (typeof (string[]));
		}

		[Test]
		public void PtrToStringWithNull ()
		{
			Assert.IsNull (Marshal.PtrToStringAnsi (IntPtr.Zero), "A");
			Assert.IsNull (Marshal.PtrToStringUni (IntPtr.Zero), "C");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PtrToStringWithNullThrow ()
		{
			Assert.IsNull (Marshal.PtrToStringAnsi (IntPtr.Zero, 0), "B");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PtrToStringWithNullThrow2 ()
		{
			Assert.IsNull (Marshal.PtrToStringUni (IntPtr.Zero, 0), "D");
		}

		[Test]
		public unsafe void UnsafeAddrOfPinnedArrayElement () {
			short[] sarr = new short [5];
			sarr [2] = 3;

			IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement (sarr, 2);
			Assert.AreEqual (3, *(short*)ptr.ToPointer ());
		}

		[Test]
		public void AllocHGlobalZeroSize () {
			IntPtr ptr = Marshal.AllocHGlobal (0);
			Assert.IsTrue (ptr != IntPtr.Zero);
			Marshal.FreeHGlobal (ptr);
		}

		struct Foo {
			int a;
			static int b;
			long c;
			static char d;
			int e;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void OffsetOfStatic () {
			Marshal.OffsetOf (typeof (Foo), "b");
		}

		// bug #76123
		[Test]
		public void StringToHGlobalUni () {
			IntPtr handle = Marshal.StringToHGlobalUni ("unicode data");
			string s = Marshal.PtrToStringUni (handle);
			Assert.AreEqual (12, s.Length, "#1");

			handle = Marshal.StringToHGlobalUni ("unicode data string");
			s = Marshal.PtrToStringUni (handle);
			Assert.AreEqual (19, s.Length, "#2");
		}

		[Test]
		public void ReadInt32_Endian ()
		{
			IntPtr ptr = Marshal.AllocHGlobal (4);
			try {
				Marshal.WriteByte (ptr, 0, 0x01);
				Marshal.WriteByte (ptr, 1, 0x02);
				Marshal.WriteByte (ptr, 2, 0x03);
				Marshal.WriteByte (ptr, 3, 0x04);
				// Marshal MUST use the native CPU data
				if (BitConverter.IsLittleEndian){
					Assert.AreEqual (0x04030201, Marshal.ReadInt32 (ptr), "ReadInt32");
				} else {
					Assert.AreEqual (0x01020304, Marshal.ReadInt32 (ptr), "ReadInt32");
				}
			}
			finally {
				Marshal.FreeHGlobal (ptr);
			}
		}

#if NET_2_0
		private const string NotSupported = "Not supported before Windows 2000 Service Pack 3";
		private static char[] PlainText = new char[] { 'a', 'b', 'c' };
		private static byte[] AsciiPlainText = new byte[] { (byte) 'a', (byte) 'b', (byte) 'c' };

		private unsafe SecureString GetSecureString ()
		{
			fixed (char* p = &PlainText[0]) {
				return new SecureString (p, PlainText.Length);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SecureStringToBSTR_Null ()
		{
			Marshal.SecureStringToBSTR (null);
		}

		[Test]
		public void SecureStringToBSTR ()
		{
			try {
				SecureString ss = GetSecureString ();
				IntPtr p = Marshal.SecureStringToBSTR (ss);

				char[] decrypted = new char[ss.Length];
				Marshal.Copy (p, decrypted, 0, decrypted.Length);
				Assert.AreEqual (PlainText, decrypted, "Decrypted");

				Marshal.ZeroFreeBSTR (p);
			}
			catch (NotSupportedException) {
				Assert.Ignore (NotSupported);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SecureStringToCoTaskMemAnsi_Null ()
		{
			Marshal.SecureStringToCoTaskMemAnsi (null);
		}

		[Test]
		public void SecureStringToCoTaskMemAnsi ()
		{
			try {
				SecureString ss = GetSecureString ();
				IntPtr p = Marshal.SecureStringToCoTaskMemAnsi (ss);

				byte[] decrypted = new byte[ss.Length];
				Marshal.Copy (p, decrypted, 0, decrypted.Length);
				Assert.AreEqual (AsciiPlainText, decrypted, "Decrypted");

				Marshal.ZeroFreeCoTaskMemAnsi (p);
			}
			catch (NotSupportedException) {
				Assert.Ignore (NotSupported);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SecureStringToCoTaskMemUnicode_Null ()
		{
			Marshal.SecureStringToCoTaskMemUnicode (null);
		}

		[Test]
		public void SecureStringToCoTaskMemUnicode ()
		{
			try {
				SecureString ss = GetSecureString ();
				IntPtr p = Marshal.SecureStringToCoTaskMemUnicode (ss);

				char[] decrypted = new char[ss.Length];
				Marshal.Copy (p, decrypted, 0, decrypted.Length);
				Assert.AreEqual (PlainText, decrypted, "Decrypted");

				Marshal.ZeroFreeCoTaskMemUnicode (p);
			}
			catch (NotSupportedException) {
				Assert.Ignore (NotSupported);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SecureStringToGlobalAllocAnsi_Null ()
		{
			Marshal.SecureStringToGlobalAllocAnsi (null);
		}

		[Test]
		public void SecureStringToGlobalAllocAnsi ()
		{
			try {
				SecureString ss = GetSecureString ();
				IntPtr p = Marshal.SecureStringToGlobalAllocAnsi (ss);

				byte[] decrypted = new byte[ss.Length];
				Marshal.Copy (p, decrypted, 0, decrypted.Length);
				Assert.AreEqual (AsciiPlainText, decrypted, "Decrypted");

				Marshal.ZeroFreeGlobalAllocAnsi (p);
			}
			catch (NotSupportedException) {
				Assert.Ignore (NotSupported);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SecureStringToGlobalAllocUnicode_Null ()
		{
			Marshal.SecureStringToGlobalAllocUnicode (null);
		}

		[Test]
		public void SecureStringToGlobalAllocUnicode ()
		{
			try {
				SecureString ss = GetSecureString ();
				IntPtr p = Marshal.SecureStringToGlobalAllocUnicode (ss);

				char[] decrypted = new char[ss.Length];
				Marshal.Copy (p, decrypted, 0, decrypted.Length);
				Assert.AreEqual (PlainText, decrypted, "Decrypted");

				Marshal.ZeroFreeGlobalAllocUnicode (p);
			}
			catch (NotSupportedException) {
				Assert.Ignore (NotSupported);
			}
		}
#endif

		[Test]
		public void TestGetComSlotForMethodInfo ()
		{
			Assert.AreEqual	(7, Marshal.GetComSlotForMethodInfo(typeof(ITestDefault).GetMethod("DoNothing")));
			Assert.AreEqual	(7, Marshal.GetComSlotForMethodInfo(typeof(ITestDual).GetMethod("DoNothing")));
			Assert.AreEqual (7, Marshal.GetComSlotForMethodInfo (typeof(ITestDefault).GetMethod ("DoNothing")));
			Assert.AreEqual (3, Marshal.GetComSlotForMethodInfo (typeof(ITestUnknown).GetMethod ("DoNothing")));

			for (int i = 0; i < 10; i++)
				Assert.AreEqual (7+i, Marshal.GetComSlotForMethodInfo(typeof(ITestInterface).GetMethod ("Method"+i.ToString())));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestGetComSlotForMethodInfoNullException()
		{
			Marshal.GetComSlotForMethodInfo (null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestGetComSlotForMethodInfoArgumentException2 ()
		{
			Marshal.GetComSlotForMethodInfo (typeof(TestCoClass).GetMethod ("DoNothing"));
		}

		[Test]
		public void TestPtrToStringAuto ()
		{
			string input = Guid.NewGuid ().ToString ();
			string output;
			string output2;
			int len = 4;
			IntPtr ptr;

			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				// Auto -> Uni
				ptr = Marshal.StringToHGlobalAuto (input);
				output = Marshal.PtrToStringUni (ptr);
				output2 = Marshal.PtrToStringUni (ptr, len);
			} else {
				// Auto -> Ansi
				ptr = Marshal.StringToHGlobalAuto (input);
				output = Marshal.PtrToStringAnsi (ptr);
				output2 = Marshal.PtrToStringAnsi (ptr, len);
			}

			try {
				Assert.AreEqual (input, output, "#1");
				Assert.AreEqual (input.Substring (0, len), output2, "#2");
			} finally {
				Marshal.FreeHGlobal (ptr);
			}
		}

		[Test]
		public void TestGlobalAlloc ()
		{
			IntPtr mem = Marshal.AllocHGlobal (100);
			mem = Marshal.ReAllocHGlobal (mem, (IntPtr) 1000000);
			Marshal.FreeHGlobal (mem);
		}
	}

	[ComImport()]
	[Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")]
	interface ITestDefault
	{
		void DoNothing ();
	}

	[ComImport()]
	[Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")]
	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	interface ITestDispatch
	{
		void DoNothing ();
	}

	[ComImport()]
	[Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	interface ITestDual
	{
		void DoNothing ();
	}

	[ComImport()]
	[Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface ITestUnknown
	{
		void DoNothing ();
	}

	[ComImport()]
	[Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")]
	interface ITestInterface
	{
		void Method0 ();
		void Method1 ();
		void Method2 ();
		void Method3 ();
		void Method4 ();
		void Method5 ();
		void Method6 ();
		void Method7 ();
		void Method8 ();
		void Method9 ();
	}

	public class TestCoClass : ITestDispatch
	{
		public void DoNothing ()
		{
		}
	}
}
#endif
