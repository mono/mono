//
// KeyUsageExtension.cs: Handles X.509 KeyUsage extensions.
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

	/*
	 * id-ce-keyUsage OBJECT IDENTIFIER ::=  { id-ce 15 }
	 * 
	 * KeyUsage ::= BIT STRING {
	 *	digitalSignature        (0),
	 * 	nonRepudiation          (1),
	 * 	keyEncipherment         (2),
	 * 	dataEncipherment        (3),
	 * 	keyAgreement            (4),
	 * 	keyCertSign             (5),
	 * 	cRLSign                 (6),
	 * 	encipherOnly            (7),
	 * 	decipherOnly            (8) 
	 * }
	 */
	// note: because nothing is simple in ASN.1 bits are reversed
	[Flags]
#if INSIDE_CORLIB || INSIDE_SYSTEM
	internal
#else
	public 
#endif
	enum KeyUsages {
		digitalSignature = 0x80,
                nonRepudiation = 0x40,
		keyEncipherment = 0x20,
		dataEncipherment = 0x10,
		keyAgreement = 0x08,
		keyCertSign = 0x04,
		cRLSign = 0x02,
		encipherOnly = 0x01,
		decipherOnly = 0x800,
		none = 0x0
	}

#if INSIDE_CORLIB || INSIDE_SYSTEM
	internal
#else
	public 
#endif
	class KeyUsageExtension : X509Extension {

		private int kubits;

		public KeyUsageExtension (ASN1 asn1) : base (asn1) {}

		public KeyUsageExtension (X509Extension extension) : base (extension) {}

		public KeyUsageExtension () : base ()
		{
			extnOid = "2.5.29.15";
		}

		protected override void Decode () 
		{
			ASN1 bitString = new ASN1 (extnValue.Value);
			if (bitString.Tag != 0x03)
				throw new ArgumentException ("Invalid KeyUsage extension");
			int i = 1; // byte zero has the number of unused bits (ASN1's BITSTRING)
			while (i < bitString.Value.Length)
				kubits = (kubits << 8) + bitString.Value [i++];
		}

		protected override void Encode ()
		{
			extnValue = new ASN1 (0x04);

			ushort ku = (ushort) kubits;
			byte unused = 16;
			if (ku > 0) {
				// count the unused bits
				for (unused = 15; unused > 0; unused--) {
					if ((ku & 0x8000) == 0x8000)
						break;
					ku <<= 1;
				}

				if (kubits > Byte.MaxValue) {
					unused -= 8;
					extnValue.Add (new ASN1 (0x03, new byte[] { unused, (byte) kubits, (byte) (kubits >> 8) }));
				} else {
					extnValue.Add (new ASN1 (0x03, new byte[] { unused, (byte) kubits }));
				}
			} else {
				// note: a BITSTRING with a 0 length is invalid (in ASN.1), so would an
				// empty OCTETSTRING (at the parent level) so we're encoding a 0
				extnValue.Add (new ASN1 (0x03, new byte[] { 7, 0 }));
			}
		}

		public KeyUsages KeyUsage {
			get { return (KeyUsages) kubits; }
			set { kubits = Convert.ToInt32 (value, CultureInfo.InvariantCulture); }
		}

		public override string Name {
			get { return "Key Usage"; }
		}

		public bool Support (KeyUsages usage) 
		{
			int x = Convert.ToInt32 (usage, CultureInfo.InvariantCulture);
			return ((x & kubits) == x);
		}

		public override string ToString () 
		{
			const string separator = " , ";
			StringBuilder sb = new StringBuilder ();
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
			return sb.ToString ();
		}
	}
}
