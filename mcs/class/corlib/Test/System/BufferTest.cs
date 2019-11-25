//
// BufferTest.cs - NUnit Test Cases for the Buffer class.
//
// Authors
//	Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) Cesar Octavio Lopez Nataren 2002
// Copyright (C) 2004 Novell (http://www.novell.com)
// 

using NUnit.Framework;
using System;

namespace MonoTests.System {

	[TestFixture]
	public class BufferTest  {

		const int SIZE = 10;
		byte [] byteArray  = new byte [SIZE];   // 8-bits unsigned integer array
		
		[Test]
		public void BlockCopy ()
		{
			int SizeOfInt32 = 4;
			int [] myArray1 = new int [5] {1, 2, 3, 4, 5};
			int [] myArray2 = new int [10] { 0, 0, 0, 0, 0, 6, 7, 8, 9, 10 };
		
			Buffer.BlockCopy (myArray1, 0, myArray2, 0, SizeOfInt32  * myArray1.Length);
		
			for (int i = 0; i < myArray1.Length; i++) 
				Assert.AreEqual (i + 1, myArray2 [i], "TestBlockCopy Error at i=" + i);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void BlockCopy_NullSource ()
		{
			byte[] dest = new byte [8];
			Buffer.BlockCopy (null, 0, dest, 0, dest.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void BlockCopy_NullDest ()
		{
			byte[] src = new byte [8];
			Buffer.BlockCopy (src, 0, null, 0, src.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BlockCopy_ObjectSource ()
		{
			object[] src = new object [8];
			byte[] dest = new byte [8];
			Buffer.BlockCopy (src, 0, dest, 0, src.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BlockCopy_ObjectDest ()
		{
			byte[] src = new byte [8];
			object[] dest = new object [8];
			Buffer.BlockCopy (src, 0, dest, 0, src.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BlockCopy_SourceTooShort ()
		{
			byte[] src = new byte [8];
			byte[] dest = new byte [8];
			Buffer.BlockCopy (src, 4, dest, 0, src.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BlockCopy_DestTooShort ()
		{
			byte[] src = new byte [8];
			byte[] dest = new byte [8];
			Buffer.BlockCopy (src, 0, dest, 4, src.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void BlockCopy_SourceOffsetNegative ()
		{
			byte[] src = new byte [8];
			byte[] dest = new byte [8];
			Buffer.BlockCopy (src, -1, dest, 0, src.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void BlockCopy_DestOffsetNegative ()
		{
			byte[] src = new byte [8];
			byte[] dest = new byte [8];
			Buffer.BlockCopy (src, 0, dest, -1, src.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BlockCopy_SourceOffsetOverflow ()
		{
			byte[] src = new byte [8];
			byte[] dest = new byte [8];
			Buffer.BlockCopy (src, Int32.MaxValue, dest, 0, src.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BlockCopy_DestOffsetOverflow ()
		{
			byte[] src = new byte [8];
			byte[] dest = new byte [8];
			Buffer.BlockCopy (src, 0, dest, Int32.MaxValue, src.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void BlockCopy_LengthNegative ()
		{
			byte[] src = new byte [8];
			byte[] dest = new byte [8];
			Buffer.BlockCopy (src, 0, dest, 0, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BlockCopy_LengthOverflow ()
		{
			byte[] src = new byte [8];
			byte[] dest = new byte [8];
			Buffer.BlockCopy (src, 0, dest, 0, Int32.MaxValue);
		}

		[Test]
		public void ByteLength ()
		{
			int numBytes;	
			float [] floatArray = new float [10];
			float [,] floatArray2 = new float [10,10];
			float [,,] floatArray3 = new float [10,10,10];
			float [,,,] floatArray4 = new float [10,0,10,10];
			float [,,,] floatArray5 = new float [0,0,0,0];
			float [] floatArray6 = new float [0];
			BufferTest [] someArray = new BufferTest [3];
		
			try {
				Buffer.ByteLength (null);	
				Assert.Fail ("TestByteLength: ArgumentNullException not thrown");
			} catch (ArgumentNullException) {
				// do nothing, this is expected
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception on Buffer.ByteLength (null):" + e);
			}
		
			try {
				Buffer.ByteLength (someArray);	
				Assert.Fail ("TestByteLength: ArgumentException not thrown");
			} catch (ArgumentException) {
				// do nothing, this is expected
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception on Buffer.ByteLength (non primitive array):" + e);
			}
		
			numBytes = Buffer.ByteLength (floatArray);
			Assert.AreEqual (40, numBytes, "TestByteLength: wrong byteLength for floatArray");

			numBytes = Buffer.ByteLength (floatArray2);
			Assert.AreEqual (400, numBytes, "TestByteLength: wrong byteLength for floatArray2");

			numBytes = Buffer.ByteLength (floatArray3);
			Assert.AreEqual (4000, numBytes, "TestByteLength: wrong byteLength for floatArray3");

			numBytes = Buffer.ByteLength (floatArray4);
			Assert.AreEqual (0, numBytes, "TestByteLength: wrong byteLength for floatArray4");

			numBytes = Buffer.ByteLength (floatArray5);
			Assert.AreEqual (0, numBytes, "TestByteLength: wrong byteLength for floatArray5");

			numBytes = Buffer.ByteLength (floatArray6);
			Assert.AreEqual (0, numBytes, "TestByteLength: wrong byteLength for floatArray6");
		}
		
		[Test]
		public void GetByte () 
		{
			Byte [] byteArray;
			bool errorThrown = false;
			byteArray = new byte [10];
			byteArray [5] = 8;
			BufferTest [] someArray = new BufferTest [3];
		
			try {
				Buffer.GetByte (null, 5);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "TestGetByte: ArgumentNullException not thrown");
		
			errorThrown = false;
		
			try {
				Buffer.GetByte (byteArray, -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "TestGetByte: ArgumentOutOfRangeException (negative index) not implemented");
		
			errorThrown = false;
		
			try {
				Buffer.GetByte (byteArray, 12); 
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "TestGetByte: ArgumentOutOfRangeException (index bigger/equal than array's size not thrown");
		
			errorThrown = false;
		
			try {
				Buffer.GetByte (someArray, 0);	
				Assert.Fail ("TestGetByte: ArgumentException not thrown");
			} catch (ArgumentException) {
				// do nothing, this is expected
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception on Buffer.GetByte (non primitive array):" + e);
			}
		
			Assert.AreEqual ((Byte)8, Buffer.GetByte (byteArray, 5), "TestGetByte Error");
		}
	
		[Test]
		public void SetByte ()
		{
			bool errorThrown = false;
			BufferTest [] someArray = new BufferTest [3];
		
			try {
				Buffer.SetByte (null, 5, 12);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "TestSetByte: ArgumentNullException not thrown");
			errorThrown = false;
		
			try {
				Buffer.SetByte (byteArray, -1, 32);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "TestSetByte: ArgumentOutOfRangeException (case: negative index) not thrown");
			errorThrown = false;
		
			try {
				Buffer.SetByte (byteArray, 12, 43);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "TestSetByte: ArgumentOutOfRangeException (case: index bigger/equal than array'size");
			errorThrown = false;
		
			try {
				Buffer.SetByte (someArray, 0, 42);	
				Assert.Fail ("TestSetByte: ArgumentException not thrown");
			} catch (ArgumentException) {
				// do nothing, this is expected
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception on Buffer.SetByte (non primitive array):" + e);
			}
		
			Buffer.SetByte (byteArray, 3, (byte) 10);
			Assert.AreEqual ((Byte)10, Buffer.GetByte (byteArray, 3), "TestSetByte");
		}

		[Test]
		public void MemoryCopy_Simple ()
		{
			uint a = 0xAABBCCDD;
			uint b = 0;
			unsafe {
				Buffer.MemoryCopy (&a, &b, 4, 2);				
			}

			Assert.AreEqual (0xAABBCCDD, a, "#1");
			// Byte order affects this test; it determines if we
			// copy the low (0xCCDD) or high (0xAABB) bytes.
			if (BitConverter.IsLittleEndian) {
				Assert.AreEqual (0x0000CCDD, b, "#2");
			} else {
				Assert.AreEqual (0xAABB0000, b, "#2");
			}
		}
	}
}
