//
// ClaimTypes.cs
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
	public static class ClaimTypes {
		internal const string Namespace2005 = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/";
		internal const string Namespace2008 = "http://schemas.microsoft.com/ws/2008/06/identity/claims/";
		internal const string Namespace2009 = "http://schemas.xmlsoap.org/ws/2009/09/identity/claims/";

		public const string Actor = Namespace2009 + "actor";
		public const string Anonymous = Namespace2005 + "anonymous";
		public const string Authentication = Namespace2005 + "authentication";
		public const string AuthenticationInstant = Namespace2008 + "authenticationinstant";
		public const string AuthenticationMethod = Namespace2008 + "authenticationmethod";
		public const string AuthorizationDecision = Namespace2005 + "authorizationdecision";
		public const string CookiePath = Namespace2008 + "cookiepath";
		public const string Country = Namespace2005 + "country";
		public const string DateOfBirth = Namespace2005 + "dateofbirth";
		public const string DenyOnlyPrimaryGroupSid = Namespace2008 + "denyonlyprimarygroupsid";
		public const string DenyOnlyPrimarySid = Namespace2008 + "denyonlyprimarysid";
		public const string DenyOnlySid = Namespace2005 + "denyonlysid";
		public const string DenyOnlyWindowsDeviceGroup = Namespace2008 + "denyonlywindowsdevicegroup";
		public const string Dns = Namespace2005 + "dns";
		public const string Dsa = Namespace2008 + "dsa";
		public const string Email = Namespace2005 + "emailaddress";
		public const string Expiration = Namespace2008 + "expiration";
		public const string Expired = Namespace2008 + "expired";
		public const string Gender = Namespace2005 + "gender";
		public const string GivenName = Namespace2005 + "givenname";
		public const string GroupSid = Namespace2008 + "groupsid";
		public const string Hash = Namespace2005 + "hash";
		public const string HomePhone = Namespace2005 + "homephone";
		public const string IsPersistent = Namespace2008 + "ispersistent";
		public const string Locality = Namespace2005 + "locality";
		public const string MobilePhone = Namespace2005 + "mobilephone";
		public const string Name = Namespace2005 + "name";
		public const string NameIdentifier = Namespace2005 + "nameidentifier";
		public const string OtherPhone = Namespace2005 + "otherphone";
		public const string PostalCode = Namespace2005 + "postalcode";
		public const string PrimaryGroupSid = Namespace2008 + "primarygroupsid";
		public const string PrimarySid = Namespace2008 + "primarysid";
		public const string Role = Namespace2008 + "role";
		public const string Rsa = Namespace2005 + "rsa";
		public const string SerialNumber = Namespace2008 + "serialnumber";
		public const string Sid = Namespace2005 + "sid";
		public const string Spn = Namespace2005 + "spn";
		public const string StateOrProvince = Namespace2005 + "stateorprovince";
		public const string StreetAddress = Namespace2005 + "streetaddress";
		public const string Surname = Namespace2005 + "surname";
		public const string System = Namespace2005 + "system";
		public const string Thumbprint = Namespace2005 + "thumbprint";
		public const string Upn = Namespace2005 + "upn";
		public const string Uri = Namespace2005 + "uri";
		public const string UserData = Namespace2008 + "userdata";
		public const string Version = Namespace2008 + "version";
		public const string Webpage = Namespace2005 + "webpage";
		public const string WindowsAccountName = Namespace2008 + "windowsaccountname";
		public const string WindowsDeviceClaim = Namespace2008 + "windowsdeviceclaim";
		public const string WindowsDeviceGroup = Namespace2008 + "windowsdevicegroup";
		public const string WindowsFqbnVersion = Namespace2008 + "windowsfqbnversion";
		public const string WindowsSubAuthority = Namespace2008 + "windowssubauthority";
		public const string WindowsUserClaim = Namespace2008 + "windowsuserclaim";
		public const string X500DistinguishedName = Namespace2005 + "x500distinguishedname";
	}
}
#endif