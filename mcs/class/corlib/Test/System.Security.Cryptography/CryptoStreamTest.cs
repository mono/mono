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

		// TODO: Test with Hash object
	}
}
