// 
// System.IntPtrTest.cs - Unit test for IntPtr
//
// Author
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System;
using NUnit.Framework;

namespace MonoTests.System  {

	[TestFixture]
	public class IntPtrTest  {

		// The corresponding fix was reverted (r136567)
		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (OverflowException))]
		public void Test64on32 () 
		{
			if (IntPtr.Size > 4)
				throw new OverflowException ("Test only applicable to 32bits machines");

			long addr = Int32.MaxValue;
			IntPtr p = new IntPtr (addr + 1);
		}

		[Test]
		public void TestLongOn32 ()
		{
			// int64 can be used (as a type) with a 32bits address
			long max32 = Int32.MaxValue;
			IntPtr p32max = new IntPtr (max32);

			long min32 = Int32.MinValue;
			IntPtr p32min = new IntPtr (min32);
		}

		[Test]
		public void Test64on64 () 
		{
			// for 64 bits machines
			if (IntPtr.Size > 4) {
				IntPtr pmax = new IntPtr (Int64.MaxValue);
				Assert.AreEqual (Int64.MaxValue, (long) pmax, "Max");

				IntPtr pmin = new IntPtr (Int64.MinValue);
				Assert.AreEqual (Int64.MinValue, (long) pmin, "Min");
			}
		}

#if NET_2_0
		[Test]
		public void ToStringWithFormat ()
		{
			Assert.AreEqual ("0", IntPtr.Zero.ToString ("x"), "#1");
			Assert.AreEqual ("3b9aca00", new IntPtr (1000000000).ToString ("x"), "#2");
		}
#endif
	}
}