//
// ClaimValueTypes.cs
//
// Authors:
//	Matthias Dittrich <matthi.d@gmail.com>
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
#if NET_4_5
using System.Runtime.InteropServices;

namespace System.Security.Claims {
	[ComVisible (false)]
	public static class ClaimValueTypes {
		internal const string SchemaNamespace2001 = "http://www.w3.org/2001/XMLSchema#";
		internal const string QueryOperatorsNamespace2002 = "http://www.w3.org/TR/2002/WD-xquery-operators-20020816#";
		internal const string ClaimsNamespace = "http://schemas.xmlsoap.org/claims/";
		internal const string XmlSigNamespace2000 = "http://www.w3.org/2000/09/xmldsig#";
		internal const string ClaimsNamespace2005 = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/";
		internal const string XACMLDatatypeNamespace = "urn:oasis:names:tc:xacml:1.0:data-type:";

		public const string Base64Binary = SchemaNamespace2001 + "base64Binary";
		public const string Base64Octet = SchemaNamespace2001 + "base64Octet";
		public const string Boolean = SchemaNamespace2001 + "boolean";
		public const string Date = SchemaNamespace2001 + "date";
		public const string DateTime = SchemaNamespace2001 + "dateTime";
		public const string DaytimeDuration = QueryOperatorsNamespace2002 + "dayTimeDuration";
		public const string DnsName = ClaimsNamespace + "dns";
		public const string Double = SchemaNamespace2001 + "double";
		public const string DsaKeyValue = XmlSigNamespace2000 + "DSAKeyValue";
		public const string Email = ClaimsNamespace2005 + "emailaddress";
		public const string Fqbn = SchemaNamespace2001 + "fqbn";
		public const string HexBinary = SchemaNamespace2001 + "hexBinary";
		public const string Integer = SchemaNamespace2001 + "integer";
		public const string Integer32 = SchemaNamespace2001 + "integer32";
		public const string Integer64 = SchemaNamespace2001 + "integer64";
		public const string KeyInfo = XmlSigNamespace2000 + "KeyInfo";
		public const string Rfc822Name = XACMLDatatypeNamespace + "rfc822Name";
		public const string Rsa = ClaimsNamespace2005 + "rsa";
		public const string RsaKeyValue = XmlSigNamespace2000 + "RSAKeyValue";
		public const string Sid = SchemaNamespace2001 + "sid";
		public const string String = SchemaNamespace2001 + "string";
		public const string Time = SchemaNamespace2001 + "time";
		public const string UInteger32 = SchemaNamespace2001 + "uinteger32";
		public const string UInteger64 = SchemaNamespace2001 + "uinteger64";
		public const string UpnName = ClaimsNamespace + "UPN";
		public const string X500Name = XACMLDatatypeNamespace + "x500Name";
		public const string YearMonthDuration = QueryOperatorsNamespace2002 + "yearMonthDuration";
	}
}
#endif