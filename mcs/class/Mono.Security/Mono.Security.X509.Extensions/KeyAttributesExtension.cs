//
// KeyAttributesExtension.cs: Handles X.509 *DEPRECATED* KeyAttributes extensions.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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
	// definition found @ http://groups.yahoo.com/group/ssl-talk/message/1964
	//
	// keyAttributes EXTENSION ::= {
	//	SYNTAX KeyAttributes
	//	IDENTIFIED BY { id-ce 2 } }
	//
	// KeyAttributes ::= SEQUENCE {
	//	keyIdentifier KeyIdentifier OPTIONAL,
	//	intendedKeyUsage KeyUsage OPTIONAL,
	//	privateKeyUsagePeriod PrivateKeyValidity OPTIONAL 
	// }
	// KeyUsage ::= BIT STRING {
	//	digitalSignature (0),
	//	nonRepudiation (1),
	//	keyEncipherment (2),
	//	dataEncipherment (3),
	//	keyAgreement (4),
	//	keyCertSign (5),
	//	offLineCRLSign (6) 
	// }
	// PrivateKeyValidity ::= SEQUENCE {
	//	notBefore [0] GeneralizedTime OPTIONAL,
	//	notAfter [1] GeneralizedTime OPTIONAL 
	// }
	// ( CONSTRAINED BY { -- at least one component shall be present -- })

	public class KeyAttributesExtension : X509Extension {

		private byte[] keyId;
		private int kubits;
		private DateTime notBefore;
		private DateTime notAfter;

		public KeyAttributesExtension () : base () 
		{
			extnOid = "2.5.29.2";
		}

		public KeyAttributesExtension (ASN1 asn1) : base (asn1)
		{
		}

		public KeyAttributesExtension (X509Extension extension) : base (extension)
		{
		}

		protected override void Decode () 
		{
			ASN1 seq = new ASN1 (extnValue.Value);
			if (seq.Tag != 0x30)
				throw new ArgumentException ("Invalid KeyAttributesExtension extension");
			int n = 0;
			// check for KeyIdentifier
			if (n < seq.Count) {
				ASN1 item = seq [n];
				if (item.Tag == 0x04) {
					n++;
					keyId = item.Value;
				}
			}
			// check for KeyUsage
			if (n < seq.Count) {
				ASN1 item = seq [n];
				if (item.Tag == 0x03) {
					n++;
					int i = 1; // byte zero has the number of unused bits (ASN1's BITSTRING)
					while (i < item.Value.Length)
						kubits = (kubits << 8) + item.Value [i++];
				}
			}
			// check for PrivateKeyValidity
			if (n < seq.Count) {
				ASN1 item = seq [n];
				if (item.Tag == 0x30) {
					int i = 0;
					if (i < item.Count) {
						ASN1 dt = item [i];
						if (dt.Tag == 0x81) {
							i++;
							notBefore = ASN1Convert.ToDateTime (dt);
						}
					}
					if (i < item.Count) {
						ASN1 dt = item [i];
						if (dt.Tag == 0x82)
							notAfter = ASN1Convert.ToDateTime (dt);
					}
				}
			}
		}

		public byte[] KeyIdentifier {
			get { 
				if (keyId == null)
					return null;
				return (byte[]) keyId.Clone ();
			}
		}

		public override string Name {
			get { return "Key Attributes"; }
		}

		public DateTime NotAfter {
			get { return notAfter; }
		}

		public DateTime NotBefore {
			get { return notBefore; }
		}

		public bool Support (KeyUsages usage) 
		{
			int x = Convert.ToInt32 (usage, CultureInfo.InvariantCulture);
			return ((x & kubits) == x);
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			if (keyId != null) {
				sb.Append ("KeyID=");
				int x = 0;
				while (x < keyId.Length) {
					sb.Append (keyId [x].ToString ("X2", CultureInfo.InvariantCulture));
					if (x % 2 == 1)
						sb.Append (" ");
					x++;
				}
				sb.Append (Environment.NewLine);
			}

			if (kubits != 0) {
				sb.Append ("Key Usage=");
				const string separator = " , ";
				if (Support (KeyUsages.digitalSignature))
					sb.Append ("Digital Signature");
				if (Support (KeyUsages.nonRepudiation)) {
					if (sb.Length > 0)
						sb.Append (separator);
					sb.Append ("Non-Repudiation");
				}
				if (Support (KeyUsages.keyEncipherment)) {
					if (sb.Length > 0)
						sb.Append (separator);
					sb.Append ("Key Encipherment");
				}
				if (Support (KeyUsages.dataEncipherment)) {
					if (sb.Length > 0)
						sb.Append (separator);
					sb.Append ("Data Encipherment");
				}
				if (Support (KeyUsages.keyAgreement)) {
					if (sb.Length > 0)
						sb.Append (separator);
					sb.Append ("Key Agreement");		
				}
				if (Support (KeyUsages.keyCertSign)) {
					if (sb.Length > 0)
						sb.Append (separator);
					sb.Append ("Certificate Signing");
				}
				if (Support (KeyUsages.cRLSign)) {
					if (sb.Length > 0)
						sb.Append (separator);
					sb.Append ("CRL Signing");
				}
				if (Support (KeyUsages.encipherOnly)) {
					if (sb.Length > 0)
						sb.Append (separator);
					sb.Append ("Encipher Only ");	// ???
				}
				if (Support (KeyUsages.decipherOnly)) {
					if (sb.Length > 0)
						sb.Append (separator);
					sb.Append ("Decipher Only");	// ???
				}
				sb.Append ("(");
				sb.Append (kubits.ToString ("X2", CultureInfo.InvariantCulture));
				sb.Append (")");
				sb.Append (Environment.NewLine);
			}

			if (notBefore != DateTime.MinValue) {
				sb.Append ("Not Before=");
				sb.Append (notBefore.ToString (CultureInfo.CurrentUICulture));
				sb.Append (Environment.NewLine);
			}
			if (notAfter != DateTime.MinValue) {
				sb.Append ("Not After=");
				sb.Append (notAfter.ToString (CultureInfo.CurrentUICulture));
				sb.Append (Environment.NewLine);
			}
			return sb.ToString ();
		}
	}
}
