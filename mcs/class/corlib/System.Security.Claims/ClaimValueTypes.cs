//
// Claim.cs
//
// Authors:
//  Miguel de Icaza (miguel@xamarin.com)
//
// Copyright 2014 Xamarin Inc
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
namespace System.Security.Claims {

	public static class ClaimValueTypes {
		public const string Base64Binary = "http://www.w3.org/2001/XMLSchema#base64Binary";
		public const string Base64Octet = "http://www.w3.org/2001/XMLSchema#base64Octet";
		public const string Boolean = "http://www.w3.org/2001/XMLSchema#boolean";
		public const string Date = "http://www.w3.org/2001/XMLSchema#date";
		public const string DateTime = "http://www.w3.org/2001/XMLSchema#dateTime";
		public const string DaytimeDuration = "http://www.w3.org/TR/2002/WD-xquery-operators-20020816#dayTimeDuration";
		public const string DnsName = "http://schemas.xmlsoap.org/claims/dns";
		public const string Double = "http://www.w3.org/2001/XMLSchema#double";
		public const string DsaKeyValue = "http://www.w3.org/2000/09/xmldsig#DSAKeyValue";
		public const string Email = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
		public const string Fqbn = "http://www.w3.org/2001/XMLSchema#fqbn";
		public const string HexBinary = "http://www.w3.org/2001/XMLSchema#hexBinary";
		public const string Integer = "http://www.w3.org/2001/XMLSchema#integer";
		public const string Integer32 = "http://www.w3.org/2001/XMLSchema#integer32";
		public const string Integer64 = "http://www.w3.org/2001/XMLSchema#integer64";
		public const string KeyInfo = "http://www.w3.org/2000/09/xmldsig#KeyInfo";
		public const string Rfc822Name = "urn:oasis:names:tc:xacml:1.0:data-type:rfc822Name";
		public const string Rsa = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa";
		public const string RsaKeyValue = "http://www.w3.org/2000/09/xmldsig#RSAKeyValue";
		public const string Sid = "http://www.w3.org/2001/XMLSchema#sid";
		public const string String = "http://www.w3.org/2001/XMLSchema#string";
		public const string Time = "http://www.w3.org/2001/XMLSchema#time";
		public const string UInteger32 = "http://www.w3.org/2001/XMLSchema#uinteger32";
		public const string UInteger64 = "http://www.w3.org/2001/XMLSchema#uinteger64";
		public const string UpnName = "http://schemas.xmlsoap.org/claims/UPN";
		public const string X500Name = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name";
		public const string YearMonthDuration = "http://www.w3.org/TR/2002/WD-xquery-operators-20020816#yearMonthDuration";

	}
}
