//
// SamlAssertionTest.cs
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

using MonoTests.System.IdentityModel.Common;

namespace MonoTests.System.IdentityModel.Tokens
{
	[TestFixture]
	public class SamlAssertionTest
	{
		[Test]
		public void DefaultValues ()
		{
			DateTime d1 = DateTime.UtcNow, d2;
			SamlAssertion a = new SamlAssertion ();
			d2 = DateTime.UtcNow;
			Assert.IsNull (a.Advice, "#1");
			Assert.IsNotNull (a.AssertionId, "#2");
			Assert.IsTrue (d1 <= a.IssueInstant && a.IssueInstant <= d2, "#3");
			Assert.IsNull (a.Issuer, "#4");
			Assert.AreEqual (1, a.MajorVersion, "#5");
			Assert.AreEqual (1, a.MinorVersion, "#6");
			Assert.IsNull (a.SigningCredentials, "#7");
			Assert.IsNull (a.SigningToken, "#8");
		}

		XmlDictionaryWriter CreateWriter (StringWriter sw)
		{
			return XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorAssertionIdStartsWithNumbers ()
		{
			new SamlAssertion ("01234567890",
				"dummy-issuer", DateTime.UtcNow, null, null, 					new SamlStatement [] {new SamlAuthenticationStatement ()});
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotDotNet")]
		public void ConstructorAssertionIdContainsInvalidChar ()
		{
			new SamlAssertion ("invalid-name%here",
				"dummy-issuer", DateTime.UtcNow, null, null, 					new SamlStatement [] {new SamlAuthenticationStatement ()});
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorEmptyIssuer ()
		{
			new SamlAssertion ("SamlSecurityToken-" + Guid.NewGuid (),
				String.Empty, DateTime.UtcNow, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullStatements ()
		{
			new SamlAssertion ("SamlSecurityToken-" + Guid.NewGuid (),
				"dummy-issuer", DateTime.UtcNow, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorStatementsContainNull ()
		{
			new SamlAssertion ("SamlSecurityToken-" + Guid.NewGuid (),
				"dummy-issuer", DateTime.UtcNow, null, null, new SamlStatement [] {null});
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNoStatement ()
		{
			new SamlAssertion ("SamlSecurityToken-" + Guid.NewGuid (),
				"dummy-issuer", DateTime.UtcNow, null, null, new SamlStatement [0]);
		}

		[Test]
		public void Constructor ()
		{
			new SamlAssertion ("SamlSecurityToken-" + Guid.NewGuid (),
				"dummy-issuer", DateTime.UtcNow, null, null,
				new SamlStatement [] {new SamlAuthenticationStatement ()});
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNullIssuer ()
		{
			SamlAssertion a = new SamlAssertion ();
			using (XmlDictionaryWriter dw = CreateWriter (new StringWriter ())) {
				a.WriteXml (dw, null, null);
			}
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNoStatement ()
		{
			SamlAssertion a = new SamlAssertion ();
			a.Issuer = "my_boss";
			using (XmlDictionaryWriter dw = CreateWriter (new StringWriter ())) {
				a.WriteXml (dw, null, null);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteXmlNullSerializer ()
		{
			SamlAssertion a = new SamlAssertion ();
			a.Statements.Add (new SamlAttributeStatement ());
			a.Issuer = "my_hero";
			using (XmlDictionaryWriter dw = CreateWriter (new StringWriter ())) {
				a.WriteXml (dw, null, null);
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteXmlWithoutSamlSubject ()
		{
			SamlAssertion a = new SamlAssertion ();
			a.Statements.Add (new SamlAttributeStatement ());
			a.Issuer = "my_boss";
			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
			Assert.AreEqual ("<?xml version=\"1.0\" ?>", sw.ToString ());
		}

		[Test]
		public void WriteXmlValid ()
		{
			SamlAssertion a = new SamlAssertion ();
			SamlSubject subject = new SamlSubject (
				SamlConstants.UserNameNamespace,
				"urn:myqualifier",
				"myname");
			SamlAttribute attr = new SamlAttribute (Claim.CreateNameClaim ("myname"));
			SamlAttributeStatement statement =
				new SamlAttributeStatement (subject, new SamlAttribute [] {attr});
			a.Advice = new SamlAdvice (new string [] {"urn:testadvice1"});
			DateTime notBefore = DateTime.SpecifyKind (new DateTime (2000, 1, 1), DateTimeKind.Utc);
			DateTime notOnAfter = DateTime.SpecifyKind (new DateTime (2006, 1, 1), DateTimeKind.Utc);
			a.Conditions = new SamlConditions (notBefore, notOnAfter);
			a.Statements.Add (statement);
			a.Issuer = "my_hero";
			StringWriter sw = new StringWriter ();
			string id = a.AssertionId;
			DateTime instant = a.IssueInstant;
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
			string expected = String.Format ("<?xml version=\"1.0\" encoding=\"utf-16\"?><saml:Assertion MajorVersion=\"1\" MinorVersion=\"1\" AssertionID=\"{0}\" Issuer=\"my_hero\" IssueInstant=\"{1}\" xmlns:saml=\"urn:oasis:names:tc:SAML:1.0:assertion\"><saml:Conditions NotBefore=\"{3}\" NotOnOrAfter=\"{4}\" /><saml:Advice><saml:AssertionIDReference>urn:testadvice1</saml:AssertionIDReference></saml:Advice><saml:AttributeStatement><saml:Subject><saml:NameIdentifier Format=\"urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName\" NameQualifier=\"urn:myqualifier\">myname</saml:NameIdentifier></saml:Subject><saml:Attribute AttributeName=\"name\" AttributeNamespace=\"{2}\"><saml:AttributeValue>myname</saml:AttributeValue></saml:Attribute></saml:AttributeStatement></saml:Assertion>",
				id,
				instant.ToString ("yyyy-MM-ddTHH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
				"http://schemas.xmlsoap.org/ws/2005/05/identity/claims",
				notBefore.ToString ("yyyy-MM-ddTHH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
				notOnAfter.ToString ("yyyy-MM-ddTHH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
			Assert.AreEqual (expected, sw.ToString ());
		}
	}
}
#endif
