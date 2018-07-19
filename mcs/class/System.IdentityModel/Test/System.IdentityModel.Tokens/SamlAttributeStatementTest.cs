//
// SamlAttributeStatementTest.cs
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
	public class SamlAttributeStatementTest
	{
		XmlDictionaryWriter CreateWriter (StringWriter sw)
		{
			return XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw));
		}

		[Test]
		public void DefaultValues ()
		{
			SamlAttributeStatement s =
				new SamlAttributeStatement ();
			Assert.IsNull (s.SamlSubject, "#1");
			Assert.AreEqual (0, s.Attributes.Count, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullSubject ()
		{
			SamlAttribute attr = new SamlAttribute (Claim.CreateNameClaim ("myname"));
			new SamlAttributeStatement (null, new SamlAttribute [] {attr});
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullAttributes ()
		{
			new SamlAttributeStatement (new SamlSubject (), null);
		}

		[Test]
		public void WriteXml1 ()
		{
			SamlAttribute attr = new SamlAttribute (Claim.CreateNameClaim ("myname"));
			SamlSubject subject = new SamlSubject (
				SamlConstants.UserNameNamespace,
				"urn:myqualifier",
				"myname");
			SamlAttributeStatement s = new SamlAttributeStatement (
				subject, new SamlAttribute [] {attr});
			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				s.WriteXml (dw, new SamlSerializer (), null);
			}
			Assert.AreEqual (String.Format ("<?xml version=\"1.0\" encoding=\"utf-16\"?><saml:AttributeStatement xmlns:saml=\"urn:oasis:names:tc:SAML:1.0:assertion\"><saml:Subject><saml:NameIdentifier Format=\"urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName\" NameQualifier=\"urn:myqualifier\">myname</saml:NameIdentifier></saml:Subject><saml:Attribute AttributeName=\"name\" AttributeNamespace=\"{0}\"><saml:AttributeValue>myname</saml:AttributeValue></saml:Attribute></saml:AttributeStatement>", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims"), sw.ToString ());
		}
	}
}
#endif
