//
// PrivateKeyUsagePeriodExtension.cs: Handles X.509 PrivateKeyUsagePeriod extensions.
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
using System.Globalization;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

namespace Mono.Security.X509.Extensions {

	/*
	 * id-ce-privateKeyUsagePeriod OBJECT IDENTIFIER ::=  { id-ce 16 }
	 * 
	 * PrivateKeyUsagePeriod ::= SEQUENCE {
	 *    notBefore       [0]     GeneralizedTime OPTIONAL,
	 *    notAfter        [1]     GeneralizedTime OPTIONAL 
	 * }
	 */
	public class PrivateKeyUsagePeriodExtension : X509Extension {

		private DateTime notBefore;
		private DateTime notAfter;

		public PrivateKeyUsagePeriodExtension () : base () 
		{
			extnOid = "2.5.29.16";
		}

		public PrivateKeyUsagePeriodExtension (ASN1 asn1) : base (asn1)
		{
		}

		public PrivateKeyUsagePeriodExtension (X509Extension extension) : base (extension)
		{
		}

		protected override void Decode () 
		{
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x30)
				throw new ArgumentException ("Invalid PrivateKeyUsagePeriod extension");
			for (int i=0; i < sequence.Count; i++) {
				switch (sequence [i].Tag) {
					case 0x80:
						notBefore = ASN1Convert.ToDateTime (sequence [i]);
						break;
					case 0x81:
						notAfter = ASN1Convert.ToDateTime (sequence [i]);
						break;
					default:
						throw new ArgumentException ("Invalid PrivateKeyUsagePeriod extension");
				}
			}
		}

		public override string Name {
			get { return "Private Key Usage Period"; }
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			if (notBefore != DateTime.MinValue) {
				sb.Append ("Not Before: ");
				sb.Append (notBefore.ToString (CultureInfo.CurrentUICulture));
				sb.Append (Environment.NewLine);
			}
			if (notAfter != DateTime.MinValue) {
				sb.Append ("Not After: ");
				sb.Append (notAfter.ToString (CultureInfo.CurrentUICulture));
				sb.Append (Environment.NewLine);
			}
			return sb.ToString ();
		}
	}
}
