//
// System.Security.Cryptography.CngAlgorithmGroup
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
	public sealed class CngAlgorithmGroup : IEquatable<CngAlgorithmGroup> {

		private string m_algorithmGroup;

		public CngAlgorithmGroup (string algorithmGroup)
		{
			if (algorithmGroup == null)
				throw new ArgumentNullException ("algorithmGroup");
			if (algorithmGroup.Length == 0)
				throw new ArgumentException ("algorithmGroup");

			m_algorithmGroup = algorithmGroup;
		}

		public string AlgorithmGroup {
			get { return m_algorithmGroup; }
		}

		public bool Equals (CngAlgorithmGroup other)
		{
			if (other == null)
				return false;
			return m_algorithmGroup == other.m_algorithmGroup;
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as CngAlgorithmGroup);
		}

		public override int GetHashCode ()
		{
			return m_algorithmGroup.GetHashCode ();
		}

		public override string ToString ()
		{
			return m_algorithmGroup;
		}

		// static

		private static CngAlgorithmGroup dh;
		private static CngAlgorithmGroup dsa;
		private static CngAlgorithmGroup ecdh;
		private static CngAlgorithmGroup ecdsa;
		private static CngAlgorithmGroup rsa;

		public static CngAlgorithmGroup DiffieHellman {
			get {
				if (dh == null)
					dh = new CngAlgorithmGroup ("DH");
				return dh;
			}
		}

		public static CngAlgorithmGroup Dsa {
			get {
				if (dsa == null)
					dsa = new CngAlgorithmGroup ("DSA");
				return dsa;
			}
		}

		public static CngAlgorithmGroup ECDiffieHellman {
			get {
				if (ecdh == null)
					ecdh = new CngAlgorithmGroup ("ECDH");
				return ecdh;
			}
		}

		public static CngAlgorithmGroup ECDsa {
			get {
				if (ecdsa == null)
					ecdsa = new CngAlgorithmGroup ("ECDSA");
				return ecdsa;
			}
		}

		public static CngAlgorithmGroup Rsa {
			get {
				if (rsa == null)
					rsa = new CngAlgorithmGroup ("RSA");
				return rsa;
			}
		}

		public static bool operator == (CngAlgorithmGroup left, CngAlgorithmGroup right)
		{
			if ((object)left == null)
				return ((object)right == null);
			return left.Equals (right);
		}

		public static bool operator != (CngAlgorithmGroup left, CngAlgorithmGroup right)
		{
			if ((object)left == null)
				return ((object)right != null);
			return !left.Equals (right);
		}
	}
}
