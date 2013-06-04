//
// AuthorityKeyIdentifierExtension.cs: Handles X.509 AuthorityKeyIdentifier extensions.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005,2007 Novell, Inc (http://www.novell.com)
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
	 * id-ce-authorityKeyIdentifier OBJECT IDENTIFIER ::=  { id-ce 35 }
	 * 
	 * AuthorityKeyIdentifier ::= SEQUENCE {
	 *    keyIdentifier             [0] KeyIdentifier           OPTIONAL,
	 *    authorityCertIssuer       [1] GeneralNames            OPTIONAL,
	 *    authorityCertSerialNumber [2] CertificateSerialNumber OPTIONAL  }
	 * 
	 * KeyIdentifier ::= OCTET STRING
	 */

#if INSIDE_SYSTEM
	internal
#else
	public
#endif
	class AuthorityKeyIdentifierExtension : X509Extension {

		private byte[] aki;

		public AuthorityKeyIdentifierExtension () : base () 
		{
			extnOid = "2.5.29.35";
		}

		public AuthorityKeyIdentifierExtension (ASN1 asn1) : base (asn1)
		{
		}

		public AuthorityKeyIdentifierExtension (X509Extension extension) : base (extension)
		{
		}

		protected override void Decode () 
		{
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x30)
				throw new ArgumentException ("Invalid AuthorityKeyIdentifier extension");
			for (int i=0; i < sequence.Count; i++) {
				ASN1 el = sequence [i];
				switch (el.Tag) {
					case 0x80:
						aki = el.Value;
						break;
					default:
						// don't throw on stuff we don't yet support
						// e.g. authorityCertIssuer/authorityCertSerialNumber
						break;
				}
			}
		}

		public override string Name {
			get { return "Authority Key Identifier"; }
		}

		public byte[] Identifier {
			get {
				if (aki == null)
					return null;
				return (byte[]) aki.Clone (); 
			}
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			if (aki != null) {
				// [0] KeyIdentifier
				int x = 0;
				sb.Append ("KeyID=");
				while (x < aki.Length) {
					sb.Append (aki [x].ToString ("X2", CultureInfo.InvariantCulture));
					if (x % 2 == 1)
						sb.Append (" ");
					x++;
				}
				// [1] GeneralNames
				// TODO
				// [2] CertificateSerialNumber
				// TODO
			}
			return sb.ToString ();
		}
	}
}
