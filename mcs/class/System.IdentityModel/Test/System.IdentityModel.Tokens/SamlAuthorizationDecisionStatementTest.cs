//
// SamlAuthorizationDecisionStatementTest.cs
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
	public class SamlAuthorizationDecisionStatementTest
	{
		XmlDictionaryWriter CreateWriter (StringWriter sw)
		{
			return XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw));
		}

		XmlDictionaryReader CreateReader (string xml)
		{
			return XmlDictionaryReader.CreateDictionaryReader (XmlReader.Create (new StringReader (xml)));
		}

		[Test]
		public void DefaultValues ()
		{
			SamlAuthorizationDecisionStatement a = new SamlAuthorizationDecisionStatement ();
			Assert.AreEqual (default (SamlAccessDecision), a.AccessDecision, "#1");
			Assert.IsNull (a.Evidence, "#2");
			Assert.IsNull (a.Resource, "#3");
			Assert.IsNull (a.SamlSubject, "#4");
			Assert.AreEqual (0, a.SamlActions.Count, "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullSubject ()
		{
			new SamlAuthorizationDecisionStatement (null, "resource", default (SamlAccessDecision), null);
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))] // ???? it is inconsistent behavior with others...
		public void ConstructorNullResource ()
		{
			new SamlAuthorizationDecisionStatement (
				new SamlSubject (), null, default (SamlAccessDecision), new SamlAction [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullActions ()
		{
			new SamlAuthorizationDecisionStatement (
				new SamlSubject (), "resource", default (SamlAccessDecision), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorActionsContainNull ()
		{
			new SamlAuthorizationDecisionStatement (
				new SamlSubject (), "resource", default (SamlAccessDecision), new SamlAction [] {null});
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetResourceEmpty ()
		{
			SamlAuthorizationDecisionStatement a = new SamlAuthorizationDecisionStatement ();
			a.Resource = String.Empty;
		}

		[Test]
		public void SetEvidenceNull ()
		{
			SamlAuthorizationDecisionStatement a = new SamlAuthorizationDecisionStatement ();
			a.Evidence = null;
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNoSubject ()
		{
			SamlAuthorizationDecisionStatement a = new SamlAuthorizationDecisionStatement ();

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNoResource ()
		{
			SamlAuthorizationDecisionStatement a = new SamlAuthorizationDecisionStatement ();
			a.SamlSubject = new SamlSubject ("myFormat", "myQualifier", "myName");

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNoAction ()
		{
			SamlAuthorizationDecisionStatement a = new SamlAuthorizationDecisionStatement ();
			a.SamlSubject = new SamlSubject ("myFormat", "myQualifier", "myName");
			a.Resource = "resource";

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
		}

		[Test]
		public void WriteXml1 ()
		{
			SamlAuthorizationDecisionStatement a = new SamlAuthorizationDecisionStatement ();
			a.SamlSubject = new SamlSubject ("myFormat", "myQualifier", "myName");
			a.Resource = "resource";
			a.SamlActions.Add (new SamlAction ("myAction"));
			a.Evidence = new SamlEvidence (new string [] {"myID"});

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
			Assert.AreEqual (String.Format ("<?xml version=\"1.0\" encoding=\"utf-16\"?><saml:AuthorizationDecisionStatement Decision=\"Permit\" Resource=\"resource\" xmlns:saml=\"{0}\"><saml:Subject><saml:NameIdentifier Format=\"myFormat\" NameQualifier=\"myQualifier\">myName</saml:NameIdentifier></saml:Subject><saml:Action>myAction</saml:Action><saml:Evidence><saml:AssertionIDReference>myID</saml:AssertionIDReference></saml:Evidence></saml:AuthorizationDecisionStatement>", SamlConstants.Namespace), sw.ToString ());
		}

		[Test]
		public void ReadXml1 ()
		{
			SamlSerializer ser = new SamlSerializer ();
			string xml = String.Format ("<saml:AuthorizationDecisionStatement Decision=\"Deny\" Resource=\"resource\" xmlns:saml=\"{0}\"><saml:Subject><saml:NameIdentifier Format=\"myFormat\" NameQualifier=\"myQualifier\">myName</saml:NameIdentifier></saml:Subject><saml:Action>myAction</saml:Action><saml:Evidence><saml:AssertionIDReference>myID</saml:AssertionIDReference></saml:Evidence></saml:AuthorizationDecisionStatement>", SamlConstants.Namespace);
			XmlDictionaryReader reader = CreateReader (xml);
			reader.MoveToContent ();

			SamlAuthorizationDecisionStatement s =
				new SamlAuthorizationDecisionStatement ();
			s.ReadXml (reader, ser, null, null);
			Assert.AreEqual (SamlAccessDecision.Deny, s.AccessDecision, "#1");
			Assert.IsNotNull (s.SamlSubject, "#2");
			Assert.AreEqual (1, s.SamlActions.Count, "#3");
			Assert.AreEqual ("myAction", s.SamlActions [0].Action, "#4");
			Assert.IsNotNull (s.Evidence, "#5");
			Assert.AreEqual (1, s.Evidence.AssertionIdReferences.Count, "#6");
			Assert.AreEqual ("myID", s.Evidence.AssertionIdReferences [0], "#7");
			Assert.AreEqual ("resource", s.Resource, "#8");
		}
	}
}
#endif
