//
// SubjectAltNameExtension.cs: Handles X.509 SubjectAltName extensions.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

namespace Mono.Security.X509.Extensions {

	/*
	 * id-ce-subjectAltName OBJECT IDENTIFIER ::=  { id-ce 17 }
	 * 
	 * SubjectAltName ::= GeneralNames
	 * 
	 * GeneralNames ::= SEQUENCE SIZE (1..MAX) OF GeneralName
	 * 
	 * GeneralName ::= CHOICE {
	 *    otherName                       [0]     OtherName,
	 *    rfc822Name                      [1]     IA5String,
	 *    dNSName                         [2]     IA5String,
	 *    x400Address                     [3]     ORAddress,
	 *    directoryName                   [4]     Name,
	 *    ediPartyName                    [5]     EDIPartyName,
	 *    uniformResourceIdentifier       [6]     IA5String,
	 *    iPAddress                       [7]     OCTET STRING,
	 *    registeredID                    [8]     OBJECT IDENTIFIER 
	 * }
	 * 
	 * OtherName ::= SEQUENCE {
	 *    type-id    OBJECT IDENTIFIER,
	 *    value      [0] EXPLICIT ANY DEFINED BY type-id 
	 * }
	 * 
	 * EDIPartyName ::= SEQUENCE {
	 *    nameAssigner            [0]     DirectoryString OPTIONAL,
	 *    partyName               [1]     DirectoryString 
	 * }
	 */

	// TODO - incomplete (only rfc822Name is supported)
	public class SubjectAltNameExtension : X509Extension {
		
		private ArrayList rfc822Name;

		public SubjectAltNameExtension () : base () 
		{
			extnOid = "2.5.29.17";
		}

		public SubjectAltNameExtension (ASN1 asn1) : base (asn1) {}

		public SubjectAltNameExtension (X509Extension extension) : base (extension) {}

		protected override void Decode () 
		{
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x30)
				throw new ArgumentException ("Invalid SubjectAltName extension");
			for (int i=0; i < sequence.Count; i++) {
				switch (sequence [i].Tag) {
					case 0x81: // rfc822Name	[1]	IA5String
						if (rfc822Name == null)
							rfc822Name = new ArrayList ();
						rfc822Name.Add (Encoding.ASCII.GetString (sequence [i].Value));
						break;
					default:
						break;
				}
			}
		}

		public override string Name {
			get { return "Subject Alternative Name"; }
		}

		public string[] RFC822 {
			get {
				string[] names = new string [rfc822Name.Count];
				for (int i=0; i < rfc822Name.Count; i++)
					names [i] = (string) rfc822Name [i];
				return names;
			}
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			if (rfc822Name != null) {
				foreach (string s in rfc822Name) {
					sb.Append ("RFC822 Name=");
					sb.Append (s);
					sb.Append (Environment.NewLine);
				}
			}
			return sb.ToString ();
		}
	}
}
