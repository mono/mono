//
// CryptoStreamTest.cs - NUnit Test Cases for CryptoStream
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

/* WARNING * WARNING * WARNING * WARNING * WARNING * WARNING * WARNING *
 * 
 * DO NOT USE ANY OF THE TEST CASE AS SAMPLES FOR YOUR OWN CODE. MANY
 * CASES CONTAINS ERRORS AND AREN'T SECURE IN THEIR USE.
 * 
 * WARNING * WARNING * WARNING * WARNING * WARNING * WARNING * WARNING */

using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

	// much useful for debugging
	public class DebugStream : MemoryStream {

		// constructor

		public DebugStream () : base () {}
		public DebugStream (byte[] buffer) : base (buffer) {}
		public DebugStream (int capacity) : base (capacity) {}

		public override bool CanRead {
			get { return base.CanRead; }
		}

		public override bool CanSeek {
			get { return base.CanSeek; }
		}

		public override bool CanWrite {
			get { return base.CanWrite; }
		}

		public override int Capacity {
			get { return base.Capacity; }
			set { base.Capacity = value; }
		}

		public override long Length {
			get { return base.Length; }
		}
		
		public override long Position {
			get { return base.Position; }
			set { base.Position = value; }
		}

		// methods
		
		public override void Close () 
		{
			base.Close ();
		}
		
		public override void Flush () 
		{
			base.Flush ();
		}

		public override byte[] GetBuffer () 
		{
			return base.GetBuffer ();
		}

		public override int Read ([In,Out] byte[] buffer, int offset, int count)
		{
			int len = base.Read (buffer, offset, count);
			return len;
		}

		public override int ReadByte () 
		{
			return base.ReadByte ();
		}

		public override long Seek (long offset, SeekOrigin loc)
		{
			return base.Seek (offset, loc);
		}
		
		public override void SetLength (long value)
		{
			base.SetLength (value);
		}
		
		public override byte[] ToArray () 
		{
			return base.ToArray ();
		}

		public override void Write (byte[] buffer, int offset, int count) 
		{
			base.Write (buffer, offset, count);
		}

		public override void WriteByte (byte value) 
		{
			base.WriteByte (value);
		}

		public override void WriteTo (Stream stream) 
		{
			base.WriteTo (stream);
		}
	}

	[TestFixture]
	public class CryptoStreamTest : Assertion {

		Stream readStream;
		Stream writeStream;
		ICryptoTransform encryptor;
		ICryptoTransform decryptor;
		CryptoStream cs;
		SymmetricAlgorithm aes;

		[SetUp]
		public void SetUp () 
		{
			if (readStream == null) {
				readStream = new FileStream ("read", FileMode.OpenOrCreate, FileAccess.Read);
				writeStream = new FileStream ("write", FileMode.OpenOrCreate, FileAccess.Write);
				aes = SymmetricAlgorithm.Create ();
				encryptor = aes.CreateEncryptor ();
				decryptor = aes.CreateEncryptor ();
			}
		}

		[TearDown]
		public void TearDown () 
		{
			try {
				if (File.Exists ("read"))
					File.Delete ("read");
				if (File.Exists ("write"))
					File.Delete ("write");
				if (File.Exists ("*.tmp"))
					File.Delete ("*.tmp");
			}
			catch {}
		}

		public void AssertEquals (string msg, byte[] array1, byte[] array2)
		{
			AllTests.AssertEquals (msg, array1, array2);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void StreamNull ()
		{
			cs = new CryptoStream (null, encryptor, CryptoStreamMode.Read);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TransformNull ()
		{
			MemoryStream write = new MemoryStream (8);
			byte[] data = {0, 1, 2, 3, 4, 5, 6, 7};
			cs = new CryptoStream (write, null, CryptoStreamMode.Write);
			cs.Write (data, 0, 8);
		}

		[Test]
		public void StreamReadModeRead () 
		{
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			Assert ("Read.CanRead", cs.CanRead);
			Assert ("Read.CanWrite", !cs.CanWrite);
			Assert ("Read.CanSeek", !cs.CanSeek);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void StreamReadModeWrite () 
		{
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Write);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void StreamWriteModeRead () 
		{
			cs = new CryptoStream (writeStream, encryptor, CryptoStreamMode.Read);
		}

		[Test]
		public void StreamWriteModeWrite () 
		{
			cs = new CryptoStream (writeStream, encryptor, CryptoStreamMode.Write);
			Assert ("Read.CanRead", !cs.CanRead);
			Assert ("Read.CanWrite", cs.CanWrite);
			Assert ("Read.CanSeek", !cs.CanSeek);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void GetLength () 
		{
			DebugStream debug = new DebugStream ();
			cs = new CryptoStream (debug, encryptor, CryptoStreamMode.Read);
			long x = cs.Length;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void GetPosition () 
		{
			DebugStream debug = new DebugStream ();
			cs = new CryptoStream (debug, encryptor, CryptoStreamMode.Read);
			long x = cs.Position;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void SetPosition () 
		{
			DebugStream debug = new DebugStream ();
			cs = new CryptoStream (debug, encryptor, CryptoStreamMode.Read);
			cs.Position = 1;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void Seek () 
		{
			DebugStream debug = new DebugStream ();
			cs = new CryptoStream (debug, encryptor, CryptoStreamMode.Read);
			cs.Seek (0, SeekOrigin.Begin);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void SetLength () 
		{
			DebugStream debug = new DebugStream ();
			cs = new CryptoStream (debug, encryptor, CryptoStreamMode.Read);
			cs.SetLength (0);
		}

		[Test]
		// LAMESPEC : [ExpectedException (typeof (NotSupportedException))]
		public void FlushReadStream () 
		{
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			cs.Flush ();
		}

		[Test]
#if !NET_2_0
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void FlushFinalBlockReadStream () 
		{
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			cs.FlushFinalBlock ();
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void FlushFinalBlock_Dual () 
		{
			// do no corrupt writeStream in further tests
			using (FileStream fs = new FileStream ("FlushFinalBlock_Dual.tmp", FileMode.OpenOrCreate, FileAccess.Write)) {
				byte[] data = {0, 1, 2, 3, 4, 5, 6, 7};
				cs = new CryptoStream (fs, encryptor, CryptoStreamMode.Write);
				cs.Write (data, 0, data.Length);
				cs.FlushFinalBlock ();
				cs.FlushFinalBlock ();
			}
		}

		[Test]
		// LAMESPEC or MS BUG [ExpectedException (typeof (ObjectDisposedException))]
#if NET_2_0
		[ExpectedException (typeof (NotSupportedException))]
#else
		// LAMESPEC or MS BUG [ExpectedException (typeof (ObjectDisposedException))]
		[ExpectedException (typeof (ArgumentNullException))]
#endif
		public void FlushFinalBlock_Disposed () 
		{
			// do no corrupt writeStream in further tests
			using (FileStream fs = new FileStream ("FlushFinalBlock_Disposed.tmp", FileMode.OpenOrCreate, FileAccess.Write)) {
				cs = new CryptoStream (fs, encryptor, CryptoStreamMode.Write);
				cs.Clear ();
				cs.FlushFinalBlock ();
			}
		}

		[Test]
		// LAMESPEC or MS BUG [ExpectedException (typeof (ObjectDisposedException))]
#if !NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#endif
		public void Read_Disposed () 
		{
			// do no corrupt readStream in further tests
			using (FileStream fs = new FileStream ("read", FileMode.OpenOrCreate, FileAccess.Read)) {
				byte[] buffer = new byte [8];
				cs = new CryptoStream (fs, encryptor, CryptoStreamMode.Read);
				cs.Clear ();
				cs.Read (buffer, 0, 8);
			}
		}

		[Test]
		// MS BUG [ExpectedException (typeof (ObjectDisposedException))]
#if NET_2_0
		[ExpectedException (typeof (IndexOutOfRangeException))]
#else
		[Ignore ("Test cause System.ExecutionEngineException on MS runtime")]
#endif
		public void Read_Disposed_Break () 
		{
			// do no corrupt readStream in further tests
			using (FileStream fs = new FileStream ("read", FileMode.OpenOrCreate, FileAccess.Read)) {
				byte[] buffer = new byte [8];
				cs = new CryptoStream (fs, encryptor, CryptoStreamMode.Read);
				int len = cs.Read (buffer, 0, 4);
				AssertEquals ("Read 4", 4, len);
				cs.Clear ();
				len = cs.Read (buffer, 3, 4);
			}
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void Read_WriteStream () 
		{
			cs = new CryptoStream (writeStream, encryptor, CryptoStreamMode.Write);
			byte[] buffer = new byte [8];
			cs.Read (buffer, 0, 8);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Read_NullBuffer () 
		{
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			cs.Read (null, 0, 8);
		}

		[Test]
		public void Read_EmptyBuffer_ZeroCount () 
		{
			byte[] buffer = new byte [0];
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			int len = cs.Read (buffer, 0, 0);
			AssertEquals ("Read 0", 0, len);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_NegativeOffset () 
		{
			byte[] buffer = new byte [8];
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			cs.Read (buffer, -1, 8);
		}

		[Test]
		public void Read_ZeroCount () 
		{
			byte[] buffer = new byte [8];
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			int len = cs.Read (buffer, 0, 0);
			AssertEquals ("Read 0", 0, len);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_NegativeCount () 
		{
			byte[] buffer = new byte [8];
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			cs.Read (buffer, 0, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Read_OverflowCount () 
		{
			byte[] buffer = new byte [8];
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			cs.Read (buffer, 0, Int32.MaxValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Read_InvalidOffset () 
		{
			byte[] buffer = new byte [8];
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			cs.Read (buffer, 5, 4);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Read_OverflowOffset () 
		{
			byte[] buffer = new byte [8];
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			cs.Read (buffer, Int32.MaxValue, 4);
		}

		[Test]
		// MS BUG [ExpectedException (typeof (ObjectDisposedException))]
#if NET_2_0
		[ExpectedException (typeof (IndexOutOfRangeException))]
#else
		[Ignore ("Test cause System.ExecutionEngineException on MS runtime")]
#endif
		public void Write_Disposed () 
		{
			// do no corrupt writeStream in further tests
			using (FileStream fs = new FileStream ("Write_Disposed.tmp", FileMode.OpenOrCreate, FileAccess.Write)) {
				byte[] buffer = new byte [8];
				cs = new CryptoStream (fs, encryptor, CryptoStreamMode.Write);
				cs.Clear ();
				cs.Write (buffer, 0, 8);
			}
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void Write_ReadStream () 
		{
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			byte[] buffer = new byte [8];
			cs.Write (buffer, 0, 8);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Write_NullBuffer () 
		{
			cs = new CryptoStream (writeStream, encryptor, CryptoStreamMode.Write);
			cs.Write (null, 0, 8);
		}

		[Test]
		public void Write_EmptyBuffer_ZeroCount () 
		{
			byte[] buffer = new byte [0];
			cs = new CryptoStream (writeStream, encryptor, CryptoStreamMode.Write);
			cs.Write (buffer, 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_NegativeOffset () 
		{
			byte[] buffer = new byte [8];
			cs = new CryptoStream (writeStream, encryptor, CryptoStreamMode.Write);
			cs.Write (buffer, -1, 8);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Write_OverflowOffset () 
		{
			byte[] buffer = new byte [8];
			cs = new CryptoStream (writeStream, encryptor, CryptoStreamMode.Write);
			cs.Write (buffer, Int32.MaxValue, 8);
		}

		[Test]
		public void Write_ZeroCount () 
		{
			byte[] buffer = new byte [8];
			cs = new CryptoStream (writeStream, encryptor, CryptoStreamMode.Write);
			cs.Write (buffer, 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_NegativeCount () 
		{
			byte[] buffer = new byte [8];
			cs = new CryptoStream (writeStream, encryptor, CryptoStreamMode.Write);
			cs.Write (buffer, 0, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Write_InvalidOffset () 
		{
			DebugStream debug = new DebugStream ();
			byte[] buffer = new byte [8];
			cs = new CryptoStream (debug, encryptor, CryptoStreamMode.Write);
			cs.Write (buffer, 5, 4);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Write_OverflowCount () 
		{
			DebugStream debug = new DebugStream ();
			byte[] buffer = new byte [8];
			cs = new CryptoStream (debug, encryptor, CryptoStreamMode.Write);
			cs.Write (buffer, 0, Int32.MaxValue);
		}

		[Test]
		public void FullRoundtripRead () 
		{
			byte[] encrypted;
			using (DebugStream mem1 = new DebugStream ()) {
				byte[] toEncrypt = Encoding.Unicode.GetBytes ("Please encode me!");
				using (CryptoStream crypt = new CryptoStream (mem1, aes.CreateEncryptor (), CryptoStreamMode.Write)) {
					crypt.Write (toEncrypt, 0, toEncrypt.Length);
					crypt.FlushFinalBlock ();
				}
				encrypted = mem1.ToArray ();
			}
					
			using (DebugStream mem2 = new DebugStream (encrypted)) {
				byte[] buffer = new byte [1024];
				CryptoStream cr = new CryptoStream (mem2, aes.CreateDecryptor (), CryptoStreamMode.Read);
				int len = cr.Read (buffer, 0, buffer.Length);
				cr.Close ();
				AssertEquals ("Full Length Read", 34, len);
				AssertEquals ("Full Block Read", "Please encode me!", Encoding.Unicode.GetString (buffer, 0, len));
			}
		}

		// bugzilla 46143 (adapted from test case by Joerg Rosenkranz)
		[Test]
		public void PartialRoundtripRead () 
		{
			byte[] encrypted;
	                using (DebugStream mem1 = new DebugStream ()) {
				byte[] toEncrypt = Encoding.Unicode.GetBytes ("Please encode me!");
				using (CryptoStream crypt = new CryptoStream (mem1, aes.CreateEncryptor (), CryptoStreamMode.Write)) {
					crypt.Write (toEncrypt, 0, toEncrypt.Length);
					crypt.FlushFinalBlock ();
				}
				encrypted = mem1.ToArray ();
			}
					
			using (DebugStream mem2 = new DebugStream (encrypted)) {
				byte[] buffer = new byte [1024];
				CryptoStream cr = new CryptoStream (mem2, aes.CreateDecryptor (), CryptoStreamMode.Read);
				int len = cr.Read (buffer, 0, 20);
				cr.Clear ();
				cr.Close ();
				AssertEquals ("Partial Length Read", 20, len);
				AssertEquals ("Partial Block Read", "Please enc", Encoding.Unicode.GetString (buffer, 0, len));
	                }
		}

		// bugzilla: 40689 (adapted from test case by Henning Westerholt)
		[Test]
		public void WriteOnBlockWithFinal () 
		{
			byte[] desKey = {0, 1, 2, 3, 4, 5, 6, 7};
			byte[] desIV = {0, 1, 2, 3, 4, 5, 6, 7};
			DES des = DES.Create ();

			MemoryStream msin = new MemoryStream ();
			CryptoStream enc = new CryptoStream (msin, des.CreateEncryptor (desKey, desIV), CryptoStreamMode.Write);
			byte[] data = new byte [2200];
			enc.Write (data, 0, 2200);
			enc.FlushFinalBlock ();
			msin.Position = 0;
			AssertEquals ("Encryped Write Length", 2208, msin.Length); // 2200 + padding

			MemoryStream msout = new MemoryStream ();
			msout.SetLength (0);

			byte[] tmp = new byte [1024];
			long readlen = 0;
			long totallen = msin.Length;

			CryptoStream dec = new CryptoStream (msout, des.CreateDecryptor (desKey, desIV), CryptoStreamMode.Write);
			int len = msin.Read (tmp, 0, 1024);
			while (len > 0) {
				dec.Write (tmp, 0, len);
				readlen += len;
				len = msin.Read (tmp, 0, 1024);
			}
			AssertEquals ("Decryped Write Length", 2200, msout.Length);

			dec.Close ();
			dec.Clear ();
			msout.Close ();
			msin.Close ();

			AssertEquals ("Read Length", 2208, readlen); // 2200 + padding
		}

		[Test]
		public void PreGeneratedStreams ()
		{
			byte[] desKey = {0, 1, 2, 3, 4, 5, 6, 7};
			byte[] desIV = {0, 1, 2, 3, 4, 5, 6, 7};
			DES des = DES.Create ();
	
			for (int i=0; i < 9; i++) {
				MemoryStream msin = new MemoryStream ();
				CryptoStream enc = new CryptoStream (msin, des.CreateEncryptor (desKey, desIV), CryptoStreamMode.Write);
				byte[] data = new byte [i];
				enc.Write (data, 0, i);
				enc.FlushFinalBlock ();

				string msg = "PreGeneratedStream #" + i;
				string result = BitConverter.ToString (msin.ToArray ());
				switch (i) {
					case 0:
						AssertEquals (msg, "92-C9-DB-45-30-0B-93-2F", result); 
						break;
					case 1:
						AssertEquals (msg, "08-CF-A1-37-BD-56-D0-65", result); 
						break;
					case 2:
						AssertEquals (msg, "58-87-D4-9B-2C-27-97-0C", result); 
						break;
					case 3:
						AssertEquals (msg, "07-35-90-94-68-7D-51-FB", result); 
						break;
					case 4:
						AssertEquals (msg, "BF-00-98-C5-20-71-D0-DB", result); 
						break;
					case 5:
						AssertEquals (msg, "1A-55-C8-6E-C1-9B-31-82", result); 
						break;
					case 6:
						AssertEquals (msg, "2D-2B-76-41-61-0E-00-0C", result); 
						break;
					case 7:
						AssertEquals (msg, "DC-FF-73-D2-7F-D7-48-5D", result); 
						break;
					case 8:
						AssertEquals (msg, "E1-B2-46-E5-A7-C7-4C-BC-0E-40-4A-FC-08-92-B1-EB", result); 
						break;
				}
			}
		}

		private byte[] EmptyStream (PaddingMode mode) 
		{
			SymmetricAlgorithm algo = Rijndael.Create ();
			algo.Key = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
			algo.IV = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
			algo.Padding = mode;
			MemoryStream ms = new MemoryStream ();
			CryptoStream cs = new CryptoStream (ms, algo.CreateEncryptor(), CryptoStreamMode.Write);
			cs.Write (ms.GetBuffer (), 0, (int) ms.Length);
			cs.FlushFinalBlock ();
			cs.Flush ();
			return ms.ToArray ();
		}

		[Test]
		public void EmptyStreamWithPaddingNone () 
		{
			byte[] result = EmptyStream (PaddingMode.None);
			AssertEquals ("Result Length", 0, result.Length);
		}

		[Test]
		public void EmptyStreamWithPaddingPKCS7 () 
		{
			byte[] expected = { 0x07, 0xFE, 0xEF, 0x74, 0xE1, 0xD5, 0x03, 0x6E, 0x90, 0x0E, 0xEE, 0x11, 0x8E, 0x94, 0x92, 0x93 };
			byte[] result = EmptyStream (PaddingMode.PKCS7);
			AssertEquals ("Result Length", 16, result.Length);
			AssertEquals ("Result", expected, result);
		}

		[Test]
		public void EmptyStreamWithPaddingZeros () 
		{
			byte[] result = EmptyStream (PaddingMode.Zeros);
			AssertEquals ("Result Length", 0, result.Length);
		}

		// bugzilla: 49323 (adapted from test case by Carlos Guzmán Álvarez)
		[Test]
		public void MultiblocksWithPartial () 
		{
			SymmetricAlgorithm tdes = new TripleDESCryptoServiceProvider ();
			tdes.Key = new byte[] {161, 54, 179, 213, 89, 75, 130, 4, 186, 99, 158, 127, 19, 195, 175, 143, 79, 109, 25, 202, 237, 235, 62, 170};
			tdes.IV	= new byte[] {193, 227, 54, 132, 68, 172, 55, 91};

			byte[] fragment = new byte[] {20, 0, 0, 12, 181, 134, 8, 230, 185, 75, 19, 129, 101, 142, 118, 190};
			byte[] mac = new byte[] {42, 148, 229, 58, 185, 249, 154, 131, 157, 79, 176, 168, 143, 71, 0, 118, 5, 10, 95, 8};
																								  
			// Encryption ( fragment + mac [+ padding + padding_length] )
			MemoryStream ms = new MemoryStream ();
			CryptoStream cs = new CryptoStream (ms, tdes.CreateEncryptor (), CryptoStreamMode.Write);
			cs.Write (fragment, 0, fragment.Length);
			cs.Write (mac, 0, mac.Length);
			// Calculate padding_length
			int fragmentLength = fragment.Length + mac.Length + 1;
			int padding = (((fragmentLength / 8) * 8) + 8) - fragmentLength;
			// Write padding length byte
			cs.WriteByte ((byte)padding);
			cs.Close ();
			byte[] encrypted = ms.ToArray ();
			byte[] expected = new byte[] { 0x9c, 0x99, 0x56, 0x8e, 0x75, 0x3e, 0x02, 0x95, 0x5b, 0x5c, 0x46, 0x8b, 0xcf, 0xf8, 0x27, 0x21, 0x53, 0x5f, 0x3d, 0xd8, 0x16, 0x95, 0x82, 0x3d, 0x88, 0x9b, 0x9a, 0x47, 0xda, 0x97, 0x90, 0x86, 0x50, 0x0e, 0x48, 0xee, 0xe7, 0x9b, 0x25, 0x41 };
			AssertEquals ("MultiblocksWithPartial", expected, encrypted);
		}

		// Adapted from Subba Rao Thirumoorthy email on mono-devel-list (december 2003)
		private byte[] NonMultipleOfBlockSize_Encrypt (ICryptoTransform ct, byte[] data)
		{
			DebugStream stream = new DebugStream ();
			CryptoStream CryptStream = new CryptoStream (stream, ct, CryptoStreamMode.Write);

			int len = 0;
			long myLength = 0;
			byte[] Buffer = new byte [1024];
			
			DebugStream fout = new DebugStream (data);
			while (myLength < data.Length) {
				len = fout.Read (Buffer, 0, 1023);
				if (len == 0)
					break;
				CryptStream.Write (Buffer, 0, len);
				CryptStream.Flush ();
				myLength = myLength + len;
			}
			CryptStream.FlushFinalBlock ();
			// we must ensure that the result is correct
			AssertEquals ("Length(final)", 64, len);
			byte[] result = stream.ToArray ();
			string end = BitConverter.ToString (result, 65520, 16);
			AssertEquals ("End part", "04-70-19-1D-28-C5-BD-9A-23-C6-60-E2-28-96-38-65", end);

			CryptStream.Close();
			stream.Close();
			return result;
		}

		private byte[] NonMultipleOfBlockSize_Decrypt (ICryptoTransform ct, byte[] data) 
		{
			DebugStream stream = new DebugStream (data);
			CryptoStream CryptStream = new CryptoStream (stream, ct, CryptoStreamMode.Read);

			int len = 0;
			long myLength = 0;
			byte[] Buffer = new Byte [1024];

			DebugStream fout = new DebugStream ();
			// each returned block must be 1023 bytes long 
			// even if this isn't a multiple of the block size
			while ((len = CryptStream.Read (Buffer, 0, 1023)) != 0) {
				fout.Write (Buffer, 0, len);
				fout.Flush ();
				myLength = myLength + len;
			}

			byte[] result = fout.ToArray ();
			CryptStream.Close ();
			stream.Close ();
			return result;
		}

		[Test]
		public void NonMultipleOfBlockSize ()
		{
			byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
			byte[] iv  = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
			byte[] data = new byte [65536];

			RijndaelManaged aes = new RijndaelManaged ();
			ICryptoTransform encryptor = aes.CreateEncryptor (key, iv);
			byte[] encdata = NonMultipleOfBlockSize_Encrypt (encryptor, data);
			AssertEquals ("Encrypted Data Length", (data.Length + (aes.BlockSize >> 3)), encdata.Length);
			
			ICryptoTransform decryptor = aes.CreateDecryptor (key, iv);
			byte[] decdata = NonMultipleOfBlockSize_Decrypt (decryptor, encdata);
			AssertEquals ("Decrypted Data Length", data.Length, decdata.Length);

			int i = 0;
			bool b = true;
			while (b && (i < data.Length)) {
				b = (data [i] == decdata [i]);
				i++;
			}
			Assert ("NonMultipleOfBlockSize", b);
		}

		// bugzilla: 51322 - indirectly related but it explains why my first (unapplied) patch didn't work
		[Test]
		public void DecryptPartial_TransformFinalBlock_required () 
		{
			byte[] key = {0, 1, 2, 3, 4, 5, 6, 7};
			byte[] iv = {0, 1, 2, 3, 4, 5, 6, 7};
			DES des = DES.Create ();

			byte[] data = Encoding.Unicode.GetBytes ("ximian");	// 12 bytes, 1.5 DES block size
			DebugStream encrypted = new DebugStream ();
			cs = new CryptoStream (encrypted, des.CreateEncryptor (key, iv), CryptoStreamMode.Write);
			cs.Write (data, 0, data.Length);
			cs.Close ();

			data = encrypted.ToArray ();
			DebugStream decrypted = new DebugStream (data);
			cs = new CryptoStream (decrypted, des.CreateDecryptor (key, iv), CryptoStreamMode.Read);
			int len = cs.Read (data, 0, data.Length);
			cs.Close ();
			AssertEquals ("Length", 12, len);
			AssertEquals ("Unicode DES Roundtrip", "ximian", Encoding.Unicode.GetString (data, 0, len));
		}

		[Test]
		public void DecryptPartial_TransformFinalBlock_2Pass () 
		{
			byte[] key = {0, 1, 2, 3, 4, 5, 6, 7};
			byte[] iv = {0, 1, 2, 3, 4, 5, 6, 7};
			DES des = DES.Create ();

			byte[] data = Encoding.Unicode.GetBytes ("ximian");	// 12 bytes, 1.5 DES block size
			DebugStream encrypted = new DebugStream ();
			cs = new CryptoStream (encrypted, des.CreateEncryptor (key, iv), CryptoStreamMode.Write);
			cs.Write (data, 0, data.Length);
			cs.Close ();

			data = encrypted.ToArray ();
			DebugStream decrypted = new DebugStream (data);
			cs = new CryptoStream (decrypted, des.CreateDecryptor (key, iv), CryptoStreamMode.Read);
			int len = cs.Read (data, 0, 6);
			AssertEquals ("Length (1st pass)", 6, len);
			AssertEquals ("Partial DES Roundtrip", "xim", Encoding.Unicode.GetString (data, 0, len));
			len += cs.Read (data, 6, 8);
			AssertEquals ("Length (1st+2nd)", 12, len);
			AssertEquals ("Full DES Roundtrip", "ximian", Encoding.Unicode.GetString (data, 0, len));
			cs.Close ();
		}

		// based on http://www.c-sharpcorner.com/Code/2002/May/FileEncryption.asp
		[Test]
		public void WriteByteReadByte () 
		{
			DebugStream original = new DebugStream (Encoding.Unicode.GetBytes ("ximian"));

			DebugStream encrypted = new DebugStream ();
			byte[] key = {0, 1, 2, 3, 4, 5, 6, 7};
			byte[] iv = {0, 1, 2, 3, 4, 5, 6, 7};
			DES des = DES.Create ();
			cs = new CryptoStream (encrypted, des.CreateEncryptor (key, iv), CryptoStreamMode.Write);

			int data;
			while ((data = original.ReadByte ()) != -1)
				cs.WriteByte((byte) data);
			cs.Close ();

			byte[] result = encrypted.ToArray ();
			AssertEquals ("Encrypted", "18-EA-93-3F-20-86-D2-AA-78-02-D7-6F-E4-47-17-9C", BitConverter.ToString (result));

			encrypted = new DebugStream (result);
			DebugStream decrypted = new DebugStream ();
			cs = new CryptoStream (encrypted, des.CreateDecryptor (key, iv), CryptoStreamMode.Read);

			while ((data = cs.ReadByte ()) != -1)
				decrypted.WriteByte((byte) data);
			cs.Close ();
			decrypted.Close ();

			AssertEquals ("W/R Byte Roundtrip", "ximian", Encoding.Unicode.GetString (decrypted.ToArray ()));
		}

		// based http://www.4guysfromrolla.com/webtech/090501-1.shtml

		public string EncryptData (ICryptoTransform des, string strData) 
		{
			strData = String.Format("{0,5:00000}" + strData, strData.Length);
			byte[] data = Encoding.ASCII.GetBytes (strData);

			MemoryStream mStream = new MemoryStream (data); 
			CryptoStream cs = new CryptoStream (mStream, des, CryptoStreamMode.Read);        
			MemoryStream mOut = new MemoryStream ();
	
			int bytesRead; 
			byte[] output = new byte [1024]; 
			do { 
				bytesRead = cs.Read (output, 0, 1024);
				if (bytesRead != 0) 
					mOut.Write (output, 0, bytesRead); 
			} 
			while (bytesRead > 0); 
	
			return Convert.ToBase64String (mOut.ToArray ());
		}

		public string DecryptData (ICryptoTransform des, string strData) 
		{
			MemoryStream mOut = new MemoryStream ();
			byte[] data = Convert.FromBase64String (strData);
			CryptoStream cs = new CryptoStream (mOut, des, CryptoStreamMode.Write);        
			cs.Write (data, 0, (int)data.Length);
			cs.FlushFinalBlock ();
			return Encoding.ASCII.GetString (mOut.ToArray ()).Substring (5);
		}

		[Test]
		public void EncryptOnRead () 
		{
			SHA1 sha = SHA1.Create ();
			byte[] vector = sha.ComputeHash (Encoding.ASCII.GetBytes ("s3kr3t"));
			byte[] key = new byte [8];
			Buffer.BlockCopy (vector, 0, key, 0, key.Length);
			byte[] iv = new byte [8];
			Buffer.BlockCopy (vector, 8, iv, 0, iv.Length);

			DES des = DES.Create ();

			StringBuilder sb = new StringBuilder ();
			sb.Append ("a");
			string data = sb.ToString ();
			string encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			string decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "9YVfvrh5yj0=", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "qNe4d0UlkU8=", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "OcernYAQ1NAME/Gny+ZuaA==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "H5UveR2lds1T+IWN4pks2Q==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "dDQ3HAVtTbiRwwUqWANaeA==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "At1r7dVDjJlQidf4QzCNkw==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "DFDJWJGaNrFVBDXovsq1ew==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "gM040QGMPOBj3u1lEK4XHQ==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "P5hRUhrxOWFX0ER/IjJL/Q==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "uDIaQ1uXtWUIboGFLt306Q==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "giJKTXfad5Z8hebhXtYZ4hmKX/EC8w6x", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "lBehBplIrjjrlIrMjYcNz1DOoXLHjZdn", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "2elWrUnjmsAOpo2s4voJyZXEJ/rtKB7P", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "GB3BaIZGf9K+T82j7T8Fri2rQ2/YUdSe", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "Gc+wkJL+CVjdJchgcIoi8dkH2BVpHJgB", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "loeuyII/PvWb91M4pFVkyaPxQoQVYpNb", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "PHXmi/sxNIgApXAfdm+Bf54/nCM//N8o", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "xpb+wj/8LmH2ScTg3OU4JOsE5Owj6flF", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "WJz4VfsZ2emzhYWoSf+PNBDpHooxEregqMWnzm4gcqU=", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "PaouZu1iOKbCMRJSu04y/kB+TcOk4yp8K2BOGDs1PPE=", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "qbTDs4dFy7eERdn5vV7JRPk2/m9smtwvZjA6+TmGlkI=", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "f2FsphcpM7Fu90S5V17ptly44lL4GvFCCaFdnnU4twk=", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "imD+ntHsUmp9ALJedzC1JmAJY0r2O4KkP8271+XuG4g=", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "80QLLUmHwx1fcEYGeFz1WXlS13kUy994sQLI6GhcjuM=", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "DtIIlj8BCOppmIgQ9AEdUj7pBB49S/9Q38kbWLjwiVs=", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "LNkprYaaUFtyan204OzX+a2pzOb/Pg5WXzXJ6WWB1rQ=", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "FRgx9m2lT2PxtYSIdRwc+SznJetNiRk1MEIZDl3D13pvo2yOtJ1MSQ==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "V7JlnpJscrdIpX4z5S+/Q5WDjKzK4aB5TiqI3JZOYJ+KE1CWQNNeow==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "wVwPv1c2KQynbwiOBCAhmQlReOQT52qFR34AX4dtjEeQ1oCQ1N1tHg==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "Zi+G0yfmuFjSjP455pjVeKBDDWB4qvTb0K0h20UtflrYG6wcWqUzDw==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);

			sb.Append ("a");
			data = sb.ToString ();
			encdata = EncryptData (des.CreateEncryptor (key, iv), data);
			decdata = DecryptData (des.CreateDecryptor (key, iv), encdata);
			AssertEquals ("Encrypt-" + data, "0hGoonZ8jrLhMNDKBuWrlvFnq15ZLvnyq+Ilq8r4aYUEDxttQMwi5w==", encdata);
			AssertEquals ("Decrypt-" + data, data, decdata);
		}

		// based on System.Security assembly XmlDsigBase64TransformTest

		[Test]
		public void FromBase64_Write () 
		{
			string expected = "http://www.go-mono.com/";
			byte[] data = Encoding.UTF8.GetBytes (expected);
			string temp = Convert.ToBase64String (data, 0, data.Length);
			data = Encoding.UTF8.GetBytes (temp);

			DebugStream debug = new DebugStream ();
			ICryptoTransform base64 = new FromBase64Transform ();
			cs = new CryptoStream (debug, base64, CryptoStreamMode.Write);
			cs.Write (data, 0, data.Length);
			cs.FlushFinalBlock ();
			byte[] encoded = debug.ToArray ();

			string result = Encoding.UTF8.GetString (encoded);
			AssertEquals ("FromBase64_Write", expected, result);
		}

		[Test]
		public void FromBase64_Read () 
		{
			byte[] original = Encoding.UTF8.GetBytes ("aHR0cDovL3d3dy5nby1tb25vLmNvbS8=");
			DebugStream debug = new DebugStream (original);

			ICryptoTransform base64 = new FromBase64Transform ();
			cs = new CryptoStream (debug, base64, CryptoStreamMode.Read);
			
			byte[] data = new byte [1024];
			int length = cs.Read (data, 0, data.Length);
			cs.Close ();

			string result = Encoding.UTF8.GetString (data, 0, length);
			AssertEquals ("ToBase64_Read", "http://www.go-mono.com/", result);
		}

		[Test]
		public void ToBase64_Write () 
		{
			byte[] data = Encoding.UTF8.GetBytes ("http://www.go-mono.com/");

			DebugStream debug = new DebugStream ();
			ICryptoTransform base64 = new ToBase64Transform ();
			cs = new CryptoStream (debug, base64, CryptoStreamMode.Write);
			cs.Write (data, 0, data.Length);
			cs.FlushFinalBlock ();
			byte[] encoded = debug.ToArray ();

			string result = Encoding.UTF8.GetString (encoded);
			AssertEquals ("ToBase64_Write", "aHR0cDovL3d3dy5nby1tb25vLmNvbS8=", result);
		}

		[Test]
		public void ToBase64_Read () 
		{
			byte[] original = Encoding.UTF8.GetBytes ("http://www.go-mono.com/");
			DebugStream debug = new DebugStream (original);

			ICryptoTransform base64 = new ToBase64Transform ();
			cs = new CryptoStream (debug, base64, CryptoStreamMode.Read);
			
			byte[] data = new byte [1024];
			int length = cs.Read (data, 0, data.Length);
			cs.Close ();

			string result = Encoding.UTF8.GetString (data, 0, length);
			AssertEquals ("ToBase64_Read", "aHR0cDovL3d3dy5nby1tb25vLmNvbS8=", result);
		}

		// Cascaded CryptoStream - like sample in book .NET Framework Security, chapter 30

		[Test]
		public void CascadedCryptoStream_Write () 
		{
			DebugStream debug = new DebugStream ();

			// calculate both the hash (before encryption) and encrypt in one Write operation
			byte[] key = {0, 1, 2, 3, 4, 5, 6, 7};
			byte[] iv = {0, 1, 2, 3, 4, 5, 6, 7};
			DES des = DES.Create ();
			CryptoStream cse = new CryptoStream (debug, des.CreateEncryptor (key, iv), CryptoStreamMode.Write);

			MD5 hash = MD5.Create ();
			CryptoStream csh = new CryptoStream (cse, hash, CryptoStreamMode.Write);

			byte[] data = Encoding.UTF8.GetBytes ("http://www.go-mono.com/");
			csh.Write (data, 0, data.Length);
			csh.FlushFinalBlock ();

			byte[] result = debug.ToArray ();
			AssertEquals ("Encrypted", "8C-24-76-74-09-79-2B-D3-47-C3-32-F5-F3-1A-5E-57-04-33-2E-B8-50-77-B2-A1", BitConverter.ToString (result));
			byte[] digest = hash.Hash;
			AssertEquals ("Hash", "71-04-12-D1-95-01-CF-F9-8D-8F-F8-0D-F9-AA-11-7D", BitConverter.ToString (digest));
		}

		[Test]
		public void CascadedCryptoStream_Read () 
		{
			byte[] encdata = new byte[] { 0x8C, 0x24, 0x76, 0x74, 0x09, 0x79, 0x2B, 0xD3, 0x47, 0xC3, 0x32, 0xF5, 0xF3, 0x1A, 0x5E, 0x57, 0x04, 0x33, 0x2E, 0xB8, 0x50, 0x77, 0xB2, 0xA1 };
			DebugStream debug = new DebugStream (encdata);

			// decrypt data and validate its hash in one Read operation
			byte[] key = {0, 1, 2, 3, 4, 5, 6, 7};
			byte[] iv = {0, 1, 2, 3, 4, 5, 6, 7};
			DES des = DES.Create ();
			CryptoStream csd = new CryptoStream (debug, des.CreateDecryptor (key, iv), CryptoStreamMode.Read);

			MD5 hash = MD5.Create ();
			CryptoStream csh = new CryptoStream (csd, hash, CryptoStreamMode.Read);

			byte[] data = new byte [1024];
			int length = csh.Read (data, 0, data.Length);
			csh.Close ();
                        
			string result = Encoding.UTF8.GetString (data, 0, length);
			AssertEquals ("Decrypted", "http://www.go-mono.com/", result);
			byte[] digest = hash.Hash;
			AssertEquals ("Hash Validation", "71-04-12-D1-95-01-CF-F9-8D-8F-F8-0D-F9-AA-11-7D", BitConverter.ToString (digest));
		}

		// bugzilla: 60573 - the number of block is not reduced for encryptors

		[Test]
		public void EncryptorWriteBlocks () 
		{
			DebugStream debug = new DebugStream ();

			byte[] key = {0, 1, 2, 3, 4, 5, 6, 7};
			byte[] iv = {0, 1, 2, 3, 4, 5, 6, 7};
			DES des = DES.Create ();
			CryptoStream cse = new CryptoStream (debug, des.CreateEncryptor (key, iv), CryptoStreamMode.Write);

			byte[] data = new byte [64];
			cse.Write (data, 0, 64);
			AssertEquals ("Length", 64, debug.Length);
			cse.Close ();
		}

		[Test]
		public void DecryptorWriteBlocks () 
		{
			DebugStream debug = new DebugStream ();

			byte[] key = {0, 1, 2, 3, 4, 5, 6, 7};
			byte[] iv = {0, 1, 2, 3, 4, 5, 6, 7};
			DES des = DES.Create ();
			CryptoStream csd = new CryptoStream (debug, des.CreateDecryptor (key, iv), CryptoStreamMode.Write);

			byte[] data = new byte [64] { 0xE1, 0xB2, 0x46, 0xE5, 0xA7, 0xC7, 0x4C, 0xBC, 0xD5, 0xF0, 0x8E, 0x25, 0x3B, 0xFA, 0x23, 0x80, 0x03, 0x16, 0x18, 0x17, 0xA3, 0x59, 0xBA, 0xAC, 0xFC, 0x47, 0x57, 0x2A, 0xF9, 0x44, 0x07, 0x84, 0x20, 0x74, 0x06, 0x38, 0xC2, 0xF3, 0xA1, 0xCE, 0x8C, 0x73, 0xB1, 0xE3, 0x75, 0x03, 0x66, 0x89, 0xF0, 0x4E, 0x98, 0x68, 0xB1, 0xBD, 0x85, 0x25, 0xFF, 0x4B, 0x11, 0x74, 0xEF, 0x14, 0xC8, 0xE9 };
			csd.Write (data, 0, 64);
			AssertEquals ("Length", 56, debug.Length);
			// last block is kept for later processing
		}
	}
}
