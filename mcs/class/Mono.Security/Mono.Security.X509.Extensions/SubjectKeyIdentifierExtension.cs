//
// SubjectKeyIdentifierExtension.cs: Handles X.509 SubjectKeyIdentifier extensions.
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
	 * id-ce-subjectKeyIdentifier OBJECT IDENTIFIER ::=  { id-ce 14 }
	 * 
	 * SubjectKeyIdentifier ::= KeyIdentifier
	 * 
	 * KeyIdentifier ::= OCTET STRING
	 */

	public class SubjectKeyIdentifierExtension : X509Extension {

		private byte[] ski;

		public SubjectKeyIdentifierExtension () : base () 
		{
			extnOid = "2.5.29.14";
		}

		public SubjectKeyIdentifierExtension (ASN1 asn1) : base (asn1) {}

		public SubjectKeyIdentifierExtension (X509Extension extension) : base (extension) {}

		protected override void Decode () 
		{
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x04)
				throw new ArgumentException ("Invalid SubjectKeyIdentifier extension");
			ski = sequence.Value;
		}

		public override string Name {
			get { return "Subject Key Identifier"; }
		}

		public override string ToString () 
		{
			if (ski == null)
				return null;

			StringBuilder sb = new StringBuilder ();
			int x = 0;
			while (x < ski.Length) {
				sb.Append (ski [x].ToString ("X2"));
				if (x % 2 == 1)
					sb.Append (" ");
				x++;
			}
			return sb.ToString ();
		}
	}
}
