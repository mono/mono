//
// X520.cs: X.520 related stuff (attributes, RDN)
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
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
using System.Text;

using Mono.Security;

namespace Mono.Security.X509 {

	// References:
	// 1.	Information technology - Open Systems Interconnection - The Directory: Selected attribute types 
	//	http://www.itu.int/rec/recommendation.asp?type=folders&lang=e&parent=T-REC-X.520 
	// 2.	Internet X.509 Public Key Infrastructure Certificate and CRL Profile
	//	http://www.ietf.org/rfc/rfc3280.txt

	/* 
	 * AttributeTypeAndValue ::= SEQUENCE {
	 * 	type     AttributeType,
	 * 	value    AttributeValue 
	 * }
	 * 
	 * AttributeType ::= OBJECT IDENTIFIER
	 * 
	 * AttributeValue ::= ANY DEFINED BY AttributeType
	 */
#if INSIDE_CORLIB
	internal
#else
	public 
#endif
	class X520 {

		public abstract class AttributeTypeAndValue {
			private string oid;
			private string attrValue;
			private int upperBound;
			private byte encoding;

			protected AttributeTypeAndValue (string oid, int upperBound)
			{
				this.oid = oid;
				this.upperBound = upperBound;
				this.encoding = 0xFF;
			}

			protected AttributeTypeAndValue (string oid, int upperBound, byte encoding) 
			{
				this.oid = oid;
				this.upperBound = upperBound;
				this.encoding = encoding;
			}

			public string Value {
				get { return attrValue; }
				set { attrValue = value; }
			}

			public ASN1 ASN1 {
				get { return GetASN1 (); }
			}

			internal ASN1 GetASN1 (byte encoding) 
			{
				byte encode = encoding;
				if (encode == 0xFF)
					encode = SelectBestEncoding ();
					
				ASN1 asn1 = new ASN1 (0x30);
				asn1.Add (ASN1Convert.FromOid (oid));
				switch (encode) {
					case 0x13:
						// PRINTABLESTRING
						asn1.Add (new ASN1 (0x13, Encoding.ASCII.GetBytes (attrValue)));
						break;
					case 0x16:
						// IA5STRING
						asn1.Add (new ASN1 (0x16, Encoding.ASCII.GetBytes (attrValue)));
						break;
					case 0x1E:
						// BMPSTRING
						asn1.Add (new ASN1 (0x1E, Encoding.BigEndianUnicode.GetBytes (attrValue)));
						break;
				}
				return asn1;
			}

			internal ASN1 GetASN1 () 
			{
				return GetASN1 (encoding);
			}

			public byte[] GetBytes (byte encoding) 
			{
				return GetASN1 (encoding) .GetBytes ();
			}

			public byte[] GetBytes () 
			{
				return GetASN1 () .GetBytes ();
			}

			private byte SelectBestEncoding ()
			{
				char[] notPrintableString = { '@', '_' };
				if (attrValue.IndexOfAny (notPrintableString) != -1)
					return 0x1E; // BMPSTRING
				else
					return 0x13; // PRINTABLESTRING
			}
		}

		public class Name : AttributeTypeAndValue {

			public Name () : base ("2.5.4.41", 32768) 
			{
			}
		}

		public class CommonName : AttributeTypeAndValue {

			public CommonName () : base ("2.5.4.3", 64) 
			{
			}
		}

		public class LocalityName : AttributeTypeAndValue {

			public LocalityName () : base ("2.5.4.7", 128)
			{
			}
		}

		public class StateOrProvinceName : AttributeTypeAndValue {

			public StateOrProvinceName () : base ("2.5.4.8", 128) 
			{
			}
		}
		 
		public class OrganizationName : AttributeTypeAndValue {

			public OrganizationName () : base ("2.5.4.10", 64)
			{
			}
		}
		 
		public class OrganizationalUnitName : AttributeTypeAndValue {

			public OrganizationalUnitName () : base ("2.5.4.11", 64)
			{
			}
		}

		// NOTE: Not part of RFC2253
		public class EmailAddress : AttributeTypeAndValue 
		{
			public EmailAddress () : base ("1.2.840.113549.1.9.1", 128, 0x16)
			{
			}
		}

		/* -- Naming attributes of type X520Title
		 * id-at-title             AttributeType ::= { id-at 12 }
		 * 
		 * X520Title ::= CHOICE {
		 *       teletexString     TeletexString   (SIZE (1..ub-title)),
		 *       printableString   PrintableString (SIZE (1..ub-title)),
		 *       universalString   UniversalString (SIZE (1..ub-title)),
		 *       utf8String        UTF8String      (SIZE (1..ub-title)),
		 *       bmpString         BMPString       (SIZE (1..ub-title)) 
		 * }
		 */
		public class Title : AttributeTypeAndValue {

			public Title () : base ("2.5.4.12", 64) {}
		}

		public class CountryName : AttributeTypeAndValue {

			// (0x13) PRINTABLESTRING
			public CountryName () : base ("2.5.4.6", 2, 0x13) 
			{
			}
		}
	}
        
	/* From RFC3280
	 * --  specifications of Upper Bounds MUST be regarded as mandatory
	 * --  from Annex B of ITU-T X.411 Reference Definition of MTS Parameter
	 * 
	 * --  Upper Bounds
	 * 
	 * ub-name INTEGER ::= 32768
	 * ub-common-name INTEGER ::= 64
	 * ub-locality-name INTEGER ::= 128
	 * ub-state-name INTEGER ::= 128
	 * ub-organization-name INTEGER ::= 64
	 * ub-organizational-unit-name INTEGER ::= 64
	 * ub-title INTEGER ::= 64
	 * ub-serial-number INTEGER ::= 64
	 * ub-match INTEGER ::= 128
	 * ub-emailaddress-length INTEGER ::= 128
	 * ub-common-name-length INTEGER ::= 64
	 * ub-country-name-alpha-length INTEGER ::= 2
	 * ub-country-name-numeric-length INTEGER ::= 3
	 * ub-domain-defined-attributes INTEGER ::= 4
	 * ub-domain-defined-attribute-type-length INTEGER ::= 8
	 * ub-domain-defined-attribute-value-length INTEGER ::= 128
	 * ub-domain-name-length INTEGER ::= 16
	 * ub-extension-attributes INTEGER ::= 256
	 * ub-e163-4-number-length INTEGER ::= 15
	 * ub-e163-4-sub-address-length INTEGER ::= 40
	 * ub-generation-qualifier-length INTEGER ::= 3
	 * ub-given-name-length INTEGER ::= 16
	 * ub-initials-length INTEGER ::= 5
	 * ub-integer-options INTEGER ::= 256
	 * ub-numeric-user-id-length INTEGER ::= 32
	 * ub-organization-name-length INTEGER ::= 64
	 * ub-organizational-unit-name-length INTEGER ::= 32
	 * ub-organizational-units INTEGER ::= 4
	 * ub-pds-name-length INTEGER ::= 16
	 * ub-pds-parameter-length INTEGER ::= 30
	 * ub-pds-physical-address-lines INTEGER ::= 6
	 * ub-postal-code-length INTEGER ::= 16
	 * ub-pseudonym INTEGER ::= 128
	 * ub-surname-length INTEGER ::= 40
	 * ub-terminal-id-length INTEGER ::= 24
	 * ub-unformatted-address-length INTEGER ::= 180
	 * ub-x121-address-length INTEGER ::= 16
	 * 
	 * -- Note - upper bounds on string types, such as TeletexString, are
	 * -- measured in characters.  Excepting PrintableString or IA5String, a
	 * -- significantly greater number of octets will be required to hold
	 * -- such a value.  As a minimum, 16 octets, or twice the specified
	 * -- upper bound, whichever is the larger, should be allowed for
	 * -- TeletexString.  For UTF8String or UniversalString at least four
	 * -- times the upper bound should be allowed.
	 */
}
