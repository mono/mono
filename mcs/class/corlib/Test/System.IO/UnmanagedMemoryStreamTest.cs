//
// System.IO.UnmanagedMemoryStreamTest.cs
//
// Authors:
// 	Sridhar Kulkarni (sridharkulkarni@gmail.com)
//
// (c) 2006 Sridhar Kulkarni.
// Copyright (C) 2004 Novell (http://www.novell.com)
//
#if NET_2_0 && !TARGET_JVM
using NUnit.Framework;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace MonoTests.System.IO
{
    	[TestFixture]
	public unsafe class UnmanagedMemoryStreamTest {
		UnmanagedMemoryStream testunmanagedStream;
		byte[] testStreamData;
		byte[] readData;
		IntPtr mem_intptr = IntPtr.Zero;
		byte* mem_byteptr = null;
		int length;
		int capacity;
		
		[SetUp]
		void SetUp()
		{
			testStreamData = UnicodeEncoding.Unicode.GetBytes("Here is some mono testdata");
			mem_intptr = Marshal.AllocHGlobal(testStreamData.Length);
			mem_byteptr = (byte*)mem_intptr.ToPointer();
			length = testStreamData.Length;
			capacity = testStreamData.Length;
			readData = new byte[length];
		}
		
		public void AssertIsNull(string message, object obj)
		{
			Assert.IsNull(obj, message);
		}
		public void AssertEquals(string message, long expected, long actual)
		{
			Assert.AreEqual(expected, actual, message);
		}

		public void AssertEquals(string message, int expected, int actual)
		{
			Assert.AreEqual(expected, actual, message);
		}

		//
		// Verify that the first count bytes in testBytes are the same as
		// the count bytes from index start in testStreamData
		//
		void VerifyTestData(string id, byte[] testBytes, int start, int count)
		{
			if (testBytes == null)
				Assert.Fail(id + "+1 testBytes is null");
			
			if (start < 0 ||
			    count < 0 ||
			    start + count > testStreamData.Length ||
			    start > testStreamData.Length)
				throw new ArgumentOutOfRangeException(id + "+2");
			
			for (int test = 0; test < count; test++)
			{
				if (testBytes[test] == testStreamData[start + test])
					continue;
				
				string failStr = "testByte {0} (testStream {1}) was <{2}>, expecting <{3}>";
				failStr = String.Format(failStr,
							test,
							start + test,
							testBytes[test],
							testStreamData[start + test]);
				Assert.Fail(id + "-3" + failStr);
			}
		}
		

		//Test construction
		[Test]
		public void ConstructorOne(){
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			AssertEquals("#01", (long)length, ums.Length);
			AssertEquals("#02", 0L, ums.Position);
			ums.Position = (length-2);
			AssertEquals("#03", (long)(length - 2), ums.Position);
			ums.Position = 0;
			ums.Seek(3L, SeekOrigin.Begin);
			AssertEquals("#04", 3L, ums.Position);
			ums.Close();
		}
		
		[Test]
		public void ConstructorTwo()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.Write);
		}
		
		[Test]
		public void ConstrucorThree(){
			

		}
		[Test]
		public void ReadBlock()
		{
			//Test simple read half of stream
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Read(readData, 0, (length/2));
			VerifyTestData("R1", readData, 0, (length/2));
			
			//Seek back to begining
			ums.Seek(0, SeekOrigin.Begin);
			
			//Read complete stream
			ums.Read(readData, 0, length);
			VerifyTestData("r2", readData, 0, length);
			
			//Seek to mid of the stream and read till end
			ums.Seek((length / 2), SeekOrigin.Begin);
			ums.Read(readData, 0, (length / 2));
			VerifyTestData("r3", readData, 0, (length/2));
			ums.Close();
		}

		[Test]
		public void ReadBytes()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Seek(0, SeekOrigin.Begin);
			AssertEquals("#R1", (int) (testStreamData.GetEnumerator()).Current, (ums.ReadByte()));
			ums.Seek(length, SeekOrigin.Begin);
			AssertEquals("#R2", -1, (ums.ReadByte()));
			ums.Close();
		}
		[Test]
		public void WriteBlock()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			ums.Write(testStreamData, 0, length);
			ums.Read(readData, 0, length);
			VerifyTestData("RW1", readData, 0, length);
			ums.Close();
		}
		[Test]
		public void WriteBytes()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			IEnumerator enumerator = testStreamData.GetEnumerator();
			ums.WriteByte((byte)enumerator.Current);
			ums.Seek(0, SeekOrigin.Begin);
			AssertEquals("RW2", (int)enumerator.Current, ums.ReadByte());
		}
		
		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void _SetLength()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.SetLength((length - 2));
			ums.Close();
		}
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Capacity_Disposed()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			long capacity = ums.Capacity;
		}
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Seek_Disposed()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.Seek(0, SeekOrigin.Begin);
		}
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Close_get_Length()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			long x = ums.Length;
		}
		
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Close_get_Position()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			long x = ums.Position;
		}
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Close_set_Position()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.Position = (length - 1);// Set to some acceptable value.
		}
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Close_get_PositionPointer()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			byte* bptr = ums.PositionPointer;
		}
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Close_set_PositionPointer()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.PositionPointer = (byte *) (length - 1); //position pointer to somewhere within the length/capacity
		}
		
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Close_Flush()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.Flush();
		}
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Close_Read()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.Read(readData, 0, (length-2));
		}
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Close_ReadByte()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			int read_byte = ums.ReadByte();
		}
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Close_Write()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.Write(testStreamData, 0, length);
		}
		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Close_WriteByte()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.WriteByte(0x12);
		}
	}
}
#endif

