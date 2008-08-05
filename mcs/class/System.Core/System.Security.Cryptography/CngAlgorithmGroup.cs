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

		private string group;

		public CngAlgorithmGroup (string algorithmGroup)
		{
			if (algorithmGroup == null)
				throw new ArgumentNullException ("algorithmGroup");
			if (algorithmGroup.Length == 0)
				throw new ArgumentException ("algorithmGroup");

			group = algorithmGroup;
		}

		public string AlgorithmGroup {
			get { return group; }
		}

		public bool Equals (CngAlgorithmGroup other)
		{
			return this == other;
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as CngAlgorithmGroup);
		}

		public override int GetHashCode ()
		{
			return group.GetHashCode ();
		}

		public override string ToString ()
		{
			return group;
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
			if ((object)right == null)
				return false;
			return left.group == right.group;
		}

		public static bool operator != (CngAlgorithmGroup left, CngAlgorithmGroup right)
		{
			if ((object)left == null)
				return ((object)right != null);
			if ((object)right == null)
				return true;
			return left.group != right.group;
		}
	}
}
