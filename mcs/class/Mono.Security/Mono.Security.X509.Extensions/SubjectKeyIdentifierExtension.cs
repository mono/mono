//
// SubjectKeyIdentifierExtension.cs: Handles X.509 SubjectKeyIdentifier extensions.
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
	 * id-ce-subjectKeyIdentifier OBJECT IDENTIFIER ::=  { id-ce 14 }
	 * 
	 * SubjectKeyIdentifier ::= KeyIdentifier
	 * 
	 * KeyIdentifier ::= OCTET STRING
	 */

#if INSIDE_CORLIB || INSIDE_SYSTEM
	internal
#else
	public 
#endif
	class SubjectKeyIdentifierExtension : X509Extension {

		private byte[] ski;

		public SubjectKeyIdentifierExtension () : base () 
		{
			extnOid = "2.5.29.14";
		}

		public SubjectKeyIdentifierExtension (ASN1 asn1) : base (asn1)
		{
		}

		public SubjectKeyIdentifierExtension (X509Extension extension) : base (extension)
		{
		}

		protected override void Decode () 
		{
			ASN1 sequence = new ASN1 (extnValue.Value);
			if (sequence.Tag != 0x04)
				throw new ArgumentException ("Invalid SubjectKeyIdentifier extension");
			ski = sequence.Value;
		}

		protected override void Encode ()
		{
			if (ski == null) {
				throw new InvalidOperationException ("Invalid SubjectKeyIdentifier extension");
			}

			var seq = new ASN1 (0x04, ski);
			extnValue = new ASN1 (0x04);
			extnValue.Add (seq);
		}

		public override string Name {
			get { return "Subject Key Identifier"; }
		}

		public byte[] Identifier {
			get { 
				if (ski == null)
					return null;
				return (byte[]) ski.Clone (); 
			}
			set { ski = value; }
		}

		public override string ToString () 
		{
			if (ski == null)
				return null;

			StringBuilder sb = new StringBuilder ();
			int x = 0;
			while (x < ski.Length) {
				sb.Append (ski [x].ToString ("X2", CultureInfo.InvariantCulture));
				if (x % 2 == 1)
					sb.Append (" ");
				x++;
			}
			return sb.ToString ();
		}
	}
}
