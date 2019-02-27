//
// InterlockedTest.cs - NUnit Test Cases for System.Threading.Interlocked
//
// Author:
//   Luca Barbieri (luca.barbieri@gmail.com)
//
// (C) 2004 Luca Barbieri
//

using NUnit.Framework;
using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace MonoTests.System.Threading
{
	[TestFixture]
	public class InterlockedTest
	{
		int int32;
		long int64;
		float flt;
		double dbl;
		object obj;
		IntPtr iptr;

		const int int32_1 = 0x12490082;
		const int int32_2 = 0x24981071;
		const int int32_3 = 0x36078912;
		const long int64_1 = 0x1412803412472901L;
		const long int64_2 = 0x2470232089701124L;
		const long int64_3 = 0x3056432945919433L;
		const float flt_1 = 141287.109874f;
		const float flt_2 = 234108.324113f;
		const float flt_3 = 342419.752395f;
		const double dbl_1 = 141287.109874;
		const double dbl_2 = 234108.324113;
		const double dbl_3 = 342419.752395;
		readonly object obj_1 = "obj_1";
		readonly object obj_2 = "obj_2";
		readonly object obj_3 = "obj_3";
		readonly IntPtr iptr_1 = (IntPtr)int32_1;
		readonly IntPtr iptr_2 = (IntPtr)int32_2;
		readonly IntPtr iptr_3 = (IntPtr)int32_3;

		// The exchange tests are broken on AIX and cause a runtime lockup.
		void AssertNotAix()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("AIX")))
			{
				Assert.Ignore ("Skipping on AIX/i");
			}
		}

		[Test]
		public void TestExchange_Int32 ()
		{
			AssertNotAix();
			int32 = int32_1;
			Assert.AreEqual(int32_1, Interlocked.Exchange(ref int32, int32_2));
			Assert.AreEqual(int32_2, int32);
		}

		[Test]
		public void TestExchange_Flt ()
		{
			AssertNotAix();
			flt = flt_1;
			Assert.AreEqual(flt_1, Interlocked.Exchange(ref flt, flt_2));
			Assert.AreEqual(flt_2, flt);
		}

		[Test]
		public void TestExchange_Obj ()
		{
			AssertNotAix();
			obj = obj_1;
			Assert.AreEqual(obj_1, Interlocked.Exchange(ref obj, obj_2));
			Assert.AreEqual(obj_2, obj);
		}

		[Test]
		public void TestExchange_Int64 ()
		{
			AssertNotAix();
			int64 = int64_1;
			Assert.AreEqual(int64_1, Interlocked.Exchange(ref int64, int64_2));
			Assert.AreEqual(int64_2, int64);
		}

		[Test]
		public void TestExchange_Dbl ()
		{
			AssertNotAix();
			dbl = dbl_1;
			Assert.AreEqual(dbl_1, Interlocked.Exchange(ref dbl, dbl_2));
			Assert.AreEqual(dbl_2, dbl);
		}

		[Test]
		public void TestExchange_Iptr ()
		{
			AssertNotAix();
			iptr = iptr_1;
			Assert.AreEqual(iptr_1, Interlocked.Exchange(ref iptr, iptr_2));
			Assert.AreEqual(iptr_2, iptr);
		}

		[Test]
		public void TestCompareExchange_Int32 ()
		{
			AssertNotAix();
			int32 = int32_1;
			Assert.AreEqual(int32_1, Interlocked.CompareExchange(ref int32, int32_2, int32_1));
			Assert.AreEqual(int32_2, int32);
		}

		[Test]
		public void TestCompareExchange_Flt ()
		{
			AssertNotAix();
			flt = flt_1;
			Assert.AreEqual(flt_1, Interlocked.CompareExchange(ref flt, flt_2, flt_1));
			Assert.AreEqual(flt_2, flt);
		}

		[Test]
		public void TestCompareExchange_Obj ()
		{
			AssertNotAix();
			obj = obj_1;
			Assert.AreEqual(obj_1, Interlocked.CompareExchange(ref obj, obj_2, obj_1));
			Assert.AreEqual(obj_2, obj);
		}

		[Test]
		public void TestCompareExchange_Int64 ()
		{
			AssertNotAix();
			int64 = int64_1;
			Assert.AreEqual(int64_1, Interlocked.CompareExchange(ref int64, int64_2, int64_1));
			Assert.AreEqual(int64_2, int64);
		}

		[Test]
		public void TestCompareExchange_Dbl ()
		{
			AssertNotAix();
			dbl = dbl_1;
			Assert.AreEqual(dbl_1, Interlocked.CompareExchange(ref dbl, dbl_2, dbl_1));
			Assert.AreEqual(dbl_2, dbl);
		}

		[Test]
		public void TestCompareExchange_Iptr ()
		{
			AssertNotAix();
			iptr = iptr_1;
			Assert.AreEqual(iptr_1, Interlocked.CompareExchange(ref iptr, iptr_2, iptr_1));
			Assert.AreEqual(iptr_2, iptr);
		}

		[Test]
		public void TestCompareExchange_Failed_Int32 ()
		{
			AssertNotAix();
			int32 = int32_1;
			Assert.AreEqual(int32_1, Interlocked.CompareExchange(ref int32, int32_2, int32_3));
			Assert.AreEqual(int32_1, int32);
		}

		[Test]
		public void TestCompareExchange_Failed_Flt ()
		{
			AssertNotAix();
			flt = flt_1;
			Assert.AreEqual(flt_1, Interlocked.CompareExchange(ref flt, flt_2, flt_3));
			Assert.AreEqual(flt_1, flt);
		}

		[Test]
		public void TestCompareExchange_Failed_Obj ()
		{
			AssertNotAix();
			obj = obj_1;
			Assert.AreEqual(obj_1, Interlocked.CompareExchange(ref obj, obj_2, obj_3));
			Assert.AreEqual(obj_1, obj);
		}

		[Test]
		public void TestCompareExchange_Failed_Int64 ()
		{
			AssertNotAix();
			int64 = int64_1;
			Assert.AreEqual(int64_1, Interlocked.CompareExchange(ref int64, int64_2, int64_3));
			Assert.AreEqual(int64_1, int64);
		}

		[Test]
		public void TestCompareExchange_Failed_Dbl ()
		{
			AssertNotAix();
			dbl = dbl_1;
			Assert.AreEqual(dbl_1, Interlocked.CompareExchange(ref dbl, dbl_2, dbl_3));
			Assert.AreEqual(dbl_1, dbl);
		}

		[Test]
		public void TestCompareExchange_Failed_Iptr ()
		{
			AssertNotAix();
			iptr = iptr_1;
			Assert.AreEqual(iptr_1, Interlocked.CompareExchange(ref iptr, iptr_2, iptr_3));
			Assert.AreEqual(iptr_1, iptr);
		}

		[Test]
		public void TestIncrement_Int32 ()
		{
			int32 = int32_1;
			Assert.AreEqual(int32_1 + 1, Interlocked.Increment(ref int32));
			Assert.AreEqual(int32_1 + 1, int32);
		}

		[Test]
		public void TestIncrement_Int64 ()
		{
			int64 = int64_1;
			Assert.AreEqual(int64_1 + 1, Interlocked.Increment(ref int64), "func");
			Assert.AreEqual(int64_1 + 1, int64, "value");
		}

		[Test]
		public void TestDecrement_Int32 ()
		{
			int32 = int32_1;
			Assert.AreEqual(int32_1 - 1, Interlocked.Decrement(ref int32));
			Assert.AreEqual(int32_1 - 1, int32);
		}

		[Test]
		public void TestDecrement_Int64 ()
		{
			int64 = int64_1;
			Assert.AreEqual(int64_1 - 1, Interlocked.Decrement(ref int64));
			Assert.AreEqual(int64_1 - 1, int64);
		}

		[Test]
		public void TestAdd_Int32 ()
		{
			int32 = int32_1;
			Assert.AreEqual(int32_1 + int32_2, Interlocked.Add(ref int32, int32_2));
			Assert.AreEqual(int32_1 + int32_2, int32);
		}
		
		[Test]
		public void TestAdd_Int64 ()
		{
			int64 = int64_1;
			Assert.AreEqual(int64_1 + int64_2, Interlocked.Add(ref int64, int64_2));
			Assert.AreEqual(int64_1 + int64_2, int64);
		}

		[Test]
		public void TestRead_Int64()
		{
			int64 = int64_1;
			Assert.AreEqual(int64_1, Interlocked.Read(ref int64));
			Assert.AreEqual(int64_1, int64);
		}

		[Test]
		public void CompareExchange_Generic ()
		{
			object a = null;
			Assert.IsNull (Interlocked.CompareExchange<object> (ref a, a, a), "null,null,null");
			object b = new object ();
			Assert.IsNull (Interlocked.CompareExchange<object> (ref a, a, b), "null,non-null,non-null");
			Assert.IsNull (Interlocked.CompareExchange<object> (ref a, b, a), "null,non-null,null");
			Assert.AreSame (b, Interlocked.CompareExchange<object> (ref a, b, b), "null,null,non-null");

			Assert.AreSame (b, Interlocked.CompareExchange<object> (ref b, a, a), "non-null,null,null");
			Assert.AreSame (b, Interlocked.CompareExchange<object> (ref b, a, b), "non-null,null,non-null");
			Assert.AreSame (b, Interlocked.CompareExchange<object> (ref b, b, a), "non-null,non-null,null");
			Assert.AreSame (b, Interlocked.CompareExchange<object> (ref b, b, b), "non-null,non-null,non-null");
		}

		[Test]
		public void Exchange_Generic ()
		{
			object a = null;
			Assert.IsNull (Interlocked.Exchange<object> (ref a, a), "null,null");
			object b = new object ();
			Assert.IsNull (Interlocked.Exchange<object> (ref a, b), "null,non-null");
			Assert.AreSame (b, Interlocked.Exchange<object> (ref b, a), "non-null,null");
			Assert.AreSame (b, Interlocked.Exchange<object> (ref b, b), "non-null,non-null");
		}
	}
}
