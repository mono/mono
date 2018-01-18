//
// ClaimTypesTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE
using System;
using System.IdentityModel.Claims;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Claims
{
	[TestFixture]
	public class ClaimTypesTest
	{
		[Test]
		public void Constants ()
		{
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/anonymous",
				ClaimTypes.Anonymous, "#1");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication",
				ClaimTypes.Authentication, "#2");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision",
				ClaimTypes.AuthorizationDecision, "#3");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country",
				ClaimTypes.Country, "#4");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth",
				ClaimTypes.DateOfBirth, "#5");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid",
				ClaimTypes.DenyOnlySid, "#6");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dns",
				ClaimTypes.Dns, "#7");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
				ClaimTypes.Email, "#8");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender",
				ClaimTypes.Gender, "#9");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
				ClaimTypes.GivenName, "#10");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash",
				ClaimTypes.Hash, "#11");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone",
				ClaimTypes.HomePhone, "#12");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality",
				ClaimTypes.Locality, "#13");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone",
				ClaimTypes.MobilePhone, "#14");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
				ClaimTypes.Name, "#15");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
				ClaimTypes.NameIdentifier, "#16");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone",
				ClaimTypes.OtherPhone, "#17");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode",
				ClaimTypes.PostalCode, "#18");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier",
				ClaimTypes.PPID, "#19");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa",
				ClaimTypes.Rsa, "#20");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid",
				ClaimTypes.Sid, "#21");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/spn",
				ClaimTypes.Spn, "#22");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince",
				ClaimTypes.StateOrProvince, "#23");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress",
				ClaimTypes.StreetAddress, "#24");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname",
				ClaimTypes.Surname, "#25");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system",
				ClaimTypes.System, "#26");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint",
				ClaimTypes.Thumbprint, "#27");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn",
				ClaimTypes.Upn, "#28");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri",
				ClaimTypes.Uri, "#29");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage",
				ClaimTypes.Webpage, "#30");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname",
				ClaimTypes.X500DistinguishedName, "#31");
		}
	}
}
#endif
