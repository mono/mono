//
// CRLDistributionPointsExtension.cs: Handles X.509 CRLDistributionPoints extensions.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Collections;
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

		internal class DP {
			public string DistributionPoint;
			public ReasonFlags Reasons;
			public string CRLIssuer;
		}

		[Flags]
		public enum ReasonFlags {
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

		private ArrayList dps;

		public CRLDistributionPointsExtension () : base () 
		{
			extnOid = "2.5.29.31";
			dps = new ArrayList ();
		}

		public CRLDistributionPointsExtension (ASN1 asn1) : base (asn1) {}

		public CRLDistributionPointsExtension (X509Extension extension) : base (extension) {}

		protected override void Decode () 
		{
			dps = new ArrayList ();
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x30)
				throw new ArgumentException ("Invalid CRLDistributionPoints extension");
			// for every distribution point
			for (int i=0; i < sequence.Count; i++) {
				dps.Add (null);
			}
		}

		public override string Name {
			get { return "CRL Distribution Points"; }
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			foreach (DP dp in dps) {
				sb.Append ("[");
				sb.Append (dp.Reasons);
				sb.Append ("]CRL Distribution Point");
				sb.Append (Environment.NewLine);
				sb.Append ("\tDistribution Point Name:");
				sb.Append (dp.DistributionPoint);
				sb.Append (Environment.NewLine);
				sb.Append ("\t\tFull Name:");
				sb.Append (Environment.NewLine);
				sb.Append ("\t\t\tDirectory Address:");
				sb.Append (dp.CRLIssuer);
				sb.Append (Environment.NewLine);
			}
			return sb.ToString ();
		}
	}
}
