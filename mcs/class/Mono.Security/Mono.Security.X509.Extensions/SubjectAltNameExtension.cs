//
// SubjectAltNameExtension.cs: Handles X.509 SubjectAltName extensions.
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
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
using System.Net;
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

	// TODO - incomplete (only rfc822Name, dNSName are supported)
	public class SubjectAltNameExtension : X509Extension {
		
		private ArrayList rfc822Name;
		private ArrayList dnsName;
		private ArrayList ipAddr;

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
					case 0x82: // dNSName           [2]     IA5String
						if (dnsName == null)
							dnsName = new ArrayList ();
						dnsName.Add (Encoding.ASCII.GetString (sequence [i].Value));
						break;
					case 0x87: // iPAddress         [7]     OCTET STRING
						if (ipAddr == null)
							ipAddr = new ArrayList ();
						// TODO - Must find sample certificates
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
				if (rfc822Name == null)
					return new string [0];
				return (string[]) rfc822Name.ToArray (typeof(string));
			}
		}

		public string[] DNSNames {
			get {
				if (dnsName == null)
					return new string [0];
				return (string[]) dnsName.ToArray (typeof(string));
			}
		}

		// Incomplete support
		public string[] IPAddresses {
			get {
				if (ipAddr == null)
					return new string [0];
				return (string[]) ipAddr.ToArray (typeof(string));
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
			if (dnsName != null) {
				foreach (string s in dnsName) {
					sb.Append ("DNS Name=");
					sb.Append (s);
					sb.Append (Environment.NewLine);
				}
			}
			if (ipAddr != null) {
				foreach (string s in ipAddr) {
					sb.Append ("IP Address=");
					sb.Append (s);
					sb.Append (Environment.NewLine);
				}
			}
			return sb.ToString ();
		}
	}
}
