//
// System.IO.StringWriter
//
// Authors:
// 	Marcin Szczepanski (marcins@zipworld.com.au)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using NUnit.Framework;
using System.IO;
using System;
using System.Text;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class MemoryStreamTest
	{
		MemoryStream testStream;
		byte [] testStreamData;

		[SetUp]
		void SetUp ()
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
				Assertion.Fail (id + "+1 testBytes is null");

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
				Assertion.Fail (id + "-3" + failStr);
			}
		}

		[Test]
		public void ConstructorsOne ()
		{
			MemoryStream ms = new MemoryStream();

			Assertion.AssertEquals ("#01", 0L, ms.Length);
			Assertion.AssertEquals ("#02", 0, ms.Capacity);
			Assertion.AssertEquals ("#03", true, ms.CanWrite);
		}

		[Test]
		public void ConstructorsTwo ()
		{
			MemoryStream ms = new MemoryStream (10);

			Assertion.AssertEquals ("#01", 0L, ms.Length);
			Assertion.AssertEquals ("#02", 10, ms.Capacity);
			ms.Capacity = 0;
			byte [] buffer = ms.GetBuffer ();
			// Begin: wow!!!
			Assertion.AssertEquals ("#03", -1, ms.ReadByte ());
			Assertion.AssertEquals ("#04", null, buffer); // <--
			ms.Read (new byte [5], 0, 5);
			Assertion.AssertEquals ("#05", 0, ms.Position);
			Assertion.AssertEquals ("#06", 0, ms.Length);
			// End
		}

		[Test]
		public void ConstructorsThree ()
		{
			MemoryStream ms = new MemoryStream (testStreamData);
			Assertion.AssertEquals ("#01", 100, ms.Length);
			Assertion.AssertEquals ("#02", 0, ms.Position);
		}

		[Test]
		public void ConstructorsFour ()
		{
			MemoryStream ms = new MemoryStream (testStreamData, true);
			Assertion.AssertEquals ("#01", 100, ms.Length);
			Assertion.AssertEquals ("#02", 0, ms.Position);
			ms.Position = 50;
			byte saved = testStreamData [50];
			try {
				ms.WriteByte (23);
				Assertion.AssertEquals ("#03", testStreamData [50], 23);
			} finally {
				testStreamData [50] = saved;
			}
			ms.Position = 100;
			try {
				ms.WriteByte (23);
			} catch (Exception) {
				return;
			}
			Assertion.Fail ("#04");
		}

		[Test]
		public void ConstructorsFive ()
		{
			MemoryStream ms = new MemoryStream (testStreamData, 50, 50);
			Assertion.AssertEquals ("#01", 50, ms.Length);
			Assertion.AssertEquals ("#02", 0, ms.Position);
			Assertion.AssertEquals ("#03", 50, ms.Capacity);
			ms.Position = 1;
			byte saved = testStreamData [51];
			try {
				ms.WriteByte (23);
				Assertion.AssertEquals ("#04", testStreamData [51], 23);
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
				Assertion.Fail ("#05");

			gotException = false;
			try {
				ms.GetBuffer ();
			} catch (UnauthorizedAccessException) {
				gotException = true;
			}

			if (!gotException)
				Assertion.Fail ("#06");

			ms.Capacity = 100; // Allowed. It's the same as the one in the ms.
					   // This is lame, as the length is 50!!!
					   
			gotException = false;
			try {
				ms.Capacity = 51;
			} catch (NotSupportedException) {
				gotException = true;
			}

			if (!gotException)
				Assertion.Fail ("#07");

			Assertion.AssertEquals ("#08", 50, ms.ToArray ().Length);
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
			Assertion.AssertEquals (-1, readByte);
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
			Assertion.AssertEquals ("#01", 100, arrayBytes.Length);
			VerifyTestData ("WB2", arrayBytes, 0, 100);
		}

		[Test]
		public void PositionLength ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Position = 4;
			ms.WriteByte ((byte) 'M');
			ms.WriteByte ((byte) 'O');
			Assertion.AssertEquals ("#01", 6, ms.Length);
			Assertion.AssertEquals ("#02", 6, ms.Position);
			ms.Position = 0;
			Assertion.AssertEquals ("#03", 0, ms.Position);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void MorePositionLength ()
		{
			MemoryStream ms = new MemoryStream (testStreamData);
			ms.Position = 101;
			Assertion.AssertEquals ("#01", 101, ms.Position);
			Assertion.AssertEquals ("#02", 100, ms.Length);
			ms.WriteByte (1); // This should throw the exception
		}

		[Test]
		public void GetBufferOne ()
		{
			MemoryStream ms = new MemoryStream ();
			byte [] buffer = ms.GetBuffer ();
			Assertion.AssertEquals ("#01", 0, buffer.Length);
		}

		[Test]
		public void GetBufferTwo ()
		{
			MemoryStream ms = new MemoryStream (100);
			byte [] buffer = ms.GetBuffer ();
			Assertion.AssertEquals ("#01", 100, buffer.Length);

			ms.Write (testStreamData, 0, 100);
			ms.Write (testStreamData, 0, 100);
			Assertion.AssertEquals ("#02", 200, ms.Length);
			buffer = ms.GetBuffer ();
			Assertion.AssertEquals ("#03", 256, buffer.Length); // Minimun size after writing
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
				Assertion.Fail ("#01");

			thrown = false;
			try {
				ms.Capacity = 1;
			} catch (ObjectDisposedException) {
				thrown = true;
			}

			if (!thrown)
				Assertion.Fail ("#02");

			// The first exception thrown is ObjectDisposed, not ArgumentNull
			thrown = false;
			try {
				ms.Read (null, 0, 1);
			} catch (ObjectDisposedException) {
				thrown = true;
			}

			if (!thrown)
				Assertion.Fail ("#03");

			thrown = false;
			try {
				ms.Write (null, 0, 1);
			} catch (ObjectDisposedException) {
				thrown = true;
			}

			if (!thrown)
				Assertion.Fail ("#03");
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
				Assertion.Fail ("#01");
			
			thrown = false;
			try {
				ms.Seek (Int64.MaxValue, SeekOrigin.Begin);
			} catch (ArgumentOutOfRangeException) {
				thrown = true;
			}

			if (!thrown)
				Assertion.Fail ("#02");

			thrown=false;
			try {
				// Oh, yes. They throw IOException for this one, but ArgumentOutOfRange for the previous one
				ms.Seek (Int64.MinValue, SeekOrigin.Begin);
			} catch (IOException) {
				thrown = true;
			}

			if (!thrown)
				Assertion.Fail ("#03");

			ms=new MemoryStream (256);

			ms.Write (testStreamData, 0, 100);
			ms.Position=0;
			Assertion.AssertEquals ("#01", 100, ms.Length);
			Assertion.AssertEquals ("#02", 0, ms.Position);

			ms.Position=128;
			Assertion.AssertEquals ("#03", 100, ms.Length);
			Assertion.AssertEquals ("#04", 128, ms.Position);

			ms.Position=768;
			Assertion.AssertEquals ("#05", 100, ms.Length);
			Assertion.AssertEquals ("#06", 768, ms.Position);

			ms.WriteByte (0);
			Assertion.AssertEquals ("#07", 769, ms.Length);
			Assertion.AssertEquals ("#08", 769, ms.Position);
		}

		[Test]
		public void SetLength ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Write (testStreamData, 0, 100);
			ms.Position = 100;
			ms.SetLength (150);
			Assertion.AssertEquals ("#01", 150, ms.Length);
			Assertion.AssertEquals ("#02", 100, ms.Position);
			ms.SetLength (80);
			Assertion.AssertEquals ("#03", 80, ms.Length);
			Assertion.AssertEquals ("#04", 80, ms.Position);
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
			Assertion.AssertEquals ("#01", 101, ms.Position);
			Assertion.AssertEquals ("#02", 101, ms.Length);
			Assertion.AssertEquals ("#03", 256, ms.Capacity);
			ms.Write (testStreamData, 0, 100);
			ms.Write (testStreamData, 0, 100);
			// 301
			Assertion.AssertEquals ("#04", 301, ms.Position);
			Assertion.AssertEquals ("#05", 301, ms.Length);
			Assertion.AssertEquals ("#06", 512, ms.Capacity);
		}

		[Test]
		public void WriteLengths () {
			MemoryStream ms=new MemoryStream (256);
			BinaryWriter writer=new BinaryWriter (ms);

			writer.Write ((byte)'1');
			Assertion.AssertEquals ("#01", 1, ms.Length);
			Assertion.AssertEquals ("#02", 256, ms.Capacity);
			
			writer.Write ((ushort)0);
			Assertion.AssertEquals ("#03", 3, ms.Length);
			Assertion.AssertEquals ("#04", 256, ms.Capacity);

			writer.Write (testStreamData, 0, 23);
			Assertion.AssertEquals ("#05", 26, ms.Length);
			Assertion.AssertEquals ("#06", 256, ms.Capacity);

			writer.Write (testStreamData);
			writer.Write (testStreamData);
			writer.Write (testStreamData);
			Assertion.AssertEquals ("#07", 326, ms.Length);
		}
	}
}

