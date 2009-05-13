//
// IdentityCardEncryptionTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System;
using System.IO;
using System.Text;
using System.Xml;
using Mono.ServiceModel.IdentitySelectors;
using NUnit.Framework;

namespace MonoTests.Mono.ServiceModel.IdentitySelectors
{
	[TestFixture]
	public class IdentityCardEncryptionTest
	{
		[Test]
		public void Import ()
		{
			string encxml = new StreamReader ("Test/resources/rupert.crds").ReadToEnd ();
			string xml = new IdentityCardEncryption ().Decrypt (
				encxml, "monkeydance");
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
		}

		[Test]
		public void Export ()
		{
			byte [] salt = Convert.FromBase64String ("ofkHGOy0pioOd7++N2a52w==");
			byte [] iv = Convert.FromBase64String ("OzFSoAlrfj11g246TM4How==");
			XmlDocument doc = new XmlDocument ();
			doc.Load ("Test/resources/rupert.xml");
			doc.RemoveChild (doc.FirstChild);
			byte [] result = new IdentityCardEncryption ().Encrypt (doc.OuterXml, "monkeydance", salt, iv);
			string resultText = Encoding.UTF8.GetString (result);

			string roundtrip = new IdentityCardEncryption ().Decrypt (resultText, "monkeydance");
			doc = new XmlDocument ();
			doc.LoadXml (roundtrip);
		}
	}
}

