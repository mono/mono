//
// SamlEvidenceTest.cs
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
using System.Collections.Generic;
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
	public class SamlEvidenceTest
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
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNullIDReferences ()
		{
			new SamlEvidence ((IEnumerable<string>) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorIDReferencesContainNull ()
		{
			new SamlEvidence (new string [] {null});
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNullAssertions ()
		{
			new SamlEvidence ((IEnumerable<SamlAssertion>) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorAssertionsContainNull ()
		{
			new SamlEvidence (new SamlAssertion [] {null});
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNoAssertionsOrIDs ()
		{
			SamlEvidence a = new SamlEvidence ();

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
		}

		[Test]
		public void WriteXml1 ()
		{
			SamlEvidence a = new SamlEvidence ();
			a.AssertionIdReferences.Add ("myref");

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
			Assert.AreEqual (String.Format ("<?xml version=\"1.0\" encoding=\"utf-16\"?><saml:Evidence xmlns:saml=\"{0}\"><saml:AssertionIDReference>myref</saml:AssertionIDReference></saml:Evidence>", SamlConstants.Namespace), sw.ToString ());
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void ReadXmlBadContent ()
		{
			SamlSerializer ser = new SamlSerializer ();
			string xml = String.Format ("<saml:Evidence xmlns:saml=\"{0}\"><saml:DoNotCacheCondition /></saml:Evidence>", SamlConstants.Namespace);
			XmlDictionaryReader reader = CreateReader (xml);
			reader.MoveToContent ();

			SamlEvidence s = new SamlEvidence ();
			s.ReadXml (reader, ser, null, null);
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void ReadXmlExternalContent ()
		{
			SamlSerializer ser = new SamlSerializer ();
			string xml = String.Format ("<saml:Evidence xmlns:saml=\"{0}\"><external-element /><saml:AssertionIDReference>myref</saml:AssertionIDReference></saml:Evidence>", SamlConstants.Namespace);
			XmlDictionaryReader reader = CreateReader (xml);
			reader.MoveToContent ();

			SamlEvidence s = new SamlEvidence ();
			s.ReadXml (reader, ser, null, null);
		}

		[Test]
		public void ReadXml1 ()
		{
			SamlSerializer ser = new SamlSerializer ();
			string xml = String.Format ("<saml:Evidence xmlns:saml=\"{0}\"><saml:AssertionIDReference>myref</saml:AssertionIDReference></saml:Evidence>", SamlConstants.Namespace);
			XmlDictionaryReader reader = CreateReader (xml);
			reader.MoveToContent ();

			SamlEvidence s = new SamlEvidence ();
			s.ReadXml (reader, ser, null, null);
			Assert.AreEqual (1, s.AssertionIdReferences.Count, "#1");
			Assert.AreEqual ("myref", s.AssertionIdReferences [0], "#2");
		}
	}
}
#endif
