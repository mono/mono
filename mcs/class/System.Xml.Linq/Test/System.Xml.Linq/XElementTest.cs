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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XElementTest
	{

		[Test]
		public void Constructor_NullParameters()
		{
			AssertThrows<ArgumentNullException>(() => new XElement((XName)null), "#1");
			AssertThrows<ArgumentNullException>(() => new XElement((XElement)null), "#2");
			AssertThrows<ArgumentNullException>(() => new XElement((XStreamingElement)null), "#3");
			AssertThrows<ArgumentNullException>(() => new XElement((XName)null, null), "#4");
			AssertThrows<ArgumentNullException>(() => new XElement((XName)null, null, null, null), "#5");

			// This is acceptable though
			new XElement(XName.Get("foo"), null);
		}

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
		public void Rename()
		{
			bool changed = false;
			bool changing = false;
			var element = new XElement("foo");
			element.Changing += (o, e) => {
				Assert.IsFalse (changing, "#1");
				Assert.IsFalse (changed, "#2");
				Assert.AreSame (element, o, "#3");
				Assert.AreEqual (XObjectChange.Name, e.ObjectChange, "#4");
				changing = true;
			};

			element.Changed += (o, e) => {
				Assert.IsTrue (changing, "#5");
				Assert.IsFalse (changed, "#6");
				Assert.AreSame (element, o, "#7");
				Assert.AreEqual (XObjectChange.Name, e.ObjectChange, "#8");
				changed = true;
			};

			element.Name = "bar";
			Assert.AreEqual("bar", element.Name.LocalName, "#name");
			Assert.IsTrue(changed, "changed");
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
		public void RemoveElement_FromChildNode_ChangeTriggers()
		{
			var childChanging = false;
			var childChanged = false;
			var rootChanging = false;
			var rootChanged = false;
			
			var subchild = new XElement("subfoo");
			var child = new XElement("foo", subchild);
			var root = new XElement("root", child);
			
			child.Changing += (o, e) => {
				Assert.IsFalse(childChanging, "#c1");
				Assert.IsFalse(childChanged, "#c2");
				Assert.IsFalse(rootChanging, "#c3");
				Assert.IsFalse(rootChanged, "#c4");
				Assert.AreSame(subchild, o, "#c5");
				Assert.AreEqual(XObjectChange.Remove, e.ObjectChange, "#c6");
				Assert.IsNotNull(subchild.Parent, "childChangingParent");
				childChanging = true;
			};
			root.Changing += (o, e) => {
				Assert.IsTrue(childChanging, "#r1");
				Assert.IsFalse(childChanged, "#r2");
				Assert.IsFalse(rootChanging, "#r3");
				Assert.IsFalse(rootChanged, "#r4");
				Assert.AreSame(subchild, o, "#r5");
				Assert.AreEqual(XObjectChange.Remove, e.ObjectChange, "#r6");
				Assert.IsNotNull(subchild.Parent, "rootChangingParent");
				rootChanging = true;
			};
			child.Changed += (o, e) =>  {
				Assert.IsTrue(childChanging, "#c7");
				Assert.IsFalse(childChanged, "#c8");
				Assert.IsTrue(rootChanging, "#c9");
				Assert.IsFalse(rootChanged, "#c10");
				Assert.AreSame(subchild, o, "#c11");
				Assert.AreEqual(XObjectChange.Remove, e.ObjectChange, "#c12");
				Assert.IsNull(subchild.Parent, "childChangedParent");
				childChanged = true;
			};
			root.Changed += (o, e) => {
				Assert.IsTrue(childChanging, "#r7");
				Assert.IsTrue(childChanged, "#r8");
				Assert.IsTrue(rootChanging, "#r9");
				Assert.IsFalse(rootChanged, "#r10");
				Assert.AreSame(subchild, o, "#11");
				Assert.AreEqual(XObjectChange.Remove, e.ObjectChange, "#12");
				Assert.IsNull(subchild.Parent, "rootChangedParent");
				rootChanged = true;
			};
			
			subchild.Remove();
			Assert.IsTrue(childChanging, "#a");
			Assert.IsTrue(childChanged, "#b");
			Assert.IsTrue(rootChanging, "#c");
			Assert.IsTrue(rootChanged, "#d");
		}

		[Test]
		public void RemoveElement_FromRootNode_ChangeTriggers()
		{
			var childChanging = false;
			var childChanged = false;
			var rootChanging = false;
			var rootChanged = false;
			
			var child = new XElement ("foo");
			var root = new XElement ("root", child);
			child.Changing += (o, e) => childChanging = true;
			child.Changed += (o, e) => childChanged = true;
			
			root.Changing += (o, e) => {
				Assert.IsFalse(rootChanging, "#1");
				Assert.IsFalse(rootChanged, "#2");
				Assert.AreSame (child, o, "#3");
				Assert.AreEqual(XObjectChange.Remove, e.ObjectChange, "#4");
				rootChanging = true;
			};
			root.Changed += (o, e) =>  {
				Assert.IsFalse(rootChanged, "#5");
				Assert.IsTrue(rootChanging, "#6");
				Assert.AreSame(child, o, "#7");
				Assert.AreEqual(XObjectChange.Remove, e.ObjectChange, "#8");
				rootChanged = true;
			};
			
			child.Remove();
			Assert.IsFalse(childChanging, "#9");
			Assert.IsFalse(childChanged, "#10");
			Assert.IsTrue(rootChanging, "#11");
			Assert.IsTrue(rootChanged, "#12");
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

#pragma warning disable 219
		[Test]
		public void CastNulls ()
		{
			const XElement a = null;

			Assert.AreEqual (null, (bool?) a, "bool?");
			Assert.AreEqual (null, (DateTime?) a, "DateTime?");
			Assert.AreEqual (null, (DateTimeOffset?) a, "DateTimeOffset?");
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
			AssertThrows<ArgumentNullException> (() => { bool z = (bool) a; }, "bool");
			AssertThrows<ArgumentNullException> (() => { DateTime z = (DateTime) a; }, "DateTime");
			AssertThrows<ArgumentNullException> (() => { DateTimeOffset z = (DateTimeOffset) a; }, "DateTimeOffset");
			AssertThrows<ArgumentNullException> (() => { decimal z = (decimal) a; }, "decimal");
			AssertThrows<ArgumentNullException> (() => { double z = (double) a; }, "double");
			AssertThrows<ArgumentNullException> (() => { float z = (float) a; }, "float");
			AssertThrows<ArgumentNullException> (() => { Guid z = (Guid) a; }, "Guid");
			AssertThrows<ArgumentNullException> (() => { int z = (int) a; }, "int");
			AssertThrows<ArgumentNullException> (() => { long z = (long) a; }, "long");
			AssertThrows<ArgumentNullException> (() => { uint z = (uint) a; }, "uint");
			AssertThrows<ArgumentNullException> (() => { ulong z = (ulong) a; }, "ulong");
			AssertThrows<ArgumentNullException> (() => { TimeSpan z = (TimeSpan) a; }, "TimeSpan");
		}

		/// <remarks>
		/// Provides functionality similar to Assert.Throws that is available on newer versions of NUnit.
		/// </remarks>
		private static T AssertThrows<T> (Action code, string message, params object[] args) where T : Exception
		{
			Exception actual = null;
			try {
				code ();
			} catch (Exception exception) {
				actual = exception;
			}
			Assert.That (actual, new NUnit.Framework.Constraints.ExactTypeConstraint (typeof (T)), message, args);
			return (T) actual;
		}

		[Test]
		public void CastEmpties ()
		{
			XElement a = new XElement ("a");

			// Verify expected "cloning" and "empty" behaviour as prerequisites
			Assert.IsTrue (a.IsEmpty, "#1-1");
			Assert.IsTrue (new XElement (a).IsEmpty, "#1-2");
			Assert.AreEqual (String.Empty, a.Value, "#2-1");
			Assert.AreEqual (String.Empty, new XElement (a).Value, "#2-2");
			Assert.AreNotSame (a, new XElement (a), "#3-1");
			Assert.AreEqual (a.ToString (), new XElement (a).ToString (), "#3-2");
			Assert.AreEqual ("<a />", a.ToString (), "#3-3");
			Assert.AreEqual (a.ToString (), new XElement ("a", null).ToString (), "#3-4");

			// Execute the primary assertions of this test
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (a); }, "bool?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (a); }, "DateTime?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (a); }, "DateTimeOffset?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XElement (a); }, "decimal?");
			AssertThrows<FormatException> (() => { double? z = (double?) new XElement (a); }, "double?");
			AssertThrows<FormatException> (() => { float? z = (float?) new XElement (a); }, "float?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (a); }, "Guid?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XElement (a); }, "int?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XElement (a); }, "long?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XElement (a); }, "uint?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XElement (a); }, "ulong?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (a); }, "TimeSpan?");
			Assert.AreEqual (String.Empty, (string) new XElement (a), "string");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (a); }, "bool");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (a); }, "DateTime");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (a); }, "DateTimeOffset");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XElement (a); }, "decimal");
			AssertThrows<FormatException> (() => { double z = (double) new XElement (a); }, "double");
			AssertThrows<FormatException> (() => { float z = (float) new XElement (a); }, "float");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (a); }, "Guid");
			AssertThrows<FormatException> (() => { int z = (int) new XElement (a); }, "int");
			AssertThrows<FormatException> (() => { long z = (long) new XElement (a); }, "long");
			AssertThrows<FormatException> (() => { uint z = (uint) new XElement (a); }, "uint");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XElement (a); }, "ulong");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (a); }, "TimeSpan");
		}

		[Test]
		public void CastBlanks ()
		{
			XElement a = new XElement ("a", String.Empty);
			XElement b = new XElement ("b", new XCData (string.Empty));

			// Verify expected "cloning" and "blank" behaviour as prerequisites
			Assert.IsFalse (a.IsEmpty, "#1-1a");
			Assert.IsFalse (b.IsEmpty, "#1-1b");
			Assert.IsFalse (new XElement (a).IsEmpty, "#1-2a");
			Assert.IsFalse (new XElement (b).IsEmpty, "#1-2b");
			Assert.AreEqual (String.Empty, a.Value, "#2-1a");
			Assert.AreEqual (String.Empty, b.Value, "#2-1b");
			Assert.AreEqual (String.Empty, new XElement (a).Value, "#2-2a");
			Assert.AreEqual (String.Empty, new XElement (b).Value, "#2-2b");
			Assert.AreNotSame (a, new XElement (a), "#3-1a");
			Assert.AreNotSame (b, new XElement (b), "#3-1b");
			Assert.AreEqual (a.ToString (), new XElement (a).ToString (), "#3-2a");
			Assert.AreEqual (b.ToString (), new XElement (b).ToString (), "#3-2b");
			Assert.AreEqual ("<a></a>", a.ToString (), "#3-3a");
			Assert.AreEqual ("<b><![CDATA[]]></b>", b.ToString (), "#3-3b");
			Assert.AreEqual (a.ToString (), new XElement ("a", "").ToString (), "#3-4a");
			Assert.AreEqual (b.ToString (), new XElement ("b", new XCData ("")).ToString (), "#3-4b");

			// Execute the primary assertions of this test
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (a); }, "a:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (b); }, "b:bool?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (a); }, "a:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (b); }, "b:DateTime?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (a); }, "a:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (b); }, "b:DateTimeOffset?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XElement (a); }, "a:decimal?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XElement (b); }, "b:decimal?");
			AssertThrows<FormatException> (() => { double? z = (double?) new XElement (a); }, "a:double?");
			AssertThrows<FormatException> (() => { double? z = (double?) new XElement (b); }, "b:double?");
			AssertThrows<FormatException> (() => { float? z = (float?) new XElement (a); }, "a:float?");
			AssertThrows<FormatException> (() => { float? z = (float?) new XElement (b); }, "b:float?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (a); }, "a:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (b); }, "b:Guid?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XElement (a); }, "a:int?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XElement (b); }, "b:int?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XElement (a); }, "a:long?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XElement (b); }, "b:long?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XElement (a); }, "a:uint?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XElement (b); }, "b:uint?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XElement (a); }, "a:ulong?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XElement (b); }, "b:ulong?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (a); }, "a:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (b); }, "b:TimeSpan?");
			Assert.AreEqual (String.Empty, (string) new XElement (a), "a:string");
			Assert.AreEqual (String.Empty, (string) new XElement (b), "b:string");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (a); }, "a:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (b); }, "b:bool");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (a); }, "a:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (b); }, "b:DateTime");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (a); }, "a:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (b); }, "b:DateTimeOffset");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XElement (a); }, "a:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XElement (b); }, "b:decimal");
			AssertThrows<FormatException> (() => { double z = (double) new XElement (a); }, "a:double");
			AssertThrows<FormatException> (() => { double z = (double) new XElement (b); }, "b:double");
			AssertThrows<FormatException> (() => { float z = (float) new XElement (a); }, "a:float");
			AssertThrows<FormatException> (() => { float z = (float) new XElement (b); }, "b:float");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (a); }, "a:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (b); }, "b:Guid");
			AssertThrows<FormatException> (() => { int z = (int) new XElement (a); }, "a:int");
			AssertThrows<FormatException> (() => { int z = (int) new XElement (b); }, "b:int");
			AssertThrows<FormatException> (() => { long z = (long) new XElement (a); }, "a:long");
			AssertThrows<FormatException> (() => { long z = (long) new XElement (b); }, "b:long");
			AssertThrows<FormatException> (() => { uint z = (uint) new XElement (a); }, "a:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XElement (b); }, "b:uint");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XElement (a); }, "a:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XElement (b); }, "b:ulong");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (a); }, "a:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (b); }, "b:TimeSpan");
		}

		[Test]
		public void CastSpaces ()
		{
			XElement a = new XElement ("a", " ");
			XElement b = new XElement ("b", new XCData (" "));

			// Verify expected "cloning" and "space" behaviour as prerequisites
			Assert.IsFalse (a.IsEmpty, "#1-1a");
			Assert.IsFalse (b.IsEmpty, "#1-1b");
			Assert.IsFalse (new XElement (a).IsEmpty, "#1-2a");
			Assert.IsFalse (new XElement (b).IsEmpty, "#1-2b");
			Assert.AreEqual (" ", a.Value, "#2-1a");
			Assert.AreEqual (" ", b.Value, "#2-1b");
			Assert.AreEqual (" ", new XElement (a).Value, "#2-2a");
			Assert.AreEqual (" ", new XElement (b).Value, "#2-2b");
			Assert.AreNotSame (a, new XElement (a), "#3-1a");
			Assert.AreNotSame (b, new XElement (b), "#3-1b");
			Assert.AreEqual (a.ToString (), new XElement (a).ToString (), "#3-2a");
			Assert.AreEqual (b.ToString (), new XElement (b).ToString (), "#3-2b");
			Assert.AreEqual ("<a> </a>", a.ToString (), "#3-3a");
			Assert.AreEqual ("<b><![CDATA[ ]]></b>", b.ToString (), "#3-3b");
			Assert.AreEqual (a.ToString (), new XElement ("a", ' ').ToString (), "#3-4");

			// Execute the primary assertions of this test
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (a); }, "a:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (b); }, "b:bool?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (a); }, "a:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (b); }, "b:DateTime?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (a); }, "a:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (b); }, "b:DateTimeOffset?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XElement (a); }, "a:decimal?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XElement (b); }, "b:decimal?");
			AssertThrows<FormatException> (() => { double? z = (double?) new XElement (a); }, "a:double?");
			AssertThrows<FormatException> (() => { double? z = (double?) new XElement (b); }, "b:double?");
			AssertThrows<FormatException> (() => { float? z = (float?) new XElement (a); }, "a:float?");
			AssertThrows<FormatException> (() => { float? z = (float?) new XElement (b); }, "b:float?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (a); }, "a:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (b); }, "b:Guid?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XElement (a); }, "a:int?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XElement (b); }, "b:int?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XElement (a); }, "a:long?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XElement (b); }, "b:long?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XElement (a); }, "a:uint?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XElement (b); }, "b:uint?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XElement (a); }, "a:ulong?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XElement (b); }, "b:ulong?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (a); }, "a:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (b); }, "b:TimeSpan?");
			Assert.AreEqual (" ", (string) new XElement (a), "a:string");
			Assert.AreEqual (" ", (string) new XElement (b), "b:string");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (a); }, "a:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (b); }, "b:bool");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (a); }, "a:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (b); }, "b:DateTime");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (a); }, "a:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (b); }, "b:DateTimeOffset");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XElement (a); }, "a:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XElement (b); }, "b:decimal");
			AssertThrows<FormatException> (() => { double z = (double) new XElement (a); }, "a:double");
			AssertThrows<FormatException> (() => { double z = (double) new XElement (b); }, "b:double");
			AssertThrows<FormatException> (() => { float z = (float) new XElement (a); }, "a:float");
			AssertThrows<FormatException> (() => { float z = (float) new XElement (b); }, "b:float");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (a); }, "a:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (b); }, "b:Guid");
			AssertThrows<FormatException> (() => { int z = (int) new XElement (a); }, "a:int");
			AssertThrows<FormatException> (() => { int z = (int) new XElement (b); }, "b:int");
			AssertThrows<FormatException> (() => { long z = (long) new XElement (a); }, "a:long");
			AssertThrows<FormatException> (() => { long z = (long) new XElement (b); }, "b:long");
			AssertThrows<FormatException> (() => { uint z = (uint) new XElement (a); }, "a:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XElement (b); }, "b:uint");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XElement (a); }, "a:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XElement (b); }, "b:ulong");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (a); }, "a:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (b); }, "b:TimeSpan");
		}

		[Test]
		public void CastNumbers ()
		{
			XElement a = new XElement ("a", "7");
			XElement b = new XElement ("b", new XCData ("  42 "));
			XElement c = new XElement ("c", " \r\n   13 \t  ");
			XElement d = new XElement ("d", -101);
			XElement o = new XElement ("o", "0");
			XElement l = new XElement ("l", "1");
			XElement I = new XElement ("I", "INF");
			XElement i = new XElement ("i", " Infinity  ");
			XElement M = new XElement ("M", "   -INF ");
			XElement m = new XElement ("m", "-Infinity");
			XElement n = new XElement ("n", "\t NaN   ");

			// Verify expected "cloning" and basic conversion behaviour as prerequisites
			Assert.IsFalse (a.IsEmpty, "#1-1");
			Assert.IsFalse (new XElement (b).IsEmpty, "#1-2");
			Assert.AreEqual (" \r\n   13 \t  ", c.Value, "#2-1");
			Assert.AreEqual ("-101", new XElement (d).Value, "#2-2");
			Assert.AreNotSame (o, new XElement (o), "#3-1");
			Assert.AreEqual (l.ToString (), new XElement (l).ToString (), "#3-2");
			Assert.AreEqual ("<a>7</a>", a.ToString (), "#3-3a");
			Assert.AreEqual ("<b><![CDATA[  42 ]]></b>", b.ToString (), "#3-3b");
			Assert.AreEqual ("<c> \r\n   13 \t  </c>", c.ToString (), "#3-3c");
			Assert.AreEqual ("<d>-101</d>", d.ToString (), "#3-3d");
			Assert.AreEqual ("<o>0</o>", new XElement ("o", 0.0).ToString (), "#3-3o");
			Assert.AreEqual ("<l>1</l>", new XElement ("l", 1.0f).ToString (), "#3-3l");
			Assert.AreEqual ("<n>NaN</n>", new XElement ("n", double.NaN).ToString (), "#3-3n");
			Assert.AreEqual (a.ToString (), new XElement ("a", '7').ToString (), "#3-4a");
			Assert.AreEqual (d.ToString (), new XElement ("d", "-101").ToString (), "#3-4d");
			Assert.AreEqual (o.ToString (), new XElement ("o", 0L).ToString (), "#3-4o");
			Assert.AreEqual (l.ToString (), new XElement ("l", 1m).ToString (), "#3-4l");

			// Execute the primary assertions of this test
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (a); }, "a:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (b); }, "b:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (c); }, "c:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (d); }, "d:bool?");
			Assert.IsNotNull ((bool?) new XElement (o), "o:bool?:null");
			Assert.AreEqual (false, ((bool?) new XElement (o)).Value, "o:bool?:value");
			Assert.IsNotNull ((bool?) new XElement (l), "l:bool?:null");
			Assert.AreEqual (true, ((bool?) new XElement (l)).Value, "l:bool?:value");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (I); }, "I:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (i); }, "i:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (M); }, "M:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (m); }, "m:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XElement (n); }, "n:bool?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (a); }, "a:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (b); }, "b:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (c); }, "c:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (d); }, "d:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (o); }, "o:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (l); }, "l:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (I); }, "I:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (i); }, "i:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (M); }, "M:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (m); }, "m:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XElement (n); }, "n:DateTime?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (a); }, "a:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (b); }, "b:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (c); }, "c:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (d); }, "d:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (o); }, "o:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (l); }, "l:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (I); }, "I:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (i); }, "i:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (M); }, "M:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (m); }, "m:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XElement (n); }, "n:DateTimeOffset?");
			Assert.IsNotNull ((decimal?) new XElement (a), "a:decimal?:null");
			Assert.AreEqual (7m, ((decimal?) new XElement (a)).Value, "a:decimal?:value");
			Assert.IsNotNull ((decimal?) new XElement (b), "b:decimal?:null");
			Assert.AreEqual (42m, ((decimal?) new XElement (b)).Value, "b:decimal?:value");
			Assert.IsNotNull ((decimal?) new XElement (c), "c:decimal?:null");
			Assert.AreEqual (13m, ((decimal?) new XElement (c)).Value, "c:decimal?:value");
			Assert.IsNotNull ((decimal?) new XElement (d), "d:decimal?:null");
			Assert.AreEqual (-101m, ((decimal?) new XElement (d)).Value, "d:decimal?:value");
			Assert.IsNotNull ((decimal?) new XElement (o), "o:decimal?:null");
			Assert.AreEqual (0m, ((decimal?) new XElement (o)).Value, "o:decimal?:value");
			Assert.IsNotNull ((decimal?) new XElement (l), "l:decimal?:null");
			Assert.AreEqual (1m, ((decimal?) new XElement (l)).Value, "l:decimal?:value");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XElement (I); }, "I:decimal?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XElement (i); }, "i:decimal?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XElement (M); }, "M:decimal?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XElement (m); }, "m:decimal?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XElement (n); }, "n:decimal?");
			Assert.IsNotNull ((double?) new XElement (a), "a:double?:null");
			Assert.AreEqual (7d, ((double?) new XElement (a)).Value, "a:double?:value");
			Assert.IsNotNull ((double?) new XElement (b), "b:double?:null");
			Assert.AreEqual (42d, ((double?) new XElement (b)).Value, "b:double?:value");
			Assert.IsNotNull ((double?) new XElement (c), "c:double?:null");
			Assert.AreEqual (13d, ((double?) new XElement (c)).Value, "c:double?:value");
			Assert.IsNotNull ((double?) new XElement (d), "d:double?:null");
			Assert.AreEqual (-101d, ((double?) new XElement (d)).Value, "d:double?:value");
			Assert.IsNotNull ((double?) new XElement (o), "o:double?:null");
			Assert.AreEqual (0d, ((double?) new XElement (o)).Value, "o:double?:value");
			Assert.IsNotNull ((double?) new XElement (l), "l:double?:null");
			Assert.AreEqual (1d, ((double?) new XElement (l)).Value, "l:double?:value");
			Assert.IsNotNull ((double?) new XElement (I), "I:double?:null");
			Assert.AreEqual (double.PositiveInfinity, ((double?) new XElement (I)).Value, "I:double?:value");
			Assert.IsNotNull ((double?) new XElement (i), "i:double?:null");
			Assert.AreEqual (double.PositiveInfinity, ((double?) new XElement (i)).Value, "i:double?:value");
			Assert.IsNotNull ((double?) new XElement (M), "M:double?:null");
			Assert.AreEqual (double.NegativeInfinity, ((double?) new XElement (M)).Value, "M:double?:value");
			Assert.IsNotNull ((double?) new XElement (m), "m:double?:null");
			Assert.AreEqual (double.NegativeInfinity, ((double?) new XElement (m)).Value, "m:double?:value");
			Assert.IsNotNull ((double?) new XElement (n), "n:double?:null");
			Assert.AreEqual (double.NaN, ((double?) new XElement (n)).Value, "n:double?:value");
			Assert.IsNotNull ((float?) new XElement (a), "a:float?:null");
			Assert.AreEqual (7f, ((float?) new XElement (a)).Value, "a:float?:value");
			Assert.IsNotNull ((float?) new XElement (b), "b:float?:null");
			Assert.AreEqual (42f, ((float?) new XElement (b)).Value, "b:float?:value");
			Assert.IsNotNull ((float?) new XElement (c), "c:float?:null");
			Assert.AreEqual (13f, ((float?) new XElement (c)).Value, "c:float?:value");
			Assert.IsNotNull ((float?) new XElement (d), "d:float?:null");
			Assert.AreEqual (-101f, ((float?) new XElement (d)).Value, "d:float?:value");
			Assert.IsNotNull ((float?) new XElement (o), "o:float?:null");
			Assert.AreEqual (0f, ((float?) new XElement (o)).Value, "o:float?:value");
			Assert.IsNotNull ((float?) new XElement (l), "l:float?:null");
			Assert.AreEqual (1f, ((float?) new XElement (l)).Value, "l:float?:value");
			Assert.IsNotNull ((float?) new XElement (I), "I:float?:null");
			Assert.AreEqual (float.PositiveInfinity, ((float?) new XElement (I)).Value, "I:float?:value");
			Assert.IsNotNull ((float?) new XElement (i), "i:float?:null");
			Assert.AreEqual (float.PositiveInfinity, ((float?) new XElement (i)).Value, "i:float?:value");
			Assert.IsNotNull ((float?) new XElement (M), "M:float?:null");
			Assert.AreEqual (float.NegativeInfinity, ((float?) new XElement (M)).Value, "M:float?:value");
			Assert.IsNotNull ((float?) new XElement (m), "m:float?:null");
			Assert.AreEqual (float.NegativeInfinity, ((float?) new XElement (m)).Value, "m:float?:value");
			Assert.IsNotNull ((float?) new XElement (n), "n:float?:null");
			Assert.AreEqual (float.NaN, ((float?) new XElement (n)).Value, "n:float?:value");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (a); }, "a:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (b); }, "b:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (c); }, "c:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (d); }, "d:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (o); }, "o:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (l); }, "l:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (I); }, "I:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (i); }, "i:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (M); }, "M:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (m); }, "m:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XElement (n); }, "n:Guid?");
			Assert.IsNotNull ((int?) new XElement (a), "a:int?:null");
			Assert.AreEqual (7, ((int?) new XElement (a)).Value, "a:int?:value");
			Assert.IsNotNull ((int?) new XElement (b), "b:int?:null");
			Assert.AreEqual (42, ((int?) new XElement (b)).Value, "b:int?:value");
			Assert.IsNotNull ((int?) new XElement (c), "c:int?:null");
			Assert.AreEqual (13, ((int?) new XElement (c)).Value, "c:int?:value");
			Assert.IsNotNull ((int?) new XElement (d), "d:int?:null");
			Assert.AreEqual (-101, ((int?) new XElement (d)).Value, "d:int?:value");
			Assert.IsNotNull ((int?) new XElement (o), "o:int?:null");
			Assert.AreEqual (0, ((int?) new XElement (o)).Value, "o:int?:value");
			Assert.IsNotNull ((int?) new XElement (l), "l:int?:null");
			Assert.AreEqual (1, ((int?) new XElement (l)).Value, "l:int?:value");
			AssertThrows<FormatException> (() => { int? z = (int?) new XElement (I); }, "I:int?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XElement (i); }, "i:int?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XElement (M); }, "M:int?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XElement (m); }, "m:int?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XElement (n); }, "n:int?");
			Assert.IsNotNull ((long?) new XElement (a), "a:long?:null");
			Assert.AreEqual (7L, ((long?) new XElement (a)).Value, "a:long?:value");
			Assert.IsNotNull ((long?) new XElement (b), "b:long?:null");
			Assert.AreEqual (42L, ((long?) new XElement (b)).Value, "b:long?:value");
			Assert.IsNotNull ((long?) new XElement (c), "c:long?:null");
			Assert.AreEqual (13L, ((long?) new XElement (c)).Value, "c:long?:value");
			Assert.IsNotNull ((long?) new XElement (d), "d:long?:null");
			Assert.AreEqual (-101L, ((long?) new XElement (d)).Value, "d:long?:value");
			Assert.IsNotNull ((long?) new XElement (o), "o:long?:null");
			Assert.AreEqual (0L, ((long?) new XElement (o)).Value, "o:long?:value");
			Assert.IsNotNull ((long?) new XElement (l), "l:long?:null");
			Assert.AreEqual (1L, ((long?) new XElement (l)).Value, "l:long?:value");
			AssertThrows<FormatException> (() => { long? z = (long?) new XElement (I); }, "I:long?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XElement (i); }, "i:long?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XElement (M); }, "M:long?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XElement (m); }, "m:long?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XElement (n); }, "n:long?");
			Assert.IsNotNull ((uint?) new XElement (a), "a:uint?:null");
			Assert.AreEqual (7u, ((uint?) new XElement (a)).Value, "a:uint?:value");
			Assert.IsNotNull ((uint?) new XElement (b), "b:uint?:null");
			Assert.AreEqual (42u, ((uint?) new XElement (b)).Value, "b:uint?:value");
			Assert.IsNotNull ((uint?) new XElement (c), "c:uint?:null");
			Assert.AreEqual (13u, ((uint?) new XElement (c)).Value, "c:uint?:value");
			// LAMESPEC: see XmlConvertTests.ToUInt32().
			//AssertThrows<FormatException> (() => { uint? z = (uint?) new XElement (d); }, "d:uint?");
			Assert.IsNotNull ((uint?) new XElement (o), "o:uint?:null");
			Assert.AreEqual (0u, ((uint?) new XElement (o)).Value, "o:uint?:value");
			Assert.IsNotNull ((uint?) new XElement (l), "l:uint?:null");
			Assert.AreEqual (1u, ((uint?) new XElement (l)).Value, "l:uint?:value");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XElement (I); }, "I:uint?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XElement (i); }, "i:uint?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XElement (M); }, "M:uint?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XElement (m); }, "m:uint?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XElement (n); }, "n:uint?");
			Assert.IsNotNull ((ulong?) new XElement (a), "a:ulong?:null");
			Assert.AreEqual (7UL, ((ulong?) new XElement (a)).Value, "a:ulong?:value");
			Assert.IsNotNull ((ulong?) new XElement (b), "b:ulong?:null");
			Assert.AreEqual (42UL, ((ulong?) new XElement (b)).Value, "b:ulong?:value");
			Assert.IsNotNull ((ulong?) new XElement (c), "c:ulong?:null");
			Assert.AreEqual (13UL, ((ulong?) new XElement (c)).Value, "c:ulong?:value");
			// LAMESPEC: see XmlConvertTests.ToUInt64().
			//AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XElement (d); }, "d:ulong?");
			Assert.IsNotNull ((ulong?) new XElement (o), "o:ulong?:null");
			Assert.AreEqual (0UL, ((ulong?) new XElement (o)).Value, "o:ulong?:value");
			Assert.IsNotNull ((ulong?) new XElement (l), "l:ulong?:null");
			Assert.AreEqual (1UL, ((ulong?) new XElement (l)).Value, "l:ulong?:value");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XElement (I); }, "I:ulong?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XElement (i); }, "i:ulong?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XElement (M); }, "M:ulong?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XElement (m); }, "m:ulong?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XElement (n); }, "n:ulong?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (a); }, "a:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (b); }, "b:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (c); }, "c:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (d); }, "d:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (o); }, "o:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (l); }, "l:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (I); }, "I:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (i); }, "i:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (M); }, "M:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (m); }, "m:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XElement (n); }, "n:TimeSpan?");
			Assert.AreEqual ("7", (string) new XElement (a), "a:string");
			Assert.AreEqual ("  42 ", (string) new XElement (b), "b:string");
			Assert.AreEqual (" \r\n   13 \t  ", (string) new XElement (c), "c:string");
			Assert.AreEqual ("-101", (string) new XElement (d), "d:string");
			Assert.AreEqual ("0", (string) new XElement (o), "o:string");
			Assert.AreEqual ("1", (string) new XElement (l), "l:string");
			Assert.AreEqual ("INF", (string) new XElement (I), "I:string");
			Assert.AreEqual (" Infinity  ", (string) new XElement (i), "i:string");
			Assert.AreEqual ("   -INF ", (string) new XElement (M), "M:string");
			Assert.AreEqual ("-Infinity", (string) new XElement (m), "m:string");
			Assert.AreEqual ("\t NaN   ", (string) new XElement (n), "n:string");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (a); }, "a:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (b); }, "b:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (c); }, "c:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (d); }, "d:bool");
			Assert.AreEqual (false, (bool) new XElement (o), "o:bool");
			Assert.AreEqual (true, (bool) new XElement (l), "l:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (I); }, "I:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (i); }, "i:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (M); }, "M:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (m); }, "m:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XElement (n); }, "n:bool");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (a); }, "a:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (b); }, "b:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (c); }, "c:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (d); }, "d:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (o); }, "o:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (l); }, "l:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (I); }, "I:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (i); }, "i:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (M); }, "M:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (m); }, "m:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XElement (n); }, "n:DateTime");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (a); }, "a:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (b); }, "b:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (c); }, "c:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (d); }, "d:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (o); }, "o:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (l); }, "l:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (I); }, "I:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (i); }, "i:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (M); }, "M:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (m); }, "m:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XElement (n); }, "n:DateTimeOffset");
			Assert.AreEqual (7m, (decimal) new XElement (a), "a:decimal");
			Assert.AreEqual (42m, (decimal) new XElement (b), "b:decimal");
			Assert.AreEqual (13m, (decimal) new XElement (c), "c:decimal");
			Assert.AreEqual (-101m, (decimal) new XElement (d), "d:decimal");
			Assert.AreEqual (0m, (decimal) new XElement (o), "o:decimal");
			Assert.AreEqual (1m, (decimal) new XElement (l), "l:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XElement (I); }, "I:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XElement (i); }, "i:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XElement (M); }, "M:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XElement (m); }, "m:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XElement (n); }, "n:decimal");
			Assert.AreEqual (7d, (double) new XElement (a), "a:double");
			Assert.AreEqual (42d, (double) new XElement (b), "b:double");
			Assert.AreEqual (13d, (double) new XElement (c), "c:double");
			Assert.AreEqual (-101d, (double) new XElement (d), "d:double");
			Assert.AreEqual (0d, (double) new XElement (o), "o:double");
			Assert.AreEqual (1d, (double) new XElement (l), "l:double");
			Assert.AreEqual (double.PositiveInfinity, (double) new XElement (I), "I:double");
			Assert.AreEqual (double.PositiveInfinity, (double) new XElement (i), "i:double");
			Assert.AreEqual (double.NegativeInfinity, (double) new XElement (M), "M:double");
			Assert.AreEqual (double.NegativeInfinity, (double) new XElement (m), "m:double");
			Assert.AreEqual (double.NaN, ((double) new XElement (n)), "n:double");
			Assert.AreEqual (7f, (float) new XElement (a), "a:float");
			Assert.AreEqual (42f, (float) new XElement (b), "b:float");
			Assert.AreEqual (13f, (float) new XElement (c), "c:float");
			Assert.AreEqual (-101f, (float) new XElement (d), "d:float");
			Assert.AreEqual (0f, (float) new XElement (o), "o:float");
			Assert.AreEqual (1f, (float) new XElement (l), "l:float");
			Assert.AreEqual (float.PositiveInfinity, (float) new XElement (I), "I:float");
			Assert.AreEqual (float.PositiveInfinity, (float) new XElement (i), "i:float");
			Assert.AreEqual (float.NegativeInfinity, (float) new XElement (M), "M:float");
			Assert.AreEqual (float.NegativeInfinity, (float) new XElement (m), "m:float");
			Assert.AreEqual (float.NaN, ((float) new XElement (n)), "n:float");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (a); }, "a:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (b); }, "b:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (c); }, "c:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (d); }, "d:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (o); }, "o:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (l); }, "l:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (I); }, "I:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (i); }, "i:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (M); }, "M:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (m); }, "m:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XElement (n); }, "n:Guid");
			Assert.AreEqual (7, (int) new XElement (a), "a:int");
			Assert.AreEqual (42, (int) new XElement (b), "b:int");
			Assert.AreEqual (13, (int) new XElement (c), "c:int");
			Assert.AreEqual (-101, (int) new XElement (d), "d:int");
			Assert.AreEqual (0, (int) new XElement (o), "o:int");
			Assert.AreEqual (1, (int) new XElement (l), "l:int");
			AssertThrows<FormatException> (() => { int z = (int) new XElement (I); }, "I:int");
			AssertThrows<FormatException> (() => { int z = (int) new XElement (i); }, "i:int");
			AssertThrows<FormatException> (() => { int z = (int) new XElement (M); }, "M:int");
			AssertThrows<FormatException> (() => { int z = (int) new XElement (m); }, "m:int");
			AssertThrows<FormatException> (() => { int z = (int) new XElement (n); }, "n:int");
			Assert.AreEqual (7L, (long) new XElement (a), "a:long");
			Assert.AreEqual (42L, (long) new XElement (b), "b:long");
			Assert.AreEqual (13L, (long) new XElement (c), "c:long");
			Assert.AreEqual (-101L, (long) new XElement (d), "d:long");
			Assert.AreEqual (0L, (long) new XElement (o), "o:long");
			Assert.AreEqual (1L, (long) new XElement (l), "l:long");
			AssertThrows<FormatException> (() => { long z = (long) new XElement (I); }, "I:long");
			AssertThrows<FormatException> (() => { long z = (long) new XElement (i); }, "i:long");
			AssertThrows<FormatException> (() => { long z = (long) new XElement (M); }, "M:long");
			AssertThrows<FormatException> (() => { long z = (long) new XElement (m); }, "m:long");
			AssertThrows<FormatException> (() => { long z = (long) new XElement (n); }, "n:long");
			Assert.AreEqual (7u, (uint) new XElement (a), "a:uint");
			Assert.AreEqual (42u, (uint) new XElement (b), "b:uint");
			Assert.AreEqual (13u, (uint) new XElement (c), "c:uint");
			// LAMESPEC: see XmlConvertTests.ToUInt32().
			//AssertThrows<FormatException> (() => { uint z = (uint) new XElement (d); }, "d:uint");
			Assert.AreEqual (0u, (uint) new XElement (o), "o:uint");
			Assert.AreEqual (1u, (uint) new XElement (l), "l:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XElement (I); }, "I:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XElement (i); }, "i:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XElement (M); }, "M:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XElement (m); }, "m:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XElement (n); }, "n:uint");
			Assert.AreEqual (7UL, (ulong) new XElement (a), "a:ulong");
			Assert.AreEqual (42UL, (ulong) new XElement (b), "b:ulong");
			Assert.AreEqual (13UL, (ulong) new XElement (c), "c:ulong");
			// LAMESPEC: see XmlConvertTests.ToUInt64().
			//AssertThrows<FormatException> (() => { ulong z = (ulong) new XElement (d); }, "d:ulong");
			Assert.AreEqual (0UL, (ulong) new XElement (o), "o:ulong");
			Assert.AreEqual (1UL, (ulong) new XElement (l), "l:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XElement (I); }, "I:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XElement (i); }, "i:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XElement (M); }, "M:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XElement (m); }, "m:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XElement (n); }, "n:ulong");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (a); }, "a:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (b); }, "b:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (c); }, "c:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (d); }, "d:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (o); }, "o:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (l); }, "l:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (I); }, "I:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (i); }, "i:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (M); }, "M:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (m); }, "m:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XElement (n); }, "n:TimeSpan");

			// Perform some round-trip tests with numbers
			XElement x;
			const decimal @decimal = -41051609414188012238960097189m;
			const double @double = 8.5506609919892972E+307d;
			const float @float = -1.70151961E+37f;
			const int @int = -1051251773;
			const long @long = 4596767133891939716L;
			const uint @uint = 4106628142u;
			const ulong @ulong = 10713797297298255927UL;
			x = new XElement ("x", @decimal);
			Assert.IsNotNull ((decimal?) new XElement (x), "x:decimal?:null");
			Assert.AreEqual (@decimal, ((decimal?) new XElement (x)).Value, "x:decimal?:value");
			Assert.AreEqual (@decimal, (decimal) new XElement (x), "x:decimal");
			x = new XElement ("x", @double);
			Assert.IsNotNull ((double?) new XElement (x), "x:double?:null");
			Assert.AreEqual (@double, ((double?) new XElement (x)).Value, "x:double?:value");
			Assert.AreEqual (@double, (double) new XElement (x), "x:double");
			x = new XElement ("x", @float);
			Assert.IsNotNull ((float?) new XElement (x), "x:float?:null");
			Assert.AreEqual (@float, ((float?) new XElement (x)).Value, "x:float?:value");
			Assert.AreEqual (@float, (float) new XElement (x), "x:float");
			x = new XElement ("x", @int);
			Assert.IsNotNull ((int?) new XElement (x), "x:int?:null");
			Assert.AreEqual (@int, ((int?) new XElement (x)).Value, "x:int?:value");
			Assert.AreEqual (@int, (int) new XElement (x), "x:int");
			x = new XElement ("x", @long);
			Assert.IsNotNull ((long?) new XElement (x), "x:long?:null");
			Assert.AreEqual (@long, ((long?) new XElement (x)).Value, "x:long?:value");
			Assert.AreEqual (@long, (long) new XElement (x), "x:long");
			x = new XElement ("x", @uint);
			Assert.IsNotNull ((uint?) new XElement (x), "x:uint?:null");
			Assert.AreEqual (@uint, ((uint?) new XElement (x)).Value, "x:uint?:value");
			Assert.AreEqual (@uint, (uint) new XElement (x), "x:uint");
			x = new XElement ("x", @ulong);
			Assert.IsNotNull ((ulong?) new XElement (x), "x:ulong?:null");
			Assert.AreEqual (@ulong, ((ulong?) new XElement (x)).Value, "x:ulong?:value");
			Assert.AreEqual (@ulong, (ulong) new XElement (x), "x:ulong");
			x = new XElement ("x", double.NaN);
			Assert.IsNotNull ((double?) new XElement (x), "NaN:double?:null");
			Assert.AreEqual (double.NaN, ((double?) new XElement (x)).Value, "NaN:double?:value");
			Assert.AreEqual (double.NaN, (double) new XElement (x), "NaN:double");
			x = new XElement ("x", float.NaN);
			Assert.IsNotNull ((float?) new XElement (x), "NaN:float?:null");
			Assert.AreEqual (float.NaN, ((float?) new XElement (x)).Value, "NaN:float?:value");
			Assert.AreEqual (float.NaN, (float) new XElement (x), "NaN:float");
			x = new XElement ("x", double.PositiveInfinity);
			Assert.IsNotNull ((double?) new XElement (x), "+Inf:double?:null");
			Assert.AreEqual (double.PositiveInfinity, ((double?) new XElement (x)).Value, "+Inf:double?:value");
			Assert.AreEqual (double.PositiveInfinity, (double) new XElement (x), "+Inf:double");
			x = new XElement ("x", float.PositiveInfinity);
			Assert.IsNotNull ((float?) new XElement (x), "+Inf:float?:null");
			Assert.AreEqual (float.PositiveInfinity, ((float?) new XElement (x)).Value, "+Inf:float?:value");
			Assert.AreEqual (float.PositiveInfinity, (float) new XElement (x), "+Inf:float");
			x = new XElement ("x", double.NegativeInfinity);
			Assert.IsNotNull ((double?) new XElement (x), "-Inf:double?:null");
			Assert.AreEqual (double.NegativeInfinity, ((double?) new XElement (x)).Value, "-Inf:double?:value");
			Assert.AreEqual (double.NegativeInfinity, (double) new XElement (x), "-Inf:double");
			x = new XElement ("x", float.NegativeInfinity);
			Assert.IsNotNull ((float?) new XElement (x), "-Inf:float?:null");
			Assert.AreEqual (float.NegativeInfinity, ((float?) new XElement (x)).Value, "-Inf:float?:value");
			Assert.AreEqual (float.NegativeInfinity, (float) new XElement (x), "-Inf:float");

			// Perform overflow tests with numbers
			AssertThrows<OverflowException> (() => { decimal z = (decimal) new XElement ("z", "91051609414188012238960097189"); }, "z:decimal");
			AssertThrows<OverflowException> (() => { decimal? z = (decimal?) new XElement ("z", "91051609414188012238960097189"); }, "z:decimal?");
			AssertThrows<OverflowException> (() => { double z = (double) new XElement ("z", "8.5506609919892972E+654"); }, "z:double");
			AssertThrows<OverflowException> (() => { double? z = (double?) new XElement ("z", "8.5506609919892972E+654"); }, "z:double?");
			AssertThrows<OverflowException> (() => { float z = (float) new XElement ("z", @double); }, "z:float");
			AssertThrows<OverflowException> (() => { float? z = (float?) new XElement ("z", @double); }, "z:float?");
			AssertThrows<OverflowException> (() => { int z = (int) new XElement ("z", @long); }, "z:int");
			AssertThrows<OverflowException> (() => { int? z = (int?) new XElement ("z", @long); }, "z:int?");
			AssertThrows<OverflowException> (() => { long z = (long) new XElement ("z", @decimal); }, "z:long");
			AssertThrows<OverflowException> (() => { long? z = (long?) new XElement ("z", @decimal); }, "z:long?");
			AssertThrows<OverflowException> (() => { uint z = (uint) new XElement ("z", @ulong); }, "z:uint");
			AssertThrows<OverflowException> (() => { uint? z = (uint?) new XElement ("z", @ulong); }, "z:uint?");
			AssertThrows<OverflowException> (() => { ulong z = (ulong) new XElement ("z", -@decimal); }, "z:ulong");
			AssertThrows<OverflowException> (() => { ulong? z = (ulong?) new XElement ("z", -@decimal); }, "z:ulong?");
		}

		[Test]
		public void CastExtremes ()
		{
			// Test extremes/constants where round-trips should work in specific ways
			Assert.AreEqual (decimal.MaxValue, (decimal) new XElement ("k", decimal.MaxValue), "MaxValue:decimal");
			Assert.AreEqual (decimal.MinValue, (decimal) new XElement ("k", decimal.MinValue), "MinValue:decimal");
			Assert.AreEqual (decimal.MinusOne, (decimal) new XElement ("k", decimal.MinusOne), "MinusOne:decimal");
			Assert.AreEqual (decimal.One, (decimal) new XElement ("k", decimal.One), "One:decimal");
			Assert.AreEqual (decimal.Zero, (decimal) new XElement ("k", decimal.Zero), "Zero:decimal");
			Assert.AreEqual (double.MaxValue, (double) new XElement ("k", double.MaxValue), "MaxValue:double");
			Assert.AreEqual (double.MinValue, (double) new XElement ("k", double.MinValue), "MinValue:double");
			Assert.AreEqual (double.Epsilon, (double) new XElement ("k", double.Epsilon), "Epsilon:double");
			Assert.AreEqual (double.NaN, (double) new XElement ("k", double.NaN), "NaN:double");
			Assert.AreEqual (double.NegativeInfinity, (double) new XElement ("k", double.NegativeInfinity), "-Inf:double");
			Assert.AreEqual (double.PositiveInfinity, (double) new XElement ("k", double.PositiveInfinity), "+Inf:double");
			Assert.AreEqual (float.MaxValue, (float) new XElement ("k", float.MaxValue), "MaxValue:float");
			Assert.AreEqual (float.MinValue, (float) new XElement ("k", float.MinValue), "MinValue:float");
			Assert.AreEqual (float.Epsilon, (float) new XElement ("k", float.Epsilon), "Epsilon:float");
			Assert.AreEqual (float.NaN, (float) new XElement ("k", float.NaN), "NaN:float");
			Assert.AreEqual (float.NegativeInfinity, (float) new XElement ("k", float.NegativeInfinity), "-Inf:float");
			Assert.AreEqual (float.PositiveInfinity, (float) new XElement ("k", float.PositiveInfinity), "+Inf:float");
			Assert.AreEqual (int.MaxValue, (int) new XElement ("k", int.MaxValue), "MaxValue:int");
			Assert.AreEqual (int.MinValue, (int) new XElement ("k", int.MinValue), "MinValue:int");
			Assert.AreEqual (long.MaxValue, (long) new XElement ("k", long.MaxValue), "MaxValue:long");
			Assert.AreEqual (long.MinValue, (long) new XElement ("k", long.MinValue), "MinValue:long");
			Assert.AreEqual (uint.MaxValue, (uint) new XElement ("k", uint.MaxValue), "MaxValue:uint");
			Assert.AreEqual (uint.MinValue, (uint) new XElement ("k", uint.MinValue), "MinValue:uint");
			Assert.AreEqual (ulong.MaxValue, (ulong) new XElement ("k", ulong.MaxValue), "MaxValue:ulong");
			Assert.AreEqual (ulong.MinValue, (ulong) new XElement ("k", ulong.MinValue), "MinValue:ulong");
			Assert.AreEqual (decimal.MaxValue, (decimal?) new XElement ("k", decimal.MaxValue), "MaxValue:decimal?");
			Assert.AreEqual (decimal.MinValue, (decimal?) new XElement ("k", decimal.MinValue), "MinValue:decimal?");
			Assert.AreEqual (decimal.MinusOne, (decimal?) new XElement ("k", decimal.MinusOne), "MinusOne:decimal?");
			Assert.AreEqual (decimal.One, (decimal?) new XElement ("k", decimal.One), "One:decimal?");
			Assert.AreEqual (decimal.Zero, (decimal?) new XElement ("k", decimal.Zero), "Zero:decimal?");
			Assert.AreEqual (double.MaxValue, (double?) new XElement ("k", double.MaxValue), "MaxValue:double?");
			Assert.AreEqual (double.MinValue, (double?) new XElement ("k", double.MinValue), "MinValue:double?");
			Assert.AreEqual (double.Epsilon, (double?) new XElement ("k", double.Epsilon), "Epsilon:double?");
			Assert.AreEqual (double.NaN, (double?) new XElement ("k", double.NaN), "NaN:double?");
			Assert.AreEqual (double.NegativeInfinity, (double?) new XElement ("k", double.NegativeInfinity), "-Inf:double?");
			Assert.AreEqual (double.PositiveInfinity, (double?) new XElement ("k", double.PositiveInfinity), "+Inf:double?");
			Assert.AreEqual (float.MaxValue, (float?) new XElement ("k", float.MaxValue), "MaxValue:float?");
			Assert.AreEqual (float.MinValue, (float?) new XElement ("k", float.MinValue), "MinValue:float?");
			Assert.AreEqual (float.Epsilon, (float?) new XElement ("k", float.Epsilon), "Epsilon:float?");
			Assert.AreEqual (float.NaN, (float?) new XElement ("k", float.NaN), "NaN:float?");
			Assert.AreEqual (float.NegativeInfinity, (float?) new XElement ("k", float.NegativeInfinity), "-Inf:float?");
			Assert.AreEqual (float.PositiveInfinity, (float?) new XElement ("k", float.PositiveInfinity), "+Inf:float?");
			Assert.AreEqual (int.MaxValue, (int?) new XElement ("k", int.MaxValue), "MaxValue:int?");
			Assert.AreEqual (int.MinValue, (int?) new XElement ("k", int.MinValue), "MinValue:int?");
			Assert.AreEqual (long.MaxValue, (long?) new XElement ("k", long.MaxValue), "MaxValue:long?");
			Assert.AreEqual (long.MinValue, (long?) new XElement ("k", long.MinValue), "MinValue:long?");
			Assert.AreEqual (uint.MaxValue, (uint?) new XElement ("k", uint.MaxValue), "MaxValue:uint?");
			Assert.AreEqual (uint.MinValue, (uint?) new XElement ("k", uint.MinValue), "MinValue:uint?");
			Assert.AreEqual (ulong.MaxValue, (ulong?) new XElement ("k", ulong.MaxValue), "MaxValue:ulong?");
			Assert.AreEqual (ulong.MinValue, (ulong?) new XElement ("k", ulong.MinValue), "MinValue:ulong?");
			Assert.AreEqual (DateTime.MaxValue, (DateTime) new XElement ("k", DateTime.MaxValue), "MaxValue:DateTime");
			Assert.AreEqual (DateTime.MinValue, (DateTime) new XElement ("k", DateTime.MinValue), "MinValue:DateTime");
			Assert.AreEqual (DateTime.MaxValue, (DateTime?) new XElement ("k", DateTime.MaxValue), "MaxValue:DateTime?");
			Assert.AreEqual (DateTime.MinValue, (DateTime?) new XElement ("k", DateTime.MinValue), "MinValue:DateTime?");
			Assert.AreEqual (DateTimeOffset.MaxValue, (DateTimeOffset) new XElement ("k", DateTimeOffset.MaxValue), "MaxValue:DateTimeOffset");
			Assert.AreEqual (DateTimeOffset.MinValue, (DateTimeOffset) new XElement ("k", DateTimeOffset.MinValue), "MinValue:DateTimeOffset");
			Assert.AreEqual (DateTimeOffset.MaxValue, (DateTimeOffset?) new XElement ("k", DateTimeOffset.MaxValue), "MaxValue:DateTimeOffset?");
			Assert.AreEqual (DateTimeOffset.MinValue, (DateTimeOffset?) new XElement ("k", DateTimeOffset.MinValue), "MinValue:DateTimeOffset?");
			Assert.AreEqual (TimeSpan.MaxValue, (TimeSpan) new XElement ("k", TimeSpan.MaxValue), "MaxValue:TimeSpan");
			Assert.AreEqual (TimeSpan.MinValue, (TimeSpan) new XElement ("k", TimeSpan.MinValue), "MinValue:TimeSpan");
			Assert.AreEqual (TimeSpan.MaxValue, (TimeSpan?) new XElement ("k", TimeSpan.MaxValue), "MaxValue:TimeSpan?");
			Assert.AreEqual (TimeSpan.MinValue, (TimeSpan?) new XElement ("k", TimeSpan.MinValue), "MinValue:TimeSpan?");
		}

		[Test]
		public void CastBooleans ()
		{
			Assert.IsNotNull ((bool?) new XElement ("fq", "false"), "#1a");
			Assert.AreEqual (false, ((bool?) new XElement ("fq", "false")).Value, "#1b");
			Assert.IsNotNull ((bool?) new XElement ("tq", "true"), "#2a");
			Assert.AreEqual (true, ((bool?) new XElement ("tq", "true")).Value, "#2b");
			Assert.IsNotNull ((bool?) new XElement ("Fq", "False"), "#3a");
			Assert.AreEqual (false, ((bool?) new XElement ("Fq", "False")).Value, "#3b");
			Assert.IsNotNull ((bool?) new XElement ("Tq", "True"), "#4a");
			Assert.AreEqual (true, ((bool?) new XElement ("Tq", "True")).Value, "#4b");
			Assert.IsNotNull ((bool?) new XElement ("Fs", "   False \t \r "), "#5a");
			Assert.AreEqual (false, ((bool?) new XElement ("Fs", "   False \t \r ")).Value, "#5b");
			Assert.IsNotNull ((bool?) new XElement ("Ts", " \t True  \n  "), "#6a");
			Assert.AreEqual (true, ((bool?) new XElement ("Ts", " \t True  \n  ")).Value, "#6b");
			Assert.AreEqual (false, (bool) new XElement ("f", "false"), "#7");
			Assert.AreEqual (true, (bool) new XElement ("t", "true"), "#8");
			Assert.AreEqual (false, (bool) new XElement ("F", "False"), "#9");
			Assert.AreEqual (true, (bool) new XElement ("T", "True"), "#10");
			Assert.AreEqual (false, (bool)new XElement ("fs", " false  "), "#11");
			Assert.AreEqual (true, (bool)new XElement ("ts", "  true "), "#12");
			Assert.IsNotNull ((bool?) new XElement ("Tc", new XCData (" \t True  \n  ")), "#13a");
			Assert.AreEqual (true, ((bool?) new XElement ("Tc", new XCData (" \t True  \n  "))).Value, "#13b");
			Assert.AreEqual (false, (bool)new XElement ("fc", new XCData (" false  ")), "#14");
			Assert.IsNotNull ((bool?) new XElement ("x", true), "#15a");
			Assert.IsTrue (((bool?) new XElement ("x", true)).Value, "#15b");
			Assert.IsTrue ((bool) new XElement ("x", true), "#15c");
			Assert.IsNotNull ((bool?) new XElement ("x", false), "#16a");
			Assert.IsFalse (((bool?) new XElement ("x", false)).Value, "#16b");
			Assert.IsFalse ((bool) new XElement ("x", false), "#16c");
			Assert.IsTrue ((bool) new XElement ("x", bool.TrueString), "#17a");
			Assert.IsFalse ((bool) new XElement ("x", bool.FalseString), "#17b");
			Assert.IsTrue ((bool) new XElement ("x", new XCData (bool.TrueString)), "#18a");
			Assert.IsFalse ((bool) new XElement ("x", new XCData (bool.FalseString)), "#18b");
		}

		[Test]
		public void CastGuids ()
		{
			Guid rb = new Guid (new byte[16] { 0x9A, 0xBF, 0xCE, 0x7E, 0x07, 0x29, 0x9C, 0x43, 0x80, 0x7D, 0x48, 0x20, 0xB9, 0x19, 0xEA, 0x57 });
			Guid rd = new Guid (new byte[16] { 0x21, 0x5B, 0x57, 0x26, 0xCD, 0x14, 0x5E, 0x44, 0x8F, 0xFA, 0xE2, 0xBC, 0x24, 0x7B, 0x2E, 0xC9 });
			Guid rn = new Guid (new byte[16] { 0xF9, 0x46, 0x41, 0xA8, 0xA5, 0x03, 0xF1, 0x4A, 0xAD, 0x97, 0x7B, 0xC7, 0x79, 0x57, 0x2B, 0x79 });
			Guid rp = new Guid (new byte[16] { 0x51, 0x6B, 0x8A, 0x17, 0xEF, 0x11, 0xFB, 0x48, 0x83, 0xBD, 0x57, 0xB4, 0x99, 0xF9, 0xC1, 0xE6 });
			Guid rz = Guid.Empty;
			Guid rx = Guid.NewGuid ();

			XElement b = new XElement ("b", "  {7ECEBF9A-2907-439c-807D-4820B919EA57}");
			XElement d = new XElement ("d", "26575b21-14cd-445e-8ffa-e2bc247b2ec9");
			XElement n = new XElement ("n", "a84146f903A54af1ad977bC779572b79\r\n");
			XElement p = new XElement ("p", "  (178a6b51-11ef-48fb-83bd-57b499f9c1e6)  \t ");
			XElement z = new XElement ("z", " \t \n 00000000-0000-0000-0000-000000000000 ");
			XElement x = new XElement ("x", rx);

			Assert.IsNotNull ((Guid?) new XElement (b), "#1a");
			Assert.AreEqual (rb, ((Guid?) new XElement (b)).Value, "#1b");
			Assert.AreEqual (rb, (Guid) new XElement (b), "#1c");
			Assert.AreEqual (rb, (Guid) new XElement ("r", rb), "#1d");
			Assert.IsNotNull ((Guid?) new XElement ("r", rb), "#1e");
			Assert.AreEqual (rb, ((Guid?) new XElement ("r", rb)).Value, "#1f");

			Assert.IsNotNull ((Guid?) new XElement (d), "#2a");
			Assert.AreEqual (rd, ((Guid?) new XElement (d)).Value, "#2b");
			Assert.AreEqual (rd, (Guid) new XElement (d), "#2c");
			Assert.AreEqual (rd, (Guid) new XElement ("r", rd), "#2d");
			Assert.IsNotNull ((Guid?) new XElement ("r", rd), "#2e");
			Assert.AreEqual (rd, ((Guid?) new XElement ("r", rd)).Value, "#2f");

			Assert.IsNotNull ((Guid?) new XElement (n), "#3a");
			Assert.AreEqual (rn, ((Guid?) new XElement (n)).Value, "#3b");
			Assert.AreEqual (rn, (Guid) new XElement (n), "#3c");
			Assert.AreEqual (rn, (Guid) new XElement ("r", rn), "#3d");
			Assert.IsNotNull ((Guid?) new XElement ("r", rn), "#3e");
			Assert.AreEqual (rn, ((Guid?) new XElement ("r", rn)).Value, "#3f");

			Assert.IsNotNull ((Guid?) new XElement (p), "#4a");
			Assert.AreEqual (rp, ((Guid?) new XElement (p)).Value, "#4b");
			Assert.AreEqual (rp, (Guid) new XElement (p), "#4c");
			Assert.AreEqual (rp, (Guid) new XElement ("r", rp), "#4d");
			Assert.IsNotNull ((Guid?) new XElement ("r", rp), "#4e");
			Assert.AreEqual (rp, ((Guid?) new XElement ("r", rp)).Value, "#4f");

			Assert.IsNotNull ((Guid?) new XElement (z), "#5a");
			Assert.AreEqual (rz, ((Guid?) new XElement (z)).Value, "#5b");
			Assert.AreEqual (rz, (Guid) new XElement (z), "#5c");

			Assert.IsNotNull ((Guid?) new XElement (x), "#6a");
			Assert.AreEqual (rx, ((Guid?) new XElement (x)).Value, "#6b");
			Assert.AreEqual (rx, (Guid) new XElement (x), "#6c");
		}

		[Test]
		public void CastDateTimes ()
		{
			DateTime ra = new DateTime (1987, 1, 23, 21, 45, 36, 89, DateTimeKind.Unspecified);
			DateTime rb = new DateTime (2001, 2, 3, 4, 5, 6, 789, DateTimeKind.Local);
			DateTime rc = new DateTime (2010, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc);
			DateTime rd = new DateTime (1956, 11, 2, 0, 34, 0);
			DateTime re = new DateTime (635085111683456297L, DateTimeKind.Utc);
			DateTime rf = re.ToLocalTime ();
			DateTime rx = DateTime.Now;
			DateTime rz = DateTime.UtcNow;

			XElement a = new XElement ("a", "1987-01-23T21:45:36.089");
			XElement b = new XElement ("b", "2001-02-03T04:05:06.789" + rb.ToString ("zzz"));
			XElement c = new XElement ("c", "2010-01-02T00:00:00Z");
			XElement d = new XElement ("d", "  Nov 2, 1956  12:34 AM \r\n   \t");
			XElement e = new XElement ("e", "  2013-07-04T05:06:08.3456297Z   ");  // UTC, all the way
			XElement f = new XElement ("f", "  2013-07-04T05:06:08.3456297+00:00   ");  // UTC initially, but should be converted automatically to local time
			XElement x = new XElement ("x", rx);
			XElement z = new XElement ("z", rz);

			Assert.IsNotNull ((DateTime?) new XElement (a), "#1a");
			Assert.AreEqual (ra, ((DateTime?) new XElement (a)).Value, "#1b");
			Assert.AreEqual (ra, (DateTime) new XElement (a), "#1c");
			Assert.AreEqual (ra, (DateTime) new XElement ("r", ra), "#1d");
			Assert.IsNotNull ((DateTime?) new XElement ("r", ra), "#1e");
			Assert.AreEqual (ra, ((DateTime?) new XElement ("r", ra)).Value, "#1f");

			Assert.IsNotNull ((DateTime?) new XElement (b), "#2a");
			Assert.AreEqual (rb, ((DateTime?) new XElement (b)).Value, "#2b");
			Assert.AreEqual (rb, (DateTime) new XElement (b), "#2c");
			Assert.AreEqual (rb, (DateTime) new XElement ("r", rb), "#2d");
			Assert.IsNotNull ((DateTime?) new XElement ("r", rb), "#2e");
			Assert.AreEqual (rb, ((DateTime?) new XElement ("r", rb)).Value, "#2f");

			Assert.IsNotNull ((DateTime?) new XElement (c), "#3a");
			Assert.AreEqual (rc, ((DateTime?) new XElement (c)).Value, "#3b");
			Assert.AreEqual (rc, (DateTime) new XElement (c), "#3c");
			Assert.AreEqual (rc, (DateTime) new XElement ("r", rc), "#3d");
			Assert.IsNotNull ((DateTime?) new XElement ("r", rc), "#3e");
			Assert.AreEqual (rc, ((DateTime?) new XElement ("r", rc)).Value, "#3f");

			Assert.IsNotNull ((DateTime?) new XElement (d), "#4a");
			Assert.AreEqual (rd, ((DateTime?) new XElement (d)).Value, "#4b");
			Assert.AreEqual (rd, (DateTime) new XElement (d), "#4c");
			Assert.AreEqual (rd, (DateTime) new XElement ("r", rd), "#4d");
			Assert.IsNotNull ((DateTime?) new XElement ("r", rd), "#4e");
			Assert.AreEqual (rd, ((DateTime?) new XElement ("r", rd)).Value, "#4f");

			Assert.IsNotNull ((DateTime?) new XElement (x), "#5a");
			Assert.AreEqual (rx, ((DateTime?) new XElement (x)).Value, "#5b");
			Assert.AreEqual (rx, (DateTime) new XElement (x), "#5c");

			Assert.IsNotNull ((DateTime?) new XElement (z), "#6a");
			Assert.AreEqual (rz, ((DateTime?) new XElement (z)).Value, "#6b");
			Assert.AreEqual (rz, (DateTime) new XElement (z), "#6c");

			Assert.IsNotNull ((DateTime?) new XElement (e), "#7a");
			Assert.AreEqual (re, ((DateTime?) new XElement (e)).Value, "#7b");
			Assert.AreEqual (re, (DateTime) new XElement (e), "#7c");
			Assert.AreEqual (re, (DateTime) new XElement ("r", re), "#7d");
			Assert.IsNotNull ((DateTime?) new XElement ("r", re), "#7e");
			Assert.AreEqual (re, ((DateTime?) new XElement ("r", re)).Value, "#7f");

			Assert.IsNotNull ((DateTime?) new XElement (f), "#8a");
			Assert.AreEqual (rf, ((DateTime?) new XElement (f)).Value, "#8b");
			Assert.AreEqual (rf, (DateTime) new XElement (f), "#8c");
			Assert.AreEqual (rf, (DateTime) new XElement ("r", rf), "#8d");
			Assert.IsNotNull ((DateTime?) new XElement ("r", rf), "#8e");
			Assert.AreEqual (rf, ((DateTime?) new XElement ("r", rf)).Value, "#8f");
		}

		[Test]
		public void CastDateTimeOffsets ()
		{
			DateTimeOffset ra = new DateTimeOffset (1987, 1, 23, 21, 45, 36, 89, TimeSpan.FromHours (+13.75));  // e.g., Chatham Islands (daylight-savings time)
			DateTimeOffset rb = new DateTimeOffset (2001, 2, 3, 4, 5, 6, 789, DateTimeOffset.Now.Offset);  // Local time
			DateTimeOffset rc = new DateTimeOffset (2010, 1, 2, 0, 0, 0, 0, TimeSpan.Zero);  // UTC
			DateTimeOffset rd = new DateTimeOffset (1956, 11, 2, 12, 34, 10, TimeSpan.FromHours (-3.5));
			DateTimeOffset re = new DateTimeOffset (630646468235678363, TimeSpan.FromHours (-1));  // UTC-1, also with full resolution and a fractional second that might lose a tick on Mono 2.6.1
			DateTimeOffset rx = DateTimeOffset.Now;
			DateTimeOffset rz = DateTimeOffset.UtcNow;

			XElement a = new XElement ("a", "1987-01-23T21:45:36.089+13:45");
			XElement b = new XElement ("b", "2001-02-03T04:05:06.789" + DateTimeOffset.Now.ToString ("zzz"));
			XElement c = new XElement ("c", "2010-01-02T00:00:00Z");
			XElement d = new XElement ("d", "  Nov 2, 1956  12:34:10 PM   -3:30 \r\n   \t");
			XElement e = new XElement ("e", " \t   \n  1999-06-10T21:27:03.5678363-01:00 ");
			XElement x = new XElement ("x", rx);
			XElement z = new XElement ("z", rz);

			Assert.IsNotNull ((DateTimeOffset?) new XElement (a), "#1a");
			Assert.AreEqual (ra, ((DateTimeOffset?) new XElement (a)).Value, "#1b");
			Assert.AreEqual (ra, (DateTimeOffset) new XElement (a), "#1c");
			Assert.AreEqual (ra, (DateTimeOffset) new XElement ("r", ra), "#1d");
			Assert.IsNotNull ((DateTimeOffset?) new XElement ("r", ra), "#1e");
			Assert.AreEqual (ra, ((DateTimeOffset?) new XElement ("r", ra)).Value, "#1f");

			Assert.IsNotNull ((DateTimeOffset?) new XElement (b), "#2a");
			Assert.AreEqual (rb, ((DateTimeOffset?) new XElement (b)).Value, "#2b");
			Assert.AreEqual (rb, (DateTimeOffset) new XElement (b), "#2c");
			Assert.AreEqual (rb, (DateTimeOffset) new XElement ("r", rb), "#2d");
			Assert.IsNotNull ((DateTimeOffset?) new XElement ("r", rb), "#2e");
			Assert.AreEqual (rb, ((DateTimeOffset?) new XElement ("r", rb)).Value, "#2f");

			Assert.IsNotNull ((DateTimeOffset?) new XElement (c), "#3a");
			Assert.AreEqual (rc, ((DateTimeOffset?) new XElement (c)).Value, "#3b");
			Assert.AreEqual (rc, (DateTimeOffset) new XElement (c), "#3c");
			Assert.AreEqual (rc, (DateTimeOffset) new XElement ("r", rc), "#3d");
			Assert.IsNotNull ((DateTimeOffset?) new XElement ("r", rc), "#3e");
			Assert.AreEqual (rc, ((DateTimeOffset?) new XElement ("r", rc)).Value, "#3f");

			AssertThrows<FormatException> (() => { DateTimeOffset? r = (DateTimeOffset?) new XElement (d); }, "#4a");
			AssertThrows<FormatException> (() => { DateTimeOffset r = (DateTimeOffset) new XElement (d); }, "#4b");
			Assert.AreEqual (rd, DateTimeOffset.Parse (d.Value), "#4c");  // Sanity check: Okay for standalone DateTimeOffset but not as XML as in above

			Assert.IsNotNull ((DateTimeOffset?) new XElement (x), "#5a");
			Assert.AreEqual (rx, ((DateTimeOffset?) new XElement (x)).Value, "#5b");
			Assert.AreEqual (rx, (DateTimeOffset) new XElement (x), "#5c");

			Assert.IsNotNull ((DateTimeOffset?) new XElement (z), "#6a");
			Assert.AreEqual (rz, ((DateTimeOffset?) new XElement (z)).Value, "#6b");
			Assert.AreEqual (rz, (DateTimeOffset) new XElement (z), "#6c");

			Assert.IsNotNull ((DateTimeOffset?) new XElement (e), "#7a");
			Assert.AreEqual (re, ((DateTimeOffset?) new XElement (e)).Value, "#7b");
			Assert.AreEqual (re, (DateTimeOffset) new XElement (e), "#7c");
			Assert.AreEqual (re, (DateTimeOffset) new XElement ("r", re), "#7d");
			Assert.IsNotNull ((DateTimeOffset?) new XElement ("r", re), "#7e");
			Assert.AreEqual (re, ((DateTimeOffset?) new XElement ("r", re)).Value, "#7f");
		}

		[Test]
		public void CastTimeSpans ()
		{
			TimeSpan ra = new TimeSpan (23, 21, 45, 36, 89);
			TimeSpan rb = -new TimeSpan (3, 4, 5, 6, 789);
			TimeSpan rc = new TimeSpan (2, 0, 0, 0, 0);
			TimeSpan rd = new TimeSpan (0, 0, 0, 1);
			TimeSpan re = new TimeSpan (1L);  // one tick, the smallest interval
			TimeSpan rx = DateTimeOffset.Now.Offset;
			TimeSpan rz = TimeSpan.Zero;

			XElement a = new XElement ("a", "P23DT21H45M36.089S");
			XElement b = new XElement ("b", "-P3DT4H5M6.789S");
			XElement c = new XElement ("c", "P2D");
			XElement d = new XElement ("d", "PT1S");
			XElement e = new XElement ("e", "     PT0.0000001S  \t \n   ");
			XElement x = new XElement ("x", rx);
			XElement z = new XElement ("z", rz);

			Assert.IsNotNull ((TimeSpan?) new XElement (a), "#1a");
			Assert.AreEqual (ra, ((TimeSpan?) new XElement (a)).Value, "#1b");
			Assert.AreEqual (ra, (TimeSpan) new XElement (a), "#1c");
			Assert.AreEqual (ra, (TimeSpan) new XElement ("r", ra), "#1d");
			Assert.IsNotNull ((TimeSpan?) new XElement ("r", ra), "#1e");
			Assert.AreEqual (ra, ((TimeSpan?) new XElement ("r", ra)).Value, "#1f");

			Assert.IsNotNull ((TimeSpan?) new XElement (b), "#2a");
			Assert.AreEqual (rb, ((TimeSpan?) new XElement (b)).Value, "#2b");
			Assert.AreEqual (rb, (TimeSpan) new XElement (b), "#2c");
			Assert.AreEqual (rb, (TimeSpan) new XElement ("r", rb), "#2d");
			Assert.IsNotNull ((TimeSpan?) new XElement ("r", rb), "#2e");
			Assert.AreEqual (rb, ((TimeSpan?) new XElement ("r", rb)).Value, "#2f");

			Assert.IsNotNull ((TimeSpan?) new XElement (c), "#3a");
			Assert.AreEqual (rc, ((TimeSpan?) new XElement (c)).Value, "#3b");
			Assert.AreEqual (rc, (TimeSpan) new XElement (c), "#3c");
			Assert.AreEqual (rc, (TimeSpan) new XElement ("r", rc), "#3d");
			Assert.IsNotNull ((TimeSpan?) new XElement ("r", rc), "#3e");
			Assert.AreEqual (rc, ((TimeSpan?) new XElement ("r", rc)).Value, "#3f");

			Assert.IsNotNull ((TimeSpan?) new XElement (d), "#4a");
			Assert.AreEqual (rd, ((TimeSpan?) new XElement (d)).Value, "#4b");
			Assert.AreEqual (rd, (TimeSpan) new XElement (d), "#4c");
			Assert.AreEqual (rd, (TimeSpan) new XElement ("r", rd), "#4d");
			Assert.IsNotNull ((TimeSpan?) new XElement ("r", rd), "#4e");
			Assert.AreEqual (rd, ((TimeSpan?) new XElement ("r", rd)).Value, "#4f");

			Assert.IsNotNull ((TimeSpan?) new XElement (x), "#5a");
			Assert.AreEqual (rx, ((TimeSpan?) new XElement (x)).Value, "#5b");
			Assert.AreEqual (rx, (TimeSpan) new XElement (x), "#5c");

			Assert.IsNotNull ((TimeSpan?) new XElement (z), "#6a");
			Assert.AreEqual (rz, ((TimeSpan?) new XElement (z)).Value, "#6b");
			Assert.AreEqual (rz, (TimeSpan) new XElement (z), "#6c");

			Assert.IsNotNull ((TimeSpan?) new XElement (e), "#7a");
			Assert.AreEqual (re, ((TimeSpan?) new XElement (e)).Value, "#7b");
			Assert.AreEqual (re, (TimeSpan) new XElement (e), "#7c");
			Assert.AreEqual (re, (TimeSpan) new XElement ("r", re), "#7d");
			Assert.IsNotNull ((TimeSpan?) new XElement ("r", re), "#7e");
			Assert.AreEqual (re, ((TimeSpan?) new XElement ("r", re)).Value, "#7f");
		}
#pragma warning restore 219

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
		public void SetValue_ChangeTriggers()
		{
			bool changed = false;
			bool changing = false;
			
			var element = new XElement("foo");
			element.Changing += (o, e) => {
				Assert.IsFalse(changing, "#1");
				Assert.IsFalse(changed, "#2");
				Assert.IsTrue (o is XText, "#3");
				Assert.AreEqual("bar", ((XText)o).Value, "#4");
				Assert.AreEqual(XObjectChange.Add, e.ObjectChange, "#5");
				changing = true;
			};
			element.Changed += (o, e) => {
				Assert.IsTrue(changing, "#5");
				Assert.IsFalse(changed, "#6");
				Assert.IsTrue (o is XText, "#7");
				Assert.AreEqual("bar", ((XText)o).Value, "#8");
				Assert.AreEqual(XObjectChange.Add, e.ObjectChange, "#9");
				changed = true;
			};
			
			element.SetValue("bar");
			Assert.IsTrue(changed, "#changed");
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
			Assert.AreEqual(2, root.Elements().Count(), "#1");
			child.Remove ();
			Assert.AreEqual(1, root.Elements().Count(), "#2");
			AssertThrows<InvalidOperationException>(() => child.Remove(), "#3");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddSameInstance2 ()
		{
			XElement root = new XElement (XName.Get ("Root"));
			XAttribute attr = new XAttribute (XName.Get ("a"), "v");

			root.Add (attr);
			root.Add (attr); // duplicate attribute
			Assert.AreEqual(2, root.Attributes().Count(), "#1");
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

		[Test]
		public void ReplaceNodes ()
		{
			var inputXml = "<Foo><C><Three>3</Three><Two></Two><One/></C><B><Aaa/><Yyy/><fff/></B><A Attrib=\"Hello World\"/></Foo>";
			var reader = XmlReader.Create (new StringReader (inputXml), new XmlReaderSettings ());
			XDocument doc = XDocument.Load (reader);
			var result = doc.Root.Elements ().OrderBy (el => el.Name.ToString());
			Assert.AreEqual (3, result.Count (), "#1");
			doc.Root.FirstNode.Remove ();
			Assert.AreEqual (2, result.Count (), "#2");

			XContainer container = doc.Root;
			container.ReplaceNodes (result);

			Assert.AreEqual (2, container.Elements ().Count (), "#3");
		}

		[Test]
		public void ReplaceCreatesSnapshotBeforeRemoval ()
		{
			// bug #592435
			XElement data1 = new XElement ("A");
			XElement data3 = new XElement ("C");
			XElement data4 = new XElement ("D");
			XElement root = new XElement ("rt", 
			                              new XElement ("z", new XElement ("Name", data1), new XElement ("Desc", data4)), data3);
			var elements = root.Elements ().Elements ();
			root.ReplaceNodes (elements);
			root.Add (elements);
			string xml = @"<rt>
  <Name>
    <A />
  </Name>
  <Desc>
    <D />
  </Desc>
  <A />
  <D />
</rt>";
			Assert.AreEqual (xml.NormalizeNewline (), root.ToString ().NormalizeNewline (), "#1");
		}

		[Test]
		public void AddBefore_ChildNode_ChangeTriggers()
		{
			int changed = 0;
			int changing = 0;
			var child = new XElement("child");
			var root = new XElement("root", child);
			root.Changed += (o, e) => changed++;
			root.Changing += (o, e) => changing++;

			child.AddAfterSelf(new XElement("a"));
			Assert.AreEqual(1, changed, "#1");
			Assert.AreEqual(1, changing, "#2");

			child.AddBeforeSelf(new XElement("b"));
			Assert.AreEqual(2, changed, "#3");
			Assert.AreEqual(2, changing, "#4");

			child.AddFirst(new XElement("c"));
			Assert.AreEqual(3, changed, "#5");
			Assert.AreEqual(3, changing, "#6");
		}

		[Test]
		public void AddAttribute_ToRootNode_ChangeTriggers()
		{
			int changed = 0;
			int changing = 0;
			var node = new XElement("foo");
			node.Changed += (o, e) => changed ++;
			node.Changing += (o, e) => changing++;

			node.Add(new XAttribute("foo", "bar"));
			Assert.AreEqual(1, changing, "#1");
			Assert.AreEqual(1, changed, "#2");

			node.Add(new XAttribute("foo2", "bar2"));
			Assert.AreEqual(2, changing, "#3");
			Assert.AreEqual(2, changed, "#4");
		}

		[Test]
		public void SetAttributeValue_ToRootNode_ChangeTriggers()
		{
			int changed = 0;
			int changing = 0;
			var node = new XElement("foo");
			node.Changed += (o, e) => changed++;
			node.Changing += (o, e) => changing++;

			node.SetAttributeValue("foo", "bar");
			Assert.AreEqual(1, changing, "#1");
			Assert.AreEqual(1, changed, "#2");

			node.SetAttributeValue("foo2", "bar2");
			Assert.AreEqual(2, changing, "#3");
			Assert.AreEqual(2, changed, "#4");

			node.SetAttributeValue("foo2", null);
			Assert.AreEqual(3, changing, "#7");
			Assert.AreEqual(3, changed, "#8");

			node.SetAttributeValue("foo52", null);
			Assert.AreEqual(3, changing, "#9");
			Assert.AreEqual(3, changed, "#10");
		}

		[Test]
		public void RemoveAttributes_FromRootNode_ChangeTriggers()
		{
			int changed = 0;
			int changing = 0;
			var node = new XElement("foo", new XAttribute("foo", "bar"), new XAttribute("foo2", "bar2"), new XElement ("Barry"));
			node.Changed += (o, e) => changed++;
			node.Changing += (o, e) => changing++;

			node.RemoveAttributes();
			Assert.AreEqual(2, changing, "#1");
			Assert.AreEqual(2, changed, "#2");
		}

		[Test]
		public void RemoveNodes_FromRootNode_ChangeTriggers()
		{
			int changed = 0;
			int changing = 0;
			var node = new XElement("foo", new XAttribute("foo", "bar"), new XAttribute("foo2", "bar2"), new XElement("Barry"));
			node.Changed += (o, e) => changed++;
			node.Changing += (o, e) => changing++;

			node.RemoveNodes();
			Assert.AreEqual(1, changing, "#1");
			Assert.AreEqual(1, changed, "#2");
		}

		[Test]
		public void RemoveAll_FromRootNode_ChangeTriggers()
		{
			int changed = 0;
			int changing = 0;
			var node = new XElement("foo", new XAttribute("foo", "bar"), new XAttribute("foo2", "bar2"), new XElement("Barry"));
			node.Changed += (o, e) => changed++;
			node.Changing += (o, e) => changing++;

			node.RemoveAll ();
			Assert.AreEqual(3, changing, "#1");
			Assert.AreEqual(3, changed, "#2");
		}

		[Test]
		public void AddElement_ToRootNode_ChangeTriggers()
		{
			var childChanging = false;
			var childChanged = false;
			var rootChanging = false;
			var rootChanged = false;
			
			var child = new XElement("foo");
			var root = new XElement("root");
			child.Changing += (o, e) => childChanging = true;
			child.Changed += (o, e) => childChanged = true;
			
			root.Changing += (o, e) => {
				Assert.IsFalse(rootChanging, "#1");
				Assert.IsFalse(rootChanged, "#2");
				Assert.AreSame(child, o, "#3");
				Assert.AreEqual(XObjectChange.Add, e.ObjectChange, "#4");
				rootChanging = true;
			};
			root.Changed += (o, e) => {
				Assert.IsFalse(rootChanged, "#5");
				Assert.IsTrue(rootChanging, "#6");
				Assert.AreSame(child, o, "#7");
				Assert.AreEqual(XObjectChange.Add, e.ObjectChange, "#8");
				rootChanged = true;
			};
			
			root.Add (child);
			Assert.IsFalse(childChanging, "#9");
			Assert.IsFalse(childChanged, "#10");
			Assert.IsTrue(rootChanging, "#11");
			Assert.IsTrue(rootChanged, "#12");
		}

		[Test]
		public void AddElement_ToChildNode_ChangeTriggers()
		{
			var childChanging = false;
			var childChanged = false;
			var rootChanging = false;
			var rootChanged = false;
			
			var subchild = new XElement("subfoo");
			var child = new XElement("foo");
			var root = new XElement("root", child);
			
			child.Changing += (o, e) =>
			{
				Assert.IsFalse(childChanging, "#c1");
				Assert.IsFalse(childChanged, "#c2");
				Assert.IsFalse(rootChanging, "#c3");
				Assert.IsFalse(rootChanged, "#c4");
				Assert.AreSame(subchild, o, "#c5");
				Assert.AreEqual(XObjectChange.Add, e.ObjectChange, "#c6");
				Assert.IsNull(subchild.Parent, "childChangingParent");
				childChanging = true;
			};
			root.Changing += (o, e) =>
			{
				Assert.IsTrue(childChanging, "#r1");
				Assert.IsFalse(childChanged, "#r2");
				Assert.IsFalse(rootChanging, "#r3");
				Assert.IsFalse(rootChanged, "#r4");
				Assert.AreSame(subchild, o, "#r5");
				Assert.AreEqual(XObjectChange.Add, e.ObjectChange, "#r6");
				Assert.IsNull(subchild.Parent, "rootChangingParent");
				rootChanging = true;
			};
			child.Changed += (o, e) =>
			{
				Assert.IsTrue(childChanging, "#c7");
				Assert.IsFalse(childChanged, "#c8");
				Assert.IsTrue(rootChanging, "#c9");
				Assert.IsFalse(rootChanged, "#c10");
				Assert.AreSame(subchild, o, "#c11");
				Assert.AreEqual(XObjectChange.Add, e.ObjectChange, "#c12");
				Assert.IsNotNull(subchild.Parent, "childChangedParent");
				childChanged = true;
			};
			root.Changed += (o, e) =>
			{
				Assert.IsTrue(childChanging, "#r7");
				Assert.IsTrue(childChanged, "#r8");
				Assert.IsTrue(rootChanging, "#r9");
				Assert.IsFalse(rootChanged, "#r10");
				Assert.AreSame(subchild, o, "#11");
				Assert.AreEqual(XObjectChange.Add, e.ObjectChange, "#12");
				Assert.IsNotNull(subchild.Parent, "rootChangedParent");
				rootChanged = true;
			};
			
			child.Add (subchild);
			Assert.IsTrue(childChanging, "#a");
			Assert.IsTrue(childChanged, "#b");
			Assert.IsTrue(rootChanging, "#c");
			Assert.IsTrue(rootChanged, "#d");
		}

		[Test]
		public void SetElementValue () // #699242
		{
			var element = XElement.Parse ("<foo><bar>bar</bar><baz>baz</baz></foo>");
			element.SetElementValue ("bar", "babar");
			element.SetElementValue ("baz", "babaz");
			element.SetElementValue ("gaz", "gazonk");

			Assert.AreEqual ("<foo><bar>babar</bar><baz>babaz</baz><gaz>gazonk</gaz></foo>", element.ToString (SaveOptions.DisableFormatting));

			element.SetElementValue ("gaz", null);
			Assert.AreEqual ("<foo><bar>babar</bar><baz>babaz</baz></foo>", element.ToString (SaveOptions.DisableFormatting));
		}

		[Test]
		public void Bug3137 ()
		{
			CultureInfo current = Thread.CurrentThread.CurrentCulture;
			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
				var element1 = new XElement ("Property1", new XAttribute ("type", "number"), 1.2343445);
				Assert.AreEqual ("<Property1 type=\"number\">1.2343445</Property1>", element1.ToString (), "en-US");
				
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("de-DE");
				// this was already working because the element was created with en-US
				Assert.AreEqual ("<Property1 type=\"number\">1.2343445</Property1>", element1.ToString (), "de-DE/1");
				// however creating a new, identical, element under de-DE return*ed* a different string
				var element2 = new XElement ("Property1", new XAttribute ("type", "number"), 1.2343445);
				Assert.AreEqual ("<Property1 type=\"number\">1.2343445</Property1>", element2.ToString (), "de-DE/2");
			}
			finally {
				Thread.CurrentThread.CurrentCulture = current;
			}
		}

		[Test]
		public void DecimalFormatting () // bug #3634
		{
			var data = 5.5M;
			var bak = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("nl-NL");
			try {
				var element = new XElement ("Demo", data);
				Assert.AreEqual ("<Demo>5.5</Demo>", element.ToString (), "#1");
			} finally {
				Thread.CurrentThread.CurrentCulture = bak;
			}
		}
		
		[Test] // bug #3972
		public void UseGetPrefixOfNamespaceForToString ()
		{
			string xml = @"
			<xsi:Event
			  xsi1:type='xsi:SubscriptionEvent'
			  xmlns:xsi='http://relevo.se/xsi'
			  xmlns:xsi1='http://www.w3.org/2001/XMLSchema-instance'>
			  <xsi:eventData xsi1:type='xsi:CallSubscriptionEvent'/>
			</xsi:Event>";
			var e = XElement.Parse (xml);
			string expected = @"<xsi:eventData xsi1:type='xsi:CallSubscriptionEvent' xmlns:xsi1='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsi='http://relevo.se/xsi' />".Replace ('\'', '"');
			Assert.AreEqual (expected, e.Nodes ().First ().ToString (), "#1");
		}
		
		[Test] // bug #5519
		public void DoUseEmptyNamespacePrefixWhenApplicable ()
		{
			XNamespace ns = "http://jabber.org/protocol/geoloc";
			XElement newElement = new XElement(ns + "geoloc");
			Assert.AreEqual ("<geoloc xmlns=\"http://jabber.org/protocol/geoloc\" />", newElement.ToString (), "#1");
		}
	}
}
