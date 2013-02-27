//
// Unit tests for System.Security.Cryptography.CngAlgorithm
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

#if !MOBILE

using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class CngAlgorithmTest {

		[Test]
		public void StaticProperties ()
		{
			Assert.IsNotNull (CngAlgorithm.ECDiffieHellmanP256, "ECDiffieHellmanP256");
			Assert.IsNotNull (CngAlgorithm.ECDiffieHellmanP384, "ECDiffieHellmanP384");
			Assert.IsNotNull (CngAlgorithm.ECDiffieHellmanP521, "ECDiffieHellmanP521");
			Assert.IsNotNull (CngAlgorithm.ECDsaP256, "ECDsaP256");
			Assert.IsNotNull (CngAlgorithm.ECDsaP384, "ECDsaP384");
			Assert.IsNotNull (CngAlgorithm.ECDsaP521, "ECDsaP521");
			Assert.IsNotNull (CngAlgorithm.MD5, "MD5");
			Assert.IsNotNull (CngAlgorithm.Sha1, "Sha1");
			Assert.IsNotNull (CngAlgorithm.Sha256, "Sha256");
			Assert.IsNotNull (CngAlgorithm.Sha384, "Sha384");
			Assert.IsNotNull (CngAlgorithm.Sha512, "Sha512");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNull ()
		{
			new CngAlgorithm (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorEmpty ()
		{
			new CngAlgorithm (String.Empty);
		}

		static CngAlgorithm mono = new CngAlgorithm ("mono");

		private void Check (CngAlgorithm algo)
		{
			Assert.AreEqual (algo.Algorithm, algo.ToString (), "Algorithm/ToString");
			Assert.AreEqual (algo.GetHashCode (), algo.Algorithm.GetHashCode (), "GetHashCode");
			Assert.IsTrue (algo.Equals (algo), "Equals(self)");
			Assert.IsTrue (algo.Equals ((object)algo), "Equals((object)self)");

			CngAlgorithm copy = new CngAlgorithm (algo.Algorithm);
			Assert.AreEqual (algo.GetHashCode (), copy.GetHashCode (), "Copy");
			Assert.IsTrue (algo.Equals (copy), "Equals(copy)");
			Assert.IsTrue (algo.Equals ((object)copy), "Equals((object)copy)");
			Assert.IsTrue (algo == copy, "algo==copy");
			Assert.IsFalse (algo != copy, "algo!=copy");

			Assert.IsFalse (algo.Equals (mono), "Equals(mono)");
			Assert.IsFalse (algo.Equals ((object)mono), "Equals((object)mono)");
			Assert.IsFalse (algo == mono, "algo==mono");
			Assert.IsTrue (algo != mono, "algo!=mono");
		}

		[Test]
		public void ConstructorCustom ()
		{
			CngAlgorithm algo = new CngAlgorithm ("custom");
			Check (algo);
			Assert.IsFalse (algo.Equals ((CngAlgorithm) null), "Equals((CngAlgorithm)null)");
			Assert.IsFalse (algo.Equals ((object) null), "Equals((object)null)");
		}

		[Test]
		public void ECDiffieHellmanP256 ()
		{
			CngAlgorithm algo = CngAlgorithm.ECDiffieHellmanP256;
			Assert.AreEqual ("ECDH_P256", algo.Algorithm, "Algorithm");
			Assert.IsTrue (algo.Equals (CngAlgorithm.ECDiffieHellmanP256), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (algo, CngAlgorithm.ECDiffieHellmanP256), "ReferenceEquals");
			Check (algo);
		}

		[Test]
		public void ECDiffieHellmanP384 ()
		{
			CngAlgorithm algo = CngAlgorithm.ECDiffieHellmanP384;
			Assert.AreEqual ("ECDH_P384", algo.Algorithm, "Algorithm");
			Assert.IsTrue (algo.Equals (CngAlgorithm.ECDiffieHellmanP384), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (algo, CngAlgorithm.ECDiffieHellmanP384), "ReferenceEquals");
			Check (algo);
		}

		[Test]
		public void ECDiffieHellmanP521 ()
		{
			CngAlgorithm algo = CngAlgorithm.ECDiffieHellmanP521;
			Assert.AreEqual ("ECDH_P521", algo.Algorithm, "Algorithm");
			Assert.IsTrue (algo.Equals (CngAlgorithm.ECDiffieHellmanP521), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (algo, CngAlgorithm.ECDiffieHellmanP521), "ReferenceEquals");
			Check (algo);
		}

		[Test]
		public void ECDsaP256 ()
		{
			CngAlgorithm algo = CngAlgorithm.ECDsaP256;
			Assert.AreEqual ("ECDSA_P256", algo.Algorithm, "Algorithm");
			Assert.IsTrue (algo.Equals (CngAlgorithm.ECDsaP256), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (algo, CngAlgorithm.ECDsaP256), "ReferenceEquals");
			Check (algo);
		}

		[Test]
		public void ECDsaP384 ()
		{
			CngAlgorithm algo = CngAlgorithm.ECDsaP384;
			Assert.AreEqual ("ECDSA_P384", algo.Algorithm, "Algorithm");
			Assert.IsTrue (algo.Equals (CngAlgorithm.ECDsaP384), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (algo, CngAlgorithm.ECDsaP384), "ReferenceEquals");
			Check (algo);
		}

		[Test]
		public void ECDsaP521 ()
		{
			CngAlgorithm algo = CngAlgorithm.ECDsaP521;
			Assert.AreEqual ("ECDSA_P521", algo.Algorithm, "Algorithm");
			Assert.IsTrue (algo.Equals (CngAlgorithm.ECDsaP521), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (algo, CngAlgorithm.ECDsaP521), "ReferenceEquals");
			Check (algo);
		}

		[Test]
		public void MD5 ()
		{
			CngAlgorithm algo = CngAlgorithm.MD5;
			Assert.AreEqual ("MD5", algo.Algorithm, "Algorithm");
			Assert.IsTrue (algo.Equals (CngAlgorithm.MD5), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (algo, CngAlgorithm.MD5), "ReferenceEquals");
			Check (algo);
		}

		[Test]
		public void Sha1 ()
		{
			CngAlgorithm algo = CngAlgorithm.Sha1;
			Assert.AreEqual ("SHA1", algo.Algorithm, "Algorithm");
			Assert.IsTrue (algo.Equals (CngAlgorithm.Sha1), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (algo, CngAlgorithm.Sha1), "ReferenceEquals");
			Check (algo);
		}

		[Test]
		public void Sha256 ()
		{
			CngAlgorithm algo = CngAlgorithm.Sha256;
			Assert.AreEqual ("SHA256", algo.Algorithm, "Algorithm");
			Assert.IsTrue (algo.Equals (CngAlgorithm.Sha256), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (algo, CngAlgorithm.Sha256), "ReferenceEquals");
			Check (algo);
		}

		[Test]
		public void Sha384 ()
		{
			CngAlgorithm algo = CngAlgorithm.Sha384;
			Assert.AreEqual ("SHA384", algo.Algorithm, "Algorithm");
			Assert.IsTrue (algo.Equals (CngAlgorithm.Sha384), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (algo, CngAlgorithm.Sha384), "ReferenceEquals");
			Check (algo);
		}

		[Test]
		public void Sha512 ()
		{
			CngAlgorithm algo = CngAlgorithm.Sha512;
			Assert.AreEqual ("SHA512", algo.Algorithm, "Algorithm");
			Assert.IsTrue (algo.Equals (CngAlgorithm.Sha512), "Equals(static)");
			Assert.IsTrue (Object.ReferenceEquals (algo, CngAlgorithm.Sha512), "ReferenceEquals");
			Check (algo);
		}
	}
}

#endif
