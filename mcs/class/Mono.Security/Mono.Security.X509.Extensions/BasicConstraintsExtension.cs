//
// BasicConstraintsExtension.cs: Handles X.509 BasicConstrains extensions.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

namespace Mono.Security.X509.Extensions {

	// References:
	// 1.	RFC 3280: Internet X.509 Public Key Infrastructure, Section 4.2.1.10
	//	http://www.ietf.org/rfc/rfc3280.txt

	/* id-ce-basicConstraints OBJECT IDENTIFIER ::=  { id-ce 19 }
	 * 
	 * BasicConstraints ::= SEQUENCE {
	 * 	cA                      BOOLEAN DEFAULT FALSE,
	 * 	pathLenConstraint       INTEGER (0..MAX) OPTIONAL 
	 * }
	 */
	public class BasicConstraintsExtension : X509Extension {

		private bool cA;
		private int pathLenConstraint;

		public BasicConstraintsExtension () : base () 
		{
			extnOid = "2.5.29.19";
		}

		public BasicConstraintsExtension (ASN1 asn1) : base (asn1) {}

		public BasicConstraintsExtension (X509Extension extension) : base (extension) {}

		protected override void Decode () 
		{
			// default values
			cA = false;
			pathLenConstraint = 0; // no constraint

			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x30)
				throw new ArgumentException ("Invalid BasicConstraints extension");
			int n = 0;
			ASN1 a = sequence [n++];
			if ((a != null) && (a.Tag == 0x01)) {
				cA = (a.Value [0] == 0xFF);
				a = sequence [n++];
			}
			if ((a != null) && (a.Tag == 0x02))
				pathLenConstraint = ASN1Convert.ToInt32 (a);
		}

		protected override void Encode () 
		{
			if (extnValue == null) {
				extnValue = new ASN1 (0x30);
				if (cA)
					extnValue.Add (new ASN1 (0x01, new byte[] { 0xFF }));
				if (pathLenConstraint > 0)
					extnValue.Add (ASN1Convert.FromInt32 (pathLenConstraint));
			}
		}

		public bool CertificateAuthority {
			get { return cA; }
			set { cA = value; }
		}

		public override string Name {
			get { return "Basic Constraints"; }
		}

		public int PathLenConstraint {
			get { return pathLenConstraint; }
			set { pathLenConstraint = value; }
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Subject Type=");
			sb.Append ((cA) ? "CA" : "End Entity");
			sb.Append (Environment.NewLine);
			sb.Append ("Path Length Constraint=");
			if (pathLenConstraint == 0)
				sb.Append ("None");
			else
				sb.Append (pathLenConstraint.ToString ());
			sb.Append (Environment.NewLine);
			return sb.ToString ();
		}
	}
}
