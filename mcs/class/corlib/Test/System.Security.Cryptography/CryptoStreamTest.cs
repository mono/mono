//
// CryptoStreamTest.cs - NUnit Test Cases for CryptoStream
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class CryptoStreamTest : Assertion {

		Stream readStream;
		Stream writeStream;
		ICryptoTransform encryptor;
		ICryptoTransform decryptor;
		CryptoStream cs;
		SymmetricAlgorithm aes;

		[SetUp]
		void SetUp () 
		{
			if (readStream == null) {
				readStream = new FileStream ("read", FileMode.OpenOrCreate, FileAccess.Read);
				writeStream = new FileStream ("write", FileMode.OpenOrCreate, FileAccess.Write);
				aes = SymmetricAlgorithm.Create ();
				encryptor = aes.CreateEncryptor ();
				decryptor = aes.CreateEncryptor ();
			}
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
		public void TransformNull ()
		{
			cs = new CryptoStream (readStream, null, CryptoStreamMode.Read);
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
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			long x = cs.Length;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void GetPosition () 
		{
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			long x = cs.Position;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void SetPosition () 
		{
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			cs.Position = 1;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void Seek () 
		{
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			cs.Seek (0, SeekOrigin.Begin);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void SetLength () 
		{
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
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
		[ExpectedException (typeof (NotSupportedException))]
		public void FlushFinalBlockReadStream () 
		{
			cs = new CryptoStream (readStream, encryptor, CryptoStreamMode.Read);
			cs.FlushFinalBlock ();
		}

		// bugzilla 46143 (adapted from test case by Joerg Rosenkranz)
		[Test]
		public void PartialRead () 
		{
			byte[] encrypted;
	                using (MemoryStream mem1 = new MemoryStream ()) {
		                byte[] toEncrypt = Encoding.Unicode.GetBytes ("Please encode me!");
				using (CryptoStream crypt = new CryptoStream (mem1, aes.CreateEncryptor (), CryptoStreamMode.Write)) {
					crypt.Write (toEncrypt, 0, toEncrypt.Length);
					crypt.FlushFinalBlock ();
				}
				encrypted = mem1.ToArray ();
			}
					
			using (MemoryStream mem2 = new MemoryStream (encrypted)) {
				byte[] buffer = new byte[1024];
				CryptoStream cr = new CryptoStream (mem2, aes.CreateDecryptor (), CryptoStreamMode.Read);
				cr.Read (buffer, 0, 20);
				cr.Close ();
				Assert ("Partial Block Read", Encoding.Unicode.GetString (buffer).StartsWith ("Please enc"));
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

		// TODO: Test with Hash object
	}
}
