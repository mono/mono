//
// System.Security.Cryptography.CngKeyBlobFormat
//
// Authors:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Juho Vähä-Herttua
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
	public sealed class CngKeyBlobFormat : IEquatable<CngKeyBlobFormat> {

		private string m_format;

		public CngKeyBlobFormat (string format)
		{
			if (format == null)
				throw new ArgumentNullException ("format");
			if (format.Length == 0)
				throw new ArgumentException ("format");

			m_format = format;
		}

		public string Format {
			get { return m_format; }
		}

		public bool Equals (CngKeyBlobFormat other)
		{
			if (other == null)
				return false;
			return m_format == other.m_format;
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as CngKeyBlobFormat);
		}

		public override int GetHashCode ()
		{
			return m_format.GetHashCode ();
		}

		public override string ToString ()
		{
			return m_format;
		}

		// static

		private static CngKeyBlobFormat opaqueTransportBlob;
		private static CngKeyBlobFormat genericPrivateBlob;
		private static CngKeyBlobFormat genericPublicBlob;
		private static CngKeyBlobFormat eccPrivateBlob;
		private static CngKeyBlobFormat eccPublicBlob;
		private static CngKeyBlobFormat pkcs8PrivateBlob;

		public static CngKeyBlobFormat OpaqueTransportBlob {
			get {
				if (opaqueTransportBlob == null)
					opaqueTransportBlob = new CngKeyBlobFormat ("OpaqueTransport");
				return opaqueTransportBlob;
			}
		}

		public static CngKeyBlobFormat GenericPrivateBlob {
			get {
				if (genericPrivateBlob == null)
					genericPrivateBlob = new CngKeyBlobFormat ("PRIVATEBLOB");
				return genericPrivateBlob;
			}
		}

		public static CngKeyBlobFormat GenericPublicBlob {
			get {
				if (genericPublicBlob == null)
					genericPublicBlob = new CngKeyBlobFormat ("PUBLICBLOB");
				return genericPublicBlob;
			}
		}

		public static CngKeyBlobFormat EccPrivateBlob {
			get {
				if (eccPrivateBlob == null)
					eccPrivateBlob = new CngKeyBlobFormat ("ECCPRIVATEBLOB");
				return eccPrivateBlob;
			}
		}

		public static CngKeyBlobFormat EccPublicBlob {
			get {
				if (eccPublicBlob == null)
					eccPublicBlob = new CngKeyBlobFormat ("ECCPUBLICBLOB");
				return eccPublicBlob;
			}
		}

		public static CngKeyBlobFormat Pkcs8PrivateBlob {
			get {
				if (pkcs8PrivateBlob == null)
					pkcs8PrivateBlob = new CngKeyBlobFormat ("PKCS8_PRIVATEKEY");
				return pkcs8PrivateBlob;
			}
		}

		public static bool operator == (CngKeyBlobFormat left, CngKeyBlobFormat right)
		{
			if ((object)left == null)
				return ((object)right == null);
			return left.Equals (right);
		}

		public static bool operator != (CngKeyBlobFormat left, CngKeyBlobFormat right)
		{
			if ((object)left == null)
				return ((object)right != null);
			return !left.Equals (right);
		}
	}
}
