/* -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
//
// DeflateStreamTest.cs - NUnit Test Cases for the System.IO.Compression.DeflateStream class
//
// Authors:
// 	Christopher James Lahey  <clahey@ximian.com>
//
// (C) 2004 Novell, Inc. <http://www.novell.com>
// 

#if NET_2_0

using NUnit.Framework;
using System;
using System.IO;
using System.IO.Compression;

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
		public void Constructor_Null ()
		{
			DeflateStream ds = new DeflateStream (null, CompressionMode.Compress);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_InvalidCompressionMode ()
		{
			DeflateStream ds = new DeflateStream (new MemoryStream (), (CompressionMode)Int32.MinValue);
		}

		[Test]
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
		public void CheckDecompress ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			StreamReader reader = new StreamReader (decompressing);
			Assert.AreEqual ("Hello", reader.ReadLine ());
			decompressing.Close();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CheckNullRead ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			decompressing.Read (null, 0, 20);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CheckCompressingRead ()
		{
			byte [] dummy = new byte[20];
			MemoryStream backing = new MemoryStream ();
			DeflateStream compressing = new DeflateStream (backing, CompressionMode.Compress);
			compressing.Read (dummy, 0, 20);
		}

		[Test]
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
		[ExpectedException (typeof (ObjectDisposedException))]
		public void CheckClosedFlush ()
		{
			MemoryStream backing = new MemoryStream ();
			DeflateStream compressing = new DeflateStream (backing, CompressionMode.Compress);
			compressing.Close ();
			compressing.Flush ();
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckSeek ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			decompressing.Seek (20, SeekOrigin.Current);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckSetLength ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			decompressing.SetLength (20);
		}

		[Test]
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
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckSetLengthProp ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			decompressing.SetLength (20);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckGetLengthProp ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			long length = decompressing.Length;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CheckGetPositionProp ()
		{
			MemoryStream backing = new MemoryStream (compressed_data);
			DeflateStream decompressing = new DeflateStream (backing, CompressionMode.Decompress);
			long position = decompressing.Position;
		}

		[Test]
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
	}
}

#endif

