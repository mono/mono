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
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XAttributeTest
	{
		[Test]
		public void Annotations_GetSubclass()
		{
			var x = new XAttribute("foo", "bar");
			var annotation = new InvalidCastException();
			x.AddAnnotation(annotation);
			
			Assert.AreSame(annotation, x.Annotation<InvalidCastException>(), "#1");
			Assert.AreSame(annotation, x.Annotation<object>(), "#2");
			Assert.AreSame(annotation, x.Annotations<object>().Single (), "#3");
		}

		[Test]
		public void Annotations_SameTypeTwice()
		{
			var x = new XAttribute("foo", "bar");
			var first = new InvalidCastException();
			var second = new InvalidCastException();
			x.AddAnnotation(first);
			x.AddAnnotation(second);
			Assert.AreEqual(2, x.Annotations<object>().Count(), "#1");
			Assert.AreSame(first, x.Annotation<object>(), "#2");
		}

		[Test]
		public void Constructor_NullParameters ()
		{
			AssertThrows<ArgumentNullException>(() => new XAttribute(null, "v"), "#1");
			AssertThrows<ArgumentNullException>(() => new XAttribute(XName.Get("a"), null), "#2");
			AssertThrows<ArgumentNullException>(() => new XAttribute((XAttribute) null), "#3");
		}

		[Test]
		public void IsNamespaceDeclaration ()
		{
			string xml = "<root a='v' xmlns='urn:foo' xmlns:x='urn:x' x:a='v' xmlns:xml='http://www.w3.org/XML/1998/namespace' />";
			XElement el = XElement.Parse (xml);
			List<XAttribute> l = new List<XAttribute> (el.Attributes ());
			Assert.IsFalse (l [0].IsNamespaceDeclaration, "#1");
			Assert.IsTrue (l [1].IsNamespaceDeclaration, "#2");
			Assert.IsTrue (l [2].IsNamespaceDeclaration, "#3");
			Assert.IsFalse (l [3].IsNamespaceDeclaration, "#4");
			Assert.IsTrue (l [4].IsNamespaceDeclaration, "#5");

			Assert.AreEqual ("a", l [0].Name.LocalName, "#2-1");
			Assert.AreEqual ("xmlns", l [1].Name.LocalName, "#2-2");
			Assert.AreEqual ("x", l [2].Name.LocalName, "#2-3");
			Assert.AreEqual ("a", l [3].Name.LocalName, "#2-4");
			Assert.AreEqual ("xml", l [4].Name.LocalName, "#2-5");

			Assert.AreEqual ("", l [0].Name.NamespaceName, "#3-1");
			// not sure how current Orcas behavior makes sense here though ...
			Assert.AreEqual ("", l [1].Name.NamespaceName, "#3-2");
			Assert.AreEqual ("http://www.w3.org/2000/xmlns/", l [2].Name.NamespaceName, "#3-3");
			Assert.AreEqual ("urn:x", l [3].Name.NamespaceName, "#3-4");
			Assert.AreEqual ("http://www.w3.org/2000/xmlns/", l [4].Name.NamespaceName, "#3-5");
		}

		[Test]
		public void Document ()
		{
			XDocument doc = XDocument.Parse ("<root a='v' />");
			Assert.AreEqual (doc, doc.Root.Document, "#1");
			foreach (XAttribute a in doc.Root.Attributes ())
				Assert.AreEqual (doc, a.Document, "#2");
			Assert.AreEqual (doc, doc.Document, "#3");
		}

		[Test]
		public void SetValue ()
		{
			XAttribute a = new XAttribute (XName.Get ("a"), "v");
			a.SetValue (new XDeclaration ("1.0", null, null));
			// value object is converted to a string.
			Assert.AreEqual ("<?xml version=\"1.0\"?>", a.Value, "#1");
		}

		[Test]
		public void SetValue_Null()
		{
			var a = new XAttribute("foo", "bar");
			AssertThrows<ArgumentNullException>(() => a.Value = null, "#1");
			AssertThrows<ArgumentNullException>(() => a.SetValue (null), "#2");
		}

		[Test]
		public void SetValue_ChangeTriggers()
		{
			bool changing = false;
			bool changed = false;

			var a = new XAttribute("foo", "bar");
			a.Changing += (o, e) => {
				Assert.IsFalse(changing, "#1");
				Assert.IsFalse(changed, "#2");
				Assert.AreSame(a, o, "#3");
				Assert.AreEqual(XObjectChange.Value, e.ObjectChange, "#4");
				changing = true;
			};
			a.Changed += (o, e) => {
				Assert.IsTrue(changing, "#5");
				Assert.IsFalse(changed, "#6");
				Assert.AreSame(a, o, "#7");
				Assert.AreEqual(XObjectChange.Value, e.ObjectChange, "#8");
				changed = true;
			};
			a.Value = "foo";
			Assert.IsTrue(changing, "changing");
			Assert.IsTrue(changed, "changed");
		}

		[Test]
		public void SetValue2_ChangeTriggers()
		{
			bool changing = false;
			bool changed = false;
			
			var a = new XAttribute("foo", "bar");
			a.Changing += (o, e) =>
			{
				Assert.IsFalse(changing, "#1");
				Assert.IsFalse(changed, "#2");
				Assert.AreSame(a, o, "#3");
				Assert.AreEqual(XObjectChange.Value, e.ObjectChange, "#4");
				changing = true;
			};
			a.Changed += (o, e) =>
			{
				Assert.IsTrue(changing, "#5");
				Assert.IsFalse(changed, "#6");
				Assert.AreSame(a, o, "#7");
				Assert.AreEqual(XObjectChange.Value, e.ObjectChange, "#8");
				changed = true;
			};
			a.SetValue("zap");
			Assert.IsTrue(changing, "changing");
			Assert.IsTrue(changed, "changed");
		}

		[Test]
		public void SetValue_SameValue_ChangeTrigger()
		{
			int changed = 0;
			int changing = 0;

			var a = new XAttribute("foo", "bar");
			a.Changed += (o, e) => changed++;
			a.Changing += (o, e) => changing++;

			a.SetValue("bar");
			Assert.AreEqual(1, changed, "#1");
			Assert.AreEqual(1, changing, "#2");

			a.Value = "bar";
			Assert.AreEqual(2, changed, "#3");
			Assert.AreEqual(2, changing, "#4");
		}

		[Test]
		public void ToString ()
		{
			XAttribute a = new XAttribute (XName.Get ("a"), "v");
			Assert.AreEqual ("a=\"v\"", a.ToString ());

			a = new XAttribute (XName.Get ("a"), " >_< ");
			Assert.AreEqual ("a=\" &gt;_&lt; \"", a.ToString ());
		}

		[Test]
		public void ToString_Xamarin29935 ()
		{
			var doc = XDocument.Parse ("<?xml version='1.0' encoding='utf-8'?><lift xmlns:test='http://test.example.com'></lift>");
			Assert.AreEqual ("xmlns:test=\"http://test.example.com\"",
				doc.Root.Attributes ().Select (s => s.ToString ()).First ());
		}

		[Test]
		public void DateTimeAttribute ()
		{
			var date = DateTime.Now;
			var attribute = new XAttribute ("Date", date);

			var value = (DateTime) attribute;

			Assert.AreEqual (date, value);
		}

#pragma warning disable 219
		[Test]
		public void CastNulls ()
		{
			const XAttribute a = null;

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
		public void CastEmptiesOrBlanks ()
		{
			XAttribute a = new XAttribute ("a", String.Empty);

			// Verify expected "cloning" and "empty/blank" behaviour as prerequisites
			Assert.AreEqual (String.Empty, a.Value, "#1-1");
			Assert.AreEqual (String.Empty, new XAttribute (a).Value, "#1-2");
			Assert.AreNotSame (a, new XAttribute (a), "#2-1");
			Assert.AreEqual (a.ToString (), new XAttribute (a).ToString (), "#2-2");
			Assert.AreEqual ("a=\"\"", a.ToString (), "#2-3");

			// Execute the primary assertions of this test
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XAttribute (a); }, "bool?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (a); }, "DateTime?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (a); }, "DateTimeOffset?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XAttribute (a); }, "decimal?");
			AssertThrows<FormatException> (() => { double? z = (double?) new XAttribute (a); }, "double?");
			AssertThrows<FormatException> (() => { float? z = (float?) new XAttribute (a); }, "float?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (a); }, "Guid?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XAttribute (a); }, "int?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XAttribute (a); }, "long?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XAttribute (a); }, "uint?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XAttribute (a); }, "ulong?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (a); }, "TimeSpan?");
			Assert.AreEqual (String.Empty, (string) new XAttribute (a), "string");
			AssertThrows<FormatException> (() => { bool z = (bool) new XAttribute (a); }, "bool");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (a); }, "DateTime");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (a); }, "DateTimeOffset");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XAttribute (a); }, "decimal");
			AssertThrows<FormatException> (() => { double z = (double) new XAttribute (a); }, "double");
			AssertThrows<FormatException> (() => { float z = (float) new XAttribute (a); }, "float");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (a); }, "Guid");
			AssertThrows<FormatException> (() => { int z = (int) new XAttribute (a); }, "int");
			AssertThrows<FormatException> (() => { long z = (long) new XAttribute (a); }, "long");
			AssertThrows<FormatException> (() => { uint z = (uint) new XAttribute (a); }, "uint");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XAttribute (a); }, "ulong");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (a); }, "TimeSpan");
		}

		[Test]
		public void CastSpaces ()
		{
			XAttribute a = new XAttribute ("a", " ");

			// Verify expected "cloning" and "space" behaviour as prerequisites
			Assert.AreEqual (" ", a.Value, "#1-1");
			Assert.AreEqual (" ", new XAttribute (a).Value, "#1-2");
			Assert.AreNotSame (a, new XAttribute (a), "#2-1");
			Assert.AreEqual (a.ToString (), new XAttribute (a).ToString (), "#2-2");
			Assert.AreEqual ("a=\" \"", a.ToString (), "#2-3");
			Assert.AreEqual (a.ToString (), new XAttribute ("a", ' ').ToString (), "#2-4");

			// Execute the primary assertions of this test
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XAttribute (a); }, "bool?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (a); }, "DateTime?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (a); }, "DateTimeOffset?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XAttribute (a); }, "decimal?");
			AssertThrows<FormatException> (() => { double? z = (double?) new XAttribute (a); }, "double?");
			AssertThrows<FormatException> (() => { float? z = (float?) new XAttribute (a); }, "float?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (a); }, "Guid?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XAttribute (a); }, "int?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XAttribute (a); }, "long?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XAttribute (a); }, "uint?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XAttribute (a); }, "ulong?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (a); }, "TimeSpan?");
			Assert.AreEqual (" ", (string) new XAttribute (a), "string");
			AssertThrows<FormatException> (() => { bool z = (bool) new XAttribute (a); }, "bool");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (a); }, "DateTime");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (a); }, "DateTimeOffset");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XAttribute (a); }, "decimal");
			AssertThrows<FormatException> (() => { double z = (double) new XAttribute (a); }, "double");
			AssertThrows<FormatException> (() => { float z = (float) new XAttribute (a); }, "float");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (a); }, "Guid");
			AssertThrows<FormatException> (() => { int z = (int) new XAttribute (a); }, "int");
			AssertThrows<FormatException> (() => { long z = (long) new XAttribute (a); }, "long");
			AssertThrows<FormatException> (() => { uint z = (uint) new XAttribute (a); }, "uint");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XAttribute (a); }, "ulong");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (a); }, "TimeSpan");
		}

		[Test]
		public void CastNumbers ()
		{
			XAttribute a = new XAttribute ("a", "7");
			XAttribute b = new XAttribute ("b", "  42 ");
			XAttribute c = new XAttribute ("c", " \n\r   123 \t  ");
			XAttribute d = new XAttribute ("d", -101);
			XAttribute o = new XAttribute ("o", "0");
			XAttribute l = new XAttribute ("l", "1");
			XAttribute I = new XAttribute ("I", "\t    INF    ");
			XAttribute i = new XAttribute ("i", "Infinity");
			XAttribute M = new XAttribute ("M", "-INF");
			XAttribute m = new XAttribute ("m", " -Infinity  ");
			XAttribute n = new XAttribute ("n", "NaN");

			// Verify expected "cloning" and basic conversion behaviour as prerequisites
			Assert.AreEqual (" \n\r   123 \t  ", c.Value, "#1-1");
			Assert.AreEqual ("-101", new XAttribute (d).Value, "#1-2");
			Assert.AreNotSame (o, new XAttribute (o), "#2-1");
			Assert.AreEqual (l.ToString (), new XAttribute (l).ToString (), "#2-2");
			Assert.AreEqual ("a=\"7\"", a.ToString (), "#2-3a");
			Assert.AreEqual ("b=\"  42 \"", b.ToString (), "#2-3b");
			Assert.AreEqual ("c=\" &#xA;&#xD;   123 &#x9;  \"", c.ToString (), "#2-3c");
			Assert.AreEqual ("d=\"-101\"", d.ToString (), "#2-3d");
			Assert.AreEqual ("o=\"0\"", new XAttribute ("o", 0.0).ToString (), "#2-3o");
			Assert.AreEqual ("l=\"1\"", new XAttribute ("l", 1.0f).ToString (), "#2-3l");
			Assert.AreEqual ("n=\"NaN\"", new XAttribute ("n", double.NaN).ToString (), "#2-3n");
			Assert.AreEqual (a.ToString (), new XAttribute ("a", '7').ToString (), "#2-4a");
			Assert.AreEqual (d.ToString (), new XAttribute ("d", "-101").ToString (), "#2-4d");
			Assert.AreEqual (o.ToString (), new XAttribute ("o", 0L).ToString (), "#2-4o");
			Assert.AreEqual (l.ToString (), new XAttribute ("l", 1m).ToString (), "#2-4l");

			// Execute the primary assertions of this test
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XAttribute (a); }, "a:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XAttribute (b); }, "b:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XAttribute (c); }, "c:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XAttribute (d); }, "d:bool?");
			Assert.IsNotNull ((bool?) new XAttribute (o), "o:bool?:null");
			Assert.AreEqual (false, ((bool?) new XAttribute (o)).Value, "o:bool?:value");
			Assert.IsNotNull ((bool?) new XAttribute (l), "l:bool?:null");
			Assert.AreEqual (true, ((bool?) new XAttribute (l)).Value, "l:bool?:value");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XAttribute (I); }, "I:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XAttribute (i); }, "i:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XAttribute (M); }, "M:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XAttribute (m); }, "m:bool?");
			AssertThrows<FormatException> (() => { bool? z = (bool?) new XAttribute (n); }, "n:bool?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (a); }, "a:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (b); }, "b:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (c); }, "c:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (d); }, "d:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (o); }, "o:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (l); }, "l:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (I); }, "I:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (i); }, "i:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (M); }, "M:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (m); }, "m:DateTime?");
			AssertThrows<FormatException> (() => { DateTime? z = (DateTime?) new XAttribute (n); }, "n:DateTime?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (a); }, "a:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (b); }, "b:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (c); }, "c:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (d); }, "d:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (o); }, "o:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (l); }, "l:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (I); }, "I:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (i); }, "i:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (M); }, "M:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (m); }, "m:DateTimeOffset?");
			AssertThrows<FormatException> (() => { DateTimeOffset? z = (DateTimeOffset?) new XAttribute (n); }, "n:DateTimeOffset?");
			Assert.IsNotNull ((decimal?) new XAttribute (a), "a:decimal?:null");
			Assert.AreEqual (7m, ((decimal?) new XAttribute (a)).Value, "a:decimal?:value");
			Assert.IsNotNull ((decimal?) new XAttribute (b), "b:decimal?:null");
			Assert.AreEqual (42m, ((decimal?) new XAttribute (b)).Value, "b:decimal?:value");
			Assert.IsNotNull ((decimal?) new XAttribute (c), "c:decimal?:null");
			Assert.AreEqual (123m, ((decimal?) new XAttribute (c)).Value, "c:decimal?:value");
			Assert.IsNotNull ((decimal?) new XAttribute (d), "d:decimal?:null");
			Assert.AreEqual (-101m, ((decimal?) new XAttribute (d)).Value, "d:decimal?:value");
			Assert.IsNotNull ((decimal?) new XAttribute (o), "o:decimal?:null");
			Assert.AreEqual (0m, ((decimal?) new XAttribute (o)).Value, "o:decimal?:value");
			Assert.IsNotNull ((decimal?) new XAttribute (l), "l:decimal?:null");
			Assert.AreEqual (1m, ((decimal?) new XAttribute (l)).Value, "l:decimal?:value");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XAttribute (I); }, "I:decimal?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XAttribute (i); }, "i:decimal?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XAttribute (M); }, "M:decimal?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XAttribute (m); }, "m:decimal?");
			AssertThrows<FormatException> (() => { decimal? z = (decimal?) new XAttribute (n); }, "n:decimal?");
			Assert.IsNotNull ((double?) new XAttribute (a), "a:double?:null");
			Assert.AreEqual (7d, ((double?) new XAttribute (a)).Value, "a:double?:value");
			Assert.IsNotNull ((double?) new XAttribute (b), "b:double?:null");
			Assert.AreEqual (42d, ((double?) new XAttribute (b)).Value, "b:double?:value");
			Assert.IsNotNull ((double?) new XAttribute (c), "c:double?:null");
			Assert.AreEqual (123d, ((double?) new XAttribute (c)).Value, "c:double?:value");
			Assert.IsNotNull ((double?) new XAttribute (d), "d:double?:null");
			Assert.AreEqual (-101d, ((double?) new XAttribute (d)).Value, "d:double?:value");
			Assert.IsNotNull ((double?) new XAttribute (o), "o:double?:null");
			Assert.AreEqual (0d, ((double?) new XAttribute (o)).Value, "o:double?:value");
			Assert.IsNotNull ((double?) new XAttribute (l), "l:double?:null");
			Assert.AreEqual (1d, ((double?) new XAttribute (l)).Value, "l:double?:value");
			Assert.IsNotNull ((double?) new XAttribute (I), "I:double?:null");
			Assert.AreEqual (double.PositiveInfinity, ((double?) new XAttribute (I)).Value, "I:double?:value");
			Assert.IsNotNull ((double?) new XAttribute (i), "i:double?:null");
			Assert.AreEqual (double.PositiveInfinity, ((double?) new XAttribute (i)).Value, "i:double?:value");
			Assert.IsNotNull ((double?) new XAttribute (M), "M:double?:null");
			Assert.AreEqual (double.NegativeInfinity, ((double?) new XAttribute (M)).Value, "M:double?:value");
			Assert.IsNotNull ((double?) new XAttribute (m), "m:double?:null");
			Assert.AreEqual (double.NegativeInfinity, ((double?) new XAttribute (m)).Value, "m:double?:value");
			Assert.IsNotNull ((double?) new XAttribute (n), "n:double?:null");
			Assert.AreEqual (double.NaN, ((double?) new XAttribute (n)).Value, "n:double?:value");
			Assert.IsNotNull ((float?) new XAttribute (a), "a:float?:null");
			Assert.AreEqual (7f, ((float?) new XAttribute (a)).Value, "a:float?:value");
			Assert.IsNotNull ((float?) new XAttribute (b), "b:float?:null");
			Assert.AreEqual (42f, ((float?) new XAttribute (b)).Value, "b:float?:value");
			Assert.IsNotNull ((float?) new XAttribute (c), "c:float?:null");
			Assert.AreEqual (123f, ((float?) new XAttribute (c)).Value, "c:float?:value");
			Assert.IsNotNull ((float?) new XAttribute (d), "d:float?:null");
			Assert.AreEqual (-101f, ((float?) new XAttribute (d)).Value, "d:float?:value");
			Assert.IsNotNull ((float?) new XAttribute (o), "o:float?:null");
			Assert.AreEqual (0f, ((float?) new XAttribute (o)).Value, "o:float?:value");
			Assert.IsNotNull ((float?) new XAttribute (l), "l:float?:null");
			Assert.AreEqual (1f, ((float?) new XAttribute (l)).Value, "l:float?:value");
			Assert.IsNotNull ((float?) new XAttribute (I), "I:float?:null");
			Assert.AreEqual (float.PositiveInfinity, ((float?) new XAttribute (I)).Value, "I:float?:value");
			Assert.IsNotNull ((float?) new XAttribute (i), "i:float?:null");
			Assert.AreEqual (float.PositiveInfinity, ((float?) new XAttribute (i)).Value, "i:float?:value");
			Assert.IsNotNull ((float?) new XAttribute (M), "M:float?:null");
			Assert.AreEqual (float.NegativeInfinity, ((float?) new XAttribute (M)).Value, "M:float?:value");
			Assert.IsNotNull ((float?) new XAttribute (m), "m:float?:null");
			Assert.AreEqual (float.NegativeInfinity, ((float?) new XAttribute (m)).Value, "m:float?:value");
			Assert.IsNotNull ((float?) new XAttribute (n), "n:float?:null");
			Assert.AreEqual (float.NaN, ((float?) new XAttribute (n)).Value, "n:float?:value");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (a); }, "a:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (b); }, "b:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (c); }, "c:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (d); }, "d:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (o); }, "o:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (l); }, "l:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (I); }, "I:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (i); }, "i:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (M); }, "M:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (m); }, "m:Guid?");
			AssertThrows<FormatException> (() => { Guid? z = (Guid?) new XAttribute (n); }, "n:Guid?");
			Assert.IsNotNull ((int?) new XAttribute (a), "a:int?:null");
			Assert.AreEqual (7, ((int?) new XAttribute (a)).Value, "a:int?:value");
			Assert.IsNotNull ((int?) new XAttribute (b), "b:int?:null");
			Assert.AreEqual (42, ((int?) new XAttribute (b)).Value, "b:int?:value");
			Assert.IsNotNull ((int?) new XAttribute (c), "c:int?:null");
			Assert.AreEqual (123, ((int?) new XAttribute (c)).Value, "c:int?:value");
			Assert.IsNotNull ((int?) new XAttribute (d), "d:int?:null");
			Assert.AreEqual (-101, ((int?) new XAttribute (d)).Value, "d:int?:value");
			Assert.IsNotNull ((int?) new XAttribute (o), "o:int?:null");
			Assert.AreEqual (0, ((int?) new XAttribute (o)).Value, "o:int?:value");
			Assert.IsNotNull ((int?) new XAttribute (l), "l:int?:null");
			Assert.AreEqual (1, ((int?) new XAttribute (l)).Value, "l:int?:value");
			AssertThrows<FormatException> (() => { int? z = (int?) new XAttribute (I); }, "I:int?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XAttribute (i); }, "i:int?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XAttribute (M); }, "M:int?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XAttribute (m); }, "m:int?");
			AssertThrows<FormatException> (() => { int? z = (int?) new XAttribute (n); }, "n:int?");
			Assert.IsNotNull ((long?) new XAttribute (a), "a:long?:null");
			Assert.AreEqual (7L, ((long?) new XAttribute (a)).Value, "a:long?:value");
			Assert.IsNotNull ((long?) new XAttribute (b), "b:long?:null");
			Assert.AreEqual (42L, ((long?) new XAttribute (b)).Value, "b:long?:value");
			Assert.IsNotNull ((long?) new XAttribute (c), "c:long?:null");
			Assert.AreEqual (123L, ((long?) new XAttribute (c)).Value, "c:long?:value");
			Assert.IsNotNull ((long?) new XAttribute (d), "d:long?:null");
			Assert.AreEqual (-101L, ((long?) new XAttribute (d)).Value, "d:long?:value");
			Assert.IsNotNull ((long?) new XAttribute (o), "o:long?:null");
			Assert.AreEqual (0L, ((long?) new XAttribute (o)).Value, "o:long?:value");
			Assert.IsNotNull ((long?) new XAttribute (l), "l:long?:null");
			Assert.AreEqual (1L, ((long?) new XAttribute (l)).Value, "l:long?:value");
			AssertThrows<FormatException> (() => { long? z = (long?) new XAttribute (I); }, "I:long?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XAttribute (i); }, "i:long?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XAttribute (M); }, "M:long?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XAttribute (m); }, "m:long?");
			AssertThrows<FormatException> (() => { long? z = (long?) new XAttribute (n); }, "n:long?");
			Assert.IsNotNull ((uint?) new XAttribute (a), "a:uint?:null");
			Assert.AreEqual (7u, ((uint?) new XAttribute (a)).Value, "a:uint?:value");
			Assert.IsNotNull ((uint?) new XAttribute (b), "b:uint?:null");
			Assert.AreEqual (42u, ((uint?) new XAttribute (b)).Value, "b:uint?:value");
			Assert.IsNotNull ((uint?) new XAttribute (c), "c:uint?:null");
			Assert.AreEqual (123u, ((uint?) new XAttribute (c)).Value, "c:uint?:value");
			// LAMESPEC: see XmlConvertTests.ToUInt32().
			//AssertThrows<FormatException> (() => { uint? z = (uint?) new XAttribute (d); }, "d:uint?");
			Assert.IsNotNull ((uint?) new XAttribute (o), "o:uint?:null");
			Assert.AreEqual (0u, ((uint?) new XAttribute (o)).Value, "o:uint?:value");
			Assert.IsNotNull ((uint?) new XAttribute (l), "l:uint?:null");
			Assert.AreEqual (1u, ((uint?) new XAttribute (l)).Value, "l:uint?:value");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XAttribute (I); }, "I:uint?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XAttribute (i); }, "i:uint?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XAttribute (M); }, "M:uint?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XAttribute (m); }, "m:uint?");
			AssertThrows<FormatException> (() => { uint? z = (uint?) new XAttribute (n); }, "n:uint?");
			Assert.IsNotNull ((ulong?) new XAttribute (a), "a:ulong?:null");
			Assert.AreEqual (7UL, ((ulong?) new XAttribute (a)).Value, "a:ulong?:value");
			Assert.IsNotNull ((ulong?) new XAttribute (b), "b:ulong?:null");
			Assert.AreEqual (42UL, ((ulong?) new XAttribute (b)).Value, "b:ulong?:value");
			Assert.IsNotNull ((ulong?) new XAttribute (c), "c:ulong?:null");
			Assert.AreEqual (123UL, ((ulong?) new XAttribute (c)).Value, "c:ulong?:value");
			// LAMESPEC: see XmlConvertTests.ToUInt64().
			//AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XAttribute (d); }, "d:ulong?");
			Assert.IsNotNull ((ulong?) new XAttribute (o), "o:ulong?:null");
			Assert.AreEqual (0UL, ((ulong?) new XAttribute (o)).Value, "o:ulong?:value");
			Assert.IsNotNull ((ulong?) new XAttribute (l), "l:ulong?:null");
			Assert.AreEqual (1UL, ((ulong?) new XAttribute (l)).Value, "l:ulong?:value");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XAttribute (I); }, "I:ulong?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XAttribute (i); }, "i:ulong?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XAttribute (M); }, "M:ulong?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XAttribute (m); }, "m:ulong?");
			AssertThrows<FormatException> (() => { ulong? z = (ulong?) new XAttribute (n); }, "n:ulong?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (a); }, "a:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (b); }, "b:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (c); }, "c:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (d); }, "d:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (o); }, "o:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (l); }, "l:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (I); }, "I:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (i); }, "i:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (M); }, "M:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (m); }, "m:TimeSpan?");
			AssertThrows<FormatException> (() => { TimeSpan? z = (TimeSpan?) new XAttribute (n); }, "n:TimeSpan?");
			Assert.AreEqual ("7", (string) new XAttribute (a), "a:string");
			Assert.AreEqual ("  42 ", (string) new XAttribute (b), "b:string");
			Assert.AreEqual (" \n\r   123 \t  ", (string) new XAttribute (c), "c:string");
			Assert.AreEqual ("-101", (string) new XAttribute (d), "d:string");
			Assert.AreEqual ("0", (string) new XAttribute (o), "o:string");
			Assert.AreEqual ("1", (string) new XAttribute (l), "l:string");
			Assert.AreEqual ("\t    INF    ", (string) new XAttribute (I), "I:string");
			Assert.AreEqual ("Infinity", (string) new XAttribute (i), "i:string");
			Assert.AreEqual ("-INF", (string) new XAttribute (M), "M:string");
			Assert.AreEqual (" -Infinity  ", (string) new XAttribute (m), "m:string");
			Assert.AreEqual ("NaN", (string) new XAttribute (n), "n:string");
			AssertThrows<FormatException> (() => { bool z = (bool) new XAttribute (a); }, "a:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XAttribute (b); }, "b:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XAttribute (c); }, "c:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XAttribute (d); }, "d:bool");
			Assert.AreEqual (false, (bool) new XAttribute (o), "o:bool");
			Assert.AreEqual (true, (bool) new XAttribute (l), "l:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XAttribute (I); }, "I:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XAttribute (i); }, "i:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XAttribute (M); }, "M:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XAttribute (m); }, "m:bool");
			AssertThrows<FormatException> (() => { bool z = (bool) new XAttribute (n); }, "n:bool");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (a); }, "a:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (b); }, "b:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (c); }, "c:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (d); }, "d:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (o); }, "o:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (l); }, "l:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (I); }, "I:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (i); }, "i:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (M); }, "M:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (m); }, "m:DateTime");
			AssertThrows<FormatException> (() => { DateTime z = (DateTime) new XAttribute (n); }, "n:DateTime");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (a); }, "a:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (b); }, "b:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (c); }, "c:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (d); }, "d:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (o); }, "o:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (l); }, "l:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (I); }, "I:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (i); }, "i:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (M); }, "M:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (m); }, "m:DateTimeOffset");
			AssertThrows<FormatException> (() => { DateTimeOffset z = (DateTimeOffset) new XAttribute (n); }, "n:DateTimeOffset");
			Assert.AreEqual (7m, (decimal) new XAttribute (a), "a:decimal");
			Assert.AreEqual (42m, (decimal) new XAttribute (b), "b:decimal");
			Assert.AreEqual (123m, (decimal) new XAttribute (c), "c:decimal");
			Assert.AreEqual (-101m, (decimal) new XAttribute (d), "d:decimal");
			Assert.AreEqual (0m, (decimal) new XAttribute (o), "o:decimal");
			Assert.AreEqual (1m, (decimal) new XAttribute (l), "l:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XAttribute (I); }, "I:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XAttribute (i); }, "i:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XAttribute (M); }, "M:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XAttribute (m); }, "m:decimal");
			AssertThrows<FormatException> (() => { decimal z = (decimal) new XAttribute (n); }, "n:decimal");
			Assert.AreEqual (7d, (double) new XAttribute (a), "a:double");
			Assert.AreEqual (42d, (double) new XAttribute (b), "b:double");
			Assert.AreEqual (123d, (double) new XAttribute (c), "c:double");
			Assert.AreEqual (-101d, (double) new XAttribute (d), "d:double");
			Assert.AreEqual (0d, (double) new XAttribute (o), "o:double");
			Assert.AreEqual (1d, (double) new XAttribute (l), "l:double");
			Assert.AreEqual (double.PositiveInfinity, (double) new XAttribute (I), "I:double");
			Assert.AreEqual (double.PositiveInfinity, (double) new XAttribute (i), "i:double");
			Assert.AreEqual (double.NegativeInfinity, (double) new XAttribute (M), "M:double");
			Assert.AreEqual (double.NegativeInfinity, (double) new XAttribute (m), "m:double");
			Assert.AreEqual (double.NaN, ((double) new XAttribute (n)), "n:double");
			Assert.AreEqual (7f, (float) new XAttribute (a), "a:float");
			Assert.AreEqual (42f, (float) new XAttribute (b), "b:float");
			Assert.AreEqual (123f, (float) new XAttribute (c), "c:float");
			Assert.AreEqual (-101f, (float) new XAttribute (d), "d:float");
			Assert.AreEqual (0f, (float) new XAttribute (o), "o:float");
			Assert.AreEqual (1f, (float) new XAttribute (l), "l:float");
			Assert.AreEqual (float.PositiveInfinity, (float) new XAttribute (I), "I:float");
			Assert.AreEqual (float.PositiveInfinity, (float) new XAttribute (i), "i:float");
			Assert.AreEqual (float.NegativeInfinity, (float) new XAttribute (M), "M:float");
			Assert.AreEqual (float.NegativeInfinity, (float) new XAttribute (m), "m:float");
			Assert.AreEqual (float.NaN, ((float) new XAttribute (n)), "n:float");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (a); }, "a:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (b); }, "b:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (c); }, "c:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (d); }, "d:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (o); }, "o:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (l); }, "l:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (I); }, "I:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (i); }, "i:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (M); }, "M:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (m); }, "m:Guid");
			AssertThrows<FormatException> (() => { Guid z = (Guid) new XAttribute (n); }, "n:Guid");
			Assert.AreEqual (7, (int) new XAttribute (a), "a:int");
			Assert.AreEqual (42, (int) new XAttribute (b), "b:int");
			Assert.AreEqual (123, (int) new XAttribute (c), "c:int");
			Assert.AreEqual (-101, (int) new XAttribute (d), "d:int");
			Assert.AreEqual (0, (int) new XAttribute (o), "o:int");
			Assert.AreEqual (1, (int) new XAttribute (l), "l:int");
			AssertThrows<FormatException> (() => { int z = (int) new XAttribute (I); }, "I:int");
			AssertThrows<FormatException> (() => { int z = (int) new XAttribute (i); }, "i:int");
			AssertThrows<FormatException> (() => { int z = (int) new XAttribute (M); }, "M:int");
			AssertThrows<FormatException> (() => { int z = (int) new XAttribute (m); }, "m:int");
			AssertThrows<FormatException> (() => { int z = (int) new XAttribute (n); }, "n:int");
			Assert.AreEqual (7L, (long) new XAttribute (a), "a:long");
			Assert.AreEqual (42L, (long) new XAttribute (b), "b:long");
			Assert.AreEqual (123L, (long) new XAttribute (c), "c:long");
			Assert.AreEqual (-101L, (long) new XAttribute (d), "d:long");
			Assert.AreEqual (0L, (long) new XAttribute (o), "o:long");
			Assert.AreEqual (1L, (long) new XAttribute (l), "l:long");
			AssertThrows<FormatException> (() => { long z = (long) new XAttribute (I); }, "I:long");
			AssertThrows<FormatException> (() => { long z = (long) new XAttribute (i); }, "i:long");
			AssertThrows<FormatException> (() => { long z = (long) new XAttribute (M); }, "M:long");
			AssertThrows<FormatException> (() => { long z = (long) new XAttribute (m); }, "m:long");
			AssertThrows<FormatException> (() => { long z = (long) new XAttribute (n); }, "n:long");
			Assert.AreEqual (7u, (uint) new XAttribute (a), "a:uint");
			Assert.AreEqual (42u, (uint) new XAttribute (b), "b:uint");
			Assert.AreEqual (123u, (uint) new XAttribute (c), "c:uint");
			// LAMESPEC: see XmlConvertTests.ToUInt32().
			//AssertThrows<FormatException> (() => { uint z = (uint) new XAttribute (d); }, "d:uint");
			Assert.AreEqual (0u, (uint) new XAttribute (o), "o:uint");
			Assert.AreEqual (1u, (uint) new XAttribute (l), "l:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XAttribute (I); }, "I:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XAttribute (i); }, "i:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XAttribute (M); }, "M:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XAttribute (m); }, "m:uint");
			AssertThrows<FormatException> (() => { uint z = (uint) new XAttribute (n); }, "n:uint");
			Assert.AreEqual (7UL, (ulong) new XAttribute (a), "a:ulong");
			Assert.AreEqual (42UL, (ulong) new XAttribute (b), "b:ulong");
			Assert.AreEqual (123UL, (ulong) new XAttribute (c), "c:ulong");
			// LAMESPEC: see XmlConvertTests.ToUInt64().
			//AssertThrows<FormatException> (() => { ulong z = (ulong) new XAttribute (d); }, "d:ulong");
			Assert.AreEqual (0UL, (ulong) new XAttribute (o), "o:ulong");
			Assert.AreEqual (1UL, (ulong) new XAttribute (l), "l:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XAttribute (I); }, "I:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XAttribute (i); }, "i:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XAttribute (M); }, "M:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XAttribute (m); }, "m:ulong");
			AssertThrows<FormatException> (() => { ulong z = (ulong) new XAttribute (n); }, "n:ulong");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (a); }, "a:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (b); }, "b:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (c); }, "c:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (d); }, "d:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (o); }, "o:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (l); }, "l:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (I); }, "I:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (i); }, "i:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (M); }, "M:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (m); }, "m:TimeSpan");
			AssertThrows<FormatException> (() => { TimeSpan z = (TimeSpan) new XAttribute (n); }, "n:TimeSpan");

			// Perform some round-trip tests with numbers
			XAttribute x;
			const decimal @decimal = -41051609414188012238960097189m;
			const double @double = 8.5506609919892972E+307d;
			const float @float = -1.70151961E+37f;
			const int @int = -1051251773;
			const long @long = 4596767133891939716L;
			const uint @uint = 4106628142u;
			const ulong @ulong = 10713797297298255927UL;
			x = new XAttribute ("x", @decimal);
			Assert.IsNotNull ((decimal?) new XAttribute (x), "x:decimal?:null");
			Assert.AreEqual (@decimal, ((decimal?) new XAttribute (x)).Value, "x:decimal?:value");
			Assert.AreEqual (@decimal, (decimal) new XAttribute (x), "x:decimal");
			x = new XAttribute ("x", @double);
			Assert.IsNotNull ((double?) new XAttribute (x), "x:double?:null");
			Assert.AreEqual (@double, ((double?) new XAttribute (x)).Value, "x:double?:value");
			Assert.AreEqual (@double, (double) new XAttribute (x), "x:double");
			x = new XAttribute ("x", @float);
			Assert.IsNotNull ((float?) new XAttribute (x), "x:float?:null");
			Assert.AreEqual (@float, ((float?) new XAttribute (x)).Value, "x:float?:value");
			Assert.AreEqual (@float, (float) new XAttribute (x), "x:float");
			x = new XAttribute ("x", @int);
			Assert.IsNotNull ((int?) new XAttribute (x), "x:int?:null");
			Assert.AreEqual (@int, ((int?) new XAttribute (x)).Value, "x:int?:value");
			Assert.AreEqual (@int, (int) new XAttribute (x), "x:int");
			x = new XAttribute ("x", @long);
			Assert.IsNotNull ((long?) new XAttribute (x), "x:long?:null");
			Assert.AreEqual (@long, ((long?) new XAttribute (x)).Value, "x:long?:value");
			Assert.AreEqual (@long, (long) new XAttribute (x), "x:long");
			x = new XAttribute ("x", @uint);
			Assert.IsNotNull ((uint?) new XAttribute (x), "x:uint?:null");
			Assert.AreEqual (@uint, ((uint?) new XAttribute (x)).Value, "x:uint?:value");
			Assert.AreEqual (@uint, (uint) new XAttribute (x), "x:uint");
			x = new XAttribute ("x", @ulong);
			Assert.IsNotNull ((ulong?) new XAttribute (x), "x:ulong?:null");
			Assert.AreEqual (@ulong, ((ulong?) new XAttribute (x)).Value, "x:ulong?:value");
			Assert.AreEqual (@ulong, (ulong) new XAttribute (x), "x:ulong");
			x = new XAttribute ("x", double.NaN);
			Assert.IsNotNull ((double?) new XAttribute (x), "NaN:double?:null");
			Assert.AreEqual (double.NaN, ((double?) new XAttribute (x)).Value, "NaN:double?:value");
			Assert.AreEqual (double.NaN, (double) new XAttribute (x), "NaN:double");
			x = new XAttribute ("x", float.NaN);
			Assert.IsNotNull ((float?) new XAttribute (x), "NaN:float?:null");
			Assert.AreEqual (float.NaN, ((float?) new XAttribute (x)).Value, "NaN:float?:value");
			Assert.AreEqual (float.NaN, (float) new XAttribute (x), "NaN:float");
			x = new XAttribute ("x", double.PositiveInfinity);
			Assert.IsNotNull ((double?) new XAttribute (x), "+Inf:double?:null");
			Assert.AreEqual (double.PositiveInfinity, ((double?) new XAttribute (x)).Value, "+Inf:double?:value");
			Assert.AreEqual (double.PositiveInfinity, (double) new XAttribute (x), "+Inf:double");
			x = new XAttribute ("x", float.PositiveInfinity);
			Assert.IsNotNull ((float?) new XAttribute (x), "+Inf:float?:null");
			Assert.AreEqual (float.PositiveInfinity, ((float?) new XAttribute (x)).Value, "+Inf:float?:value");
			Assert.AreEqual (float.PositiveInfinity, (float) new XAttribute (x), "+Inf:float");
			x = new XAttribute ("x", double.NegativeInfinity);
			Assert.IsNotNull ((double?) new XAttribute (x), "-Inf:double?:null");
			Assert.AreEqual (double.NegativeInfinity, ((double?) new XAttribute (x)).Value, "-Inf:double?:value");
			Assert.AreEqual (double.NegativeInfinity, (double) new XAttribute (x), "-Inf:double");
			x = new XAttribute ("x", float.NegativeInfinity);
			Assert.IsNotNull ((float?) new XAttribute (x), "-Inf:float?:null");
			Assert.AreEqual (float.NegativeInfinity, ((float?) new XAttribute (x)).Value, "-Inf:float?:value");
			Assert.AreEqual (float.NegativeInfinity, (float) new XAttribute (x), "-Inf:float");

			// Perform overflow tests with numbers
			AssertThrows<OverflowException> (() => { decimal z = (decimal) new XAttribute ("z", "91051609414188012238960097189"); }, "z:decimal");
			AssertThrows<OverflowException> (() => { decimal? z = (decimal?) new XAttribute ("z", "91051609414188012238960097189"); }, "z:decimal?");
			AssertThrows<OverflowException> (() => { double z = (double) new XAttribute ("z", "8.5506609919892972E+654"); }, "z:double");
			AssertThrows<OverflowException> (() => { double? z = (double?) new XAttribute ("z", "8.5506609919892972E+654"); }, "z:double?");
			AssertThrows<OverflowException> (() => { float z = (float) new XAttribute ("z", @double); }, "z:float");
			AssertThrows<OverflowException> (() => { float? z = (float?) new XAttribute ("z", @double); }, "z:float?");
			AssertThrows<OverflowException> (() => { int z = (int) new XAttribute ("z", @long); }, "z:int");
			AssertThrows<OverflowException> (() => { int? z = (int?) new XAttribute ("z", @long); }, "z:int?");
			AssertThrows<OverflowException> (() => { long z = (long) new XAttribute ("z", @decimal); }, "z:long");
			AssertThrows<OverflowException> (() => { long? z = (long?) new XAttribute ("z", @decimal); }, "z:long?");
			AssertThrows<OverflowException> (() => { uint z = (uint) new XAttribute ("z", @ulong); }, "z:uint");
			AssertThrows<OverflowException> (() => { uint? z = (uint?) new XAttribute ("z", @ulong); }, "z:uint?");
			AssertThrows<OverflowException> (() => { ulong z = (ulong) new XAttribute ("z", -@decimal); }, "z:ulong");
			AssertThrows<OverflowException> (() => { ulong? z = (ulong?) new XAttribute ("z", -@decimal); }, "z:ulong?");
		}

		[Test]
		public void CastExtremes ()
		{
			// Test extremes/constants where round-trips should work in specific ways
			Assert.AreEqual (decimal.MaxValue, (decimal) new XAttribute ("k", decimal.MaxValue), "MaxValue:decimal");
			Assert.AreEqual (decimal.MinValue, (decimal) new XAttribute ("k", decimal.MinValue), "MinValue:decimal");
			Assert.AreEqual (decimal.MinusOne, (decimal) new XAttribute ("k", decimal.MinusOne), "MinusOne:decimal");
			Assert.AreEqual (decimal.One, (decimal) new XAttribute ("k", decimal.One), "One:decimal");
			Assert.AreEqual (decimal.Zero, (decimal) new XAttribute ("k", decimal.Zero), "Zero:decimal");
			Assert.AreEqual (double.MaxValue, (double) new XAttribute ("k", double.MaxValue), "MaxValue:double");
			Assert.AreEqual (double.MinValue, (double) new XAttribute ("k", double.MinValue), "MinValue:double");
			Assert.AreEqual (double.Epsilon, (double) new XAttribute ("k", double.Epsilon), "Epsilon:double");
			Assert.AreEqual (double.NaN, (double) new XAttribute ("k", double.NaN), "NaN:double");
			Assert.AreEqual (double.NegativeInfinity, (double) new XAttribute ("k", double.NegativeInfinity), "-Inf:double");
			Assert.AreEqual (double.PositiveInfinity, (double) new XAttribute ("k", double.PositiveInfinity), "+Inf:double");
			Assert.AreEqual (float.MaxValue, (float) new XAttribute ("k", float.MaxValue), "MaxValue:float");
			Assert.AreEqual (float.MinValue, (float) new XAttribute ("k", float.MinValue), "MinValue:float");
			Assert.AreEqual (float.Epsilon, (float) new XAttribute ("k", float.Epsilon), "Epsilon:float");
			Assert.AreEqual (float.NaN, (float) new XAttribute ("k", float.NaN), "NaN:float");
			Assert.AreEqual (float.NegativeInfinity, (float) new XAttribute ("k", float.NegativeInfinity), "-Inf:float");
			Assert.AreEqual (float.PositiveInfinity, (float) new XAttribute ("k", float.PositiveInfinity), "+Inf:float");
			Assert.AreEqual (int.MaxValue, (int) new XAttribute ("k", int.MaxValue), "MaxValue:int");
			Assert.AreEqual (int.MinValue, (int) new XAttribute ("k", int.MinValue), "MinValue:int");
			Assert.AreEqual (long.MaxValue, (long) new XAttribute ("k", long.MaxValue), "MaxValue:long");
			Assert.AreEqual (long.MinValue, (long) new XAttribute ("k", long.MinValue), "MinValue:long");
			Assert.AreEqual (uint.MaxValue, (uint) new XAttribute ("k", uint.MaxValue), "MaxValue:uint");
			Assert.AreEqual (uint.MinValue, (uint) new XAttribute ("k", uint.MinValue), "MinValue:uint");
			Assert.AreEqual (ulong.MaxValue, (ulong) new XAttribute ("k", ulong.MaxValue), "MaxValue:ulong");
			Assert.AreEqual (ulong.MinValue, (ulong) new XAttribute ("k", ulong.MinValue), "MinValue:ulong");
			Assert.AreEqual (decimal.MaxValue, (decimal?) new XAttribute ("k", decimal.MaxValue), "MaxValue:decimal?");
			Assert.AreEqual (decimal.MinValue, (decimal?) new XAttribute ("k", decimal.MinValue), "MinValue:decimal?");
			Assert.AreEqual (decimal.MinusOne, (decimal?) new XAttribute ("k", decimal.MinusOne), "MinusOne:decimal?");
			Assert.AreEqual (decimal.One, (decimal?) new XAttribute ("k", decimal.One), "One:decimal?");
			Assert.AreEqual (decimal.Zero, (decimal?) new XAttribute ("k", decimal.Zero), "Zero:decimal?");
			Assert.AreEqual (double.MaxValue, (double?) new XAttribute ("k", double.MaxValue), "MaxValue:double?");
			Assert.AreEqual (double.MinValue, (double?) new XAttribute ("k", double.MinValue), "MinValue:double?");
			Assert.AreEqual (double.Epsilon, (double?) new XAttribute ("k", double.Epsilon), "Epsilon:double?");
			Assert.AreEqual (double.NaN, (double?) new XAttribute ("k", double.NaN), "NaN:double?");
			Assert.AreEqual (double.NegativeInfinity, (double?) new XAttribute ("k", double.NegativeInfinity), "-Inf:double?");
			Assert.AreEqual (double.PositiveInfinity, (double?) new XAttribute ("k", double.PositiveInfinity), "+Inf:double?");
			Assert.AreEqual (float.MaxValue, (float?) new XAttribute ("k", float.MaxValue), "MaxValue:float?");
			Assert.AreEqual (float.MinValue, (float?) new XAttribute ("k", float.MinValue), "MinValue:float?");
			Assert.AreEqual (float.Epsilon, (float?) new XAttribute ("k", float.Epsilon), "Epsilon:float?");
			Assert.AreEqual (float.NaN, (float?) new XAttribute ("k", float.NaN), "NaN:float?");
			Assert.AreEqual (float.NegativeInfinity, (float?) new XAttribute ("k", float.NegativeInfinity), "-Inf:float?");
			Assert.AreEqual (float.PositiveInfinity, (float?) new XAttribute ("k", float.PositiveInfinity), "+Inf:float?");
			Assert.AreEqual (int.MaxValue, (int?) new XAttribute ("k", int.MaxValue), "MaxValue:int?");
			Assert.AreEqual (int.MinValue, (int?) new XAttribute ("k", int.MinValue), "MinValue:int?");
			Assert.AreEqual (long.MaxValue, (long?) new XAttribute ("k", long.MaxValue), "MaxValue:long?");
			Assert.AreEqual (long.MinValue, (long?) new XAttribute ("k", long.MinValue), "MinValue:long?");
			Assert.AreEqual (uint.MaxValue, (uint?) new XAttribute ("k", uint.MaxValue), "MaxValue:uint?");
			Assert.AreEqual (uint.MinValue, (uint?) new XAttribute ("k", uint.MinValue), "MinValue:uint?");
			Assert.AreEqual (ulong.MaxValue, (ulong?) new XAttribute ("k", ulong.MaxValue), "MaxValue:ulong?");
			Assert.AreEqual (ulong.MinValue, (ulong?) new XAttribute ("k", ulong.MinValue), "MinValue:ulong?");
			Assert.AreEqual (DateTime.MaxValue, (DateTime) new XAttribute ("k", DateTime.MaxValue), "MaxValue:DateTime");
			Assert.AreEqual (DateTime.MinValue, (DateTime) new XAttribute ("k", DateTime.MinValue), "MinValue:DateTime");
			Assert.AreEqual (DateTime.MaxValue, (DateTime?) new XAttribute ("k", DateTime.MaxValue), "MaxValue:DateTime?");
			Assert.AreEqual (DateTime.MinValue, (DateTime?) new XAttribute ("k", DateTime.MinValue), "MinValue:DateTime?");
			Assert.AreEqual (DateTimeOffset.MaxValue, (DateTimeOffset) new XAttribute ("k", DateTimeOffset.MaxValue), "MaxValue:DateTimeOffset");
			Assert.AreEqual (DateTimeOffset.MinValue, (DateTimeOffset) new XAttribute ("k", DateTimeOffset.MinValue), "MinValue:DateTimeOffset");
			Assert.AreEqual (DateTimeOffset.MaxValue, (DateTimeOffset?) new XAttribute ("k", DateTimeOffset.MaxValue), "MaxValue:DateTimeOffset?");
			Assert.AreEqual (DateTimeOffset.MinValue, (DateTimeOffset?) new XAttribute ("k", DateTimeOffset.MinValue), "MinValue:DateTimeOffset?");
			Assert.AreEqual (TimeSpan.MaxValue, (TimeSpan) new XAttribute ("k", TimeSpan.MaxValue), "MaxValue:TimeSpan");
			Assert.AreEqual (TimeSpan.MinValue, (TimeSpan) new XAttribute ("k", TimeSpan.MinValue), "MinValue:TimeSpan");
			Assert.AreEqual (TimeSpan.MaxValue, (TimeSpan?) new XAttribute ("k", TimeSpan.MaxValue), "MaxValue:TimeSpan?");
			Assert.AreEqual (TimeSpan.MinValue, (TimeSpan?) new XAttribute ("k", TimeSpan.MinValue), "MinValue:TimeSpan?");
		}

		[Test]
		public void CastBooleans ()
		{
			Assert.IsNotNull ((bool?) new XAttribute ("fq", "false"), "#1a");
			Assert.AreEqual (false, ((bool?) new XAttribute ("fq", "false")).Value, "#1b");
			Assert.IsNotNull ((bool?) new XAttribute ("tq", "true"), "#2a");
			Assert.AreEqual (true, ((bool?) new XAttribute ("tq", "true")).Value, "#2b");
			Assert.IsNotNull ((bool?) new XAttribute ("Fq", "False"), "#3a");
			Assert.AreEqual (false, ((bool?) new XAttribute ("Fq", "False")).Value, "#3b");
			Assert.IsNotNull ((bool?) new XAttribute ("Tq", "True"), "#4a");
			Assert.AreEqual (true, ((bool?) new XAttribute ("Tq", "True")).Value, "#4b");
			Assert.IsNotNull ((bool?) new XAttribute ("Fs", "   False \t \r "), "#5a");
			Assert.AreEqual (false, ((bool?) new XAttribute ("Fs", "   False \t \r ")).Value, "#5b");
			Assert.IsNotNull ((bool?) new XAttribute ("Ts", " \t True  \n  "), "#6a");
			Assert.AreEqual (true, ((bool?) new XAttribute ("Ts", " \t True  \n  ")).Value, "#6b");
			Assert.AreEqual (false, (bool) new XAttribute ("f", "false"), "#7");
			Assert.AreEqual (true, (bool) new XAttribute ("t", "true"), "#8");
			Assert.AreEqual (false, (bool) new XAttribute ("F", "False"), "#9");
			Assert.AreEqual (true, (bool) new XAttribute ("T", "True"), "#10");
			Assert.AreEqual (false, (bool)new XAttribute ("fs", " false  "), "#11");
			Assert.AreEqual (true, (bool)new XAttribute ("ts", "  true "), "#12");
			Assert.IsNotNull ((bool?) new XAttribute ("x", true), "#13a");
			Assert.IsTrue (((bool?) new XAttribute ("x", true)).Value, "#13b");
			Assert.IsTrue ((bool) new XAttribute ("x", true), "#13c");
			Assert.IsNotNull ((bool?) new XAttribute ("x", false), "#14a");
			Assert.IsFalse (((bool?) new XAttribute ("x", false)).Value, "#14b");
			Assert.IsFalse ((bool) new XAttribute ("x", false), "#14c");
			Assert.IsTrue ((bool) new XAttribute ("x", bool.TrueString), "#15a");
			Assert.IsFalse ((bool) new XAttribute ("x", bool.FalseString), "#15b");
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

			XAttribute b = new XAttribute ("b", "{7ECEBF9A-2907-439c-807D-4820B919EA57}");
			XAttribute d = new XAttribute ("d", "  26575b21-14cd-445e-8ffa-e2bc247b2ec9");
			XAttribute n = new XAttribute ("n", "  a84146f903A54af1ad977bC779572b79  \t ");
			XAttribute p = new XAttribute ("p", " \t \n (178a6b51-11ef-48fb-83bd-57b499f9c1e6) ");
			XAttribute z = new XAttribute ("z", "00000000-0000-0000-0000-000000000000\r\n");
			XAttribute x = new XAttribute ("x", rx);

			Assert.IsNotNull ((Guid?) new XAttribute (b), "#1a");
			Assert.AreEqual (rb, ((Guid?) new XAttribute (b)).Value, "#1b");
			Assert.AreEqual (rb, (Guid) new XAttribute (b), "#1c");
			Assert.AreEqual (rb, (Guid) new XAttribute ("r", rb), "#1d");
			Assert.IsNotNull ((Guid?) new XAttribute ("r", rb), "#1e");
			Assert.AreEqual (rb, ((Guid?) new XAttribute ("r", rb)).Value, "#1f");

			Assert.IsNotNull ((Guid?) new XAttribute (d), "#2a");
			Assert.AreEqual (rd, ((Guid?) new XAttribute (d)).Value, "#2b");
			Assert.AreEqual (rd, (Guid) new XAttribute (d), "#2c");
			Assert.AreEqual (rd, (Guid) new XAttribute ("r", rd), "#2d");
			Assert.IsNotNull ((Guid?) new XAttribute ("r", rd), "#2e");
			Assert.AreEqual (rd, ((Guid?) new XAttribute ("r", rd)).Value, "#2f");

			Assert.IsNotNull ((Guid?) new XAttribute (n), "#3a");
			Assert.AreEqual (rn, ((Guid?) new XAttribute (n)).Value, "#3b");
			Assert.AreEqual (rn, (Guid) new XAttribute (n), "#3c");
			Assert.AreEqual (rn, (Guid) new XAttribute ("r", rn), "#3d");
			Assert.IsNotNull ((Guid?) new XAttribute ("r", rn), "#3e");
			Assert.AreEqual (rn, ((Guid?) new XAttribute ("r", rn)).Value, "#3f");

			Assert.IsNotNull ((Guid?) new XAttribute (p), "#4a");
			Assert.AreEqual (rp, ((Guid?) new XAttribute (p)).Value, "#4b");
			Assert.AreEqual (rp, (Guid) new XAttribute (p), "#4c");
			Assert.AreEqual (rp, (Guid) new XAttribute ("r", rp), "#4d");
			Assert.IsNotNull ((Guid?) new XAttribute ("r", rp), "#4e");
			Assert.AreEqual (rp, ((Guid?) new XAttribute ("r", rp)).Value, "#4f");

			Assert.IsNotNull ((Guid?) new XAttribute (z), "#5a");
			Assert.AreEqual (rz, ((Guid?) new XAttribute (z)).Value, "#5b");
			Assert.AreEqual (rz, (Guid) new XAttribute (z), "#5c");

			Assert.IsNotNull ((Guid?) new XAttribute (x), "#6a");
			Assert.AreEqual (rx, ((Guid?) new XAttribute (x)).Value, "#6b");
			Assert.AreEqual (rx, (Guid) new XAttribute (x), "#6c");
		}

		[Test]
		public void CastDateTimes ()
		{
			const long weirdTicks = 8070L;  // Examples of problematic fractions of seconds on Mono 2.6.1: xxx.8796082, xxx.7332000, xxx.0050300, xxx.5678437, xxx.0008070, xxx.0769000 and xxx.2978530 (about 1 in 8 of all possible fractions seem to fail)
			DateTime ra = new DateTime (1987, 1, 23, 21, 45, 36, 89, DateTimeKind.Unspecified);
			DateTime rb = new DateTime (2001, 2, 3, 4, 5, 6, 789, DateTimeKind.Local);
			DateTime rc = new DateTime (2010, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromTicks (weirdTicks);
			DateTime rd = new DateTime (1956, 11, 2, 0, 34, 0);
			DateTime re = new DateTime (DateTime.Today.Ticks + 619917654321L);
			DateTime rx = DateTime.Now;
			DateTime rz = DateTime.UtcNow;

			XAttribute a = new XAttribute ("a", "1987-01-23T21:45:36.089");
			XAttribute b = new XAttribute ("b", " \n 2001-02-03T04:05:06.789" + rb.ToString ("zzz") + "  \r   ");
			XAttribute c = new XAttribute ("c", "2010-01-02T00:00:00." + weirdTicks.ToString ("d7").TrimEnd ('0') + "Z");
			XAttribute d = new XAttribute ("d", "  Nov 2, 1956  12:34 AM \r\n   \t");
			XAttribute e = new XAttribute ("e", " \t 17:13:11.7654321 ");
			XAttribute x = new XAttribute ("x", rx);
			XAttribute z = new XAttribute ("z", rz);

			Assert.IsNotNull ((DateTime?) new XAttribute (a), "#1a");
			Assert.AreEqual (ra, ((DateTime?) new XAttribute (a)).Value, "#1b");
			Assert.AreEqual (ra, (DateTime) new XAttribute (a), "#1c");
			Assert.AreEqual (ra, (DateTime) new XAttribute ("r", ra), "#1d");
			Assert.IsNotNull ((DateTime?) new XAttribute ("r", ra), "#1e");
			Assert.AreEqual (ra, ((DateTime?) new XAttribute ("r", ra)).Value, "#1f");

			Assert.IsNotNull ((DateTime?) new XAttribute (b), "#2a");
			Assert.AreEqual (rb, ((DateTime?) new XAttribute (b)).Value, "#2b");
			Assert.AreEqual (rb, (DateTime) new XAttribute (b), "#2c");
			Assert.AreEqual (rb, (DateTime) new XAttribute ("r", rb), "#2d");
			Assert.IsNotNull ((DateTime?) new XAttribute ("r", rb), "#2e");
			Assert.AreEqual (rb, ((DateTime?) new XAttribute ("r", rb)).Value, "#2f");

			Assert.AreEqual (10000000L, TimeSpan.TicksPerSecond, "#3a");  // Should be same on all platforms (sanity check for next logical step)
			Assert.AreEqual (weirdTicks, rc.Ticks % TimeSpan.TicksPerSecond, "#3b");  // Prerequisite: No ticks lost in raw data
			Assert.IsNotNull ((DateTime?) new XAttribute (c), "#3c");
			Assert.AreEqual (weirdTicks, ((DateTime?) new XAttribute (c)).Value.Ticks % TimeSpan.TicksPerSecond, "#3d");  // Did casting lose any ticks belonging to fractional seconds?
			Assert.AreEqual (rc, ((DateTime?) new XAttribute (c)).Value, "#3e");
			Assert.AreEqual (weirdTicks, ((DateTime) new XAttribute (c)).Ticks % TimeSpan.TicksPerSecond, "#3f");
			Assert.AreEqual (rc, (DateTime) new XAttribute (c), "#3g");
			Assert.AreEqual (rc, (DateTime) new XAttribute ("r", rc), "#3h");
			Assert.IsNotNull ((DateTime?) new XAttribute ("r", rc), "#3i");
			Assert.AreEqual (rc, ((DateTime?) new XAttribute ("r", rc)).Value, "#3j");

			Assert.IsNotNull ((DateTime?) new XAttribute (d), "#4a");
			Assert.AreEqual (rd, ((DateTime?) new XAttribute (d)).Value, "#4b");
			Assert.AreEqual (rd, (DateTime) new XAttribute (d), "#4c");
			Assert.AreEqual (rd, (DateTime) new XAttribute ("r", rd), "#4d");
			Assert.IsNotNull ((DateTime?) new XAttribute ("r", rd), "#4e");
			Assert.AreEqual (rd, ((DateTime?) new XAttribute ("r", rd)).Value, "#4f");

			Assert.IsNotNull ((DateTime?) new XAttribute (e), "#5a");
			Assert.AreEqual (re, ((DateTime?) new XAttribute (e)).Value, "#5b");
			Assert.AreEqual (re, (DateTime) new XAttribute (e), "#5c");
			Assert.AreEqual (re, (DateTime) new XAttribute ("r", re), "#5d");
			Assert.IsNotNull ((DateTime?) new XAttribute ("r", re), "#5e");
			Assert.AreEqual (re, ((DateTime?) new XAttribute ("r", re)).Value, "#5f");

			Assert.IsNotNull ((DateTime?) new XAttribute (x), "#6a");
			Assert.AreEqual (rx, ((DateTime?) new XAttribute (x)).Value, "#6b");
			Assert.AreEqual (rx, (DateTime) new XAttribute (x), "#6c");

			Assert.IsNotNull ((DateTime?) new XAttribute (z), "#7a");
			Assert.AreEqual (rz, ((DateTime?) new XAttribute (z)).Value, "#7b");
			Assert.AreEqual (rz, (DateTime) new XAttribute (z), "#7c");
		}

		[Test]
		public void CastDateTimeOffsets ()
		{
			DateTimeOffset ra = new DateTimeOffset (1987, 1, 23, 21, 45, 36, 89, TimeSpan.FromHours (+13.75));  // e.g., Chatham Islands (daylight-savings time)
			DateTimeOffset rb = new DateTimeOffset (2001, 2, 3, 4, 5, 6, 789, DateTimeOffset.Now.Offset);  // Local time
			DateTimeOffset rc = new DateTimeOffset (2010, 1, 2, 0, 0, 0, 0, TimeSpan.Zero);  // UTC, and midnight
			DateTimeOffset rd = new DateTimeOffset (1956, 11, 2, 12, 34, 10, TimeSpan.FromHours (-3.5));
			DateTimeOffset re = new DateTimeOffset (630646468231147764, TimeSpan.FromHours (2));  // UTC+2, and with full resolution
			DateTimeOffset rf = new DateTimeOffset (643392740967552000, TimeSpan.Zero);  // UTC, and with a potentially problematic fractional second that might lose a tick on Mono 2.6.1
			DateTimeOffset rx = DateTimeOffset.Now;
			DateTimeOffset rz = DateTimeOffset.UtcNow;

			XAttribute a = new XAttribute ("a", "1987-01-23T21:45:36.089+13:45");
			XAttribute b = new XAttribute ("b", "2001-02-03T04:05:06.789" + DateTimeOffset.Now.ToString ("zzz"));
			XAttribute c = new XAttribute ("c", "   2010-01-02T00:00:00Z \t");
			XAttribute d = new XAttribute ("d", "  Nov 2, 1956  12:34:10 PM   -3:30 \r\n   \t");
			XAttribute e = new XAttribute ("e", "1999-06-10T21:27:03.1147764+02:00");
			XAttribute f = new XAttribute ("f", "2039-10-31T12:34:56.7552+00:00");
			XAttribute x = new XAttribute ("x", rx);
			XAttribute z = new XAttribute ("z", rz);

			Assert.IsNotNull ((DateTimeOffset?) new XAttribute (a), "#1a");
			Assert.AreEqual (ra, ((DateTimeOffset?) new XAttribute (a)).Value, "#1b");
			Assert.AreEqual (ra, (DateTimeOffset) new XAttribute (a), "#1c");
			Assert.AreEqual (ra, (DateTimeOffset) new XAttribute ("r", ra), "#1d");
			Assert.IsNotNull ((DateTimeOffset?) new XAttribute ("r", ra), "#1e");
			Assert.AreEqual (ra, ((DateTimeOffset?) new XAttribute ("r", ra)).Value, "#1f");

			Assert.IsNotNull ((DateTimeOffset?) new XAttribute (b), "#2a");
			Assert.AreEqual (rb, ((DateTimeOffset?) new XAttribute (b)).Value, "#2b");
			Assert.AreEqual (rb, (DateTimeOffset) new XAttribute (b), "#2c");
			Assert.AreEqual (rb, (DateTimeOffset) new XAttribute ("r", rb), "#2d");
			Assert.IsNotNull ((DateTimeOffset?) new XAttribute ("r", rb), "#2e");
			Assert.AreEqual (rb, ((DateTimeOffset?) new XAttribute ("r", rb)).Value, "#2f");

			Assert.IsNotNull ((DateTimeOffset?) new XAttribute (c), "#3a");
			Assert.AreEqual (rc, ((DateTimeOffset?) new XAttribute (c)).Value, "#3b");
			Assert.AreEqual (rc, (DateTimeOffset) new XAttribute (c), "#3c");
			Assert.AreEqual (rc, (DateTimeOffset) new XAttribute ("r", rc), "#3d");
			Assert.IsNotNull ((DateTimeOffset?) new XAttribute ("r", rc), "#3e");
			Assert.AreEqual (rc, ((DateTimeOffset?) new XAttribute ("r", rc)).Value, "#3f");

			AssertThrows<FormatException> (() => { DateTimeOffset? r = (DateTimeOffset?) new XAttribute (d); }, "#4a");
			AssertThrows<FormatException> (() => { DateTimeOffset r = (DateTimeOffset) new XAttribute (d); }, "#4b");
			Assert.AreEqual (rd, DateTimeOffset.Parse (d.Value), "#4c");  // Sanity check: Okay for standalone DateTimeOffset but not as XML as in above

			Assert.IsNotNull ((DateTimeOffset?) new XAttribute (e), "#5a");
			Assert.AreEqual (re, ((DateTimeOffset?) new XAttribute (e)).Value, "#5b");
			Assert.AreEqual (re, (DateTimeOffset) new XAttribute (e), "#5c");
			Assert.AreEqual (re, (DateTimeOffset) new XAttribute ("r", re), "#5d");
			Assert.IsNotNull ((DateTimeOffset?) new XAttribute ("r", re), "#5e");
			Assert.AreEqual (re, ((DateTimeOffset?) new XAttribute ("r", re)).Value, "#5f");

			Assert.IsNotNull ((DateTimeOffset?) new XAttribute (f), "#6a");
			Assert.AreEqual (rf, ((DateTimeOffset?) new XAttribute (f)).Value, "#6b");
			Assert.AreEqual (rf, (DateTimeOffset) new XAttribute (f), "#6c");
			Assert.AreEqual (rf, (DateTimeOffset) new XAttribute ("r", rf), "#6d");
			Assert.IsNotNull ((DateTimeOffset?) new XAttribute ("r", rf), "#6e");
			Assert.AreEqual (rf, ((DateTimeOffset?) new XAttribute ("r", rf)).Value, "#6f");

			Assert.IsNotNull ((DateTimeOffset?) new XAttribute (x), "#7a");
			Assert.AreEqual (rx, ((DateTimeOffset?) new XAttribute (x)).Value, "#7b");
			Assert.AreEqual (rx, (DateTimeOffset) new XAttribute (x), "#7c");

			Assert.IsNotNull ((DateTimeOffset?) new XAttribute (z), "#8a");
			Assert.AreEqual (rz, ((DateTimeOffset?) new XAttribute (z)).Value, "#8b");
			Assert.AreEqual (rz, (DateTimeOffset) new XAttribute (z), "#8c");
		}

		[Test]
		public void CastTimeSpans ()
		{
			TimeSpan ra = new TimeSpan (23, 21, 45, 36, 89);
			TimeSpan rb = -new TimeSpan (3, 4, 5, 6, 789);
			TimeSpan rc = new TimeSpan (2, 0, 0, 0, 0);
			TimeSpan rd = new TimeSpan (26798453L);  // in ticks, using full resolution and longer than a second
			TimeSpan re = new TimeSpan (2710L);  // in ticks, a sub-millisecond interval
			TimeSpan rx = new TimeSpan (0, 3, 8, 29, 734);
			TimeSpan rz = TimeSpan.Zero;

			XAttribute a = new XAttribute ("a", "P23DT21H45M36.089S");
			XAttribute b = new XAttribute ("b", "-P3DT4H5M6.789S");
			XAttribute c = new XAttribute ("c", " \r\n   \tP2D  ");
			XAttribute d = new XAttribute ("d", "PT2.6798453S");
			XAttribute e = new XAttribute ("e", "PT0.000271S");
			XAttribute x = new XAttribute ("x", rx);
			XAttribute z = new XAttribute ("z", rz);

			Assert.IsNotNull ((TimeSpan?) new XAttribute (a), "#1a");
			Assert.AreEqual (ra, ((TimeSpan?) new XAttribute (a)).Value, "#1b");
			Assert.AreEqual (ra, (TimeSpan) new XAttribute (a), "#1c");
			Assert.AreEqual (ra, (TimeSpan) new XAttribute ("r", ra), "#1d");
			Assert.IsNotNull ((TimeSpan?) new XAttribute ("r", ra), "#1e");
			Assert.AreEqual (ra, ((TimeSpan?) new XAttribute ("r", ra)).Value, "#1f");

			Assert.IsNotNull ((TimeSpan?) new XAttribute (b), "#2a");
			Assert.AreEqual (rb, ((TimeSpan?) new XAttribute (b)).Value, "#2b");
			Assert.AreEqual (rb, (TimeSpan) new XAttribute (b), "#2c");
			Assert.AreEqual (rb, (TimeSpan) new XAttribute ("r", rb), "#2d");
			Assert.IsNotNull ((TimeSpan?) new XAttribute ("r", rb), "#2e");
			Assert.AreEqual (rb, ((TimeSpan?) new XAttribute ("r", rb)).Value, "#2f");

			Assert.IsNotNull ((TimeSpan?) new XAttribute (c), "#3a");
			Assert.AreEqual (rc, ((TimeSpan?) new XAttribute (c)).Value, "#3b");
			Assert.AreEqual (rc, (TimeSpan) new XAttribute (c), "#3c");
			Assert.AreEqual (rc, (TimeSpan) new XAttribute ("r", rc), "#3d");
			Assert.IsNotNull ((TimeSpan?) new XAttribute ("r", rc), "#3e");
			Assert.AreEqual (rc, ((TimeSpan?) new XAttribute ("r", rc)).Value, "#3f");

			Assert.IsNotNull ((TimeSpan?) new XAttribute (d), "#4a");
			Assert.AreEqual (rd, ((TimeSpan?) new XAttribute (d)).Value, "#4b");
			Assert.AreEqual (rd, (TimeSpan) new XAttribute (d), "#4c");
			Assert.AreEqual (rd, (TimeSpan) new XAttribute ("r", rd), "#4d");
			Assert.IsNotNull ((TimeSpan?) new XAttribute ("r", rd), "#4e");
			Assert.AreEqual (rd, ((TimeSpan?) new XAttribute ("r", rd)).Value, "#4f");

			Assert.IsNotNull ((TimeSpan?) new XAttribute (e), "#5a");
			Assert.AreEqual (re, ((TimeSpan?) new XAttribute (e)).Value, "#5b");
			Assert.AreEqual (re, (TimeSpan) new XAttribute (e), "#5c");
			Assert.AreEqual (re, (TimeSpan) new XAttribute ("r", re), "#5d");
			Assert.IsNotNull ((TimeSpan?) new XAttribute ("r", re), "#5e");
			Assert.AreEqual (re, ((TimeSpan?) new XAttribute ("r", re)).Value, "#5f");

			Assert.IsNotNull ((TimeSpan?) new XAttribute (x), "#6a");
			Assert.AreEqual (rx, ((TimeSpan?) new XAttribute (x)).Value, "#6b");
			Assert.AreEqual (rx, (TimeSpan) new XAttribute (x), "#6c");

			Assert.IsNotNull ((TimeSpan?) new XAttribute (z), "#7a");
			Assert.AreEqual (rz, ((TimeSpan?) new XAttribute (z)).Value, "#7b");
			Assert.AreEqual (rz, (TimeSpan) new XAttribute (z), "#7c");
		}
#pragma warning restore 219
	}
}
