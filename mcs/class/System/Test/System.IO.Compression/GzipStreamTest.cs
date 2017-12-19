/* -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
//
// GZipStreamTest.cs - NUnit Test Cases for the System.IO.Compression.GZipStream class
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

namespace MonoTests.System.IO.Compression
{
	[TestFixture]
	public class GZipStreamTest
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
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Null ()
		{
			GZipStream ds = new GZipStream (null, CompressionMode.Compress);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_InvalidCompressionMode ()
		{
			GZipStream ds = new GZipStream (new MemoryStream (), (CompressionMode)Int32.MinValue);
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
			GZipStream compressing = new GZipStream (backing, CompressionMode.Compress, true);
			CopyStream (dataStream, compressing);
			dataStream.Close();
			compressing.Close();
			backing.Seek (0, SeekOrigin.Begin);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
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
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			StreamReader reader = new StreamReader (decompressing);
			Assert.AreEqual ("Hello", reader.ReadLine ());
			decompressing.Close();
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (ArgumentNullException))]
		public void CheckNullRead ()
		{
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.Read (null, 0, 20);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (InvalidOperationException))]
		public void CheckCompressingRead ()
		{
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream ();
			GZipStream compressing = new GZipStream (backing, CompressionMode.Compress);
			compressing.Read (dummy, 0, 20);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (ArgumentException))]
		public void CheckRangeRead ()
		{
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.Read (dummy, 10, 20);
		}

#if !MOBILE
		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[Category("NotWorking")]
		public void CheckInvalidDataRead ()
		{
			byte [] data = {0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0x11, 0x78, 0x89, 0x91, 0xbe, 0xf3, 0x48, 0xcd, 0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31, 0x06, 0x00, 0x00, 0x00 };
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream (data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			try {
				decompressing.Read (dummy, 0, 20);
				Assert.Fail ();
			} catch (InvalidDataException) {
			}
		}
#endif

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void CheckClosedRead ()
		{
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream (compressed_data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.Close ();
			try {
				decompressing.Read (dummy, 0, 20);
				Assert.Fail ();
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (ObjectDisposedException))]
		public void CheckClosedFlush ()
		{
			MemoryStream backing = new MemoryStream ();
			GZipStream compressing = new GZipStream (backing, CompressionMode.Compress);
			compressing.Close ();
			compressing.Flush ();
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckSeek ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.Seek (20, SeekOrigin.Current);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckSetLength ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.SetLength (20);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void CheckGetCanSeekProp ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			GZipStream decompress = new GZipStream (backing, CompressionMode.Decompress);
			Assert.IsFalse (decompress.CanSeek, "#A1");
			Assert.IsTrue (backing.CanSeek, "#A2");
			decompress.Dispose ();
			Assert.IsFalse (decompress.CanSeek, "#A3");
			Assert.IsFalse (backing.CanSeek, "#A4");

			backing = new MemoryStream ();
			GZipStream compress = new GZipStream (backing, CompressionMode.Compress);
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
			GZipStream decompress = new GZipStream (backing, CompressionMode.Decompress);
			Assert.IsTrue (decompress.CanRead, "#A1");
			Assert.IsTrue (backing.CanRead, "#A2");
			decompress.Dispose ();
			Assert.IsFalse (decompress.CanRead, "#A3");
			Assert.IsFalse (backing.CanRead, "#A4");

			backing = new MemoryStream ();
			GZipStream compress = new GZipStream (backing, CompressionMode.Compress);
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
			MemoryStream backing = new MemoryStream (compressed_data);
			GZipStream decompress = new GZipStream (backing, CompressionMode.Decompress);
			Assert.IsFalse (decompress.CanWrite, "#A1");
			Assert.IsTrue (backing.CanWrite, "#A2");
			decompress.Dispose ();
			Assert.IsFalse (decompress.CanWrite, "#A3");
			Assert.IsFalse (backing.CanWrite, "#A4");

			backing = new MemoryStream ();
			GZipStream compress = new GZipStream (backing, CompressionMode.Compress);
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
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			decompressing.SetLength (20);
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckGetLengthProp ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			long length = decompressing.Length;
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckGetPositionProp ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			long position = decompressing.Position;
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void DisposeTest ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			GZipStream decompress = new GZipStream (backing, CompressionMode.Decompress);
			decompress.Dispose ();
			decompress.Dispose ();
		}

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Native MPH loading issues
		public void DisposeOrderTest ()
		{
			var fs = new MemoryStream();
			GZipStream compressed = new GZipStream(fs, CompressionMode.Compress);
			byte[] buffer = new byte[1024];
			compressed.Write(buffer, 0, buffer.Length);
			compressed.Close();

			try {
				fs.WriteByte(2);
				Assert.Fail ();
			} catch (ObjectDisposedException) {
			}			
		}

		static byte [] compressed_data = {
			0x1f, 0x8b, 0x08, 0x08, 0x70, 0xbb, 0x5d, 0x41, 0x00,
			0x03, 0x74, 0x65, 0x73, 0x74, 0x00, 0xf3, 0x48, 0xcd,
			0xc9, 0xc9, 0xe7, 0x02, 0x00, 0x16, 0x35, 0x96, 0x31,
			0x06, 0x00, 0x00, 0x00};

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
			GZipStream compressing = new GZipStream (backing, CompressionLevel.Fastest, true);
			CopyStream (dataStream, compressing);
			dataStream.Close();
			compressing.Close();

			backing.Seek (0, SeekOrigin.Begin);
			GZipStream decompressing = new GZipStream (backing, CompressionMode.Decompress);
			StreamReader reader = new StreamReader (decompressing);
			Assert.AreEqual ("Hello", reader.ReadLine ());
			decompressing.Close();
			backing.Close();
		}
	}
}

