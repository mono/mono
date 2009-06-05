//
// System.Security.Cryptography.Pkcs.Pkcs9SigningTime class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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

#if NET_2_0 && SECURITY_DEP

using System.Globalization;
using System.Text;

using Mono.Security;

namespace System.Security.Cryptography.Pkcs {

	public sealed class Pkcs9SigningTime : Pkcs9AttributeObject {

		internal const string oid = "1.2.840.113549.1.9.5";
		internal const string friendlyName = "Signing Time";

		private DateTime _signingTime;

		public Pkcs9SigningTime () 
		{
			// Pkcs9Attribute remove the "set" accessor on Oid :-(
			(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
			_signingTime = DateTime.Now;
			RawData = Encode ();
		}

		public Pkcs9SigningTime (DateTime signingTime)
		{
			(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
			_signingTime = signingTime;
			RawData = Encode ();
		}

		public Pkcs9SigningTime (byte[] encodedSigningTime)
		{
			if (encodedSigningTime == null)
				throw new ArgumentNullException ("encodedSigningTime");

			(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
			RawData = encodedSigningTime;
			Decode (encodedSigningTime);
		}

		public DateTime SigningTime {
			get { return _signingTime; }
		}

		public override void CopyFrom (AsnEncodedData asnEncodedData)
		{
			if (asnEncodedData == null)
				throw new ArgumentNullException ("asnEncodedData");

			Decode (asnEncodedData.RawData);
			Oid = asnEncodedData.Oid;
			RawData = asnEncodedData.RawData;
		}

		// internal stuff

		internal void Decode (byte[] attribute)
		{
			// Only UTCTIME is supported by FX 2.0
			if (attribute [0] != 0x17)
				throw new CryptographicException (Locale.GetText ("Only UTCTIME is supported."));

			ASN1 attr = new ASN1 (attribute);
			byte[] value = attr.Value;
			string date = Encoding.ASCII.GetString (value, 0, value.Length - 1);
			_signingTime = DateTime.ParseExact (date, "yyMMddHHmmss", null);
		}

		internal byte[] Encode ()
		{
			if (_signingTime.Year <= 1600)
				throw new ArgumentOutOfRangeException ("<= 1600");
			// Only UTCTIME is supported by FX 2.0
			if ((_signingTime.Year < 1950) || (_signingTime.Year >= 2050))
				throw new CryptographicException ("[1950,2049]");

			string date = _signingTime.ToString ("yyMMddHHmmss", CultureInfo.InvariantCulture) + "Z";
			ASN1 attr = new ASN1 (0x17, Encoding.ASCII.GetBytes (date));
			return attr.GetBytes ();
		}
	}
}

#endif
