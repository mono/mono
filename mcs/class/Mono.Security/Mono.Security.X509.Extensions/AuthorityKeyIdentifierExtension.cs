//
// AuthorityKeyIdentifierExtension.cs: Handles X.509 AuthorityKeyIdentifier extensions.
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

	/*
	 * id-ce-authorityKeyIdentifier OBJECT IDENTIFIER ::=  { id-ce 35 }
	 * 
	 * AuthorityKeyIdentifier ::= SEQUENCE {
	 *    keyIdentifier             [0] KeyIdentifier           OPTIONAL,
	 *    authorityCertIssuer       [1] GeneralNames            OPTIONAL,
	 *    authorityCertSerialNumber [2] CertificateSerialNumber OPTIONAL  }
	 * 
	 * KeyIdentifier ::= OCTET STRING
	 */

	public class AuthorityKeyIdentifierExtension : X509Extension {

		private byte[] aki;

		public AuthorityKeyIdentifierExtension () : base () 
		{
			extnOid = "2.5.29.35";
		}

		public AuthorityKeyIdentifierExtension (ASN1 asn1) : base (asn1) {}

		public AuthorityKeyIdentifierExtension (X509Extension extension) : base (extension) {}

		protected override void Decode () 
		{
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x30)
				throw new ArgumentException ("Invalid AuthorityKeyIdentifier extension");
			for (int i=0; i < sequence.Count; i++) {
				ASN1 el = sequence [i];
				switch (el.Tag) {
					case 0x80:
						aki = el.Value;
						break;
					case 0x81:
					case 0x82:
					default:
						throw new ArgumentException ("Invalid AuthorityKeyIdentifier extension");
				}
			}
		}

		public override string Name {
			get { return "Authority Key Identifier"; }
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			if (aki != null) {
				// [0] KeyIdentifier
				int x = 0;
				sb.Append ("KeyID=");
				while (x < aki.Length) {
					sb.Append (aki [x].ToString ("X2"));
					if (x % 2 == 1)
						sb.Append (" ");
					x++;
				}
				// [1] GeneralNames
				// TODO
				// [2] CertificateSerialNumber
				// TODO
			}
			return sb.ToString ();
		}
	}
}
