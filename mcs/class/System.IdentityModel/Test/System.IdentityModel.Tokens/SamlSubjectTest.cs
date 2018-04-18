//
// SamlSubjectTest.cs
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
	public class SamlSubjectTest
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
			SamlSubject a = new SamlSubject ();
			Assert.IsNull (a.NameFormat, "#1");
			Assert.IsNull (a.NameQualifier, "#2");
			Assert.IsNull (a.Name, "#3");
		}

		[Test]
		public void ConstructorNullFormat ()
		{
			new SamlSubject (null, "myQ", "myName");
		}

		[Test]
		public void ConstructorNullQualifier ()
		{
			new SamlSubject ("myF", null, "myName");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNullName ()
		{
			new SamlSubject ("myF", "myQ", null);
		}

		[Test]
		public void SetFormatEmpty ()
		{
			SamlSubject a = new SamlSubject ();
			a.NameFormat = String.Empty;
		}

		[Test]
		public void SetQualifierEmpty ()
		{
			SamlSubject a = new SamlSubject ();
			a.NameQualifier = String.Empty;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetNameEmpty ()
		{
			SamlSubject a = new SamlSubject ();
			a.Name = String.Empty;
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNoFormat ()
		{
			SamlSubject a = new SamlSubject ();

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
		}

		[Test]
		public void WriteXml1 ()
		{
			SamlSubject a = new SamlSubject ("myFormat", "myQualifier", "myName");

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
			Assert.AreEqual (String.Format ("<?xml version=\"1.0\" encoding=\"utf-16\"?><saml:Subject xmlns:saml=\"{0}\"><saml:NameIdentifier Format=\"myFormat\" NameQualifier=\"myQualifier\">myName</saml:NameIdentifier></saml:Subject>", SamlConstants.Namespace), sw.ToString ());
		}

		[Test]
		public void ReadXml1 ()
		{
			SamlSerializer ser = new SamlSerializer ();
			string xml = String.Format ("<saml:Subject xmlns:saml=\"{0}\"><saml:NameIdentifier Format=\"myFormat\" NameQualifier=\"myQualifier\">myName</saml:NameIdentifier></saml:Subject>", SamlConstants.Namespace);
			XmlDictionaryReader reader = CreateReader (xml);
			reader.MoveToContent ();

			SamlSubject s = new SamlSubject ();
			s.ReadXml (reader, ser, null, null);
			Assert.AreEqual ("myFormat", s.NameFormat, "#1");
			Assert.AreEqual ("myQualifier", s.NameQualifier, "#2");
			Assert.AreEqual ("myName", s.Name, "#3");
		}
	}
}
#endif