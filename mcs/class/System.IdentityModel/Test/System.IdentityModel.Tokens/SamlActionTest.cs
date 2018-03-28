//
// SamlActionTest.cs
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
	public class SamlActionTest
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
			SamlAction a = new SamlAction ();
			Assert.IsNull (a.Action, "#1");
			Assert.IsNull (a.Namespace, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNullAction ()
		{
			new SamlAction (null);
		}

		[Test]
		public void ConstructorNullNamespace ()
		{
			new SamlAction ("urn:myAction", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetActionNull ()
		{
			SamlAction a = new SamlAction ();
			a.Action = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetActionEmpty ()
		{
			SamlAction a = new SamlAction ();
			a.Action = String.Empty;
		}

		[Test]
		public void SetNamespaceNull ()
		{
			SamlAction a = new SamlAction ();
			a.Namespace = null;
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNullAction ()
		{
			SamlAction c = new SamlAction ();
			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				c.WriteXml (dw, new SamlSerializer (), null);
			}
		}

		[Test]
		public void WriteXml1 ()
		{
			SamlAction c = new SamlAction ("urn:myAction", "urn:myNS");
			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				c.WriteXml (dw, new SamlSerializer (), null);
			}
			Assert.AreEqual (String.Format ("<?xml version=\"1.0\" encoding=\"utf-16\"?><saml:Action Namespace=\"urn:myNS\" xmlns:saml=\"{0}\">urn:myAction</saml:Action>", SamlConstants.Namespace), sw.ToString ());
		}

		[Test]
		public void ReadXml1 ()
		{
			SamlSerializer ser = new SamlSerializer ();
			string xml = String.Format ("<saml:Action Namespace=\"urn:myNS\" xmlns:saml=\"{0}\">urn:myAction</saml:Action>", SamlConstants.Namespace);
			XmlDictionaryReader reader = CreateReader (xml);
			reader.MoveToContent ();

			SamlAction s = new SamlAction ();
			s.ReadXml (reader, ser, null, null);
			Assert.AreEqual ("urn:myAction", s.Action, "#1");
			Assert.AreEqual ("urn:myNS", s.Namespace, "#2");
		}
	}
}
#endif
