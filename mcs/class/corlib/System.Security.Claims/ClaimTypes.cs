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
#if NET_4_5
using System;

namespace System.Security.Claims
{
	public static class ClaimTypes
	{
		public const string Actor = "http://schemas.xmlsoap.org/ws/2009/09/identity/claims/actor";

		public const string Anonymous = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/anonymous";

		public const string Authentication = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication";

		public const string AuthenticationInstant = "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant";

		public const string AuthenticationMethod = "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod";

		public const string AuthorizationDecision = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision";

		public const string ClaimsType2005Namespace = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims";

		public const string ClaimsType2009Namespace = "http://schemas.xmlsoap.org/ws/2009/09/identity/claims";

		public const string ClaimsTypeNamespace = "http://schemas.microsoft.com/ws/2008/06/identity/claims";

		public const string CookiePath = "http://schemas.microsoft.com/ws/2008/06/identity/claims/cookiepath";

		public const string Country = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country";

		public const string DateOfBirth = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth";

		public const string DenyOnlyPrimaryGroup = "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarygroup";

		public const string DenyOnlyPrimarySid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarysid";

		public const string DenyOnlySid = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid";

		public const string Dns = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dns";

		public const string Dsa = "http://schemas.microsoft.com/ws/2008/06/identity/claims/dsa";

		public const string Email = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/email";

		public const string Expiration = "http://schemas.microsoft.com/ws/2008/06/identity/claims/expiration";

		public const string Expired = "http://schemas.microsoft.com/ws/2008/06/identity/claims/expired";

		public const string Gender = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender";

		public const string GivenName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";

		public const string GroupSid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid";

		public const string Hash = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash";

		public const string HomePhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone";

		public const string IsPersistent = "http://schemas.microsoft.com/ws/2008/06/identity/claims/ispersistent";

		public const string Locality = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality";

		public const string MobilePhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone";

		public const string Name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

		public const string NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

		public const string OtherPhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone";

		public const string PostalCode = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode";

		public const string PPID = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier";

		public const string PrimaryGroupSid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarygroupsid";

		public const string PrimarySid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid";

		public const string Role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

		public const string Rsa = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa";

		public const string SerialNumber = "http://schemas.microsoft.com/ws/2008/06/identity/claims/serialnumber";

		public const string Sid = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";

		public const string Spn = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/spn";

		public const string StateOrProvince = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince";

		public const string StreetAddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress";

		public const string Surname = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";

		public const string System = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system";

		public const string Thumbprint = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint";

		public const string Upn = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";

		public const string Uri = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri";

		public const string UserData = "http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata";

		public const string Version = "http://schemas.microsoft.com/ws/2008/06/identity/claims/version";

		public const string Webpage = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage";

		public const string WindowsAccountName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname";

		public const string X500DistinguishedName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname";
	}
}
#endif
