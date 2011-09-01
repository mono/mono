//
// System.IO.MemoryStreamTest
//
// Authors:
// 	Marcin Szczepanski (marcins@zipworld.com.au)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class MemoryStreamTest
	{
		MemoryStream testStream;
		byte [] testStreamData;

		[SetUp]
		public void SetUp ()
		{
			testStreamData = new byte [100];

			for (int i = 0; i < 100; i++)
				testStreamData[i] = (byte) (100 - i);

			testStream = new MemoryStream (testStreamData);
		}

		// 
		// Verify that the first count bytes in testBytes are the same as
		// the count bytes from index start in testStreamData
		//
		void VerifyTestData (string id, byte [] testBytes, int start, int count)
		{
			if (testBytes == null)
				Assert.Fail (id + "+1 testBytes is null");

			if (start < 0 ||
			    count < 0  ||
			    start + count > testStreamData.Length ||
			    start > testStreamData.Length)
				throw new ArgumentOutOfRangeException (id + "+2");

			for (int test = 0; test < count; test++) {
				if (testBytes [test] == testStreamData [start + test])
					continue;

				string failStr = "testByte {0} (testStream {1}) was <{2}>, expecting <{3}>";
				failStr = String.Format (failStr,
							test,
							start + test,
							testBytes [test],
							testStreamData [start + test]);
				Assert.Fail (id + "-3" + failStr);
			}
		}

		public void AssertEquals (string message, int expected, int actual)
		{
			Assert.AreEqual (expected, actual, message);
		}

		public void AssertEquals (string message, long expected, long actual)
		{
			Assert.AreEqual (expected, actual, message);
		}

		public void AssertEquals (string message, bool expected, bool actual)
		{
			Assert.AreEqual (expected, actual, message);
		}

		[Test]
		public void ConstructorsOne ()
		{
			MemoryStream ms = new MemoryStream();

			AssertEquals ("#01", 0L, ms.Length);
			AssertEquals ("#02", 0, ms.Capacity);
			AssertEquals ("#03", true, ms.CanWrite);
		}

		[Test]
		public void ConstructorsTwo ()
		{
			MemoryStream ms = new MemoryStream (10);

			AssertEquals ("#01", 0L, ms.Length);
			AssertEquals ("#02", 10, ms.Capacity);
			ms.Capacity = 0;
			byte [] buffer = ms.GetBuffer ();
			// Begin: wow!!!
			AssertEquals ("#03", -1, ms.ReadByte ());
			Assert.IsNull (buffer, "#04"); // <--
			ms.Read (new byte [5], 0, 5);
			AssertEquals ("#05", 0, ms.Position);
			AssertEquals ("#06", 0, ms.Length);
			// End
		}

		[Test]
		public void ConstructorsThree ()
		{
			MemoryStream ms = new MemoryStream (testStreamData);
			AssertEquals ("#01", 100, ms.Length);
			AssertEquals ("#02", 0, ms.Position);
		}

		[Test]
		public void ConstructorsFour ()
		{
			MemoryStream ms = new MemoryStream (testStreamData, true);
			AssertEquals ("#01", 100, ms.Length);
			AssertEquals ("#02", 0, ms.Position);
			ms.Position = 50;
			byte saved = testStreamData [50];
			try {
				ms.WriteByte (23);
				AssertEquals ("#03", testStreamData [50], 23);
			} finally {
				testStreamData [50] = saved;
			}
			ms.Position = 100;
			try {
				ms.WriteByte (23);
			} catch (Exception) {
				return;
			}
			Assert.Fail ("#04");
		}

		[Test]
		public void ConstructorsFive ()
		{
			MemoryStream ms = new MemoryStream (testStreamData, 50, 50);
			AssertEquals ("#01", 50, ms.Length);
			AssertEquals ("#02", 0, ms.Position);
			AssertEquals ("#03", 50, ms.Capacity);
			ms.Position = 1;
			byte saved = testStreamData [51];
			try {
				ms.WriteByte (23);
				AssertEquals ("#04", testStreamData [51], 23);
			} finally {
				testStreamData [51] = saved;
			}
			ms.Position = 100;
			bool gotException = false;
			try {
				ms.WriteByte (23);
			} catch (NotSupportedException) {
				gotException = true;
			}

			if (!gotException)
				Assert.Fail ("#05");

			ms.Capacity = 100; // Allowed. It's the same as the one in the ms.
					   // This is lame, as the length is 50!!!
					   
			gotException = false;
			try {
				ms.Capacity = 51;
			} catch (NotSupportedException) {
				gotException = true;
			}

			if (!gotException)
				Assert.Fail ("#07");

			AssertEquals ("#08", 50, ms.ToArray ().Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructorsSix ()
		{
			MemoryStream ms = new MemoryStream (-2);
		}

		[Test]
		public void Read ()
		{
			byte [] readBytes = new byte [20];

			/* Test simple read */
			testStream.Read (readBytes, 0, 10);
			VerifyTestData ("R1", readBytes, 0, 10);

			/* Seek back to beginning */

			testStream.Seek (0, SeekOrigin.Begin);

			/* Read again, bit more this time */
			testStream.Read (readBytes, 0, 20);
			VerifyTestData ("R2", readBytes, 0, 20);

			/* Seek to 20 bytes from End */
			testStream.Seek (-20, SeekOrigin.End);
			testStream.Read (readBytes, 0, 20);
			VerifyTestData ("R3", readBytes, 80, 20);

			int readByte = testStream.ReadByte();
			Assert.AreEqual (-1, readByte, "R4");
		}

		[Test]
		public void WriteBytes ()
		{
			byte[] readBytes = new byte[100];

			MemoryStream ms = new MemoryStream (100);

			for (int i = 0; i < 100; i++)
				ms.WriteByte (testStreamData [i]);

			ms.Seek (0, SeekOrigin.Begin); 
			testStream.Read (readBytes, 0, 100);
			VerifyTestData ("W1", readBytes, 0, 100);
		}

		[Test]
		public void WriteBlock ()
		{
			byte[] readBytes = new byte[100];

			MemoryStream ms = new MemoryStream (100);

			ms.Write (testStreamData, 0, 100);
			ms.Seek (0, SeekOrigin.Begin); 
			testStream.Read (readBytes, 0, 100);
			VerifyTestData ("WB1", readBytes, 0, 100);
			byte[] arrayBytes = testStream.ToArray();
			AssertEquals ("#01", 100, arrayBytes.Length);
			VerifyTestData ("WB2", arrayBytes, 0, 100);
		}

		[Test]
		public void PositionLength ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Position = 4;
			ms.WriteByte ((byte) 'M');
			ms.WriteByte ((byte) 'O');
			AssertEquals ("#01", 6, ms.Length);
			AssertEquals ("#02", 6, ms.Position);
			ms.Position = 0;
			AssertEquals ("#03", 0, ms.Position);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void MorePositionLength ()
		{
			MemoryStream ms = new MemoryStream (testStreamData);
			ms.Position = 101;
			AssertEquals ("#01", 101, ms.Position);
			AssertEquals ("#02", 100, ms.Length);
			ms.WriteByte (1); // This should throw the exception
		}

		[Test]
		public void GetBufferOne ()
		{
			MemoryStream ms = new MemoryStream ();
			byte [] buffer = ms.GetBuffer ();
			AssertEquals ("#01", 0, buffer.Length);
		}

		[Test]
		public void GetBufferTwo ()
		{
			MemoryStream ms = new MemoryStream (100);
			byte [] buffer = ms.GetBuffer ();
			AssertEquals ("#01", 100, buffer.Length);

			ms.Write (testStreamData, 0, 100);
			ms.Write (testStreamData, 0, 100);
			AssertEquals ("#02", 200, ms.Length);
			buffer = ms.GetBuffer ();
			AssertEquals ("#03", 256, buffer.Length); // Minimun size after writing
		}

		[Test]
		public void Closed ()
		{
			MemoryStream ms = new MemoryStream (100);
			ms.Close ();
			bool thrown = false;
			try {
				int x = ms.Capacity;
			} catch (ObjectDisposedException) {
				thrown = true;
			}

			if (!thrown)
				Assert.Fail ("#01");

			thrown = false;
			try {
				ms.Capacity = 1;
			} catch (ObjectDisposedException) {
				thrown = true;
			}

			if (!thrown)
				Assert.Fail ("#02");

			// The first exception thrown is ObjectDisposed, not ArgumentNull
			thrown = false;
			try {
				ms.Read (null, 0, 1);
			} catch (ObjectDisposedException) {
				thrown = true;
			}

			if (!thrown)
				Assert.Fail ("#03");

			thrown = false;
			try {
				ms.Write (null, 0, 1);
			} catch (ObjectDisposedException) {
				thrown = true;
			}

			if (!thrown)
				Assert.Fail ("#03");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Close_get_Length () 
		{
			MemoryStream ms = new MemoryStream (100);
			ms.Close ();
			long x = ms.Length;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Close_get_Position () 
		{
			MemoryStream ms = new MemoryStream (100);
			ms.Close ();
			long x = ms.Position;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Close_set_Position () 
		{
			MemoryStream ms = new MemoryStream (100);
			ms.Close ();
			ms.Position = 0;
		}

		[Test]
		public void Seek ()
		{
			MemoryStream ms = new MemoryStream (100);
			ms.Write (testStreamData, 0, 100);
			ms.Seek (0, SeekOrigin.Begin);
			ms.Position = 50;
			ms.Seek (-50, SeekOrigin.Current);
			ms.Seek (-50, SeekOrigin.End);

			bool thrown = false;
			ms.Position = 49;
			try {
				ms.Seek (-50, SeekOrigin.Current);
			} catch (IOException) {
				thrown = true;
			}
			if (!thrown)
				Assert.Fail ("#01");
			
			thrown = false;
			try {
				ms.Seek (Int64.MaxValue, SeekOrigin.Begin);
			} catch (ArgumentOutOfRangeException) {
				thrown = true;
			}

			if (!thrown)
				Assert.Fail ("#02");

			thrown=false;
			try {
				// Oh, yes. They throw IOException for this one, but ArgumentOutOfRange for the previous one
				ms.Seek (Int64.MinValue, SeekOrigin.Begin);
			} catch (IOException) {
				thrown = true;
			}

			if (!thrown)
				Assert.Fail ("#03");

			ms=new MemoryStream (256);

			ms.Write (testStreamData, 0, 100);
			ms.Position=0;
			AssertEquals ("#01", 100, ms.Length);
			AssertEquals ("#02", 0, ms.Position);

			ms.Position=128;
			AssertEquals ("#03", 100, ms.Length);
			AssertEquals ("#04", 128, ms.Position);

			ms.Position=768;
			AssertEquals ("#05", 100, ms.Length);
			AssertEquals ("#06", 768, ms.Position);

			ms.WriteByte (0);
			AssertEquals ("#07", 769, ms.Length);
			AssertEquals ("#08", 769, ms.Position);
		}

		[Test]
		public void Seek_Disposed () 
		{
			MemoryStream ms = new MemoryStream ();
			ms.Close ();
			try {
				ms.Seek (0, SeekOrigin.Begin);
				Assert.Fail ();
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		public void SetLength ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Write (testStreamData, 0, 100);
			ms.Position = 100;
			ms.SetLength (150);
			AssertEquals ("#01", 150, ms.Length);
			AssertEquals ("#02", 100, ms.Position);
			ms.SetLength (80);
			AssertEquals ("#03", 80, ms.Length);
			AssertEquals ("#04", 80, ms.Position);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void SetLength_ReadOnly ()
		{
			MemoryStream ms = new MemoryStream (testStreamData, false);
			ms.SetLength (10);
		}

        	[Test]
		public void Capacity ()
		{
			MemoryStream ms = new MemoryStream ();

			Assert.AreEqual (0, ms.Capacity, "#A1");
			Assert.AreEqual (0, ms.GetBuffer ().Length, "#A2");

			ms.WriteByte ((byte)'6');
			Assert.AreEqual (256, ms.Capacity, "#B1");
			Assert.AreEqual (256, ms.GetBuffer ().Length, "#B2");

			// Shrink
			ms.Capacity = 100;
			Assert.AreEqual (100, ms.Capacity, "#C1");
			Assert.AreEqual (100, ms.GetBuffer ().Length, "#C2");

			// Grow
			ms.Capacity = 120;
			Assert.AreEqual (120, ms.Capacity, "#D1");
			Assert.AreEqual (120, ms.GetBuffer ().Length, "#D2");

			// Grow the buffer, reduce length -so we have a dirty area-
			// and then we assign capacity to the same. The idea is that we should
			// avoid creating a new internal buffer it's not needed.

			ms = new MemoryStream ();
			ms.Capacity = 8;
			byte [] buff = new byte [] { 0x01, 0x02, 0x03, 0x04, 0x05 };
			ms.Write (buff, 0, buff.Length);
			Assert.AreEqual (8, ms.Capacity, "#E1");
			Assert.AreEqual (8, ms.GetBuffer ().Length, "#E2");

			// Reduce *length*, not capacity
			byte [] buff_copy = ms.GetBuffer ();
			ms.SetLength (3);
			Assert.AreEqual (3, ms.Length, "#F1");
			Assert.AreEqual (true, AreBuffersEqual (buff_copy, ms.GetBuffer ()), "#F2");

			// Set Capacity to the very same value it has now
			ms.Capacity = ms.Capacity;
			Assert.AreEqual (true, AreBuffersEqual (buff_copy, ms.GetBuffer ()), "#G1"); // keep the same buffer

			// Finally, growing it discards the prev buff
			ms.Capacity = ms.Capacity + 1;
			Assert.AreEqual (false, AreBuffersEqual (buff_copy, ms.GetBuffer ()), "#H1");
		}

		bool AreBuffersEqual (byte [] buff1, byte [] buff2)
		{
			if ((buff1 == null) != (buff2 == null))
				return false;

			if (buff1.Length != buff2.Length)
				return false;

			for (int i = 0; i < buff1.Length; i++)
				if (buff1 [i] != buff2 [i])
					return false;

			return true;
		}

        	[Test] // bug #327053
		public void ZeroingOnExpand ()
		{
			byte [] values = { 3, 2, 1 };
			byte [] reference = { 3, 2, 1 };
			byte [] cropped = { 3, 0, 0 };
			MemoryStream ms = new MemoryStream (values);
			Assert.AreEqual (values, reference, "#A1");
			ms.Seek (3, SeekOrigin.Begin);
			Assert.AreEqual (reference, values, "#A2");
			ms.SetLength (1);
			Assert.AreEqual (reference, values, "#B1");
			byte [] read = new byte [5];
			ms.Read (read, 0, 5);
			Assert.AreEqual (new byte [] { 0, 0, 0, 0, 0 }, read, "#B2");
			Assert.AreEqual (reference, values, "#B3");
			ms.SetLength (3);
			Assert.AreEqual (cropped, values, "#C1");
			ms.Seek (0, SeekOrigin.Begin);
			read = new byte [3];
			ms.Read (read, 0, 3);
			Assert.AreEqual (cropped, read, "#C2");
			Assert.AreEqual (cropped, values, "#C3");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void WriteNonWritable ()
		{
			MemoryStream ms = new MemoryStream (testStreamData, false);
			ms.Write (testStreamData, 0, 100);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void WriteExpand ()
		{
			MemoryStream ms = new MemoryStream (testStreamData);
			ms.Write (testStreamData, 0, 100);
			ms.Write (testStreamData, 0, 100); // This one throws the exception
		}

		[Test]
		public void WriteByte ()
		{
			MemoryStream ms = new MemoryStream (100);
			ms.Write (testStreamData, 0, 100);
			ms.Position = 100;
			ms.WriteByte (101);
			AssertEquals ("#01", 101, ms.Position);
			AssertEquals ("#02", 101, ms.Length);
			AssertEquals ("#03", 256, ms.Capacity);
			ms.Write (testStreamData, 0, 100);
			ms.Write (testStreamData, 0, 100);
			// 301
			AssertEquals ("#04", 301, ms.Position);
			AssertEquals ("#05", 301, ms.Length);
			AssertEquals ("#06", 512, ms.Capacity);
		}

		[Test]
		public void WriteLengths () {
			MemoryStream ms=new MemoryStream (256);
			BinaryWriter writer=new BinaryWriter (ms);

			writer.Write ((byte)'1');
			AssertEquals ("#01", 1, ms.Length);
			AssertEquals ("#02", 256, ms.Capacity);
			
			writer.Write ((ushort)0);
			AssertEquals ("#03", 3, ms.Length);
			AssertEquals ("#04", 256, ms.Capacity);

			writer.Write (testStreamData, 0, 23);
			AssertEquals ("#05", 26, ms.Length);
			AssertEquals ("#06", 256, ms.Capacity);

			writer.Write (testStreamData);
			writer.Write (testStreamData);
			writer.Write (testStreamData);
			AssertEquals ("#07", 326, ms.Length);
		}

		[Test]
		public void MoreWriteByte ()
		{
			byte[] buffer = new byte [44];
			
			MemoryStream ms = new MemoryStream (buffer);
			BinaryWriter bw = new BinaryWriter (ms);
			for(int i=0; i < 44; i++)
				bw.Write ((byte) 1);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void MoreWriteByte2 ()
		{
			byte[] buffer = new byte [43]; // Note the 43 here
			
			MemoryStream ms = new MemoryStream (buffer);
			BinaryWriter bw = new BinaryWriter (ms);
			for(int i=0; i < 44; i++)
				bw.Write ((byte) 1);
		}

		[Test]
		public void Expand () 
		{
			byte[] array = new byte [8] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 };
			MemoryStream ms = new MemoryStream ();
			ms.Write (array, 0, array.Length);
			ms.SetLength (4);
			ms.Seek (4, SeekOrigin.End);
			ms.WriteByte (0xFF);
			Assert.AreEqual ("01-01-01-01-00-00-00-00-FF", BitConverter.ToString (ms.ToArray ()), "Result");
		}

		[Test]
		public void PubliclyVisible ()
		{
			MemoryStream ms = new MemoryStream ();
			Assert.IsNotNull (ms.GetBuffer (), "ctor()");

			ms = new MemoryStream (1);
			Assert.IsNotNull (ms.GetBuffer (), "ctor(1)");

			ms = new MemoryStream (new byte[1], 0, 1, true, true);
			Assert.IsNotNull (ms.GetBuffer (), "ctor(byte[],int,int,bool,bool");
		}

		[Test]
		[ExpectedException (typeof (UnauthorizedAccessException))]
		public void PubliclyVisible_Ctor_ByteArray ()
		{
			MemoryStream ms = new MemoryStream (new byte[0]);
			Assert.IsNotNull (ms.GetBuffer ());
		}

		[Test]
		[ExpectedException (typeof (UnauthorizedAccessException))]
		public void PubliclyVisible_Ctor_ByteArray_Boolean ()
		{
			MemoryStream ms = new MemoryStream (new byte[0], true);
			Assert.IsNotNull (ms.GetBuffer ());
		}

		[Test]
		[ExpectedException (typeof (UnauthorizedAccessException))]
		public void PubliclyVisible_Ctor_ByteArray_Int_Int ()
		{
			MemoryStream ms = new MemoryStream (new byte[1], 0, 1);
			Assert.IsNotNull (ms.GetBuffer ());
		}

		[Test]
		[ExpectedException (typeof (UnauthorizedAccessException))]
		public void PubliclyVisible_Ctor_ByteArray_Int_Int_Boolean ()
		{
			MemoryStream ms = new MemoryStream (new byte[1], 0, 1, true);
			Assert.IsNotNull (ms.GetBuffer ());
		}

		[Test]
		[ExpectedException (typeof (UnauthorizedAccessException))]
		public void PubliclyVisible_Ctor_ByteArray_Int_Int_Boolean_Boolean ()
		{
			MemoryStream ms = new MemoryStream (new byte[1], 0, 1, true, false);
			Assert.IsNotNull (ms.GetBuffer ());
		}

		[Test] // bug #350860
		public void ToArray_Empty ()
		{
			MemoryStream ms = new MemoryStream (1);
			ms.Capacity = 0;
			ms.ToArray ();
		}

		[Test] // bug #80205
		[Category ("NotWorking")]
		public void SerializeTest ()
		{
			MemoryStream input = new MemoryStream ();
			byte [] bufferIn = Encoding.UTF8.GetBytes ("some test");
			input.Write (bufferIn, 0, bufferIn.Length);
			input.Position = 0;

			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, input);

			byte [] bufferOut = new byte [ms.Length];
			ms.Position = 0;
			ms.Read (bufferOut, 0, bufferOut.Length);

			Assert.AreEqual (_serialized, bufferOut);
		}

		[Test] // bug #676060
		public void ZeroCapacity ()
		{
			MemoryStream ms = new MemoryStream();
			ms.WriteByte(1);
			ms.Position = 0;
			ms.SetLength(0);
			ms.Capacity = 0;
			ms.WriteByte(1);
			byte[] bytes = ms.ToArray();
		}

		[Test] // bug #80205
		[Category ("NotWorking")]
		public void DeserializeTest ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Write (_serialized, 0, _serialized.Length);
			ms.Position = 0;

			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream output = (MemoryStream) bf.Deserialize (ms);
			using (StreamReader sr = new StreamReader (output)) {
				Assert.AreEqual ("some test", sr.ReadToEnd ());
			}
		}

		private static byte [] _serialized = new byte [] {
			0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x00,
			0x16, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x49, 0x4f, 0x2e,
			0x4d, 0x65, 0x6d, 0x6f, 0x72, 0x79, 0x53, 0x74, 0x72, 0x65, 0x61,
			0x6d, 0x0a, 0x00, 0x00, 0x00, 0x07, 0x5f, 0x62, 0x75, 0x66, 0x66,
			0x65, 0x72, 0x07, 0x5f, 0x6f, 0x72, 0x69, 0x67, 0x69, 0x6e, 0x09,
			0x5f, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x07, 0x5f,
			0x6c, 0x65, 0x6e, 0x67, 0x74, 0x68, 0x09, 0x5f, 0x63, 0x61, 0x70,
			0x61, 0x63, 0x69, 0x74, 0x79, 0x0b, 0x5f, 0x65, 0x78, 0x70, 0x61,
			0x6e, 0x64, 0x61, 0x62, 0x6c, 0x65, 0x09, 0x5f, 0x77, 0x72, 0x69,
			0x74, 0x61, 0x62, 0x6c, 0x65, 0x0a, 0x5f, 0x65, 0x78, 0x70, 0x6f,
			0x73, 0x61, 0x62, 0x6c, 0x65, 0x07, 0x5f, 0x69, 0x73, 0x4f, 0x70,
			0x65, 0x6e, 0x1d, 0x4d, 0x61, 0x72, 0x73, 0x68, 0x61, 0x6c, 0x42,
			0x79, 0x52, 0x65, 0x66, 0x4f, 0x62, 0x6a, 0x65, 0x63, 0x74, 0x2b,
			0x5f, 0x5f, 0x69, 0x64, 0x65, 0x6e, 0x74, 0x69, 0x74, 0x79, 0x07,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x02, 0x08,
			0x08, 0x08, 0x08, 0x01, 0x01, 0x01, 0x01, 0x09, 0x02, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00,
			0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x0a,
			0x0f, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x02, 0x73,
			0x6f, 0x6d, 0x65, 0x20, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x0b };

		class MyMemoryStream : MemoryStream {

			public bool DisposedCalled = false;

			protected override void Dispose(bool disposing)
			{
				DisposedCalled = true;
			}
		}

		[Test] // https://bugzilla.novell.com/show_bug.cgi?id=322672
		public void BaseDisposeCalled ()
		{
			MyMemoryStream ms = new MyMemoryStream ();
			Assert.IsFalse (ms.DisposedCalled, "Before");
			ms.Close ();
			Assert.IsTrue (ms.DisposedCalled, "After");
		}
	}
}
