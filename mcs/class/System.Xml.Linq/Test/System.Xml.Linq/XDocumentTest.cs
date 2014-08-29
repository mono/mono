//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XDocumentTest
	{
		[Test]
		public void Load1 ()
		{
			string xml = "<?xml version='1.0'?><root />";

			XDocument doc = XDocument.Load (new StringReader (xml));
			Assert.IsTrue (doc.FirstNode is XElement, "#1");
			Assert.IsTrue (doc.LastNode is XElement, "#2");
			Assert.IsNull (doc.NextNode, "#3");
			Assert.IsNull (doc.PreviousNode, "#4");
			Assert.AreEqual (1, new List<XNode> (doc.Nodes ()).Count, "#5");
			Assert.IsNull (doc.FirstNode.Parent, "#6");
			Assert.AreEqual (doc.FirstNode, doc.LastNode, "#7");
			Assert.AreEqual (XmlNodeType.Document, doc.NodeType, "#8");
			Assert.AreEqual (doc.FirstNode, doc.Root, "#7");
		}

		[Test]
		public void Load2 ()
		{
			// https://bugzilla.novell.com/show_bug.cgi?id=496285
			byte [] bytes = Encoding.UTF8.GetBytes ("<root/>");
			var reader = new XmlTextReader (new MemoryStream (bytes));
			var doc = XDocument.Load (reader);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LoadInvalid ()
		{
			string xml = "text";
			XmlReaderSettings s = new XmlReaderSettings ();
			s.ConformanceLevel = ConformanceLevel.Fragment;

			XDocument.Load (XmlReader.Create (new StringReader (xml), s));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void LoadWhitespaces ()
		{
			string xml = "   ";
			XmlReaderSettings s = new XmlReaderSettings ();
			s.ConformanceLevel = ConformanceLevel.Fragment;

			XDocument.Load (XmlReader.Create (new StringReader (xml), s));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddTextToDocument ()
		{
			XDocument doc = new XDocument ();
			doc.Add ("test");
		}

		[Test]
		[Category ("NotDotNet")]
		//[ExpectedException (typeof (ArgumentException))]
		public void AddXDeclarationToDocument ()
		{
			XDocument doc = new XDocument ();
			// LAMESPEC: XDeclaration is treated as a general object
			// and hence converted to a string -> error
			doc.Add (new XDeclaration ("1.0", null, null));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddXAttributeToDocument ()
		{
			var doc = new XDocument ();
			doc.Add (new XAttribute ("foo", " "));
		}
		
		[Test] // bug #4850
		public void AddXmlDeclarationEvenForDecllessDoc ()
		{
			var doc = new XDocument (
				new XElement ("resources",
					new XElement ("string",
						new XAttribute ("name", "whatever"),
						"This is sparta")));
			var sw = new StringWriter ();
			using (var writer = new XmlTextWriter (sw))
				doc.WriteTo (writer);
			Assert.IsTrue (sw.ToString ().StartsWith ("<?xml"), "#1");
		}

		[Test] // bug #18772
		public void ChangedEvent ()
		{
			const string xml = "<?xml version='1.0' encoding='utf-8'?><Start><Ele1/></Start>";
			var testXmlDoc = XDocument.Load (new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			var changed = false;
			testXmlDoc.Changed += (sender, e) => {
				changed = true;
			};

			XElement p = new XElement ("Hello");
			testXmlDoc.Root.Add (p);

			Assert.IsTrue (changed);
		}
	}
}
