/* -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
//
// DeflateStreamTest.cs - NUnit Test Cases for the System.IO.Compression.DeflateStream class
//
// Authors:
// 	Christopher James Lahey  <clahey@ximian.com>
//
// (C) 2004 Novell, Inc. <http://www.novell.com>
// 


using NUnit.Framework;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

using MonoTests.Helpers;

namespace MonoTests.System.IO.Compression
{
	[TestFixture]
	public class DeflateStreamTest
	{
		private static void CopyStream (Stream src, Stream dest)
		{
			byte[] array = new byte[1024];
			int bytes_read;
			bytes_read = src.Read (array, 0, 1024);
			while (bytes_read != 0) {
				dest.Write (array, 0, bytes_read);
				bytes_read = src.Read (array, 0, 1024);
			}
		}

		private static bool compare_buffers (byte[] first, byte[] second, int length)
		{
			if (first.Length < length || second.Length < length) {
				return false;
			}
			for (int i = 0; i < length; i++) {
				if (first[i] != second[i]) {
					return false;
				}
			}
			return true;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void Constructor_Null ()
		{
			DeflateStream ds = new DeflateStream (null, CompressionMode.Compress);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void Constructor_InvalidCompressionMode ()
		{
			DeflateStream ds = new DeflateStream (new MemoryStream (), (CompressionMode)Int32.MinValue);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void CheckCompressDecompress ()
		{
			byte [] data = new byte[100000];
			for (int i = 0; i < 100000; i++) {
				data[i] = (byte) i;
			}
			MemoryStream dataStream = new MemoryStream (data);
			MemoryStream backing = new MemoryStream ();
			DeflateStream compressing = new DeflateStream (backing, CompressionMode.Compress, true);
			CopyStream (dataStream, compressing);
			dataStream.Close();
			compressing.Close();
			backing.Seek (0, SeekOrigin.Begin);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			MemoryStream output = new MemoryStream ();
			CopyStream (decompressing, output);
			Assert.IsTrue (compare_buffers (data, output.GetBuffer(), (int) output.Length));
			decompressing.Close();
			output.Close();
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void CheckDecompress ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			StreamReader reader = new StreamReader (decompressing);
			Assert.AreEqual ("Hello", reader.ReadLine ());
			decompressing.Close();
		}

		// https://bugzilla.xamarin.com/show_bug.cgi?id=22346
		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void CheckEmptyRead ()
		{
			byte [] dummy = new byte[1];
			byte [] data = new byte[0];
			MemoryStream backing = new MemoryStream (data);
			DeflateStream compressing = new DeflateStream (backing, CompressionMode.Decompress);
			compressing.Read (dummy, 0, 1);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (ArgumentNullException))]
		public void CheckNullRead ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			decompressing.Read (null, 0, 20);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (InvalidOperationException))]
		public void CheckCompressingRead ()
		{
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream ();
			DeflateStream compressing = new DeflateStream (backing, CompressionMode.Compress);
			compressing.Read (dummy, 0, 20);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (ArgumentException))]
		public void CheckRangeRead ()
		{
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			decompressing.Read (dummy, 10, 20);
		}

#if !MOBILE
		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[Category("NotWorking")]
		[ExpectedException (typeof (InvalidDataException))]
		public void CheckInvalidDataRead ()
		{
			byte [] data = {0x11, 0x78, 0x89, 0x91, 0xbe, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00 };
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream (data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			decompressing.Read (dummy, 0, 20);
		}
#endif

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (ObjectDisposedException))]
		public void CheckClosedRead ()
		{
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			decompressing.Close ();
			decompressing.Read (dummy, 0, 20);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (ObjectDisposedException))]
		public void CheckClosedFlush ()
		{
			MemoryStream backing = new MemoryStream ();
			DeflateStream compressing = new DeflateStream (backing, CompressionMode.Compress);
			compressing.Close ();
			compressing.Flush ();
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckSeek ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			decompressing.Seek (20, SeekOrigin.Current);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckSetLength ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			decompressing.SetLength (20);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void CheckGetCanSeekProp ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompress = new DeflateStream (backing, CompressionMode.Decompress);
			Assert.IsFalse (decompress.CanSeek, "#A1");
			Assert.IsTrue (backing.CanSeek, "#A2");
			decompress.Dispose ();
			Assert.IsFalse (decompress.CanSeek, "#A3");
			Assert.IsFalse (backing.CanSeek, "#A4");

			backing = new MemoryStream ();
			DeflateStream compress = new DeflateStream (backing, CompressionMode.Compress);
			Assert.IsFalse (compress.CanSeek, "#B1");
			Assert.IsTrue (backing.CanSeek, "#B2");
			compress.Dispose ();
			Assert.IsFalse (decompress.CanSeek, "#B3");
			Assert.IsFalse (backing.CanSeek, "#B4");
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void CheckGetCanReadProp ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompress = new DeflateStream (backing, CompressionMode.Decompress);
			Assert.IsTrue (decompress.CanRead, "#A1");
			Assert.IsTrue (backing.CanRead, "#A2");
			decompress.Dispose ();
			Assert.IsFalse (decompress.CanRead, "#A3");
			Assert.IsFalse (backing.CanRead, "#A4");

			backing = new MemoryStream ();
			DeflateStream compress = new DeflateStream (backing, CompressionMode.Compress);
			Assert.IsFalse (compress.CanRead, "#B1");
			Assert.IsTrue (backing.CanRead, "#B2");
			compress.Dispose ();
			Assert.IsFalse (decompress.CanRead, "#B3");
			Assert.IsFalse (backing.CanRead, "#B4");
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void CheckGetCanWriteProp ()
		{
			MemoryStream backing = new MemoryStream ();
			DeflateStream decompress = new DeflateStream (backing, CompressionMode.Decompress);
			Assert.IsFalse (decompress.CanWrite, "#A1");
			Assert.IsTrue (backing.CanWrite, "#A2");
			decompress.Dispose ();
			Assert.IsFalse (decompress.CanWrite, "#A3");
			Assert.IsFalse (backing.CanWrite, "#A4");

			backing = new MemoryStream ();
			DeflateStream compress = new DeflateStream (backing, CompressionMode.Compress);
			Assert.IsTrue (compress.CanWrite, "#B1");
			Assert.IsTrue (backing.CanWrite, "#B2");
			compress.Dispose ();
			Assert.IsFalse (decompress.CanWrite, "#B3");
			Assert.IsFalse (backing.CanWrite, "#B4");
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckSetLengthProp ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			decompressing.SetLength (20);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckGetLengthProp ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			long length = decompressing.Length;
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckGetPositionProp ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			long position = decompressing.Position;
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void DisposeTest ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompress = new DeflateStream (backing, CompressionMode.Decompress);
			decompress.Dispose ();
			decompress.Dispose ();
		}

		static byte [] compressed_data = { 0xf3, 0x48, 0xcd, 0xc9, 0xc9,
			0xe7, 0x02, 0x00 };


		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void JunkAtTheEnd ()
		{
			// Write a deflated stream, then some additional data...
			using (MemoryStream ms = new MemoryStream())
			{
				// The compressed stream
				using (DeflateStream stream = new DeflateStream(ms, CompressionMode.Compress, true))
				{
					stream.WriteByte(1);
					stream.Flush();
				}
				// Junk
				ms.WriteByte(2);

				ms.Position = 0;
				// Reading: this should not hang
				using (DeflateStream stream = new DeflateStream(ms, CompressionMode.Decompress))
				{
					byte[] buffer  = new byte[512];
					int len = stream.Read(buffer, 0, buffer.Length);
					Console.WriteLine(len == 1);
				}
			}
		}
		
		class Bug19313Stream : MemoryStream
		{
			public Bug19313Stream (byte [] buffer)
				: base (buffer)
			{
			}

			public override int Read (byte [] buffer, int offset, int count)
			{
				// Thread was blocking when DeflateStream uses a NetworkStream.
				// Because the NetworkStream.Read calls Socket.Receive that
				// blocks the thread waiting for at least a byte to return.
				// This assert guarantees that Read is called only when there 
				// is something to be read.
				Assert.IsTrue (Position < Length, "Trying to read empty stream.");

				return base.Read (buffer, offset, count);
			}
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void Bug19313 ()
		{
			byte [] buffer  = new byte [512];
			using (var backing = new Bug19313Stream (compressed_data))
			using (var decompressing = new DeflateStream (backing, CompressionMode.Decompress))
				decompressing.Read (buffer, 0, buffer.Length);
		}

		public MemoryStream GenerateStreamFromString(string s)
		{
			return new MemoryStream (Encoding.UTF8.GetBytes (s));
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void CheckNet45Overloads () // Xambug #21982
		{
			MemoryStream dataStream = GenerateStreamFromString("Hello");
			MemoryStream backing = new MemoryStream ();
			DeflateStream compressing = new DeflateStream (backing, CompressionLevel.Fastest, true);
			CopyStream (dataStream, compressing);
			dataStream.Close();
			compressing.Close();

			backing.Seek (0, SeekOrigin.Begin);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			StreamReader reader = new StreamReader (decompressing);
			Assert.AreEqual ("Hello", reader.ReadLine ());
			decompressing.Close();
			backing.Close();
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (ArgumentException))]
		public void CheckBufferOverrun ()
		{
			byte[] data = new byte [20];
			MemoryStream backing = new MemoryStream ();
			DeflateStream compressing = new DeflateStream (backing, CompressionLevel.Fastest, true);
			compressing.Write (data, 0, data.Length + 1);
			compressing.Close ();
			backing.Close ();
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void Bug28777_EmptyFlush ()
		{
			MemoryStream backing = new MemoryStream ();
			DeflateStream compressing = new DeflateStream (backing, CompressionLevel.Fastest, true);
			compressing.Flush ();
			compressing.Close ();
			backing.Close ();
		}
		
		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void Bug28777_DoubleFlush ()
		{
			byte[] buffer = new byte [4096];
			MemoryStream backing = new MemoryStream ();
			DeflateStream compressing = new DeflateStream (backing, CompressionLevel.Fastest, true);
			compressing.Write (buffer, 0, buffer.Length);
			compressing.Flush ();
			compressing.Flush ();
			compressing.Close ();
			backing.Close ();
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void Bug34916_Inflate ()
		{
			var base64String = @"H4sIAAAAAAAAA6yVu27bQBBF/4VtZGHeD3ZJmhTp5C5IIUiEIcCWDEUugiD/nmEQwYRNURFAsuFwd2exZ++d+farud89davT+um5aRsC1DuEO+R7lJayRV9m5gegFqBZNB83m5fjevOzadGWUPHjaXd62XYVEy3Z04wiMTKIX0dfV0G/6FO3Pu72D/+iL916W9GbOV/X58SaS6zEKKyoGUA1eNg/nLfF2jUEBBNMtT4Wzeq567Z9HkZkE1Osf93msN/+WO32m+7zsavsh30/BUU8fy+uUCC+QIHpPQW1RAXkEGWUmSnUy2iUYSMYOGpARYViiIHcqY5kExS8rg2vY8gLGEjeYsClBVE4ORQHz3kxsEF4iS01xzBIZkgYQcYQQ7C54LQaIrxWn5+4ioT1BiRQN8Fh6MrOPjOS9Eh3M8YRJJQMZioJkUODFA8RNJ9AYuYBNyGJW5D0oi3/EpZ3dWYk5X5PN81RJGJgDATMQ5X02nFS1imVlMGvu0XwBg5/K1hY1U8tecxcNDy1/FAnG+OAQSi9PliHRaNUiuoxQYFB6T8oyAUKEu9LJ6oipbr1spyZArhWX6qbi7EOUrs7SCAoDNVgzKagMlUz+q6DQ4N8/yM=";

			byte[] byteArray = Convert.FromBase64String(base64String);
			string unZipped = null;

			using (var zippedMemoryStream = new MemoryStream (byteArray))
			using (var gZipStream = new GZipStream (zippedMemoryStream, CompressionMode.Decompress))
			using (var unzippedMemStream = new MemoryStream())
			using (var unZippedStream = new StreamReader (gZipStream, Encoding.UTF8)) {
				unZipped = unZippedStream.ReadToEnd ();
			}

			Assert.AreEqual(1877, unZipped.Length);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void Bug44994_Inflate()
		{
			var base64String = @"7cWxCQAgDACwpeBjgqsgXiHU0fd9QzBLErX1EQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADepcxcuU/atm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm3btm37zy8=";

			byte[] byteArray = Convert.FromBase64String(base64String);
			string unZipped = null;

			using (var zippedMemoryStream = new MemoryStream(byteArray))
			using (var gZipStream = new DeflateStream(zippedMemoryStream, CompressionMode.Decompress))
			using (var unzippedMemStream = new MemoryStream())
			using (var unZippedStream = new StreamReader(gZipStream, Encoding.UTF8))
			{
				unZipped = unZippedStream.ReadToEnd();
			}

			Assert.AreEqual(81942, unZipped.Length);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[Category ("MobileNotWorking")]
		public void Bug44994_InflateByteByByte()
		{
			int byteCount = 0;
			using (var fileStream = File.OpenRead(TestResourceHelper.GetFullPathOfResource ("Test/compressed.bin")))
			{
				using (var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress, false))
				{
					while (deflateStream.ReadByte() != -1)
					{
						byteCount++;
					}
				}
			}

			Assert.AreEqual(125387, byteCount);
		}

		[Test]
		public void Issue10054_ZeroRead()
		{
			// "HelloWorld" as UTF8/DeflateStream(...,CompressionMode.Compress)
			var buffer = new byte[] { 243, 72, 205, 201, 201, 15, 207, 47, 202, 73, 1, 0 };
			var mem = new MemoryStream(buffer);
			var chu = new ChunkedReader(mem);
			var def = new DeflateStream(chu, CompressionMode.Decompress);
			
			var buffer2 = new byte[4096];
			int read2 = 0;

			chu.limit = 3;
			read2 += def.Read(buffer2, read2, buffer2.Length - read2);
			chu.limit = 100;
			read2 += def.Read(buffer2, read2, buffer2.Length - read2);

			var res = Encoding.UTF8.GetString(buffer2, 0, read2);
			Assert.AreEqual("HelloWorld", res);
		}

		public class ChunkedReader : Stream
		{
			public int limit = 0;
			private Stream baseStream;
			
			public ChunkedReader(Stream baseStream)
			{
				this.baseStream = baseStream;
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				int read = baseStream.Read(buffer, offset, Math.Min(count, this.limit));
				this.limit -= read;
				return read;
			}

			public override void Flush() => baseStream.Flush();
			public override long Seek(long offset, SeekOrigin origin) => baseStream.Seek(offset, origin);
			public override void SetLength(long value) => baseStream.SetLength(value);
			public override void Write(byte[] buffer, int offset, int count) => baseStream.Write(buffer, offset, count);

			public override bool CanRead => baseStream.CanRead;
			public override bool CanSeek => baseStream.CanSeek;
			public override bool CanWrite => baseStream.CanWrite;
			public override long Length => baseStream.Length;

			public override long Position
			{
				get => baseStream.Position;
				set => baseStream.Position = value;
			}
		}
	}
}


