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
	public class BufferTest : Assertion {

		const int SIZE = 10;
		byte [] byteArray  = new byte [SIZE];   // 8-bits unsigned integer array
		float [] floatArray = new float [SIZE];
		
		[Test]
		public void BlockCopy ()
		{
			int SizeOfInt32 = 4;
			int [] myArray1 = new int [5] {1, 2, 3, 4, 5};
			int [] myArray2 = new int [10] { 0, 0, 0, 0, 0, 6, 7, 8, 9, 10 };
		
			Buffer.BlockCopy (myArray1, 0, myArray2, 0, SizeOfInt32  * myArray1.Length);
		
			for (int i = 0; i < myArray1.Length; i++) 
				AssertEquals ("TestBlockCopy Error at i=" + i, i + 1, myArray2 [i]);		
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
			TestCase [] someArray = new TestCase [3];
		
			try {
				Buffer.ByteLength (null);	
				Fail ("TestByteLength: ArgumentNullException not thrown");
			} catch (ArgumentNullException) {
				// do nothing, this is expected
			} catch (Exception e) {
				Fail ("Unexpected exception on Buffer.ByteLength (null):" + e);
			}
		
			try {
				Buffer.ByteLength (someArray);	
				Fail ("TestByteLength: ArgumentException not thrown");
			} catch (ArgumentException) {
				// do nothing, this is expected
			} catch (Exception e) {
				Fail ("Unexpected exception on Buffer.ByteLength (non primitive array):" + e);
			}
		
			numBytes = Buffer.ByteLength (floatArray);
			AssertEquals ("TestByteLength: wrong byteLength for floatArray", 40, numBytes);

			numBytes = Buffer.ByteLength (floatArray2);
			AssertEquals ("TestByteLength: wrong byteLength for floatArray2", 400, numBytes);

			numBytes = Buffer.ByteLength (floatArray3);
			AssertEquals ("TestByteLength: wrong byteLength for floatArray3", 4000, numBytes);

			numBytes = Buffer.ByteLength (floatArray4);
			AssertEquals ("TestByteLength: wrong byteLength for floatArray4", 0, numBytes);

			numBytes = Buffer.ByteLength (floatArray5);
			AssertEquals ("TestByteLength: wrong byteLength for floatArray5", 0, numBytes);

			numBytes = Buffer.ByteLength (floatArray6);
			AssertEquals ("TestByteLength: wrong byteLength for floatArray6", 0, numBytes);
		}
		
		[Test]
		public void GetByte () 
		{
			Byte [] byteArray;
			bool errorThrown = false;
			byteArray = new byte [10];
			byteArray [5] = 8;
			TestCase [] someArray = new TestCase [3];
		
			try {
				Buffer.GetByte (null, 5);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert ("TestGetByte: ArgumentNullException not thrown",
				errorThrown);
		
			errorThrown = false;
		
			try {
				Buffer.GetByte (byteArray, -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert ("TestGetByte: ArgumentOutOfRangeException (negative index) not implemented",
				errorThrown);
		
			errorThrown = false;
		
			try {
				Buffer.GetByte (byteArray, 12); 
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert ("TestGetByte: ArgumentOutOfRangeException (index bigger/equal than array's size not thrown", errorThrown);
		
			errorThrown = false;
		
			try {
				Buffer.GetByte (someArray, 0);	
				Fail ("TestGetByte: ArgumentException not thrown");
			} catch (ArgumentException) {
				// do nothing, this is expected
			} catch (Exception e) {
				Fail ("Unexpected exception on Buffer.GetByte (non primitive array):" + e);
			}
		
			AssertEquals ("TestGetByte Error", (Byte)8, Buffer.GetByte (byteArray, 5));
		}
	
		[Test]
		public void SetByte ()
		{
			bool errorThrown = false;
			TestCase [] someArray = new TestCase [3];
		
			try {
				Buffer.SetByte (null, 5, 12);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert ("TestSetByte: ArgumentNullException not thrown", errorThrown);
			errorThrown = false;
		
			try {
				Buffer.SetByte (byteArray, -1, 32);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert ("TestSetByte: ArgumentOutOfRangeException (case: negative index) not thrown",
				errorThrown);
			errorThrown = false;
		
			try {
				Buffer.SetByte (byteArray, 12, 43);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert ("TestSetByte: ArgumentOutOfRangeException (case: index bigger/equal than array'size",
				errorThrown);
			errorThrown = false;
		
			try {
				Buffer.SetByte (someArray, 0, 42);	
				Fail ("TestSetByte: ArgumentException not thrown");
			} catch (ArgumentException) {
				// do nothing, this is expected
			} catch (Exception e) {
				Fail ("Unexpected exception on Buffer.SetByte (non primitive array):" + e);
			}
		
			Buffer.SetByte (byteArray, 3, (byte) 10);
			AssertEquals ("TestSetByte", (Byte)10, Buffer.GetByte (byteArray, 3));
		}
	}
}
