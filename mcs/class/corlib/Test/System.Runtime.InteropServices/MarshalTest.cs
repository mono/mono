//
// System.Runtime.InteropServices.Marshal Test Cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

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
	}
}

