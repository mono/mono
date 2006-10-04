//
// PKCS1Test.cs - NUnit Test Cases for PKCS #1
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004,2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.Cryptography;
using NUnit.Framework;

namespace MonoTests.Mono.Security.Cryptography {

	/* copied from MD5SHA1 which is internal in Mono.Security.dll */
	class MD5SHA1: HashAlgorithm {

		private HashAlgorithm md5;
		private HashAlgorithm sha;

		public MD5SHA1 ()
			: base ()
		{
			this.md5 = MD5.Create ();
			this.sha = SHA1.Create ();

			// Set HashSizeValue
			this.HashSizeValue = this.md5.HashSize + this.sha.HashSize;
		}

		public override void Initialize ()
		{
			this.md5.Initialize ();
			this.sha.Initialize ();
		}

		protected override byte[] HashFinal ()
		{
			// Finalize the original hash
			this.md5.TransformFinalBlock (new byte[0], 0, 0);
			this.sha.TransformFinalBlock (new byte[0], 0, 0);

			byte[] hash = new byte[36];

			Buffer.BlockCopy (this.md5.Hash, 0, hash, 0, 16);
			Buffer.BlockCopy (this.sha.Hash, 0, hash, 16, 20);

			return hash;
		}

		protected override void HashCore (
			byte[] array,
			int ibStart,
			int cbSize)
		{
			this.md5.TransformBlock (array, ibStart, cbSize, array, ibStart);
			this.sha.TransformBlock (array, ibStart, cbSize, array, ibStart);
		}
	}

	[TestFixture]
	public class PKCS1Test {

		// Test cases and key pair provided by Joerg Rosenkranz
		// Note: I couldn't produce any similar (short) results with Fx 2.0 beta 1
		// so I was either (very) unlucky or this was fixed post 1.1.
		private const string short017 = "JbLFk42L8GEXm2xfJ79RopitBBwPCnS5BjYbkL44EL9iVYKEHLEIuO1LGqqzjh9DKB+KhYUQEz+5KRWNeHsqY4FfvoRdmj9yhbffPxw/TA5P7ez6i7R3K7U0p2sLsP4jSQS+aMPumudMJDXAfRPq5raSFvYS03IldEC3IkZDBg==";
		private const string short063 = "kws/A0BZI3si9Rjbt5gG6B3T/h0jFyEagnq31vl9kSakfry3369ncsHMtDeQAPnMaMHSegUvMgrTLv+ZmCFwtldhXhp4ICzu9HpVrZyYj55Q8cdO9DrOqJsTW89rdwYStIXfWsLw2wPNNrfe6vSb5yjI2sC4iFTbNbxdAyPqjw==";
		private const string short136 = "0vf3pwg0qWF36ff2KTt6DB7QlRKtOEWn+urZKnZ8Gh+EalTHsbmiBaOxhpl16MYxdMt9oeW+ioNWXZAnF4XomKPeQpBwbUDTTVvoPEDudkOo271R1O87A7oE4w+n2qz9Xgp9pLe2IlEw2jSOZkUK6hv4Zm6gv79h+g3Dz+EQOw==";
		private const string short264 = "NAWbAU7roLAQT9TZJRXhJoAEhRgBDubUAMTZUr+gs9Soszzb3eh/Hy/pqHdgIwqSzQIqDiMmwZvgiFgkQxWquI9xLwffCES4bnO1eVOFHSPeHkMIR+o6YmNKhsud8fr3y5SSTn1zb2UHgNN5ooX9A7IwjAIHuJkCR9zW1kmkzg==";
		private const string short482 = "tR27JbaiCrKu/1L2xBohbrtlrTOtRd4c2EEI2ThlwIby7LAJr6i8GAIxpPQFhs/RFSISW1lujfMIIlPH13HrzyfblluYunqhGzgNk57/kM/Wo2y1TRUbBZ1uWpD96kqpRBqN63MY8E1L0kMIICHMPn4gaaHNv/wIvd3bRgVrOQ==";
		private const string short631 = "4afUaTo+bHTVwAlg/XE/saI7KlCGHAbIA4y7hbDIkUWrkwAU3BeMNoNMEBBFKyVXobpDdgNxE7K64Tkmc8KLDhyUHsgCNvPoBL8OQnFIxLRxIOzrpT7I461vIm7dqaFMvAkodLDiy36l/OS8V9r4E1InSLEH+4h/0La4SWEBeA==";
		private const string short634 = "s56QfTTHzvP0h9I5LNu1lq8KrRXdOficbnzbzT15WxST0psg03I+c6ZiCS/y2WsXTVzL183rQjrT7OxE4NBZMLh3g86QGNeIjarxrJnY3sqb43jeNmC8+EuTGpuDigEW0GEGMUGmv//pnwpmE9xnOseuLPPPJx2z5sWcCYHsgg==";
		private const string short890 = "UiLJObGTgKTYkWQLnZbleG7MmLlGg1/uJWsoBCQbSAd1R2+2JMFgGbO+j3G2sR60Ds8ZFwQ7IVSOp7dAOb85LughCSkkTzSPiewTLohPw9LWUn1Cj6oi19C+de87VIM4abTwolUlSwsggm2zBnIMwZud5EKJAoYE4OQKnJX3cw==";
		private const string rsaxml = "<RSAKeyValue><Modulus>hEoYW3qOJ/MySeZKIWf0nkKswvkB6xdn7EQQvxkd9EfAQMAVxyBsR/Rch407RTbcYl70mcotYKGYkAg0vq1miGFGfp8N+tum7zhBNJwFOd9k4H8A8g+5CUu15BZTw11mr68PCCJMTTNAstH3vx3S4aRgty9IlztfpQVi/e57u7M=</Modulus><Exponent>EQ==</Exponent><P>rlhw/xADkDYRn1U07RXuVb4WLAZwIpTc93xVD/Evz6o6/y/nEdzRa49ob9taKPsNKYqU9Bbec5+mt+xzIbgggw==</P><Q>wj8/FXZUvScP+EUuv4FeQxZVVwl4dduKZyQHWyM53K1y7Yv6xc821ie6Z7lWGSeS9Z7VYJtX4F/gGuZHWW2xEQ==</Q><DP>hVKwwww+9c7+asiwAJhM2Cfy1l9GsQhsvUD1wOWdCDbh0jOws06CBvUxvvMIte0oLtNi2MYxo7ZSUGmFN+cn6w==</DP><DQ>fbB0HPI21L7sNzvS9GLEhcMoKUJdAPd3rCZfHNqO2hXg9A9H6Wf2TlXxFfBk4xmbU6MCic3tkS76a8IuKs6BoQ==</DQ><InverseQ>b8VOOfI/6Wvx/ZE7NrmS/NSawO7wAZviBmoNwyO40si3N8bDJQgsCl1Zbz93mBUEiKG+BF8tf04Y85NDBilmKw==</InverseQ><D>TdE7gRrqNZ4dlOHRQNO9EczeGFY9XRzTuCgJ2dKKFzlD6dpnKdbWSHGf10QEv01ylDfbLUnAVvWlCW49JN54i5yjEBVAuflYlxM081+qExpzF6NCfgnufV5I5mS72mgm8fnWjv64oQaNJ4Q6RPmy+V1r0trvC9Nlt2kJkRarAjE=</D></RSAKeyValue>";

		private RSAManaged rsa;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			rsa = new RSAManaged ();
			rsa.FromXmlString (rsaxml);
		}

		[Test]
		public void ShortResults ()
		{
			byte[] decdata = PKCS1.Decrypt_v15 (rsa, Convert.FromBase64String (short017));
			Assert.AreEqual ("<password>", Encoding.UTF8.GetString (decdata), "017");

			decdata = PKCS1.Decrypt_v15 (rsa, Convert.FromBase64String (short063));
			Assert.AreEqual ("<password>", Encoding.UTF8.GetString (decdata), "063");

			decdata = PKCS1.Decrypt_v15 (rsa, Convert.FromBase64String (short136));
			Assert.AreEqual ("<password>", Encoding.UTF8.GetString (decdata), "136");

			decdata = PKCS1.Decrypt_v15 (rsa, Convert.FromBase64String (short264));
			Assert.AreEqual ("<password>", Encoding.UTF8.GetString (decdata), "264");

			decdata = PKCS1.Decrypt_v15 (rsa, Convert.FromBase64String (short482));
			Assert.AreEqual ("<password>", Encoding.UTF8.GetString (decdata), "482");

			decdata = PKCS1.Decrypt_v15 (rsa, Convert.FromBase64String (short631));
			Assert.AreEqual ("<password>", Encoding.UTF8.GetString (decdata), "631");

			decdata = PKCS1.Decrypt_v15 (rsa, Convert.FromBase64String (short634));
			Assert.AreEqual ("<password>", Encoding.UTF8.GetString (decdata), "634");

			decdata = PKCS1.Decrypt_v15 (rsa, Convert.FromBase64String (short890));
			Assert.AreEqual ("<password>", Encoding.UTF8.GetString (decdata), "890");
		}

		private byte[] GetValue (int size)
		{
			byte[] data = new byte [size];
			for (int i = 0; i < size; i++)
				data[i] = (byte) (i + 1);
			return data;
		}

		[Test]
		public void PKCS15_SignAndVerify_UnknownHash ()
		{
			// this hash algorithm isn't known from CryptoConfig so no OID is available
			MD5SHA1 hash = new MD5SHA1 ();
			byte[] value = GetValue (hash.HashSize >> 3);
			byte[] unknown = PKCS1.Sign_v15 (rsa, hash, value);
			Assert.IsTrue (PKCS1.Verify_v15 (rsa, hash, value, unknown), "Verify");
		}
	}
}
