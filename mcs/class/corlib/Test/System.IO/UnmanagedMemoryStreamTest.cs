//
// System.IO.UnmanagedMemoryStreamTest.cs
//
// Authors:
// 	Sridhar Kulkarni (sridharkulkarni@gmail.com)
// 	Gert Driesen (drieseng@users.sourceforge.net)
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
		byte[] testStreamData;
		byte[] readData;
		IntPtr mem_intptr = IntPtr.Zero;
		byte* mem_byteptr = null;
		int length;
		int capacity;
		
		[SetUp]
		public void SetUp()
		{
			testStreamData = UnicodeEncoding.Unicode.GetBytes("Here is some mono testdata");
			mem_intptr = Marshal.AllocHGlobal(testStreamData.Length);
			mem_byteptr = (byte*)mem_intptr.ToPointer();
			length = testStreamData.Length;
			capacity = testStreamData.Length;
			readData = new byte[length];
		}

		[TearDown]
		public void TearDown ()
		{
			if (mem_intptr != IntPtr.Zero)
				Marshal.FreeHGlobal (mem_intptr);
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

		[Test]
		public void Constructor1 ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			Assert.AreEqual ((long) length, ums.Capacity, "#1");
			Assert.AreEqual ((long) length, ums.Length, "#2");
			Assert.AreEqual (0L, ums.Position, "#3");
			ums.Position = (length-2);
			Assert.AreEqual ((long)(length - 2), ums.Position, "#4");
			ums.Position = 0;
			ums.Seek(3L, SeekOrigin.Begin);
			Assert.AreEqual (3L, ums.Position, "#5");
			Assert.IsTrue (ums.CanRead, "#6");
			Assert.IsFalse (ums.CanWrite, "#7");
			ums.Close();
		}

		[Test]
		public void Constructor1_Length_Negative ()
		{
			try {
				new UnmanagedMemoryStream(mem_byteptr, -1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("length", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor1_Pointer_Null ()
		{
			try {
				new UnmanagedMemoryStream((byte*) null, -1);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				// Value cannot be null
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("pointer", ex.ParamName, "#6");
			}
		}

		[Test]
		[Category ("NotWorking")] // CanRead should return true even if we're at or beyond end of stream
		public void Constructor2 ()
		{
			UnmanagedMemoryStream ums;

			ums = new UnmanagedMemoryStream(mem_byteptr,
				length, 999, FileAccess.Read);
			Assert.IsTrue (ums.CanRead, "#A1");
			Assert.IsTrue (ums.CanSeek, "#A2");
			Assert.IsFalse (ums.CanWrite, "#A3");
			Assert.AreEqual (999, ums.Capacity, "#A4");
			Assert.AreEqual (length, ums.Length, "#A5");
			Assert.AreEqual (0, ums.Position, "#A6");
			ums.Close ();

			ums = new UnmanagedMemoryStream(mem_byteptr,
				length, 666, FileAccess.Write);
			Assert.IsFalse (ums.CanRead, "#B1");
			Assert.IsTrue (ums.CanSeek, "#B2");
			Assert.IsTrue (ums.CanWrite, "#B3");
			Assert.AreEqual (666, ums.Capacity, "#B4");
			Assert.AreEqual (length, ums.Length, "#B5");
			Assert.AreEqual (0, ums.Position, "#B6");
			ums.Close ();

			ums = new UnmanagedMemoryStream(mem_byteptr,
				0, 0, FileAccess.ReadWrite);
			Assert.IsTrue (ums.CanRead, "#C1");
			Assert.IsTrue (ums.CanSeek, "#C2");
			Assert.IsTrue (ums.CanWrite, "#C3");
			Assert.AreEqual (0, ums.Capacity, "#C4");
			Assert.AreEqual (0, ums.Length, "#C5");
			Assert.AreEqual (0, ums.Position, "#C6");
			ums.Close ();
		}

		[Test]
		public void Constructor2_Access_Invalid ()
		{
			try {
				new UnmanagedMemoryStream(mem_byteptr, 0, 0, (FileAccess) 666);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Enum value was out of legal range
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("access", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor2_Capacity_Negative ()
		{
			try {
				new UnmanagedMemoryStream(mem_byteptr, 0, -1, FileAccess.Read);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("capacity", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor2_Length_Negative ()
		{
			try {
				new UnmanagedMemoryStream(mem_byteptr, -1, 0, FileAccess.Read);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("length", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor2_Length_Overflow ()
		{
			try {
				new UnmanagedMemoryStream(mem_byteptr, 5, 3, FileAccess.Read);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// The length cannot be greater than the capacity
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("length", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor2_Pointer_Null ()
		{
			try {
				new UnmanagedMemoryStream((byte*) null, 5, 3, FileAccess.Read);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				// Value cannot be null
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("pointer", ex.ParamName, "#6");
			}
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Flush_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.Flush();
		}

		[Test]
		[Category ("NotWorking")]
		public void Read ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, length, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			ums.Position = 0;

			ums.Read (readData, 0, (length/2));
			VerifyTestData ("R1", readData, 0, (length / 2));
			
			//Seek back to begining
			ums.Seek (0, SeekOrigin.Begin);
			
			//Read complete stream
			ums.Read (readData, 0, length);
			VerifyTestData ("r2", readData, 0, length);
			
			//Seek to mid of the stream and read till end
			ums.Seek ((length / 2), SeekOrigin.Begin);
			ums.Read (readData, 0, (length / 2));
			VerifyTestData ("r3", readData, (length / 2), (length / 2));
			ums.Close ();
		}

		[Test]
		public void Read_Buffer_Null ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			try {
				ums.Read((byte []) null, 0, 0);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				// Value cannot be null
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("buffer", ex.ParamName, "#6");
			}
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Read_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.Read(readData, 0, 0);
		}

		[Test]
		public void Read_Count_Negative ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, length, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			ums.Position = 0;
			try {
				ums.Read (readData, 0, -1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("count", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Read_Count_Overlow ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, length, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			ums.Position = 0;
			try {
				ums.Read (readData, 1, readData.Length);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		[Category ("NotWorking")] // when reading on or beyond the end of the stream we must return 0
		public void Read_EndOfStream ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, length * 2, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			Assert.AreEqual (0, ums.Read (readData, 0, 1), "#1");
			ums.Seek(length + 1, SeekOrigin.Begin);
			Assert.AreEqual (0, ums.Read (readData, 0, 1), "#2");
			ums.Close ();
		}

		[Test]
		public void Read_Offset_Negative ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, length, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			ums.Position = 0;
			try {
				ums.Read (readData, -1, 0);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("offset", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Read_WriteOnly ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length, length, FileAccess.Write);
			try {
				ums.Read(readData, 0, 1);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// Stream does not support reading
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
			ums.Close();
		}

		[Test]
		public void ReadByte ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length, length, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			ums.Position = 0;
			Assert.AreEqual (testStreamData [0], ums.ReadByte (), "#1");
			Assert.AreEqual (testStreamData [1], ums.ReadByte (), "#2");
			ums.Close();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void ReadByte_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.ReadByte();
		}

		[Test]
		[Category ("NotWorking")] // when reading on or beyond the end of the stream we must return -1
		public void ReadByte_EndOfStream ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length, length * 2, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			ums.Position = 0;
			ums.Seek(length, SeekOrigin.Begin);
			Assert.AreEqual (-1, ums.ReadByte (), "#3");
			ums.Seek(length + 1, SeekOrigin.Begin);
			Assert.AreEqual (-1, ums.ReadByte (), "#4");
			ums.Close();
		}

		[Test]
		public void ReadByte_WriteOnly ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length, length, FileAccess.Write);
			try {
				ums.ReadByte ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// Stream does not support reading
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
			ums.Close();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Seek_Closed () 
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close ();
			ums.Seek (0, SeekOrigin.Begin);
		}

		[Test]
		public void Seek_EndOfStream ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, 5, 10, FileAccess.Read);
			ums.Seek(5, SeekOrigin.Begin);
			ums.Close();
		}

		[Test]
		[Category ("NotWorking")] // seeking beyond length or even beyond capacity must be allowed
		public void Seek_Offset_Capacity ()
		{
			UnmanagedMemoryStream ums;

			// offset equals capacity
			ums = new UnmanagedMemoryStream(mem_byteptr, 5, 6, FileAccess.Read);
			ums.Seek(ums.Capacity, SeekOrigin.Begin);
			ums.Close();

			// offset exceeds capacity
			ums = new UnmanagedMemoryStream(mem_byteptr, 5, 6, FileAccess.Read);
			ums.Seek(int.MaxValue, SeekOrigin.Begin);
			ums.Close();
		}

		[Test]
		public void Seek_Offset_Negative ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length, length, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			ums.Position = 0;
			try {
				ums.Seek(-1, SeekOrigin.Begin);
				Assert.Fail ("#1");
			} catch (IOException ex) {
				// An attempt was made to move the position before the beginning
				// of the stream
				Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
			ums.Close();
		}

		[Test]
		public void Write ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			ums.Write(testStreamData, 0, length);
			ums.Position = 0;
			ums.Read(readData, 0, length);
			VerifyTestData("RW1", readData, 0, length);
			ums.Close();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Write_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.Write(testStreamData, 0, length);
		}

		[Test]
		public void Write_ReadOnly ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length);
			try {
				ums.Write(testStreamData, 0, length);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// Stream does not support writing
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
			ums.Close();
		}

		[Test]
		public void WriteByte ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			ums.WriteByte (testStreamData [0]);
			ums.Seek (0, SeekOrigin.Begin);
			Assert.AreEqual (testStreamData [0], ums.ReadByte ());
			ums.Close ();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void WriteByte_Closed ()
		{
			UnmanagedMemoryStream ums = new UnmanagedMemoryStream(mem_byteptr,
				length, length, FileAccess.Write);
			ums.Close();
			ums.WriteByte(0x12);
		}

		[Test]
		public void WriteByte_ReadOnly ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			try {
				ums.WriteByte (testStreamData [0]);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// Stream does not support writing
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
			ums.Close ();
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void _SetLength()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.SetLength((length - 2));
			ums.Close();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Capacity_Disposed()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			long capacity = ums.Capacity;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Close_get_Length()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			long x = ums.Length;
		}
		
		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Close_get_Position()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			long x = ums.Position;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Close_set_Position()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.Position = (length - 1);// Set to some acceptable value.
		}
		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Close_get_PositionPointer()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			byte* bptr = ums.PositionPointer;
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Close_set_PositionPointer()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.PositionPointer = (byte *) (length - 1); //position pointer to somewhere within the length/capacity
		}
	}
}
#endif

