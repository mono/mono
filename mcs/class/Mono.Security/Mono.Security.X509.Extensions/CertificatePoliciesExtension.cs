//
// CertificatePoliciesExtension.cs: Handles X.509 CertificatePolicies extensions.
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
using System.Collections;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

namespace Mono.Security.X509.Extensions {

	/*
	 * id-ce-certificatePolicies OBJECT IDENTIFIER ::=  { id-ce 32 }
	 * 
	 * anyPolicy OBJECT IDENTIFIER ::= { id-ce-certificate-policies 0 }
	 * 
	 * certificatePolicies ::= SEQUENCE SIZE (1..MAX) OF PolicyInformation
	 * 
	 * PolicyInformation ::= SEQUENCE {
	 *    policyIdentifier   CertPolicyId,
	 *    policyQualifiers   SEQUENCE SIZE (1..MAX) OF PolicyQualifierInfo OPTIONAL 
	 * }
	 * 
	 * CertPolicyId ::= OBJECT IDENTIFIER
	 * 
	 * PolicyQualifierInfo ::= SEQUENCE {
	 *    policyQualifierId  PolicyQualifierId,
	 *    qualifier          ANY DEFINED BY policyQualifierId 
	 * }
	 * 
	 * -- policyQualifierIds for Internet policy qualifiers
	 * id-qt          OBJECT IDENTIFIER ::=  { id-pkix 2 }
	 * id-qt-cps      OBJECT IDENTIFIER ::=  { id-qt 1 }
	 * id-qt-unotice  OBJECT IDENTIFIER ::=  { id-qt 2 }
	 * 
	 * PolicyQualifierId ::= OBJECT IDENTIFIER ( id-qt-cps | id-qt-unotice )
	 * 
	 * Qualifier ::= CHOICE {
	 *    cPSuri           CPSuri,
	 *    userNotice       UserNotice 
	 * }
	 * 
	 * CPSuri ::= IA5String
	 * 
	 * UserNotice ::= SEQUENCE {
	 *    noticeRef        NoticeReference OPTIONAL,
	 *    explicitText     DisplayText OPTIONAL
	 * }
	 * 
	 * NoticeReference ::= SEQUENCE {
	 *    organization     DisplayText,
	 *    noticeNumbers    SEQUENCE OF INTEGER 
	 * }
	 * 
	 * DisplayText ::= CHOICE {
	 *    ia5String        IA5String      (SIZE (1..200)),
	 *    visibleString    VisibleString  (SIZE (1..200)),
	 *    bmpString        BMPString      (SIZE (1..200)),
	 *    utf8String       UTF8String     (SIZE (1..200)) 
	 * }
	 */

	// note: partial implementation (only policyIdentifier OID are supported)
	public class CertificatePoliciesExtension : X509Extension {

		private Hashtable policies;

		public CertificatePoliciesExtension () : base () 
		{
			extnOid = "2.5.29.32";
			policies = new Hashtable ();
		}

		public CertificatePoliciesExtension (ASN1 asn1) : base (asn1)
		{
		}

		public CertificatePoliciesExtension (X509Extension extension) : base (extension)
		{
		}

		protected override void Decode () 
		{
			policies = new Hashtable ();
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x30)
				throw new ArgumentException ("Invalid CertificatePolicies extension");
			// for every policy OID
			for (int i=0; i < sequence.Count; i++) {
				policies.Add (ASN1Convert.ToOid (sequence [i][0]), null);
			}
		}

		public override string Name {
			get { return "Certificate Policies"; }
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			int n = 1;
			foreach (DictionaryEntry policy in policies) {
				sb.Append ("[");
				sb.Append (n++);
				sb.Append ("]Certificate Policy:");
				sb.Append (Environment.NewLine);
				sb.Append ("\tPolicyIdentifier=");
				sb.Append ((string)policy.Key);
				sb.Append (Environment.NewLine);
			}
			return sb.ToString ();
		}
	}
}
