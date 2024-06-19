//
// Unit tests for MachineKeySectionUtils (internals)
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
#if false
using System;
using System.IO;
using System.Web.Configuration;
using System.Web.Util;
using NUnit.Framework;

namespace MonoTests.System.Web.Util {

	[TestFixture]
	public class MachineKeySectionUtilsTest {

		static byte ChangeByte (byte b)
		{
			return (b == Byte.MaxValue) ? Byte.MinValue : (byte) (b + 1);
		}

		public void Encrypt_RoundTrip (MachineKeySection section)
		{
			byte [] data = new byte [14];
			byte [] encdata = MachineKeySectionUtils.Encrypt (section, data);
			byte [] decdata = MachineKeySectionUtils.Decrypt (section, encdata);
			Assert.AreEqual (data, decdata, "roundtrip");

			// changing length (missing first byte)
			byte [] cut = new byte [encdata.Length - 1];
			Array.Copy (encdata, 1, cut, 0, cut.Length);
			Assert.IsNull (MachineKeySectionUtils.Decrypt (section, cut), "bad length");

			// changing last byte (padding)
			byte be = encdata [encdata.Length - 1];
			encdata [encdata.Length - 1] = ChangeByte (be);
			byte[] result = MachineKeySectionUtils.Decrypt (section, encdata);
			// this will return null if a bad padding is detected - OTOH since we're using a random key and we
			// encrypt a random IV it's possible the decrypted stuff will randomly have a "valid" padding (there's
			// only so much possible values and the bots runs those tests pretty often and give false positive)
			// To avoid this we fallback to ensure the data is invalid (if should be empty)
			int total = 0;
			if (result != null) {
				for (int i=0; i < result.Length; i++)
					total += result [i];
			}
			Assert.IsTrue (result == null || total != 0, "bad padding");
		}

		[Test]
		public void Encrypt_RoundTrip_Default ()
		{
			Encrypt_RoundTrip (new MachineKeySection ());
		}

		[Test]
		public void Encrypt_RoundTrip_AES ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.AES;
			Encrypt_RoundTrip (section);
		}

		[Test]
		public void Encrypt_RoundTrip_TripleDES ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.TripleDES;
			Encrypt_RoundTrip (section);
		}

		[Test]
		public void Encrypt_RoundTrip_MD5 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.MD5;
			Encrypt_RoundTrip (section);
		}

		[Test]
		public void Encrypt_RoundTrip_SHA1 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.SHA1;
			Encrypt_RoundTrip (section);
		}
		[Test]
		public void Encrypt_RoundTrip_HMACSHA256 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.HMACSHA256;
			EncryptSign_RoundTrip (section);
		}

		[Test]
		public void Encrypt_RoundTrip_HMACSHA384 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.HMACSHA384;
			EncryptSign_RoundTrip (section);
		}

		[Test]
		public void Encrypt_RoundTrip_HMACSHA512 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.HMACSHA512;
			EncryptSign_RoundTrip (section);
		}

		[Test]
		public void Encrypt_RoundTrip_Custom_RIPEMD160 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.ValidationAlgorithm = "alg:HMACRIPEMD160";
			EncryptSign_RoundTrip (section);
		}
		public void EncryptSign_RoundTrip (MachineKeySection section)
		{
			byte [] data = new byte [14];
			byte [] block = MachineKeySectionUtils.EncryptSign (section, data);
			byte [] decdata = MachineKeySectionUtils.VerifyDecrypt (section, block);
			Assert.AreEqual (data, decdata, "roundtrip");

			// changing a byte of the data
			byte b0 = block [0];
			block [0] = ChangeByte (b0);
			Assert.IsNull (MachineKeySectionUtils.VerifyDecrypt (section, block), "bad data");
			block [0] = b0;

			// changing a byte of the signature
			byte be = block [block.Length - 1];
			block [block.Length - 1] = ChangeByte (be);
			Assert.IsNull (MachineKeySectionUtils.VerifyDecrypt (section, block), "bad signature");
		}

		[Test]
		public void EncryptSign_RoundTrip_Default ()
		{
			EncryptSign_RoundTrip (new MachineKeySection ());
		}

		[Test]
		public void EncryptSign_RoundTrip_AES ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.AES;
			EncryptSign_RoundTrip (section);
		}

		[Test]
		public void EncryptSign_RoundTrip_TripleDES ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.TripleDES;
			EncryptSign_RoundTrip (section);
		}

		[Test]
		public void EncryptSign_RoundTrip_MD5 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.MD5;
			EncryptSign_RoundTrip (section);
		}

		[Test]
		public void EncryptSign_RoundTrip_SHA1 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.SHA1;
			EncryptSign_RoundTrip (section);
		}
		[Test]
		public void EncryptSign_RoundTrip_HMACSHA256 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.HMACSHA256;
			EncryptSign_RoundTrip (section);
		}

		[Test]
		public void EncryptSign_RoundTrip_HMACSHA384 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.HMACSHA384;
			EncryptSign_RoundTrip (section);
		}

		[Test]
		public void EncryptSign_RoundTrip_HMACSHA512 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.HMACSHA512;
			EncryptSign_RoundTrip (section);
		}

		[Test]
		public void EncryptSign_RoundTrip_Custom_RIPEMD160 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.ValidationAlgorithm = "alg:HMACRIPEMD160";
			EncryptSign_RoundTrip (section);
		}
		public void Validation_RoundTrip (MachineKeySection section)
		{
			byte [] data = new byte [] { 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0 };
			byte [] block = MachineKeySectionUtils.Sign (section, data);
			Assert.AreEqual (data, MachineKeySectionUtils.Verify (section, block), "OK");

			// changing last byte
			for (int i = 0; i < data.Length; i++) {
				byte b = block [i];
				block [i] = ChangeByte (b);
				Assert.IsNull (MachineKeySectionUtils.Verify (section, block), "bad-" + i.ToString ());
				block [i] = b;
			}
		}

		[Test]
		public void Validation_RoundTrip_Default ()
		{
			Validation_RoundTrip (new MachineKeySection ());
		}

		[Test]
		public void Validation_RoundTrip_AES ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.AES;
			Validation_RoundTrip (section);
		}

		[Test]
		public void Validation_RoundTrip_TripleDES ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.TripleDES;
			Validation_RoundTrip (section);
		}

		[Test]
		public void Validation_RoundTrip_MD5 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.MD5;
			Validation_RoundTrip (section);
		}

		[Test]
		public void Validation_RoundTrip_SHA1 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.SHA1;
			Validation_RoundTrip (section);
		}

		[Test]
		public void Validation_RoundTrip_HMACSHA256 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.HMACSHA256;
			Validation_RoundTrip (section);
		}

		[Test]
		public void Validation_RoundTrip_HMACSHA384 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.HMACSHA384;
			Validation_RoundTrip (section);
		}

		[Test]
		public void Validation_RoundTrip_HMACSHA512 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.HMACSHA512;
			Validation_RoundTrip (section);
		}

		[Test]
		public void Validation_RoundTrip_Custom_RIPEMD160 ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.ValidationAlgorithm = "alg:HMACRIPEMD160";
			Validation_RoundTrip (section);
		}
		[Test]
		public void GetHexString ()
		{
			Assert.AreEqual ("DEADC0DE", MachineKeySectionUtils.GetHexString (new byte [] { 0xde, 0xad, 0xc0, 0xde }), "deadcode");
		}
	}
}
#endif
