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
using System.Xml;
using System.Xml.Linq;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XElementTest
	{
		[Test] // xml declaration is skipped.
		public void LoadWithXmldecl ()
		{
			string xml = "<?xml version='1.0'?><root />";
			XElement.Load (new StringReader (xml));
		}

		[Test]
		public void Load1 ()
		{
			string xml = "<root><foo/></root>";

			XElement el = XElement.Load (new StringReader (xml));
			XElement first = el.FirstNode as XElement;
			Assert.IsNotNull (first, "#1");
			Assert.IsTrue (el.LastNode is XElement, "#2");
			Assert.IsNull (el.NextNode, "#3");
			Assert.IsNull (el.PreviousNode, "#4");
			Assert.AreEqual (1, new List<XNode> (el.Nodes ()).Count, "#5");
			Assert.AreEqual (el, first.Parent, "#6");
			Assert.AreEqual (first, el.LastNode, "#7");

			Assert.AreEqual ("root", el.Name.ToString (), "#8");
			Assert.AreEqual ("foo", first.Name.ToString (), "#9");
			Assert.IsFalse (el.Attributes ().GetEnumerator ().MoveNext (), "#10");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void LoadInvalid ()
		{
			string xml = "text";
			XmlReaderSettings s = new XmlReaderSettings ();
			s.ConformanceLevel = ConformanceLevel.Fragment;

			XElement.Load (XmlReader.Create (new StringReader (xml), s));
		}

		[Test]
		public void PrecedingWhitespaces ()
		{
			string xml = "  <root/>";
			XmlReaderSettings s = new XmlReaderSettings ();
			s.ConformanceLevel = ConformanceLevel.Fragment;

			XElement.Load (XmlReader.Create (new StringReader (xml), s));
		}

		[Test]
		public void PrecedingWhitespaces2 ()
		{
			string xml = "  <root/>";
			XmlReaderSettings s = new XmlReaderSettings ();
			s.ConformanceLevel = ConformanceLevel.Fragment;

			XmlReader r = XmlReader.Create (new StringReader (xml), s);
			r.Read (); // at whitespace
			XElement.Load (r);
		}

		[Test]
		public void Load2 ()
		{
			string xml = "<root>foo</root>";

			XElement el = XElement.Load (new StringReader (xml));
			XText first = el.FirstNode as XText;
			Assert.IsNotNull (first, "#1");
			Assert.IsTrue (el.LastNode is XText, "#2");
			Assert.AreEqual (1, new List<XNode> (el.Nodes ()).Count, "#3");
			Assert.AreEqual (el, first.Parent, "#4");
			Assert.AreEqual (first, el.LastNode, "#5");

			Assert.AreEqual ("foo", first.Value, "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddDocumentTypeToElement ()
		{
			XElement el = new XElement (XName.Get ("foo"));
			el.Add (new XDocumentType ("foo", null, null, null));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotDotNet")]
		public void AddXDeclarationToElement ()
		{
			XElement el = new XElement (XName.Get ("foo"));
			// LAMESPEC: in .NET, XDeclaration is not treated as
			// invalid, and converted to a string without error.
			el.Add (new XDeclaration ("1.0", null, null));
		}

		[Test]
		public void SetAttribute ()
		{
			XElement el = new XElement (XName.Get ("foo"));
			el.SetAttributeValue (XName.Get ("a1"), "v1");
			XAttribute a = el.FirstAttribute;
			Assert.IsNotNull (a, "#1-1");
			Assert.AreEqual (el, a.Parent, "#1-2");
			Assert.IsNotNull (el.LastAttribute, "#1-3");
			Assert.AreEqual (a, el.LastAttribute, "#1-4");
			Assert.AreEqual ("a1", a.Name.LocalName, "#1-5");
			Assert.AreEqual ("v1", a.Value, "#1-6");
			Assert.IsNull (a.PreviousAttribute, "#1-7");
			Assert.IsNull (a.NextAttribute, "#1-8");

			el.SetAttributeValue (XName.Get ("a2"), "v2");
			Assert.IsFalse (el.FirstAttribute == el.LastAttribute, "#2-1");
			Assert.AreEqual ("a2", el.LastAttribute.Name.LocalName, "#2-2");

			el.SetAttributeValue (XName.Get ("a1"), "v3");
			XAttribute b = el.FirstAttribute;
			Assert.IsNotNull (b, "#2-3");
			Assert.IsNotNull (el.LastAttribute, "#2-4");
			Assert.AreEqual ("a1", b.Name.LocalName, "#2-5");
			Assert.AreEqual ("v3", b.Value, "#2-6");
			Assert.AreEqual (a, b, "#2-7");
			XAttribute c = el.LastAttribute;
			Assert.AreEqual (a, c.PreviousAttribute, "#2-8");

			a.Remove ();
			Assert.IsNull (a.Parent, "#3-1");
			Assert.IsNull (a.PreviousAttribute, "#3-2");
			Assert.IsNull (a.NextAttribute, "#3-3");
			Assert.IsNull (c.PreviousAttribute, "#3-4");
			Assert.IsNull (c.NextAttribute, "#3-5");

			el.RemoveAttributes ();
			Assert.IsFalse (el.HasAttributes, "#4-1");
			Assert.IsNull (b.Parent, "#4-2");
			Assert.IsNull (c.Parent, "#4-3");
			Assert.IsNull (el.FirstAttribute, "#4-4");
			Assert.IsNull (el.LastAttribute, "#4-5");
		}

		[Test]
		public void AddAfterSelf ()
		{
			XElement el = XElement.Parse ("<root><foo/><bar/></root>");
			el.FirstNode.AddAfterSelf ("text");
			XText t = el.FirstNode.NextNode as XText;
			Assert.IsNotNull (t, "#1");
			Assert.AreEqual ("text", t.Value, "#2");
			XElement bar = t.NextNode as XElement;
			Assert.IsNotNull (bar, "#3");
			Assert.AreEqual ("bar", bar.Name.LocalName, "#4");
		}

		[Test]
		public void AddAfterSelfList ()
		{
			XElement el = XElement.Parse ("<root><foo/><bar/></root>");
			el.FirstNode.AddAfterSelf (new XText [] {
				new XText ("t1"),
				new XText ("t2"),
				new XText ("t3")});
			XText t = el.FirstNode.NextNode as XText;
			Assert.IsNotNull (t, "#1");
			Assert.AreEqual ("t1", t.Value, "#2");
			Assert.AreEqual ("t2", ((XText) t.NextNode).Value, "#3");
			Assert.AreEqual ("t3", ((XText) t.NextNode.NextNode).Value, "#4");
			XElement bar = t.NextNode.NextNode.NextNode as XElement;
			Assert.IsNotNull (bar, "#5");
			Assert.AreEqual ("bar", bar.Name.LocalName, "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddAfterSelfAttribute ()
		{
			var el = new XElement ("root", new XElement ("child"));
			var el2 = el.FirstNode as XElement;
			el2.AddAfterSelf (new XAttribute ("foo", "bar"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddAfterSelfXDocument ()
		{
			var el = new XElement ("root", new XElement ("child"));
			var el2 = el.FirstNode as XElement;
			el2.AddAfterSelf (new XDocument ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotDotNet")]
		[Category ("NotWorking")]
		// LAMESPEC: there is no reason to not reject XDeclaration while it rejects XDocument.
		public void AddAfterSelfXDeclaration ()
		{
			var el = new XElement ("root", new XElement ("child"));
			var el2 = el.FirstNode as XElement;
			el2.AddAfterSelf (new XDeclaration ("1.0", null, null));
		}

		[Test]
		public void AddAfterSelfCollection ()
		{
			var el = new XElement ("root", new XElement ("child"));
			el.FirstNode.AddAfterSelf (new List<XElement> (new XElement [] {new XElement ("foo"), new XElement ("bar")}));
			Assert.AreEqual ("<root><child /><foo /><bar /></root>", el.ToString (SaveOptions.DisableFormatting), "#1");
			Assert.AreEqual ("bar", (el.LastNode as XElement).Name.LocalName, "#2");
		}

		[Test]
		public void AddAfterSelfJoinsStringAfterText ()
		{
			var el = XElement.Parse ("<foo>text1</foo>");
			el.LastNode.AddAfterSelf ("text2");
			el.LastNode.AddAfterSelf (new XText ("text3"));
			IEnumerator<XNode> e = el.Nodes ().GetEnumerator ();
			Assert.IsTrue (e.MoveNext (), "#1");
			Assert.AreEqual ("text1text2", e.Current.ToString (), "#2");
			Assert.IsTrue (e.MoveNext (), "#3");
			Assert.AreEqual ("text3", e.Current.ToString (), "#4");
			Assert.IsFalse (e.MoveNext (), "#5");
		}

		[Test]
		public void AddBeforeSelf ()
		{
			XElement el = XElement.Parse ("<root><foo/><bar/></root>");
			el.FirstNode.AddBeforeSelf ("text");
			XText t = el.FirstNode as XText;
			Assert.IsNotNull (t, "#1");
			Assert.AreEqual ("text", t.Value, "#2");
			XElement foo = t.NextNode as XElement;
			Assert.IsNotNull (foo, "#3");
			Assert.AreEqual ("foo", foo.Name.LocalName, "#4");
		}

		[Test]
		public void AddBeforeSelfList ()
		{
			XElement el = XElement.Parse ("<root><foo/><bar/></root>");
			el.FirstNode.AddBeforeSelf (new XText [] {
				new XText ("t1"),
				new XText ("t2"),
				new XText ("t3")});
			XText t = el.FirstNode as XText;
			Assert.IsNotNull (t, "#1");
			Assert.AreEqual ("t1", t.Value, "#2");
			Assert.AreEqual ("t2", ((XText) t.NextNode).Value, "#3");
			Assert.AreEqual ("t3", ((XText) t.NextNode.NextNode).Value, "#4");
			XElement foo = t.NextNode.NextNode.NextNode as XElement;
			Assert.IsNotNull (foo, "#5");
			Assert.AreEqual ("foo", foo.Name.LocalName, "#6");
		}

		[Test]
		public void AddBeforeSelfList2 ()
		{
			XElement el = XElement.Parse ("<root><foo/><bar/></root>");
			el.FirstNode.AddBeforeSelf ("t1", "t2", "t3");
			XText t = el.FirstNode as XText;
			Assert.IsNotNull (t, "#1");
			Assert.AreEqual ("t1t2t3", t.Value, "#2");
			XElement foo = t.NextNode as XElement;
			Assert.IsNotNull (foo, "#3");
			Assert.AreEqual ("foo", foo.Name.LocalName, "#4");
		}

		[Test]
		public void AddJoinsStringAfterText ()
		{
			var el = XElement.Parse ("<foo>text1</foo>");
			el.Add ("text2");
			el.Add (new XText ("text3"));
			IEnumerator<XNode> e = el.Nodes ().GetEnumerator ();
			Assert.IsTrue (e.MoveNext (), "#1");
			Assert.AreEqual ("text1text2", e.Current.ToString (), "#2");
			Assert.IsTrue (e.MoveNext (), "#3");
			Assert.AreEqual ("text3", e.Current.ToString (), "#4");
			Assert.IsFalse (e.MoveNext (), "#5");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddDuplicateAttribute ()
		{
			var el = new XElement ("foo",
				new XAttribute ("bar", "baz"));
			el.Add (new XAttribute ("bar", "baz"));
		}

		[Test]
		public void ReplaceWith ()
		{
			XElement el = XElement.Parse ("<root><foo/><bar/></root>");
			XNode fc = el.FirstNode;
			fc.ReplaceWith ("test");
			XText t = el.FirstNode as XText;
			Assert.IsNotNull (t, "#1");
			Assert.AreEqual ("test", t.Value, "#2");
		}

		[Test]
		public void ReplaceAll ()
		{
			XElement el = XElement.Parse ("<root><foo/><bar/></root>");
			el.ReplaceAll ("test");
			XText t = el.FirstNode as XText;
			Assert.IsNotNull (t, "#1");
			Assert.AreEqual ("test", t.Value, "#2");
			Assert.AreEqual (1, new List<XNode> (el.Nodes ()).Count, "#3");
		}

		[Test]
		public void ReplaceAllList ()
		{
			XElement el = XElement.Parse ("<root><foo/><bar/></root>");
			el.ReplaceAll (
				new XText ("test1"),
				new XText ("test2"),
				new XText ("test3"));
			XText t = el.FirstNode as XText;
			Assert.IsNotNull (t, "#1");
			Assert.AreEqual ("test1", t.Value, "#2");
			t = el.LastNode as XText;
			Assert.IsNotNull (t, "#3");
			Assert.AreEqual ("test3", t.Value, "#4");
			Assert.AreEqual (3, new List<XNode> (el.Nodes ()).Count, "#5");
		}

		[Test]
		public void ReplaceAttributes ()
		{
			XElement el = XElement.Parse ("<root x='y'><foo a='b'/></root>");
			Assert.IsTrue (el.Attributes ().GetEnumerator ().MoveNext (), "#0");
			el.ReplaceAttributes ("test");
			Assert.IsTrue (el.FirstNode is XElement, "#1");
			Assert.IsTrue (el.LastNode is XText, "#2");
			Assert.IsFalse (el.Attributes ().GetEnumerator ().MoveNext (), "#3");
		}

		[Test]
		public void GetDefaultNamespace ()
		{
			XElement el = XElement.Parse ("<root xmlns='urn:foo'><foo><xxx/></foo><x:bar xmlns:x='urn:bar'><yyy/></x:bar><baz xmlns=''><zzz /></baz></root>");
			XNamespace ns1 = XNamespace.Get ("urn:foo");
			Assert.AreEqual (ns1, el.GetDefaultNamespace (), "#1");
			XElement foo = (XElement) el.FirstNode;
			Assert.AreEqual (ns1, foo.GetDefaultNamespace (), "#2");
			Assert.AreEqual (ns1, ((XElement) foo.FirstNode).GetDefaultNamespace (), "#3");
			XElement bar = (XElement) foo.NextNode;
			Assert.AreEqual (ns1, bar.GetDefaultNamespace (), "#4");
			Assert.AreEqual (ns1, ((XElement) bar.FirstNode).GetDefaultNamespace (), "#5");
			XElement baz = (XElement) bar.NextNode;
			Assert.AreEqual (XNamespace.Get (String.Empty), baz.GetDefaultNamespace (), "#6");
			Assert.AreEqual (XNamespace.Get (String.Empty), ((XElement) baz.FirstNode).GetDefaultNamespace (), "#7");
		}

		[Test]
		public void GetPrefixNamespace ()
		{
			XElement el = XElement.Parse ("<x:root xmlns:x='urn:foo'><foo><xxx/></foo><x:bar xmlns:x='urn:bar'><yyy/></x:bar><baz xmlns=''><zzz /></baz></x:root>");
			XNamespace ns1 = XNamespace.Get ("urn:foo");
			XNamespace ns2 = XNamespace.Get ("urn:bar");
			Assert.AreEqual (ns1, el.GetNamespaceOfPrefix ("x"), "#1-1");
			Assert.AreEqual ("x", el.GetPrefixOfNamespace (ns1), "#1-2");
			XElement foo = (XElement) el.FirstNode;
			Assert.AreEqual (ns1, foo.GetNamespaceOfPrefix ("x"), "#2-1");
			Assert.AreEqual ("x", foo.GetPrefixOfNamespace (ns1), "#2-2");
			Assert.AreEqual (ns1, ((XElement) foo.FirstNode).GetNamespaceOfPrefix ("x"), "#3-1");
			Assert.AreEqual ("x", ((XElement) foo.FirstNode).GetPrefixOfNamespace (ns1), "#3-2");
			XElement bar = (XElement) foo.NextNode;
			Assert.AreEqual (ns2, bar.GetNamespaceOfPrefix ("x"), "#4-1");
			Assert.AreEqual ("x", bar.GetPrefixOfNamespace (ns2), "#4-2");
			Assert.AreEqual (null, bar.GetPrefixOfNamespace (ns1), "#4-3");
			Assert.AreEqual (ns2, ((XElement) bar.FirstNode).GetNamespaceOfPrefix ("x"), "#5-1");
			Assert.AreEqual ("x", ((XElement) bar.FirstNode).GetPrefixOfNamespace (ns2), "#5-2");
			Assert.AreEqual (null, ((XElement) bar.FirstNode).GetPrefixOfNamespace (ns1), "#5-3");
		}

		[Test]
		public void NullCasts ()
		{
			XElement a = null;

			Assert.AreEqual (null, (bool?) a, "bool?");
			Assert.AreEqual (null, (DateTime?) a, "DateTime?");
			Assert.AreEqual (null, (decimal?) a, "decimal?");
			Assert.AreEqual (null, (double?) a, "double?");
			Assert.AreEqual (null, (float?) a, "float?");
			Assert.AreEqual (null, (Guid?) a, "Guid?");
			Assert.AreEqual (null, (int?) a, "int?");
			Assert.AreEqual (null, (long?) a, "long?");
			Assert.AreEqual (null, (uint?) a, "uint?");
			Assert.AreEqual (null, (ulong?) a, "ulong?");
			Assert.AreEqual (null, (TimeSpan?) a, "TimeSpan?");
			Assert.AreEqual (null, (string) a, "string");
		}

		[Test]
		public void Value ()
		{
			// based on bug #360858
			XElement a = new XElement("root",
				new XElement ("foo"),
				"Linux&Windows",
				new XComment ("comment"),
				new XElement ("bar"));
			Assert.AreEqual ("Linux&Windows", a.Value);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetValueXAttribute ()
		{
			new XElement ("foo").SetValue (new XAttribute ("foo", "bar"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetValueXDocumnent ()
		{
			new XElement ("foo").SetValue (new XDocument ());
		}

		[Test]
		// LAMESPEC: there is no reason to not reject XDeclaration while it rejects XDocument.
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotDotNet")]
		public void SetValueXDeclaration ()
		{
			var el = new XElement ("foo");
			el.SetValue (new XDeclaration ("1.0", null, null));
			Assert.AreEqual ("<?xml version=\"1.0\"?>", el.Value);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetValueNull ()
		{
			new XElement ("foo", "text").SetValue (null);
		}

		[Test]
		public void AddSameInstance () // bug #392063
		{
			XElement root = new XElement (XName.Get ("Root", ""));
			XElement child = new XElement (XName.Get ("Child", ""));

			root.Add (child);
			root.Add (child);
			root.ToString ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddSameInstance2 ()
		{
			XElement root = new XElement (XName.Get ("Root"));
			XAttribute attr = new XAttribute (XName.Get ("a"), "v");

			root.Add (attr);
			root.Add (attr); // duplicate attribute
			root.ToString ();
		}

		[Test]
		public void AddAttributeFromDifferentTree ()
		{
			XElement e1 = new XElement (XName.Get ("e1"));
			XElement e2 = new XElement (XName.Get ("e2"));
			XAttribute attr = new XAttribute (XName.Get ("a"), "v");

			e1.Add (attr);
			e2.Add (attr);
			Assert.AreEqual ("<e1 a=\"v\" />", e1.ToString (), "#1");
			Assert.AreEqual ("<e2 a=\"v\" />", e2.ToString (), "#2");
		}

		[Test]
		public void SavePreservePrefixes ()
		{
			var x = XDocument.Parse (@"
			<xxx:a xmlns:xxx='http://www.foobar.com'>
    <xxx:b>blah blah blah</xxx:b>
</xxx:a>");
			StringWriter sw = new StringWriter ();
			x.Save (sw, SaveOptions.DisableFormatting);
			Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?><xxx:a xmlns:xxx=""http://www.foobar.com""><xxx:b>blah blah blah</xxx:b></xxx:a>", sw.ToString ());
		}

		[Test]
		public void LoadFromXmlTextReader ()
		{
			var foo = XElement.Load (new XmlTextReader (new StringReader ("<foo></foo>")));
			Assert.IsNotNull (foo);
		}
	}
}
