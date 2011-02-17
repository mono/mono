//
// System.Security.Cryptography.CngAlgorithm
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

using System;

namespace System.Security.Cryptography {

	// note: CNG stands for "Cryptography API: Next Generation"

	[Serializable]
	public sealed class CngAlgorithm : IEquatable<CngAlgorithm> {

		private string m_algorithm;

		public CngAlgorithm (string algorithm)
		{
			if (algorithm == null)
				throw new ArgumentNullException ("algorithm");
			if (algorithm.Length == 0)
				throw new ArgumentException ("algorithm");

			m_algorithm = algorithm;
		}

		public string Algorithm {
			get { return m_algorithm; }
		}

		public bool Equals (CngAlgorithm other)
		{
			if (other == null)
				return false;
			return m_algorithm == other.m_algorithm;
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as CngAlgorithm);
		}

		public override int GetHashCode ()
		{
			return m_algorithm.GetHashCode ();
		}

		public override string ToString ()
		{
			return m_algorithm;
		}

		// static

		static CngAlgorithm dh256;
		static CngAlgorithm dh384;
		static CngAlgorithm dh521;
		static CngAlgorithm dsa256;
		static CngAlgorithm dsa384;
		static CngAlgorithm dsa521;
		static CngAlgorithm md5;
		static CngAlgorithm sha1;
		static CngAlgorithm sha256;
		static CngAlgorithm sha384;
		static CngAlgorithm sha512;

		public static CngAlgorithm ECDiffieHellmanP256 {
			get {
				if (dh256 == null)
					dh256 = new CngAlgorithm ("ECDH_P256");
				return dh256;
			}
		}

		public static CngAlgorithm ECDiffieHellmanP384 {
			get {
				if (dh384 == null)
					dh384 = new CngAlgorithm ("ECDH_P384");
				return dh384;
			}
		}

		public static CngAlgorithm ECDiffieHellmanP521 {
			get {
				if (dh521 == null)
					dh521 = new CngAlgorithm ("ECDH_P521");
				return dh521;
			}
		}

		public static CngAlgorithm ECDsaP256 {
			get {
				if (dsa256 == null)
					dsa256 = new CngAlgorithm ("ECDSA_P256");
				return dsa256;
			}
		}

		public static CngAlgorithm ECDsaP384 {
			get {
				if (dsa384 == null)
					dsa384 = new CngAlgorithm ("ECDSA_P384");
				return dsa384;
			}
		}

		public static CngAlgorithm ECDsaP521 {
			get {
				if (dsa521 == null)
					dsa521 = new CngAlgorithm ("ECDSA_P521");
				return dsa521;
			}
		}

		public static CngAlgorithm MD5 {
			get {
				if (md5 == null)
					md5 = new CngAlgorithm ("MD5");
				return md5;
			}
		}

		public static CngAlgorithm Sha1 {
			get {
				if (sha1 == null)
					sha1 = new CngAlgorithm ("SHA1");
				return sha1;
			}
		}

		public static CngAlgorithm Sha256 {
			get {
				if (sha256 == null)
					sha256 = new CngAlgorithm ("SHA256");
				return sha256;
			}
		}

		public static CngAlgorithm Sha384 {
			get {
				if (sha384 == null)
					sha384 = new CngAlgorithm ("SHA384");
				return sha384;
			}
		}

		public static CngAlgorithm Sha512 {
			get {
				if (sha512 == null)
					sha512 = new CngAlgorithm ("SHA512");
				return sha512;
			}
		}

		public static bool operator == (CngAlgorithm left, CngAlgorithm right)
		{
			if ((object)left == null)
				return ((object)right == null);
			return left.Equals (right);
		}

		public static bool operator != (CngAlgorithm left, CngAlgorithm right)
		{
			if ((object)left == null)
				return ((object)right != null);
			return !left.Equals (right);
		}
	}
}
