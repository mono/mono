//
// KeyUsageExtension.cs: Handles X.509 KeyUsage extensions.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
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
	public enum KeyUsage {
		digitalSignature = 0x80,
                nonRepudiation = 0x40,
		keyEncipherment = 0x20,
		dataEncipherment = 0x10,
		keyAgreement = 0x08,
		keyCertSign = 0x04,
		cRLSign = 0x02,
		encipherOnly = 0x01,
		decipherOnly = 0x800
	}

	public class KeyUsageExtension : X509Extension {

		private int kubits;

		public KeyUsageExtension (ASN1 asn1) : base (asn1) {}

		public KeyUsageExtension (X509Extension extension) : base (extension) {}

		protected override void Decode () 
		{
			ASN1 bitString = new ASN1 (extnValue.Value);
			if (bitString.Tag != 0x03)
				throw new ArgumentException ("Invalid KeyUsage extension");
			int i = 1; // byte zero has the number of unused bits (ASN1's BITSTRING)
			while (i < bitString.Value.Length)
				kubits = (kubits << 8) + bitString.Value [i++];
		}

		public override string Name {
			get { return "Key Usage"; }
		}

		public bool Support (KeyUsage usage) 
		{
			int x = Convert.ToInt32 (usage);
			return ((x & kubits) == x);
		}

		public override string ToString () 
		{
			const string separator = " , ";
			StringBuilder sb = new StringBuilder ();
			if (Support (KeyUsage.digitalSignature))
				sb.Append ("Digital Signature");
			if (Support (KeyUsage.nonRepudiation)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("Non-Repudiation");
			}
			if (Support (KeyUsage.keyEncipherment)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("Key Encipherment");
			}
			if (Support (KeyUsage.dataEncipherment)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("Data Encipherment");
			}
			if (Support (KeyUsage.keyAgreement)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("Key Agreement");		
			}
			if (Support (KeyUsage.keyCertSign)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("Certificate Signing");
			}
			if (Support (KeyUsage.cRLSign)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("CRL Signing");
			}
			if (Support (KeyUsage.encipherOnly)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("Encipher Only ");	// ???
			}
			if (Support (KeyUsage.decipherOnly)) {
				if (sb.Length > 0)
					sb.Append (separator);
				sb.Append ("Decipher Only");	// ???
			}
			sb.Append ("(");
			sb.Append (kubits.ToString ("X2"));
			sb.Append (")");
			sb.Append (Environment.NewLine);
			return sb.ToString ();
		}
	}
}
