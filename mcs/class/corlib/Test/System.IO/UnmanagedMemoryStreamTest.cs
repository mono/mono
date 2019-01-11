//
// System.IO.UnmanagedMemoryStreamTest.cs
//
// Authors:
// 	Sridhar Kulkarni (sridharkulkarni@gmail.com)
// 	Gert Driesen (drieseng@users.sourceforge.net)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (c) 2006 Sridhar Kulkarni.
// Copyright (C) 2004, 2009 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using NUnit.Framework;

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
			length = testStreamData.Length;
			capacity = testStreamData.Length * 2;
			mem_intptr = Marshal.AllocHGlobal(capacity);
			mem_byteptr = (byte*)mem_intptr.ToPointer();
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
		void VerifyTestData (string id, byte [] testBytes, int start, int count)
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
		public void Flush_Stream_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.Flush();
		}

		[Test]
		public void CanRead ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.Read);
			Assert.IsTrue (ums.CanRead, "#1");
			ums.Seek (length, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanRead, "#2");
			ums.Seek (capacity, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanRead, "#3");
			ums.Seek (capacity + 1, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanRead, "#4");
			ums.Seek (0, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanRead, "#5");
			ums.Close ();
		}

		[Test]
		public void CanRead_Stream_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			ums.Close ();
			Assert.IsFalse (ums.CanRead);
		}

		[Test]
		public void CanSeek ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.Read);
			Assert.IsTrue (ums.CanSeek, "#1");
			ums.Seek (length, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanSeek, "#2");
			ums.Seek (capacity, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanSeek, "#3");
			ums.Seek (capacity + 1, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanSeek, "#4");
			ums.Seek (0, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanSeek, "#5");
			ums.Close ();
		}

		[Test]
		public void CanSeek_Stream_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			Assert.IsTrue (ums.CanSeek, "#1");
			ums.Close ();
			Assert.IsFalse (ums.CanSeek, "#2");
		}

		[Test]
		public void CanWrite ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			Assert.IsTrue (ums.CanWrite, "#1");
			ums.Seek (length, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanWrite, "#2");
			ums.Seek (capacity, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanWrite, "#3");
			ums.Seek (capacity + 1, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanWrite, "#4");
			ums.Seek (0, SeekOrigin.Begin);
			Assert.IsTrue (ums.CanWrite, "#5");
			ums.Close ();
		}

		[Test]
		public void CanWrite_Stream_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			Assert.IsTrue (ums.CanWrite, "#1");
			ums.Close ();
			Assert.IsFalse (ums.CanWrite, "#2");
		}

		[Test]
		public void Read ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			ums.Position = 0;

			Assert.AreEqual (length / 2, ums.Read (readData, 0, (length / 2)), "#1");
			VerifyTestData ("#2", readData, 0, (length / 2));
			Assert.AreEqual (length / 2, ums.Position, "#3");
			
			//Seek back to begining
			ums.Seek (0, SeekOrigin.Begin);
			
			//Read complete stream
			Assert.AreEqual (length, ums.Read (readData, 0, length), "#4");
			VerifyTestData ("#5", readData, 0, length);
			Assert.AreEqual (length, ums.Position, "#6");
			
			//Seek to mid of the stream and read till end
			ums.Seek ((length / 2), SeekOrigin.Begin);
			ums.Read (readData, 0, (length / 2));
			VerifyTestData ("#7", readData, (length / 2), (length / 2));
			Assert.AreEqual (length, ums.Position, "#8");
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
		public void Read_Stream_Closed ()
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
				// Offset and length were out of bounds for the array or count
				// is greater than the number of elements from index to the end
				// of the source collection
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void Read_EndOfStream ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, length * 2, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			Assert.AreEqual (0, ums.Read (readData, 0, 1), "#1");
			ums.Seek(length + 1, SeekOrigin.Begin);
			Assert.AreEqual (0, ums.Read (readData, 0, 1), "#2");
			ums.Seek(length - 3, SeekOrigin.Begin);
			Assert.AreEqual (3, ums.Read (readData, 0, 5), "#3");
			ums.Seek(capacity + 1, SeekOrigin.Begin);
			Assert.AreEqual (0, ums.Read (readData, 0, 1), "#4");
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
		public void Read_Offset_Overflow ()
		{
			using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream (mem_byteptr, length, length, FileAccess.ReadWrite)) {
				ums.Write (testStreamData, 0, testStreamData.Length);
				ums.Position = 0;
				try {
					ums.Read (readData, Int32.MaxValue, 0);
					Assert.Fail ("#1");
				}
				catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			}
		}

		[Test]
		public void Read_Count_Overflow ()
		{
			using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream (mem_byteptr, length, length, FileAccess.ReadWrite)) {
				ums.Write (testStreamData, 0, testStreamData.Length);
				ums.Position = 0;
				try {
					ums.Read (readData, 0, Int32.MaxValue);
					Assert.Fail ("#1");
				}
				catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
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
		public void ReadByte_Stream_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.ReadByte();
		}

		[Test]
		public void ReadByte_EndOfStream ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			ums.Position = 0;
			ums.Seek(length, SeekOrigin.Begin);
			Assert.AreEqual (-1, ums.ReadByte (), "#3");
			ums.Seek(length + 1, SeekOrigin.Begin);
			Assert.AreEqual (-1, ums.ReadByte (), "#4");
			ums.Seek(capacity, SeekOrigin.Begin);
			Assert.AreEqual (-1, ums.ReadByte (), "#5");
			ums.Seek(capacity + 1, SeekOrigin.Begin);
			Assert.AreEqual (-1, ums.ReadByte (), "#6");
			ums.Close();
		}

		[Test]
		public void ReadByte_WriteOnly ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.Write);
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
		public void Seek ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.Write);
			Assert.AreEqual (5, ums.Seek (5, SeekOrigin.Begin), "#A1");
			Assert.AreEqual (5, ums.Position, "#A2");
			Assert.AreEqual (length, ums.Length, "#A3");

			ums.Seek (2, SeekOrigin.Current);
			//Assert.AreEqual (7, ums.Seek (2, SeekOrigin.Current), "#B1");
			Assert.AreEqual (7, ums.Position, "#B2");
			Assert.AreEqual (length, ums.Length, "#B3");

			Assert.AreEqual (length + 2, ums.Seek (2, SeekOrigin.End), "#C1");
			Assert.AreEqual (length + 2, ums.Position, "#C2");
			Assert.AreEqual (length, ums.Length, "#C3");

			Assert.AreEqual (0, ums.Seek (0, SeekOrigin.Begin), "#D1");
			Assert.AreEqual (0, ums.Position, "#D2");
			Assert.AreEqual (length, ums.Length, "#D3");

			Assert.AreEqual (length - 2, ums.Seek (-2, SeekOrigin.End), "#E1");
			Assert.AreEqual (length - 2, ums.Position, "#E2");
			Assert.AreEqual (length, ums.Length, "#E3");

			Assert.AreEqual (length - 5, ums.Seek (-3, SeekOrigin.Current), "#F1");
			Assert.AreEqual (length - 5, ums.Position, "#F2");
			Assert.AreEqual (length, ums.Length, "#F3");

			Assert.AreEqual (capacity + 5, ums.Seek (capacity + 5, SeekOrigin.Begin), "#G1");
			Assert.AreEqual (capacity + 5, ums.Position, "#G2");
			Assert.AreEqual (length, ums.Length, "#G3");
		}

		[Test]
		public void Seek_Origin_Invalid ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, 5, 10, FileAccess.Read);
			try {
				ums.Seek(1, (SeekOrigin) 666);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid seek origin
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
			ums.Close();
		}

		[Test]
		public void Seek_Offset_Invalid ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			ums.Position = 0;
			try {
				ums.Seek(-1, SeekOrigin.Begin);
				Assert.Fail ("#A1");
			} catch (IOException ex) {
				// An attempt was made to move the position before the beginning
				// of the stream
				Assert.AreEqual (typeof (IOException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			ums.Position = 2;
			try {
				ums.Seek(-3, SeekOrigin.Current);
				Assert.Fail ("#B1");
			} catch (IOException ex) {
				// An attempt was made to move the position before the beginning
				// of the stream
				Assert.AreEqual (typeof (IOException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			ums.Close();
		}

		[Test]
		public void Seek_Begin_Overflow ()
		{
			using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream (mem_byteptr, 5, 10, FileAccess.Read)) {
				Assert.AreEqual (Int64.MaxValue, ums.Seek (Int64.MaxValue, SeekOrigin.Begin), "Seek");
				Assert.AreEqual (Int64.MaxValue, ums.Position, "Position");
				try {
					byte* p = ums.PositionPointer;
					Assert.Fail ("#1");
				}
				catch (IndexOutOfRangeException ex) {
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			}
		}

		[Test]
		public void Seek_Current_Overflow ()
		{
			using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream (mem_byteptr, 5, 10, FileAccess.Read)) {
				ums.ReadByte ();
				try {
					ums.Seek (Int64.MaxValue, SeekOrigin.Current);
					Assert.Fail ("#1");
				}
				catch (IOException ex) {
					Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			}
		}

		[Test]
		public void Seek_End_Overflow ()
		{
			using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream (mem_byteptr, 5, 10, FileAccess.Read)) {
				try {
					ums.Seek (Int64.MaxValue, SeekOrigin.End);
					Assert.Fail ("#1");
				}
				catch (IOException ex) {
					Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Seek_Stream_Closed () 
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close ();
			ums.Seek (0, SeekOrigin.Begin);
		}

		[Test]
		public void Write ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, length);
			Assert.AreEqual (capacity, ums.Capacity, "#A1");
			Assert.AreEqual (length, ums.Position, "#A2");
			Assert.AreEqual (length, ums.Length, "#A3");
			ums.Position = 0;
			ums.Read (readData, 0, length);
			Assert.AreEqual (capacity, ums.Capacity, "#B1");
			Assert.AreEqual (length, ums.Position, "#B2");
			Assert.AreEqual (length, ums.Length, "#B3");
			VerifyTestData ("#B4", readData, 0, length);
			ums.Write (testStreamData, 2, 2);
			Assert.AreEqual (capacity, ums.Capacity, "#C1");
			Assert.AreEqual (length + 2, ums.Position, "#C1");
			Assert.AreEqual (length + 2, ums.Length, "#C2");
			ums.Position = length;
			Assert.AreEqual (testStreamData [2], ums.ReadByte (), "#D1");
			Assert.AreEqual (testStreamData [3], ums.ReadByte (), "#D2");
			ums.Close();
		}

		[Test]
		public void Write_Capacity_Exceeded ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length, length + 2, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, length);
			ums.Write (testStreamData, 0, 2);
			try {
				ums.Write (testStreamData, 0, 1);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// Unable to expand length of this stream beyond its capacity
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
			ums.Close();
		}

		[Test]
		public void Write_Count_Negative ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			try {
				ums.Write (testStreamData, 0, -1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("count", ex.ParamName, "#6");
			}
			ums.Close();
		}

		[Test]
		public void Write_Offset_Negative ()
		{
			UnmanagedMemoryStream ums = new
				UnmanagedMemoryStream(mem_byteptr, length, capacity, FileAccess.ReadWrite);
			try {
				ums.Write (testStreamData, -1, testStreamData.Length);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("offset", ex.ParamName, "#6");
			}
			ums.Close();
		}

		[Test]
		public void Write_Offset_Overflow ()
		{
			using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream (mem_byteptr, length, capacity, FileAccess.ReadWrite)) {
				try {
					ums.Write (testStreamData, Int32.MaxValue, 1);
					Assert.Fail ("#1");
				}
				catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			}
		}

		[Test]
		public void Write_Count_Overflow ()
		{
			using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream (mem_byteptr, length, capacity, FileAccess.ReadWrite)) {
				try {
					ums.Write (testStreamData, 1, Int32.MaxValue);
					Assert.Fail ("#1");
				}
				catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Write_Stream_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			ums.Write(testStreamData, 0, length);
		}

		[Test]
		public void Write_Stream_ReadOnly ()
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
				UnmanagedMemoryStream(mem_byteptr, 3, 5, FileAccess.ReadWrite);
			ums.WriteByte (testStreamData [0]);
			Assert.AreEqual (5, ums.Capacity, "#A1");
			Assert.AreEqual (1, ums.Position, "#A2");
			Assert.AreEqual (3, ums.Length, "#A3");
			ums.WriteByte (testStreamData [1]);
			Assert.AreEqual (5, ums.Capacity, "#B1");
			Assert.AreEqual (2, ums.Position, "#B2");
			Assert.AreEqual (3, ums.Length, "#B3");
			ums.WriteByte (testStreamData [2]);
			Assert.AreEqual (5, ums.Capacity, "#C1");
			Assert.AreEqual (3, ums.Position, "#C2");
			Assert.AreEqual (3, ums.Length, "#C3");
			ums.WriteByte (testStreamData [3]);
			Assert.AreEqual (5, ums.Capacity, "#D1");
			Assert.AreEqual (4, ums.Position, "#D2");
			Assert.AreEqual (4, ums.Length, "#D3");
			ums.WriteByte (testStreamData [4]);
			Assert.AreEqual (5, ums.Capacity, "#E1");
			Assert.AreEqual (5, ums.Position, "#E2");
			Assert.AreEqual (5, ums.Length, "#E3");
			ums.Seek (0, SeekOrigin.Begin);
			Assert.AreEqual (testStreamData [0], ums.ReadByte (), "#F1");
			Assert.AreEqual (testStreamData [1], ums.ReadByte (), "#F2");
			Assert.AreEqual (testStreamData [2], ums.ReadByte (), "#F3");
			Assert.AreEqual (testStreamData [3], ums.ReadByte (), "#F4");
			Assert.AreEqual (testStreamData [4], ums.ReadByte (), "#F5");
			ums.Close ();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void WriteByte_Stream_Closed ()
		{
			UnmanagedMemoryStream ums = new UnmanagedMemoryStream(mem_byteptr,
				length, length, FileAccess.Write);
			ums.Close();
			ums.WriteByte(0x12);
		}

		[Test]
		public void WriteByte_Capacity_Exceeded ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, 1, 2, FileAccess.ReadWrite);
			ums.WriteByte (0x44);
			ums.WriteByte (0x45);
			try {
				ums.WriteByte (0x46);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// Unable to expand length of this stream beyond its capacity
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
			ums.Close();
		}

		[Test]
		public void WriteByte_Stream_ReadOnly ()
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
		public void SetLength ()
		{
			UnmanagedMemoryStream ums = new UnmanagedMemoryStream(mem_byteptr,
				length, capacity, FileAccess.ReadWrite);
			ums.Write (testStreamData, 0, testStreamData.Length);
			ums.SetLength (length - 1);
			Assert.AreEqual (capacity, ums.Capacity, "#A1");
			Assert.AreEqual (length - 1, ums.Length, "#A2");
			Assert.AreEqual (length - 1, ums.Position, "#A3");
			ums.SetLength (length + 1);
			Assert.AreEqual (capacity, ums.Capacity, "#B1");
			Assert.AreEqual (length + 1, ums.Length, "#B2");
			Assert.AreEqual (length - 1, ums.Position, "#B3");
			ums.SetLength (length);
			Assert.AreEqual (capacity, ums.Capacity, "#C1");
			Assert.AreEqual (length, ums.Length, "#C2");
			Assert.AreEqual (length - 1, ums.Position, "#C3");
			ums.SetLength (0);
			Assert.AreEqual (capacity, ums.Capacity, "#D1");
			Assert.AreEqual (0, ums.Length, "#D2");
			Assert.AreEqual (0, ums.Position, "#D3");
			ums.SetLength (capacity);
			Assert.AreEqual (capacity, ums.Capacity, "#E1");
			Assert.AreEqual (capacity, ums.Length, "#E2");
			Assert.AreEqual (0, ums.Position, "#E3");
			ums.Close();
		}

		[Test]
		public void SetLength_Capacity_Exceeded ()
		{
			UnmanagedMemoryStream ums = new UnmanagedMemoryStream(mem_byteptr,
				length, capacity, FileAccess.ReadWrite);
			try {
				ums.SetLength (capacity + 1);
				Assert.Fail ("#1");
			} catch (IOException ex) {
				// Unable to expand length of this stream beyond its capacity
				Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
			ums.Close();
		}

		[Test]
		public void SetLength_Negative ()
		{
			UnmanagedMemoryStream ums = new UnmanagedMemoryStream(mem_byteptr,
				length, capacity, FileAccess.ReadWrite);
			try {
				ums.SetLength(-1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
			}
			ums.Close();
		}

		[Test]
		public void SetLength_Stream_ReadOnly ()
		{
			UnmanagedMemoryStream ums = new UnmanagedMemoryStream(mem_byteptr,
				length);
			try {
				ums.SetLength (length);
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
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Capacity_Stream_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			long capacity = ums.Capacity;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Length_Stream_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			long x = ums.Length;
		}

		[Test]
		public void Position ()
		{
			UnmanagedMemoryStream ums = new UnmanagedMemoryStream(mem_byteptr,
				length, capacity, FileAccess.ReadWrite);
			Assert.AreEqual (0, ums.Position, "#1");
			ums.Position = capacity;
			Assert.AreEqual (capacity, ums.Position, "#2");
			ums.Position = length;
			Assert.AreEqual (length, ums.Position, "#3");
			ums.Position = int.MaxValue;
			Assert.AreEqual (int.MaxValue, ums.Position, "#4");
			ums.Position = 0;
			Assert.AreEqual (0, ums.Position, "#5");
			ums.Close();
		}

		[Test]
		public void Position_MaxValue_Exceeded ()
		{
			UnmanagedMemoryStream ums = new UnmanagedMemoryStream(mem_byteptr,
				length, capacity, FileAccess.ReadWrite);
			ums.Position = 0x80000000;
			ums.Close();
		}

		[Test]
		public void Position_Negative ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			try {
				ums.Position = -1;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
			ums.Close ();
		}

		[Test]
		public void Position_Overflow ()
		{
			byte [] n = new byte [8];
			fixed (byte* p = n) {
				UnmanagedMemoryStream m = new UnmanagedMemoryStream (p, 8);
				Assert.AreEqual (0, m.Position, "Position-0");
				m.Position += 9;
				Assert.AreEqual (9, m.Position, "Position-1");
				try {
					byte* p2 = m.PositionPointer;
					Assert.Fail ("PositionPointer");
				}
				catch (IndexOutOfRangeException) {
					// expected
				}
			}
		}

		[Test]
		public void Position_Stream_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			try {
				long x = ums.Position;
				Assert.Fail ("#1: " + x);
			} catch (ObjectDisposedException) {
			}

			try {
				ums.Position = 0;
				Assert.Fail ("#2");
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		public void PositionPointer_Stream_Closed ()
		{
			UnmanagedMemoryStream ums = new 
				UnmanagedMemoryStream(mem_byteptr, length);
			ums.Close();
			try {
				byte* bptr = ums.PositionPointer;
				Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			try {
				// position pointer to somewhere within the capacity
				ums.PositionPointer = (byte*) (capacity - 1);
				Assert.Fail ("#2");
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		[ExpectedException (typeof(IOException))]
		public void PositionPointer_Underflow ()
		{
			byte [] n = new byte [8];
			fixed (byte *p = n){
				UnmanagedMemoryStream m = new UnmanagedMemoryStream (p, 8);
				m.PositionPointer = p-1;
			}
		}

		[Test]
		public void PositionPointer_Overflow ()
		{
			byte [] n = new byte [8];
			fixed (byte* p = n) {
				UnmanagedMemoryStream m = new UnmanagedMemoryStream (p, 8);
				Assert.AreEqual (0, m.Position, "Position-0");
				m.PositionPointer = p + 9;
				Assert.AreEqual (9, m.Position, "Position-1");
				try {
					byte* p2 = m.PositionPointer;
					Assert.Fail ("PositionPointer");
				}
				catch (IndexOutOfRangeException) {
					// expected
				}
			}
		}

		[Test]
		public void PositionPointer_Set ()
		{
			byte [] n = new byte [8];
			n [4] = 65;
			fixed (byte* p = n) {
				UnmanagedMemoryStream m = new UnmanagedMemoryStream (p, 8, 8, FileAccess.ReadWrite);
				m.PositionPointer = p + 4;
				Assert.AreEqual (65, m.ReadByte (), "read");
				m.WriteByte (42);
			}
			Assert.AreEqual (42, n [5], "write");
		}
		
		class MyUnmanagedMemoryStream : UnmanagedMemoryStream {

			public MyUnmanagedMemoryStream ()
			{
			}

			public void MyInitialize (byte* pointer, long length, long capacity, FileAccess access)
			{
				Initialize (pointer, length, capacity, access);
			}
		}

		[Test]
		public void Defaults_Can_Properties ()
		{
			MyUnmanagedMemoryStream s = new MyUnmanagedMemoryStream ();
			Assert.IsFalse (s.CanRead, "CanRead");
			Assert.IsFalse (s.CanSeek, "CanSeek");
			Assert.IsFalse (s.CanWrite, "CanWrite");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Defaults_Capacity ()
		{
			MyUnmanagedMemoryStream s = new MyUnmanagedMemoryStream ();
			Assert.AreEqual (0, s.Capacity, "Capacity");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Defaults_Length ()
		{
			MyUnmanagedMemoryStream s = new MyUnmanagedMemoryStream ();
			Assert.AreEqual (0, s.Length, "Length");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Defaults_Position ()
		{
			MyUnmanagedMemoryStream s = new MyUnmanagedMemoryStream ();
			Assert.AreEqual (0, s.Position, "Position");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Defaults_PositionPointer ()
		{
			MyUnmanagedMemoryStream s = new MyUnmanagedMemoryStream ();
			byte* pp = s.PositionPointer;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Defaults_ReadTimeout ()
		{
			MyUnmanagedMemoryStream s = new MyUnmanagedMemoryStream ();
			Assert.AreEqual (0, s.ReadTimeout, "ReadTimeout");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Defaults_WriteTimeout ()
		{
			MyUnmanagedMemoryStream s = new MyUnmanagedMemoryStream ();
			Assert.AreEqual (0, s.WriteTimeout, "WriteTimeout");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Initialize_Pointer_Null ()
		{
			using (MyUnmanagedMemoryStream s = new MyUnmanagedMemoryStream ()) {
				s.MyInitialize (null, 0, 0, FileAccess.Read);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Initialize_Length_Negative ()
		{
			using (MyUnmanagedMemoryStream s = new MyUnmanagedMemoryStream ()) {
				s.MyInitialize (mem_byteptr, -1, 0, FileAccess.Read);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Initialize_Capacity_Negative ()
		{
			using (MyUnmanagedMemoryStream s = new MyUnmanagedMemoryStream ()) {
				s.MyInitialize (mem_byteptr, 0, -1, FileAccess.Read);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Initialize_Access_Invalid ()
		{
			using (MyUnmanagedMemoryStream s = new MyUnmanagedMemoryStream ()) {
				s.MyInitialize (mem_byteptr, 0, 1, (FileAccess) Int32.MinValue);
			}
		}
	}
}
