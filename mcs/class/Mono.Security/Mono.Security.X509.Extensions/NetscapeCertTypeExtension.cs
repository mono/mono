//
// NetscapeCertTypeExtension.cs: Handles Netscape CertType extensions.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
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

	public class NetscapeCertTypeExtension : X509Extension {

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
		public enum CertType {
			SslClient = 0x80,
			SslServer = 0x40,
			Smime = 0x20,
			ObjectSigning = 0x10,
			SslCa = 0x04,
			SmimeCa = 0x02,
			ObjectSigningCA = 0x01
		}

		private int ctbits;

		public NetscapeCertTypeExtension () : base () 
		{
			extnOid = "2.16.840.1.113730.1.1";
		}

		public NetscapeCertTypeExtension (ASN1 asn1) : base (asn1) {}

		public NetscapeCertTypeExtension (X509Extension extension) : base (extension) {}

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

		public bool Support (CertType usage) 
		{
			int x = Convert.ToInt32 (usage);
			return ((x & ctbits) == x);
		}

		public override string ToString () 
		{
			const string separator = " , ";
			StringBuilder sb = new StringBuilder ();
			if (Support (CertType.SslClient))
				sb.Append ("SSL Client Authentication");
			if (Support (CertType.SslServer)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("SSL Server Authentication");
			}
			if (Support (CertType.Smime)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("SMIME");
			}
			if (Support (CertType.ObjectSigning)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("Object Signing");
			}
			if (Support (CertType.SslCa)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("SSL CA");
			}
			if (Support (CertType.SmimeCa)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("SMIME CA");
			}
			if (Support (CertType.ObjectSigningCA)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("Object Signing CA");
			}
			sb.Append ("(");
			sb.Append (ctbits.ToString ("X2"));
			sb.Append (")");
			sb.Append (Environment.NewLine);
			return sb.ToString ();
		}
	}
}
