//
// IdentityCardTest.cs
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
using System.Xml;
using Mono.ServiceModel.IdentitySelectors;
using NUnit.Framework;

namespace MonoTests.Mono.ServiceModel.IdentitySelectors
{
	[TestFixture]
	public class IdentityCardTest
	{
		[Test]
		public void Load ()
		{
			IdentityCard ic = new IdentityCard ();
			ic.Load (XmlReader.Create ("Test/resources/rupert.xml"));
			Assert.AreEqual (DateTimeKind.Utc, ic.TimeIssued.Kind, "#1");
			Assert.AreEqual (11, ic.TimeIssued.Hour, "#2");
			Assert.AreEqual (23, ic.TimeExpires.Hour, "#3");
			new IdentityCard ().Load (XmlReader.Create (
				"Test/resources/managed.xml"));
		}

		[Test]
		public void SaveRoundtrip ()
		{
			SaveRoundtrip ("Test/resources/rupert.xml");
			SaveRoundtrip ("Test/resources/managed.xml");
		}
		
		void SaveRoundtrip (string file)
		{
			IdentityCard ic = new IdentityCard ();
			ic.Load (XmlReader.Create (file));
			MemoryStream ms = new MemoryStream ();
			XmlWriterSettings xws = new XmlWriterSettings ();
			xws.OmitXmlDeclaration = true;
			using (XmlWriter xw = XmlWriter.Create (ms, xws)) {
				ic.Save (xw);
			}
			XmlDocument doc = new XmlDocument ();
			doc.Load (file);
			if (doc.FirstChild is XmlDeclaration)
				doc.RemoveChild (doc.FirstChild);
			string expected = doc.OuterXml;
			doc.Load (new MemoryStream (ms.ToArray ()));
			string actual = doc.OuterXml;
			Assert.AreEqual (expected, actual, file);
		}
	}
}

