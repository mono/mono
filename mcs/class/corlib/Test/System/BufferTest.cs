// BufferTest.cs - NUnit Test Cases for the Buffer class.
//
// Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) Cesar Octavio Lopez Nataren 2002
// 

using NUnit.Framework;
using System;


namespace MonoTests.System
{
	public class BufferTest : TestCase
	{
		const int SIZE = 10;
		byte [] byteArray  = new byte [SIZE];   // 8-bits unsigned integer array
		float [] floatArray = new float [SIZE];
		
		public BufferTest ()
			: base ("System.Buffer test") {}
		
		public BufferTest (string name)
			: base (name) {}

		protected override void SetUp () {}


		protected override void TearDown () {}

		public static ITest Suite {
			get { 
				return new TestSuite (typeof (BufferTest)); 
			}
		}

	
		public void TestBlockCopy ()
		{
			int [] myArray1 = new int [5] {1, 2, 3, 4, 5};
			int [] myArray2 = new int [10] { 0, 0, 0, 0, 0, 6, 7, 8, 9, 10 };
		
			Buffer.BlockCopy (myArray1, 0, myArray2, 0, 20);
		
			for (int i = 0; i < 10; i++) 
				AssertEquals ("TestBlockCopy Error", i + 1, myArray1 [i]);		
		}
	
	
		public void TestByteLength ()
		{
			int numBytes;	
			bool errorThrown = false;
			float [] floatArray = new float [10];
		
			try {
				Buffer.ByteLength (null);	
			} catch (ArgumentNullException e) {
				errorThrown = true;
			}
			Assert ("TestByteLength: ArgumentNullException not thrown", errorThrown);
		
			errorThrown = false;
		
			// FIXME: test the case when the ArgumentException is thrown.
		
			numBytes = Buffer.ByteLength (floatArray);
			AssertEquals ("TestByteLength: wrong byteLength", 40, numBytes);
		}
		
		public void TestGetByte () 
		{
			Byte [] byteArray;
			bool errorThrown = false;
			byteArray = new byte [10];
			byteArray [5] = 8;
		
			try {
				Buffer.GetByte (null, 5);
			} catch (ArgumentNullException e) {
				errorThrown = true;
			}
			Assert ("TestGetByte: ArgumentNullException not thrown",
				errorThrown);
		
			errorThrown = false;
		
			try {
				Buffer.GetByte (byteArray, -1);
			} catch (ArgumentOutOfRangeException e) {
				errorThrown = true;
			}
			Assert ("TestGetByte: ArgumentOutOfRangeException (negative index) not implemented",
				errorThrown);
		
			errorThrown = false;
		
			try {
				Buffer.GetByte (byteArray, 12); 
			} catch (ArgumentOutOfRangeException e) {
				errorThrown = true;
			}
			Assert ("TestGetByte: ArgumentOutOfRangeException (index bigger/equal than array's size not thrown", errorThrown);
		
			errorThrown = false;
		
			// FIXME: test the case when the ArgumentException is thrown.
		
			AssertEquals ("TestGetByte Error", 8, Buffer.GetByte (byteArray, 5));
		}
	
	
		public void TestSetByte ()
		{
			bool errorThrown = false;
		
			try {
				Buffer.SetByte (null, 5, 12);
			} catch (ArgumentNullException e) {
				errorThrown = true;
			}
			Assert ("TestSetByte: ArgumentNullException not thrown", errorThrown);
			errorThrown = false;
		
			try {
				Buffer.SetByte (byteArray, -1, 32);
			} catch (ArgumentOutOfRangeException e) {
				errorThrown = true;
			}
			Assert ("TestSetByte: ArgumentOutOfRangeException (case: negative index) not thrown",
				errorThrown);
			errorThrown = false;
		
			try {
				Buffer.SetByte (byteArray, 12, 43);
			} catch (ArgumentOutOfRangeException e) {
				errorThrown = true;
			}
			Assert ("TestSetByte: ArgumentOutOfRangeException (case: index bigger/equal than array'size",
				errorThrown);
			errorThrown = false;
		
			// FIXME: test the case when the ArgumentException is thrown
			
			Buffer.SetByte (byteArray, 3, (byte) 10);
			AssertEquals ("TestSetByte", 10, Buffer.GetByte (byteArray, 3));
		}
	}
}
