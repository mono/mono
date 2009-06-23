//
// ChtmlTextWriterTest.cs: Unit tests for System.Web.UI.ChtmlTextWriter
//
// Author:
//	Cesar Lopez Nataren <cnataren@novell.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.IO;
using System.Web.UI;
using NUnit.Framework;
using System.Collections;

namespace MonoTests.System.Web.UI {

	public class ChtmlTextWriterTester : ChtmlTextWriter {

		public ChtmlTextWriterTester (TextWriter writer)
			: this (writer, DefaultTabString)
		{
		}

		public ChtmlTextWriterTester (TextWriter writer, string tabString)
			: base (writer, tabString)
		{
		}

		public bool IsRecognizedAttribute (string elementName, string attributeName)
		{
			Hashtable elem_attrs = (Hashtable) RecognizedAttributes [elementName];

			if (elem_attrs == null)
				return false;
			return elem_attrs [attributeName] != null;
		}

		public string PublicGetAttributeName (HtmlTextWriterAttribute attrKey)
		{
			return GetAttributeName (attrKey);
		}

		public bool PublicOnAttributeRender (string name, string value, HtmlTextWriterAttribute attr)
		{
			return OnAttributeRender (name, value, attr);
		}

		public bool PublicOnStyleAttributeRender (string name, string value, HtmlTextWriterStyle key)
		{
			return OnStyleAttributeRender (name, value, key);
		}

		public bool PublicOnTagRender (string name, HtmlTextWriterTag tag)
		{
			return OnTagRender (name, tag);
		}

		public Hashtable PublicGlobalSuppressedAttributes {
			get { return GlobalSuppressedAttributes; }
		}

		public Hashtable PublicSuppressedAttributes {
			get { return SuppressedAttributes; }
		}
	}

	[TestFixture]
	public class ChtmlTextWriterTest {

		ChtmlTextWriterTester chtml;
		StringWriter writer;

		string absent_element = "absent-elem";
		string absent_attr = "absent-attr";

		[SetUp]
		public void SetupTests ()
		{
			writer = new StringWriter ();
			chtml = new ChtmlTextWriterTester (writer);
		}

		[Test]
		public void AddRecognizedAttributeTest ()
		{
			chtml.AddRecognizedAttribute (absent_element, absent_attr);
			Assert.AreEqual (true, chtml.IsRecognizedAttribute (absent_element, absent_attr), "#A01");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddRecognizedAttribute2 ()
		{
			AddRecognizedAttributeTest ();
			AddRecognizedAttributeTest ();
		}

		[Test]
		public void RemoveRecognizedAttributeTest ()
		{
			AddRecognizedAttributeTest ();

			chtml.RemoveRecognizedAttribute (absent_element, absent_attr);
			Assert.AreEqual (false, chtml.IsRecognizedAttribute (absent_element, absent_attr), "#B01");

			string version_two = "v2";
			chtml.RemoveRecognizedAttribute (absent_element + version_two, absent_attr + version_two);
		}

		[Test]
		public void WriteBreakTest ()
		{
			string br = "<br>";
			chtml.WriteBreak ();
			Assert.AreEqual (true, br == writer.ToString (), "#C01");
		}

		[Test]
		public void WriteEncodedTest ()
		{
			string encoded_text = "<custID> & <invoice#>";
			string unencoded_text = "&lt;custID&gt; &amp; &lt;invoice#&gt;";
			chtml.WriteEncodedText (encoded_text);

			Assert.AreEqual (true, unencoded_text == writer.ToString (), "#D01");
		}

		[Test]
		public void OnAttributeRenderTest ()
		{
			HtmlTextWriterAttribute [] enum_values = (HtmlTextWriterAttribute []) Enum.GetValues (typeof (HtmlTextWriterAttribute));
			int i = 0;

			foreach (HtmlTextWriterAttribute attr in enum_values) {
				try {
					chtml.PublicOnAttributeRender (chtml.PublicGetAttributeName (attr), "accesskey", attr);
				} catch (ArgumentNullException e) {
					i++;
				}
			}
			Assert.AreEqual (enum_values.Length, i, "#E01");
		}

		[Test]
		public void OnStyleAttributeRenderTest ()
		{
			bool expected;
			int i = 0;

			foreach (HtmlTextWriterStyle tag in Enum.GetValues (typeof (HtmlTextWriterStyle))) {
				expected = (tag == HtmlTextWriterStyle.Display);
				Assert.AreEqual (expected, chtml.PublicOnStyleAttributeRender ("foo", "foo", tag), "#F0" + i++);
			}
		}


		[Test]
		public void OnTagRenderTest ()
		{
			int i = 0;
			bool expected;

			foreach (HtmlTextWriterTag tag in Enum.GetValues (typeof (HtmlTextWriterTag))) {
				expected = (tag != HtmlTextWriterTag.Span);
				Assert.AreEqual (expected, chtml.PublicOnTagRender ("foo", tag), "#G0" + i++);
			}
		}
	}
}

#endif
