//
// System.Security.Cryptography.Pkcs.Pkcs9MessageDigest class
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
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

#if NET_2_0

namespace System.Security.Cryptography.Pkcs {

	[MonoTODO ("missing internals")]
	public sealed class Pkcs9MessageDigest : Pkcs9AttributeObject {

		internal const string oid = "1.2.840.113549.1.9.4";
		internal const string friendlyName = "Message Digest";

		private byte[] _messageDigest;

		// constructors

		public Pkcs9MessageDigest () 
		{
			// Pkcs9Attribute remove the "set" accessor on Oid :-(
			(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
		}

		internal Pkcs9MessageDigest (byte[] messageDigest, bool encoded) 
		{
			if (messageDigest == null)
				throw new ArgumentNullException ("messageDigest");

			if (encoded) {
				(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
				RawData = messageDigest;
				Decode (messageDigest);
			} else {
				(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
				_messageDigest = (byte[]) _messageDigest.Clone ();
				RawData = Encode ();
			}
		}

		// properties

		public byte[] MessageDigest {
			get { return _messageDigest; }
		}

		// methods

		public override void CopyFrom (AsnEncodedData asnEncodedData)
		{
			base.CopyFrom (asnEncodedData);
			Decode (this.RawData);
		}

		// internal stuff

		internal void Decode (byte[] attribute)
		{
			// TODO
			_messageDigest = null;
		}

		internal byte[] Encode ()
		{
			// TODO
			return null;
		}
	}
}

#endif
