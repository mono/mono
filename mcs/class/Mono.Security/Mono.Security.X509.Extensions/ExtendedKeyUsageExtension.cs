//
// ExtendedKeyUsageExtension.cs: Handles X.509 ExtendedKeyUsage extensions.
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
	 * id-ce-extKeyUsage OBJECT IDENTIFIER ::= { id-ce 37 }
	 * 
	 * ExtKeyUsageSyntax ::= SEQUENCE SIZE (1..MAX) OF KeyPurposeId
	 * 
	 * KeyPurposeId ::= OBJECT IDENTIFIER
	 */

	public class ExtendedKeyUsageExtension : X509Extension {

		private ArrayList keyPurpose;

		public ExtendedKeyUsageExtension () : base () 
		{
			extnOid = "2.5.29.37";
			keyPurpose = new ArrayList ();
		}

		public ExtendedKeyUsageExtension (ASN1 asn1) : base (asn1) {}

		public ExtendedKeyUsageExtension (X509Extension extension) : base (extension) {}

		protected override void Decode () 
		{
			keyPurpose = new ArrayList ();
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x30)
				throw new ArgumentException ("Invalid ExtendedKeyUsage extension");
			// for every policy OID
			for (int i=0; i < sequence.Count; i++)
				keyPurpose.Add (ASN1Convert.ToOID (sequence [i]));
		}

		protected override void Encode () 
		{
			if (extnValue == null) {
				extnValue = new ASN1 (0x30);
				foreach (string oid in keyPurpose) {
					extnValue.Add (ASN1Convert.FromOID (oid));
				}
			}
		}

		public ArrayList KeyPurpose {
			get { return keyPurpose; }
		}

		public override string Name {
			get { return "Extended Key Usage"; }
		}

		// serverAuth		1.3.6.1.5.5.7.3.1
		// clientAuth		1.3.6.1.5.5.7.3.2
		// codeSigning		1.3.6.1.5.5.7.3.3
		// emailProtection	1.3.6.1.5.5.7.3.4
		// timeStamping		1.3.6.1.5.5.7.3.8
		// OCSPSigning		1.3.6.1.5.5.7.3.9
		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			foreach (string s in keyPurpose) {
				switch (s) {
					case "1.3.6.1.5.5.7.3.3":
						sb.Append ("Code Signing");
						break;
					default:
						sb.Append ("unknown");
						break;
				}
				sb.Append (" (");
				sb.Append (s);
				sb.Append (")");
				sb.Append (Environment.NewLine);
			}
			return sb.ToString ();
		}
	}
}
