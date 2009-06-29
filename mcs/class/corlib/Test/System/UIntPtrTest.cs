// 
// System.UIntPtrTest.cs - Unit test for UIntPtr
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
	public class UIntPtrTest  {

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void Test64on32 () 
		{
			if (UIntPtr.Size > 4)
				throw new OverflowException ("Test only applicable to 32bits machines");

			ulong addr = UInt32.MaxValue;
			UIntPtr p = new UIntPtr (addr + 1);
		}

		[Test]
		public void TestUlongOn32 ()
		{
			// int64 can be used (as a type) with a 32bits address
			ulong max32 = UInt32.MaxValue;
			UIntPtr p32max = new UIntPtr (max32);

			ulong min32 = UInt32.MinValue;
			UIntPtr p32min = new UIntPtr (min32);
		}

		[Test]
		public void Test64on64 () 
		{
			// for 64 bits machines
			if (UIntPtr.Size > 4) {
				UIntPtr pmax = new UIntPtr (UInt64.MaxValue);
				Assert.AreEqual (UInt64.MaxValue, (ulong) pmax, "Max");

				UIntPtr pmin = new UIntPtr (UInt64.MinValue);
				Assert.AreEqual (UInt64.MinValue, (ulong) pmin, "Min");
			}
		}
	}
}