//
// X501Name.cs: X.501 Distinguished Names stuff 
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
using System.Globalization;
using System.Text;

using Mono.Security;

namespace Mono.Security.X509 {

	// References:
	// 1.	Information technology - Open Systems Interconnection - The Directory: Models
	//	http://www.itu.int/rec/recommendation.asp?type=items&lang=e&parent=T-REC-X.501-200102-I
	// 2.	RFC2253: Lightweight Directory Access Protocol (v3): UTF-8 String Representation of Distinguished Names
	//	http://www.ietf.org/rfc/rfc2253.txt

	/*
	 * Name ::= CHOICE { RDNSequence }
	 * 
	 * RDNSequence ::= SEQUENCE OF RelativeDistinguishedName
	 * 
	 * RelativeDistinguishedName ::= SET OF AttributeTypeAndValue
	 */
#if INSIDE_CORLIB
	internal
#else
	public 
#endif
	sealed class X501 {

		static byte[] countryName = { 0x55, 0x04, 0x06 };
		static byte[] organizationName = { 0x55, 0x04, 0x0A };
		static byte[] organizationalUnitName = { 0x55, 0x04, 0x0B };
		static byte[] commonName = { 0x55, 0x04, 0x03 };
		static byte[] localityName = { 0x55, 0x04, 0x07 };
		static byte[] stateOrProvinceName = { 0x55, 0x04, 0x08 };
		static byte[] streetAddress = { 0x55, 0x04, 0x09 };
		static byte[] serialNumber = { 0x55, 0x04, 0x05 };
		static byte[] domainComponent = { 0x09, 0x92, 0x26, 0x89, 0x93, 0xF2, 0x2C, 0x64, 0x01, 0x19 };
		static byte[] userid = { 0x09, 0x92, 0x26, 0x89, 0x93, 0xF2, 0x2C, 0x64, 0x01, 0x01 };
		static byte[] email = { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x09, 0x01 };

		private X501 () 
		{
		}

		static public string ToString (ASN1 seq) 
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < seq.Count; i++) {
				ASN1 entry = seq [i];
				ASN1 pair = entry [0];

				ASN1 s = pair [1];
				if (s == null)
					continue;

				ASN1 poid = pair [0];
				if (poid == null)
					continue;

				if (poid.CompareValue (countryName))
					sb.Append ("C=");
				else if (poid.CompareValue (organizationName))
					sb.Append ("O=");
				else if (poid.CompareValue (organizationalUnitName))
					sb.Append ("OU=");
				else if (poid.CompareValue (commonName))
					sb.Append ("CN=");
				else if (poid.CompareValue (localityName))
					sb.Append ("L=");
				else if (poid.CompareValue (stateOrProvinceName))
					sb.Append ("S=");	// NOTE: RFC2253 uses ST=
				else if (poid.CompareValue (streetAddress))
					sb.Append ("STREET=");
				else if (poid.CompareValue (domainComponent))
					sb.Append ("DC=");
				else if (poid.CompareValue (userid))
					sb.Append ("UID=");
				else if (poid.CompareValue (email))
					sb.Append ("E=");	// NOTE: Not part of RFC2253
				else {
					// unknown OID
					sb.Append ("OID.");	// NOTE: Not present as RFC2253
					sb.Append (ASN1Convert.ToOid (poid));
					sb.Append ("=");
				}

				string sValue = null;
				// 16bits or 8bits string ? TODO not complete (+special chars!)
				if (s.Tag == 0x1E) {
					// BMPSTRING
					StringBuilder sb2 = new StringBuilder ();
					for (int j = 1; j < s.Value.Length; j+=2)
						sb2.Append ((char) s.Value[j]);
					sValue = sb2.ToString ();
				}
				else {
					sValue = System.Text.Encoding.UTF8.GetString (s.Value);
					// in some cases we must quote (") the value
					// Note: this doesn't seems to conform to RFC2253
					char[] specials = { ',', '+', '"', '\\', '<', '>', ';' };
					if (sValue.IndexOfAny(specials, 0, sValue.Length) > 0)
						sValue = "\"" + sValue + "\"";
					else if (sValue.StartsWith (" "))
						sValue = "\"" + sValue + "\"";
					else if (sValue.EndsWith (" "))
						sValue = "\"" + sValue + "\"";
				}

				sb.Append (sValue);

				// separator (not on last iteration)
				if (i < seq.Count - 1)
					sb.Append (", ");
			}
			return sb.ToString ();
		}

		static private X520.AttributeTypeAndValue GetAttributeFromOid (string attributeType) 
		{
			switch (attributeType.ToUpper (CultureInfo.InvariantCulture).Trim ()) {
				case "C":
					return new X520.CountryName ();
				case "O":
					return new X520.OrganizationName ();
				case "OU":
					return new X520.OrganizationalUnitName ();
				case "CN":
					return new X520.CommonName ();
				case "L":
					return new X520.LocalityName ();
				case "S":	// Microsoft
				case "ST":	// RFC2253
					return new X520.StateOrProvinceName ();
				case "E":	// NOTE: Not part of RFC2253
					return new X520.EmailAddress ();
				case "DC":
//					return streetAddress;
				case "UID":
//					return domainComponent;
				default:
					return null;
			}
		}

		static public ASN1 FromString (string rdn) 
		{
			if (rdn == null)
				throw new ArgumentNullException ("rdn");
			// get string from here to ',' or end of string
			int start = 0;
			int end = 0;
			ASN1 asn1 = new ASN1 (0x30);
			while (start < rdn.Length) {
				end = rdn.IndexOf (',', end) + 1;
				if (end == 0)
					end = rdn.Length + 1;
				string av = rdn.Substring (start, end - start - 1);
				// get '=' position in substring
				int equal = av.IndexOf ('=');
				// get AttributeType
				string attributeType = av.Substring (0, equal);
				// get value
				string attributeValue = av.Substring (equal + 1);

				X520.AttributeTypeAndValue atv = GetAttributeFromOid (attributeType);
				atv.Value = attributeValue;
				asn1.Add (new ASN1 (0x31, atv.GetBytes ()));

				// next part
				start = end;
				if (start != - 1) {
					if (end > rdn.Length)
						break;
				}
			}
			return asn1;
		}
	}
}
