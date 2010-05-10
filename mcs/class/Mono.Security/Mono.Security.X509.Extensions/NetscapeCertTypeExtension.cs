//
// NetscapeCertTypeExtension.cs: Handles Netscape CertType extensions.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

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
using System.Globalization;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

namespace Mono.Security.X509.Extensions {

	// References:
	// a.	Netscape Certificate Extensions Navigator 3.0 Version
	//	http://wp.netscape.com/eng/security/cert-exts.html
	// b.	Netscape Certificate Extensions	Communicator 4.0 Version
	//	http://wp.netscape.com/eng/security/comm4-cert-exts.html
	// c.	2.16.840.1.113730.1.1 - Netscape certificate type
	//	http://www.alvestrand.no/objectid/2.16.840.1.113730.1.1.html

#if MOONLIGHT
	internal
#else
	public 
#endif
	class NetscapeCertTypeExtension : X509Extension {

		/*
		 * bit-0 SSL client - this cert is certified for SSL client authentication use 
		 * bit-1 SSL server - this cert is certified for SSL server authentication use 
		 * bit-2 S/MIME - this cert is certified for use by clients(New in PR3) 
		 * bit-3 Object Signing - this cert is certified for signing objects such as Java applets and plugins(New in PR3) 
		 * bit-4 Reserved - this bit is reserved for future use 
		 * bit-5 SSL CA - this cert is certified for issuing certs for SSL use 
		 * bit-6 S/MIME CA - this cert is certified for issuing certs for S/MIME use(New in PR3) 
		 * bit-7 Object Signing CA - this cert is certified for issuing certs for Object Signing(New in PR3) 
		 */

		// note: because nothing is simple in ASN.1 bits are reversed
		[Flags]
		public enum CertTypes {
			SslClient = 0x80,
			SslServer = 0x40,
			Smime = 0x20,
			ObjectSigning = 0x10,
			SslCA = 0x04,
			SmimeCA = 0x02,
			ObjectSigningCA = 0x01
		}

		private int ctbits;

		public NetscapeCertTypeExtension () : base () 
		{
			extnOid = "2.16.840.1.113730.1.1";
		}

		public NetscapeCertTypeExtension (ASN1 asn1) : base (asn1) 
		{
		}

		public NetscapeCertTypeExtension (X509Extension extension) : base (extension)
		{
		}

		protected override void Decode () 
		{
			ASN1 bitString = new ASN1 (extnValue.Value);
			if (bitString.Tag != 0x03)
				throw new ArgumentException ("Invalid NetscapeCertType extension");
			int i = 1; // byte zero has the number of unused bits (ASN1's BITSTRING)
			while (i < bitString.Value.Length)
				ctbits = (ctbits << 8) + bitString.Value [i++];
		}

		public override string Name {
			get { return "NetscapeCertType"; }
		}

/*		public CertType Type {
			get { return ctbits; }
			set { ctbits = value; }
		}*/

		public bool Support (CertTypes usage) 
		{
			int x = Convert.ToInt32 (usage, CultureInfo.InvariantCulture);
			return ((x & ctbits) == x);
		}

		public override string ToString () 
		{
			const string separator = " , ";
			StringBuilder sb = new StringBuilder ();
			if (Support (CertTypes.SslClient))
				sb.Append ("SSL Client Authentication");
			if (Support (CertTypes.SslServer)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("SSL Server Authentication");
			}
			if (Support (CertTypes.Smime)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("SMIME");
			}
			if (Support (CertTypes.ObjectSigning)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("Object Signing");
			}
			if (Support (CertTypes.SslCA)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("SSL CA");
			}
			if (Support (CertTypes.SmimeCA)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("SMIME CA");
			}
			if (Support (CertTypes.ObjectSigningCA)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("Object Signing CA");
			}
			sb.Append ("(");
			sb.Append (ctbits.ToString ("X2", CultureInfo.InvariantCulture));
			sb.Append (")");
			sb.Append (Environment.NewLine);
			return sb.ToString ();
		}
	}
}
