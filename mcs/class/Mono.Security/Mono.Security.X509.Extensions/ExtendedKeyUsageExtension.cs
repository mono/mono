//
// ExtendedKeyUsageExtension.cs: Handles X.509 ExtendedKeyUsage extensions.
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
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

#if MOONLIGHT
	internal
#else
	public 
#endif
	class ExtendedKeyUsageExtension : X509Extension {

		private ArrayList keyPurpose;

		public ExtendedKeyUsageExtension () : base () 
		{
			extnOid = "2.5.29.37";
			keyPurpose = new ArrayList ();
		}

		public ExtendedKeyUsageExtension (ASN1 asn1) : base (asn1)
		{
		}

		public ExtendedKeyUsageExtension (X509Extension extension) : base (extension)
		{
		}

		protected override void Decode () 
		{
			keyPurpose = new ArrayList ();
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x30)
				throw new ArgumentException ("Invalid ExtendedKeyUsage extension");
			// for every policy OID
			for (int i=0; i < sequence.Count; i++)
				keyPurpose.Add (ASN1Convert.ToOid (sequence [i]));
		}

		protected override void Encode () 
		{
			ASN1 seq = new ASN1 (0x30);
			foreach (string oid in keyPurpose) {
				seq.Add (ASN1Convert.FromOid (oid));
			}

			extnValue = new ASN1 (0x04);
			extnValue.Add (seq);
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
					case "1.3.6.1.5.5.7.3.1":
						sb.Append ("Server Authentication");
						break;
					case "1.3.6.1.5.5.7.3.2":
						sb.Append ("Client Authentication");
						break;
					case "1.3.6.1.5.5.7.3.3":
						sb.Append ("Code Signing");
						break;
					case "1.3.6.1.5.5.7.3.4":
						sb.Append ("Email Protection");
						break;
					case "1.3.6.1.5.5.7.3.8":
						sb.Append ("Time Stamping");
						break;
					case "1.3.6.1.5.5.7.3.9":
						sb.Append ("OCSP Signing");
						break;
					default:
						sb.Append ("unknown");
						break;
				}
				sb.AppendFormat (" ({0}){1}", s, Environment.NewLine);
			}
			return sb.ToString ();
		}
	}
}
