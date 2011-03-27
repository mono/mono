//
// CRLDistributionPointsExtension.cs: Handles X.509 CRLDistributionPoints extensions.
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
using System.Collections.Generic;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

namespace Mono.Security.X509.Extensions {

	// References:
	// a.	Internet X.509 Public Key Infrastructure Certificate and Certificate Revocation List (CRL) Profile
	//	http://www.ietf.org/rfc/rfc3280.txt
	// b.	2.5.29.31 - CRL Distribution Points
	//	http://www.alvestrand.no/objectid/2.5.29.31.html

	/*
	 * id-ce-cRLDistributionPoints OBJECT IDENTIFIER ::=  { id-ce 31 }
	 * 
	 * CRLDistributionPoints ::= SEQUENCE SIZE (1..MAX) OF DistributionPoint
	 * 
	 * DistributionPoint ::= SEQUENCE {
	 *    distributionPoint       [0]     DistributionPointName OPTIONAL,
	 *    reasons                 [1]     ReasonFlags OPTIONAL,
	 *    cRLIssuer               [2]     GeneralNames OPTIONAL 
	 * }
	 * 
	 * DistributionPointName ::= CHOICE {
	 *    fullName                [0]     GeneralNames,
	 *    nameRelativeToCRLIssuer [1]     RelativeDistinguishedName 
	 * }
	 * 
	 * ReasonFlags ::= BIT STRING {
	 *    unused                  (0),
	 *    keyCompromise           (1),
	 *    cACompromise            (2),
	 *    affiliationChanged      (3),
	 *    superseded              (4),
	 *    cessationOfOperation    (5),
	 *    certificateHold         (6),
	 *    privilegeWithdrawn      (7),
	 *    aACompromise            (8) }
	 */

	public class CRLDistributionPointsExtension : X509Extension {

		public class DistributionPoint {
			public string Name { get; private set; }
			public ReasonFlags Reasons { get; private set; }
			public string CRLIssuer { get; private set; }

			public DistributionPoint (string dp, ReasonFlags reasons, string issuer) 
			{
				Name = dp;
				Reasons = reasons;
				CRLIssuer = issuer;
			}

			public DistributionPoint (ASN1 dp)
			{
				for (int i = 0; i < dp.Count; i++) {
					ASN1 el = dp[i];
					switch (el.Tag) {
					case 0xA0: // DistributionPointName OPTIONAL
						for (int j = 0; j < el.Count; j++) {
							ASN1 dpn = el [j];
							if (dpn.Tag == 0xA0) {
								Name = new GeneralNames (dpn).ToString ();
							}
						}
						break;
					case 0xA1: // ReasonFlags OPTIONAL
						break;
					case 0xA2: // RelativeDistinguishedName
						break;
					}
				}
			}
		}

		[Flags]
		public enum ReasonFlags
		{
			Unused = 0,
			KeyCompromise = 1,
			CACompromise = 2,
			AffiliationChanged = 3,
			Superseded = 4,
			CessationOfOperation = 5,
			CertificateHold = 6,
			PrivilegeWithdrawn = 7,
			AACompromise = 8
		}

		private List<DistributionPoint> dps;

		public CRLDistributionPointsExtension () : base () 
		{
			extnOid = "2.5.29.31";
			dps = new List<DistributionPoint> ();
		}

		public CRLDistributionPointsExtension (ASN1 asn1) 
			: base (asn1)
		{
		}

		public CRLDistributionPointsExtension (X509Extension extension) 
			: base (extension)
		{
		}

		protected override void Decode () 
		{
			dps = new List<DistributionPoint> ();
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x30)
				throw new ArgumentException ("Invalid CRLDistributionPoints extension");
			// for every distribution point
			for (int i=0; i < sequence.Count; i++) {
				dps.Add (new DistributionPoint (sequence [i]));
			}
		}

		public override string Name {
			get { return "CRL Distribution Points"; }
		}

		public IEnumerable<DistributionPoint> DistributionPoints {
			get { return dps; }
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			int i = 1;
			foreach (DistributionPoint dp in dps) {
				sb.Append ("[");
				sb.Append (i++);
				sb.Append ("]CRL Distribution Point");
				sb.Append (Environment.NewLine);
				sb.Append ("\tDistribution Point Name:");
				sb.Append ("\t\tFull Name:");
				sb.Append (Environment.NewLine);
				sb.Append ("\t\t\t");
				sb.Append (dp.Name);
				sb.Append (Environment.NewLine);
			}
			return sb.ToString ();
		}
	}
}
