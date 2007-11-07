//
// SecurityElementTest.cs - NUnit Test Cases for System.Security.SecurityElement
//
// Authors:
//	Lawrence Pit (loz@cable.a2000.nl)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Portions (C) 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Globalization;
using System.Security;

using NUnit.Framework;

namespace MonoTests.System.Security {

	[TestFixture]
	public class SecurityElementTest {

		SecurityElement elem;
		
		[SetUp]
		public void SetUp () 
		{
			elem = CreateElement ();
		}

		private SecurityElement CreateElement ()
		{
			SecurityElement elem = new SecurityElement ("IPermission");
			elem.AddAttribute ("class", "System");
			elem.AddAttribute ("version", "1");
			
			SecurityElement child = new SecurityElement ("ConnectAccess");
			elem.AddChild (child);
			
			SecurityElement grandchild = new SecurityElement ("ENDPOINT", "some text");
			grandchild.AddAttribute ("transport", "All");
			grandchild.AddAttribute ("host", "localhost");
			grandchild.AddAttribute ("port", "8080");
			child.AddChild (grandchild);

			SecurityElement grandchild2 = new SecurityElement ("ENDPOINT");
			grandchild2.AddAttribute ("transport", "Tcp");
			grandchild2.AddAttribute ("host", "www.ximian.com");
			grandchild2.AddAttribute ("port", "All");
			child.AddChild (grandchild2);
			
			return elem;
		}

		[Test]
		public void Constructor1 ()
		{
			SecurityElement se = new SecurityElement ("tag");
			Assert.IsNull (se.Attributes, "#A1");
			Assert.IsNull (se.Children, "#A2");
			Assert.AreEqual ("tag", se.Tag, "#A3");
			Assert.IsNull (se.Text, "#A4");

			se = new SecurityElement (string.Empty);
			Assert.IsNull (se.Attributes, "#B1");
			Assert.IsNull (se.Children, "#B2");
			Assert.AreEqual (string.Empty, se.Tag, "#B3");
			Assert.IsNull (se.Text, "#B4");
		}

		[Test]
		public void Constructor1_Tag_Invalid ()
		{
			try {
				new SecurityElement ("Na<me");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid element tag Nam<e
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("Na<me") != -1, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
			}

			try {
				new SecurityElement ("Nam>e");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid element tag Nam>e
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("Nam>e") != -1, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
			}
		}

		[Test]
		public void Constructor1_Tag_Null ()
		{
			try {
				new SecurityElement (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("tag", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor2 () 
		{
			SecurityElement se = new SecurityElement ("tag", "text");
			Assert.IsNull (se.Attributes, "EmptyAttributes");
			Assert.IsNull (se.Children, "EmptyChildren");
			Assert.AreEqual ("tag", se.Tag, "Tag");
			Assert.AreEqual ("text", se.Text, "Text");
		}

		[Test]
		public void Constructor2_Tag_Invalid ()
		{
			try {
				new SecurityElement ("Na<me", "text");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid element tag Nam<e
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("Na<me") != -1, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
			}

			try {
				new SecurityElement ("Nam>e", "text");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid element tag Nam>e
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("Nam>e") != -1, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
			}
		}

		[Test]
		public void Constructor2_Tag_Null ()
		{
			try {
				new SecurityElement (null, "text");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("tag", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Constructor2_Text_Null () 
		{
			SecurityElement se = new SecurityElement ("tag", null);
			Assert.IsNull (se.Attributes, "EmptyAttributes");
			Assert.IsNull (se.Children, "EmptyChildren");
			Assert.AreEqual ("tag", se.Tag, "Tag");
			Assert.IsNull (se.Text, "Text");
		}

		[Test]
		public void AddAttribute_Name_Null () 
		{
			try {
				elem.AddAttribute (null, "valid");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test]
		public void AddAttribute_Value_Null () 
		{
			try {
				elem.AddAttribute ("valid", null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddAttribute_Name_Invalid () 
		{
			elem.AddAttribute ("<invalid>", "valid");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddAttribute_Value_Invalid () 
		{
			elem.AddAttribute ("valid", "invalid\"");
		}

		[Test]
		public void AddAttribute_InvalidValue2 () 
		{
			elem.AddAttribute ("valid", "valid&");
			// in xml world this is actually not considered valid
			// but it is by MS.Net
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddAttribute_InvalidValue3 () 
		{
			elem.AddAttribute ("valid", "<invalid>");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddAttribute_Duplicate () 
		{
			elem.AddAttribute ("valid", "first time");
			elem.AddAttribute ("valid", "second time");
		}

		[Test]
		public void AddAttribute () 
		{
			elem.AddAttribute ("valid", "valid\'");
		}

		[Test]
		public void AddChild_Null () 
		{
			try {
				elem.AddChild (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("child", ex.ParamName, "#6");
			}
		}

		[Test]
		public void AddChild () 
		{
			int n = elem.Children.Count;
			// add itself
			elem.AddChild (elem);
			Assert.AreEqual ((n + 1), elem.Children.Count, "Count");
		}

		[Test]
		[Category ("NotDotNet")] // MS bug: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=304549
		public void Attributes_Name_Invalid () 
		{
			Hashtable h = elem.Attributes;
			h.Add ("<invalid>", "valid");
			try {
				elem.Attributes = h;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid attribute name '<invalid>'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("<invalid>") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
			}
		}

		[Test]
		[Category ("NotWorking")] // MS bug: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=304549
		public void Attributes_Name_Invalid_MS ()
		{
			Hashtable h = elem.Attributes;
			h.Add ("<invalid>", "valid");
			try {
				elem.Attributes = h;
				Assert.Fail ();
			} catch (InvalidCastException) {
			}
		}

		[Test]
		public void Attributes_Value_Invalid () 
		{
			Hashtable h = elem.Attributes;
			h.Add ("valid", "\"invalid\"");
			try {
				elem.Attributes = h;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid attribute value '"invalid"'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("\"invalid\"") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
			}
		}
		
		[Test]
		public void Attributes ()
		{
			Hashtable h = elem.Attributes;

			h = elem.Attributes;
			h.Add ("foo", "bar");
			Assert.IsTrue (elem.Attributes.Count != h.Count, "#1");
			
			elem.Attributes = h;
			Assert.IsNotNull (elem.Attribute ("foo"), "#2");
		}
		
		[Test]
		public void Equal ()
		{
			int iTest = 0;
			SecurityElement elem2 = CreateElement ();
			iTest++;
			Assert.IsTrue (elem.Equal (elem2), "#1");
			iTest++;
			SecurityElement child = (SecurityElement) elem2.Children [0];
			iTest++;
			child = (SecurityElement) child.Children [1];
			iTest++;
			child.Text = "some text";
			iTest++;
			Assert.IsFalse (elem.Equal (elem2), "#2");
		}
		
		[Test]
		public void Escape ()
		{
			Assert.AreEqual ("foo&lt;&gt;&quot;&apos;&amp; bar",
				SecurityElement.Escape ("foo<>\"'& bar"), "#1");
			Assert.IsNull (SecurityElement.Escape (null), "#2");
		}

		[Test]
		public void IsValidAttributeName ()
		{
			Assert.IsFalse (SecurityElement.IsValidAttributeName ("x x"), "#1");
			Assert.IsFalse (SecurityElement.IsValidAttributeName ("x<x"), "#2");
			Assert.IsFalse (SecurityElement.IsValidAttributeName ("x>x"), "#3");
			Assert.IsTrue (SecurityElement.IsValidAttributeName ("x\"x"), "#4");
			Assert.IsTrue (SecurityElement.IsValidAttributeName ("x'x"), "#5");
			Assert.IsTrue (SecurityElement.IsValidAttributeName ("x&x"), "#6");
			Assert.IsFalse (SecurityElement.IsValidAttributeName (null), "#7");
			Assert.IsTrue (SecurityElement.IsValidAttributeName (string.Empty), "#8");
		}

		[Test]
		public void IsValidAttributeValue ()
		{
			Assert.IsTrue (SecurityElement.IsValidAttributeValue ("x x"), "#1");
			Assert.IsFalse (SecurityElement.IsValidAttributeValue ("x<x"), "#2");
			Assert.IsFalse (SecurityElement.IsValidAttributeValue ("x>x"), "#3");
			Assert.IsFalse (SecurityElement.IsValidAttributeValue ("x\"x"), "#4");
			Assert.IsTrue (SecurityElement.IsValidAttributeValue ("x'x"), "#5");
			Assert.IsTrue (SecurityElement.IsValidAttributeValue ("x&x"), "#6");
			Assert.IsFalse (SecurityElement.IsValidAttributeValue (null), "#7");
			Assert.IsTrue (SecurityElement.IsValidAttributeValue (string.Empty), "#8");
		}

		[Test]
		public void IsValidTag ()
		{
			Assert.IsFalse (SecurityElement.IsValidTag ("x x"), "#1");
			Assert.IsFalse (SecurityElement.IsValidTag ("x<x"), "#2");
			Assert.IsFalse (SecurityElement.IsValidTag ("x>x"), "#3");
			Assert.IsTrue (SecurityElement.IsValidTag ("x\"x"), "#4");
			Assert.IsTrue (SecurityElement.IsValidTag ("x'x"), "#5");
			Assert.IsTrue (SecurityElement.IsValidTag ("x&x"), "#6");
			Assert.IsFalse (SecurityElement.IsValidTag (null), "#7");
			Assert.IsTrue (SecurityElement.IsValidTag (string.Empty), "#8");
		}

		[Test]
		public void IsValidText ()
		{
			Assert.IsTrue (SecurityElement.IsValidText ("x x"), "#1");
			Assert.IsFalse (SecurityElement.IsValidText ("x<x"), "#2");
			Assert.IsFalse (SecurityElement.IsValidText ("x>x"), "#3");
			Assert.IsTrue (SecurityElement.IsValidText ("x\"x"), "#4");
			Assert.IsTrue (SecurityElement.IsValidText ("x'x"), "#5");
			Assert.IsTrue (SecurityElement.IsValidText ("x&x"), "#6");
			Assert.IsFalse (SecurityElement.IsValidText (null), "#7");
			Assert.IsTrue (SecurityElement.IsValidText (string.Empty), "#8");
		}
		
		[Test]
		public void SearchForChildByTag_Null ()
		{
			try {
				elem.SearchForChildByTag (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("tag", ex.ParamName, "#6");
			}
		}

		[Test]
		public void SearchForChildByTag () 
		{
			SecurityElement child = elem.SearchForChildByTag ("doesnotexist");
			Assert.IsNull (child, "#1");
			
			child = elem.SearchForChildByTag ("ENDPOINT");
			Assert.IsNull (child, "#2");
			
			child = (SecurityElement) elem.Children [0];
			child = child.SearchForChildByTag ("ENDPOINT");
			Assert.AreEqual ("All", child.Attribute ("transport"), "#3");
		}
		
		[Test]
		public void SearchForTextOfTag_Tag_Null ()
		{
			try {
				elem.SearchForTextOfTag (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("tag", ex.ParamName, "#6");
			}
		}
			
		[Test]
		public void SearchForTextOfTag () 
		{
			string s = elem.SearchForTextOfTag ("ENDPOINT");
			Assert.AreEqual ("some text", s);
		}

		[Test]
		public void Tag ()
		{
			SecurityElement se = new SecurityElement ("Values");
			Assert.AreEqual ("Values", se.Tag, "#A1");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<Values/>{0}", Environment.NewLine), 
				se.ToString (), "#A2");
			se.Tag = "abc:Name";
			Assert.AreEqual ("abc:Name", se.Tag, "#B1");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<abc:Name/>{0}", Environment.NewLine),
				se.ToString (), "#B2");
			se.Tag = "Name&Address";
			Assert.AreEqual ("Name&Address", se.Tag, "#C1");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<Name&Address/>{0}", Environment.NewLine),
				se.ToString (), "#C2");
			se.Tag = string.Empty;
			Assert.AreEqual (string.Empty, se.Tag, "#D1");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"</>{0}", Environment.NewLine),
				se.ToString (), "#D2");
		}

		[Test]
		public void Tag_Invalid ()
		{
			SecurityElement se = new SecurityElement ("Values");

			try {
				se.Tag = "Na<me";
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid element tag Nam<e
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("Na<me") != -1, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
			}

			try {
				se.Tag = "Nam>e";
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid element tag Nam>e
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("Nam>e") != -1, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
			}
		}

		[Test]
		public void Tag_Null () 
		{
			try {
				elem.Tag = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("Tag", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Text ()
		{
			elem.Text = "Miguel&Sébastien";
			Assert.AreEqual ("Miguel&Sébastien", elem.Text, "#1");
			elem.Text = null;
			Assert.IsNull (elem.Text, "#2");
			elem.Text = "Sébastien\"Miguel";
			Assert.AreEqual ("Sébastien\"Miguel", elem.Text, "#3");
			elem.Text = string.Empty;
			Assert.AreEqual (string.Empty, elem.Text, "#4");
			elem.Text = "&lt;sample&amp;practice&unresolved;&gt;";
			Assert.AreEqual ("<sample&practice&unresolved;>", elem.Text, "#5");
		}

		[Test]
		public void Text_Invalid ()
		{
			try {
				elem.Text = "Mig<uelSébastien";
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid element tag Mig<uelSébastien
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("Mig<uelSébastien") != -1, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
			}

			try {
				elem.Text = "Mig>uelSébastien";
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid element tag Mig>uelSébastien
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("Mig>uelSébastien") != -1, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
			}
		}

		[Test]
		public void MultipleAttributes () 
		{
			SecurityElement se = new SecurityElement ("Multiple");
			se.AddAttribute ("Attribute1", "One");
			se.AddAttribute ("Attribute2", "Two");
#if NET_2_0
			string expected = String.Format ("<Multiple Attribute1=\"One\"{0}Attribute2=\"Two\"/>{0}", Environment.NewLine);
#else
			string expected = String.Format ("<Multiple Attribute1=\"One\"{0}          Attribute2=\"Two\"/>{0}", Environment.NewLine);
#endif
			Assert.AreEqual (expected, se.ToString (), "ToString()");
		}

#if NET_2_0
		[Test]
		public void Copy ()
		{
			SecurityElement se = SecurityElement.FromString ("<tag attribute=\"value\"><child attr=\"1\">mono</child><child/></tag>");
			SecurityElement copy = se.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (se, copy), "se!ReferenceEquals");
			Assert.IsTrue (Object.ReferenceEquals (se.Children [0], copy.Children [0]), "c1=ReferenceEquals");
			Assert.IsTrue (Object.ReferenceEquals (se.Children [1], copy.Children [1]), "c2=ReferenceEquals");
		}

		[Test]
		public void FromString_Null () 
		{
			try {
				SecurityElement.FromString (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("xml", ex.ParamName, "#6");
			}
		}

		[Test]
		[ExpectedException (typeof (XmlSyntaxException))]
		public void FromString_Empty ()
		{
			SecurityElement.FromString (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (XmlSyntaxException))]
		public void FromString_NonXml ()
		{
			SecurityElement.FromString ("mono");
		}

		[Test]
		public void FromString ()
		{
			SecurityElement se = SecurityElement.FromString ("<tag attribute=\"value\"><x:child attr=\"1\">mono</x:child><child/></tag>");
			Assert.AreEqual ("tag", se.Tag, "#A1");
			Assert.IsNull (se.Text, "#A2");
			Assert.AreEqual (1, se.Attributes.Count, "#A3");
			Assert.AreEqual ("value", se.Attribute ("attribute"), "#A4");
			Assert.AreEqual (2, se.Children.Count, "#A5");

			SecurityElement child = (SecurityElement) se.Children [0];
			Assert.AreEqual ("x:child", child.Tag, "#B1");
			Assert.AreEqual ("mono", child.Text, "#B2");
			Assert.AreEqual (1, child.Attributes.Count, "#B3");
			Assert.AreEqual ("1", child.Attribute ("attr"), "#B4");

			child = (SecurityElement) se.Children [1];
			Assert.AreEqual ("child", child.Tag, "#C1");
			Assert.IsNull (child.Text, "#C2");
			Assert.IsNull (child.Attributes, "#C3");
		}

		[Test]
		[Category ("NotDotNet")] // MS bug: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=304583
		public void FromString_Quote_Delimiter ()
		{
			const string xml = "<value name='Company'>Novell</value>";
			SecurityElement se = SecurityElement.FromString (xml);
			Assert.AreEqual ("Company", se.Attribute ("name"), "#1");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<value name=\"Company\">Novell</value>{0}",
				Environment.NewLine), se.ToString (), "#2");
		}

		[Test]
		[Category ("NotWorking")] // MS bug: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=304583
		public void FromString_Quote_Delimiter_MS ()
		{
			const string xml = "<value name='Company'>Novell</value>";
			SecurityElement se = SecurityElement.FromString (xml);
			Assert.AreEqual ("'Company'", se.Attribute ("name"), "#1");
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<value name=\"'Company'\">Novell</value>{0}",
				Environment.NewLine), se.ToString (), "#2");
		}

		[Test] // bug #333699
		public void FromString_EntityReferences ()
		{
			const string xml = @"
				<values>
					<value name=""&quot;name&quot;&amp;&lt;address&gt;"">&lt;&apos;Suds&apos; &amp; &quot;Soda&quot;&gt;!</value>
				</values>";

			SecurityElement se = SecurityElement.FromString (xml);
			Assert.IsNotNull (se, "#A1");
			Assert.IsNull (se.Attributes, "#A2");
			Assert.IsNotNull (se.Children, "#A3");
			Assert.AreEqual (1, se.Children.Count, "#A4");
			Assert.AreEqual ("values", se.Tag, "#A5");
			Assert.IsNull (se.Text, "#A6");

			SecurityElement child = se.Children [0] as SecurityElement;
			Assert.IsNotNull (child, "#B1");
			Assert.IsNotNull (child.Attributes, "#B2");
			Assert.AreEqual ("\"name\"&<address>", child.Attribute ("name"), "#B3");
			Assert.AreEqual ("value", child.Tag, "#B4");
			Assert.AreEqual ("<'Suds' & \"Soda\">!", child.Text, "#B5");
			Assert.IsNull (child.Children, "#B6");
		}

		[Test] // bug #	
		[Category ("NotWorking")]
		public void FromString_CharacterReferences ()
		{
			const string xml = @"
				<value name=""name&#38;address"">Suds&#x26;Soda&#38;</value>";

			SecurityElement se = SecurityElement.FromString (xml);
			Assert.IsNotNull (se, "#1");
			Assert.IsNotNull (se.Attributes, "#2");
			Assert.AreEqual ("name&#38;address", se.Attribute ("name"), "#3");
			Assert.AreEqual ("value", se.Tag, "#4");
			Assert.AreEqual ("Suds&#x26;Soda&#38;", se.Text, "#5");
			Assert.IsNull (se.Children, "#6");
		}
#endif

		[Test] // bug #333699 (ugh, mostly a dup)
		public void TestToString ()
		{
			SecurityElement values = new SecurityElement ("values");
			SecurityElement infoValue = new SecurityElement ("value");
			infoValue.AddAttribute ("name", "string");
			infoValue.Text = SecurityElement.Escape ("<'Suds' & \"Soda\">!");
			values.AddChild (infoValue);
			Assert.AreEqual ("<value name=\"string\">&lt;&apos;Suds&apos; &amp; &quot;Soda&quot;&gt;!</value>" + Environment.NewLine, infoValue.ToString (), "#1");
			Assert.AreEqual ("<'Suds' & \"Soda\">!", infoValue.Text, "#2");
			Assert.IsNull (values.Text, "#3");

#if NET_2_0
			Assert.AreEqual (String.Format ("<values>{0}<value name=\"string\">&lt;&apos;Suds&apos; &amp; &quot;Soda&quot;&gt;!</value>{0}</values>{0}", Environment.NewLine), values.ToString (), "#4");
#else
			Assert.AreEqual (String.Format ("<values>{0}   <value name=\"string\">&lt;&apos;Suds&apos; &amp; &quot;Soda&quot;&gt;!</value>{0}</values>{0}", Environment.NewLine), values.ToString (), "#4");
#endif

#if NET_2_0
			SecurityElement sec = SecurityElement.FromString (values.ToString ());
			Assert.AreEqual (1, sec.Children.Count, "#5");
			Assert.AreEqual ("<'Suds' & \"Soda\">!", ((SecurityElement) sec.Children [0]).Text, "#6");
#endif
		}
	}
}
