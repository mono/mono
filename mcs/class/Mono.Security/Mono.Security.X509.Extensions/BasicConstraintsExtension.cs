//
// BasicConstraintsExtension.cs: Handles X.509 BasicConstrains extensions.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
	// 1.	RFC 3280: Internet X.509 Public Key Infrastructure, Section 4.2.1.10
	//	http://www.ietf.org/rfc/rfc3280.txt

	/* id-ce-basicConstraints OBJECT IDENTIFIER ::=  { id-ce 19 }
	 * 
	 * BasicConstraints ::= SEQUENCE {
	 * 	cA                      BOOLEAN DEFAULT FALSE,
	 * 	pathLenConstraint       INTEGER (0..MAX) OPTIONAL 
	 * }
	 */
#if INSIDE_CORLIB || INSIDE_SYSTEM
	internal
#else
	public 
#endif
	class BasicConstraintsExtension : X509Extension {

		public const int NoPathLengthConstraint = -1;

		private bool cA;
		private int pathLenConstraint;

		public BasicConstraintsExtension () : base () 
		{
			extnOid = "2.5.29.19";
			pathLenConstraint = NoPathLengthConstraint;
		}

		public BasicConstraintsExtension (ASN1 asn1) : base (asn1) {}

		public BasicConstraintsExtension (X509Extension extension) : base (extension) {}

		protected override void Decode () 
		{
			// default values
			cA = false;
			pathLenConstraint = NoPathLengthConstraint;

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
			ASN1 seq = new ASN1 (0x30);
			if (cA)
				seq.Add (new ASN1 (0x01, new byte[] { 0xFF }));
			// CAs MUST NOT include the pathLenConstraint field unless the cA boolean is asserted
			if (cA && (pathLenConstraint >= 0))
				seq.Add (ASN1Convert.FromInt32 (pathLenConstraint));

			extnValue = new ASN1 (0x04);
			extnValue.Add (seq);
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
			set {
				if (value < NoPathLengthConstraint) {
					string msg = Locale.GetText ("PathLenConstraint must be positive or -1 for none ({0}).", value);
					throw new ArgumentOutOfRangeException (msg);
				}
				pathLenConstraint = value;
			}
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Subject Type=");
			sb.Append ((cA) ? "CA" : "End Entity");
			sb.Append (Environment.NewLine);
			sb.Append ("Path Length Constraint=");
			if (pathLenConstraint == NoPathLengthConstraint)
				sb.Append ("None");
			else
				sb.Append (pathLenConstraint.ToString (CultureInfo.InvariantCulture));
			sb.Append (Environment.NewLine);
			return sb.ToString ();
		}
	}
}
