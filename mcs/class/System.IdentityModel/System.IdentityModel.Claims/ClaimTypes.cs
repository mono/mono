//
// ClaimTypes.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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

namespace System.IdentityModel.Claims
{
	public static class ClaimTypes
	{
		public static string Anonymous {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/anonymous"; }
		}

		public static string Authentication {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication"; }
		}

		public static string AuthorizationDecision {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision"; }
		}

		public static string Country {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country"; }
		}

		public static string DateOfBirth {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth"; }
		}

		public static string DenyOnlySid {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid"; }
		}

		public static string Dns {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dns"; }
		}

		public static string Email {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"; }
		}

		public static string Gender {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender"; }
		}

		public static string GivenName {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"; }
		}

		public static string Hash {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash"; }
		}

		public static string HomePhone {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone"; }
		}

		public static string Locality {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality"; }
		}

		public static string MobilePhone {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone"; }
		}

		public static string Name {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"; }
		}

		public static string NameIdentifier {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"; }
		}

		public static string OtherPhone {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone"; }
		}

		public static string PostalCode {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode"; }
		}

		public static string PPID {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier"; }
		}

		public static string Rsa {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa"; }
		}

		public static string Sid {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid"; }
		}

		public static string Spn {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/spn"; }
		}

		public static string StateOrProvince {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince"; }
		}

		public static string StreetAddress {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress"; }
		}

		public static string Surname {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"; }
		}

		public static string System {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system"; }
		}

		public static string Thumbprint {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint"; }
		}

		public static string Upn {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn"; }
		}

		public static string Uri {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri"; }
		}

		public static string Webpage {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage"; }
		}

		public static string X500DistinguishedName {
			get { return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname"; }
		}
	}
}
