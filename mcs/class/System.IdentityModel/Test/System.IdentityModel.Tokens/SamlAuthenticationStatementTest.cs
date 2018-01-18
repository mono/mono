//
// SamlAuthenticationStatementTest.cs
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
using System.Globalization;
using System.IO;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Tokens
{
	[TestFixture]
	public class SamlAuthenticationStatementTest
	{
		XmlDictionaryWriter CreateWriter (StringWriter sw)
		{
			return XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw));
		}

		[Test]
		public void DefaultValues ()
		{
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication", SamlAuthenticationStatement.ClaimType, "#0");
			SamlAuthenticationStatement a = new SamlAuthenticationStatement ();
			Assert.AreEqual ("urn:oasis:names:tc:SAML:1.0:am:unspecified", a.AuthenticationMethod, "#1");
			Assert.IsNull (a.DnsAddress, "#2");
			Assert.IsNull (a.IPAddress, "#3");
			Assert.IsNull (a.SamlSubject, "#4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullSubject ()
		{
			new SamlAuthenticationStatement (null,
				null, DateTime.Now, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorEmptyAuthenticationMethod ()
		{
			new SamlAuthenticationStatement (new SamlSubject (),
				String.Empty, DateTime.Now, null, null, null);
		}

		[Test]
		public void ConstructorNullDnsIPBindings ()
		{
			new SamlAuthenticationStatement (new SamlSubject (),
				"mymethod", DateTime.Now, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetNullSubject ()
		{
			new SamlAuthenticationStatement ().SamlSubject = null;
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNullSubject ()
		{
			SamlAuthenticationStatement c = new SamlAuthenticationStatement ();
			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				c.WriteXml (dw, new SamlSerializer (), null);
			}
		}

		[Test]
		public void WriteXml1 ()
		{
			SamlAuthenticationStatement c = new SamlAuthenticationStatement ();
			c.SamlSubject = new SamlSubject ("myFormat", "myQualifier", "myName");
			DateTime instant = DateTime.SpecifyKind (new DateTime (2000, 1, 1), DateTimeKind.Utc);
			c.AuthenticationInstant = instant;
			c.DnsAddress = "123.45.67.89";
			c.IPAddress = "98.76.54.32";

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				c.WriteXml (dw, new SamlSerializer (), null);
			}
			Assert.AreEqual (String.Format ("<?xml version=\"1.0\" encoding=\"utf-16\"?><saml:AuthenticationStatement AuthenticationMethod=\"urn:oasis:names:tc:SAML:1.0:am:unspecified\" AuthenticationInstant=\"2000-01-01T00:00:00.000Z\" xmlns:saml=\"{0}\"><saml:Subject><saml:NameIdentifier Format=\"myFormat\" NameQualifier=\"myQualifier\">myName</saml:NameIdentifier></saml:Subject><saml:SubjectLocality IPAddress=\"98.76.54.32\" DNSAddress=\"123.45.67.89\" /></saml:AuthenticationStatement>",
				SamlConstants.Namespace), sw.ToString ());
		}
	}
}
#endif
