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

using NUnit.Framework;
using System;
using System.Collections;
using System.Security;

namespace MonoTests.System.Security {

	[TestFixture]
	public class SecurityElementTest : Assertion {

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
		public void ConstructorsTagTest () 
		{
			SecurityElement se = new SecurityElement ("tag", "text");
			AssertNull ("EmptyAttributes", se.Attributes);
			AssertNull ("EmptyChildren", se.Children);
			AssertEquals ("Tag", "tag", se.Tag);
			AssertEquals ("Text", "text", se.Text);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorsTagNullText ()
		{
			SecurityElement se = new SecurityElement (null, "text");
		}

		[Test]
		public void ConstructorsTagTextNull () 
		{
			SecurityElement se = new SecurityElement ("tag", null);
			AssertNull ("EmptyAttributes", se.Attributes);
			AssertNull ("EmptyChildren", se.Children);
			AssertEquals ("Tag", "tag", se.Tag);
			AssertNull ("Text", se.Text);
		}

		[Test]
		public void ConstructorsTag () 
		{
			SecurityElement se = new SecurityElement ("tag");
			AssertNull ("EmptyAttributes", se.Attributes);
			AssertNull ("EmptyChildren", se.Children);
			AssertEquals ("Tag", "tag", se.Tag);
			AssertNull ("Text", se.Text);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorsTagNull () 
		{
			SecurityElement se = new SecurityElement (null);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddAttribute_NameNullValue () 
		{
			elem.AddAttribute (null, "valid");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddAttribute_NameValueNull () 
		{
			elem.AddAttribute ("valid", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddAttribute_InvalidName () 
		{
			elem.AddAttribute ("<invalid>", "valid");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddAttribute_InvalidValue () 
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddChild_Null () 
		{
			elem.AddChild (null);
		}

		[Test]
		public void AddChild () 
		{
			int n = elem.Children.Count;
			// add itself
			elem.AddChild (elem);
			AssertEquals ("Count", (n+1), elem.Children.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotDotNet")] // this will result in an InvalidCastException on MS.Net - I have no clue why
		public void Attributes_StrangeCase () 
		{
			Hashtable h = elem.Attributes;
			h.Add ("<invalid>", "valid");
			elem.Attributes = h;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Attributes_ArgumentException () 
		{
			Hashtable h = elem.Attributes;
			h.Add ("valid", "\"invalid\"");
			elem.Attributes = h;
		}
		
		[Test]
		public void Attributes ()
		{
			Hashtable h = elem.Attributes;

			h = elem.Attributes;
			h.Add ("foo", "bar");
			Assert ("#1", elem.Attributes.Count != h.Count);
			
			elem.Attributes = h;
			AssertNotNull ("#2", elem.Attribute ("foo"));
		}
		
		[Test]
		public void Equal ()
		{
			int iTest = 0;
			SecurityElement elem2 = CreateElement ();
			iTest++;
			Assert ("#1", elem.Equal (elem2));
			iTest++;
			SecurityElement child = (SecurityElement) elem2.Children [0];
			iTest++;
			child = (SecurityElement) child.Children [1];
			iTest++;
			child.Text = "some text";
			iTest++;
			Assert ("#2", !elem.Equal (elem2));
		}
		
		[Test]
		public void Escape ()
		{
			AssertEquals ("#1", "foo&lt;&gt;&quot;&apos;&amp; bar", SecurityElement.Escape ("foo<>\"'& bar"));
		}
		
		[Test]
		public void IsValidAttributeName ()
		{
			Assert ("#1", !SecurityElement.IsValidAttributeName ("x x")); 
			Assert ("#2", !SecurityElement.IsValidAttributeName ("x<x")); 
			Assert ("#3", !SecurityElement.IsValidAttributeName ("x>x"));
			Assert ("#4", SecurityElement.IsValidAttributeName ("x\"x"));
			Assert ("#5", SecurityElement.IsValidAttributeName ("x'x"));
			Assert ("#6", SecurityElement.IsValidAttributeName ("x&x"));			
		}

		[Test]
		public void IsValidAttributeValue ()
		{
			Assert ("#1", SecurityElement.IsValidAttributeValue ("x x")); 
			Assert ("#2", !SecurityElement.IsValidAttributeValue ("x<x")); 
			Assert ("#3", !SecurityElement.IsValidAttributeValue ("x>x"));
			Assert ("#4", !SecurityElement.IsValidAttributeValue ("x\"x"));
			Assert ("#5", SecurityElement.IsValidAttributeValue ("x'x"));
			Assert ("#6", SecurityElement.IsValidAttributeValue ("x&x"));		
		}

		[Test]
		public void IsValidTag ()
		{
			Assert ("#1", !SecurityElement.IsValidTag ("x x")); 
			Assert ("#2", !SecurityElement.IsValidTag ("x<x")); 
			Assert ("#3", !SecurityElement.IsValidTag ("x>x"));
			Assert ("#4", SecurityElement.IsValidTag ("x\"x"));
			Assert ("#5", SecurityElement.IsValidTag ("x'x"));
			Assert ("#6", SecurityElement.IsValidTag ("x&x"));
		}

		[Test]
		public void IsValidText ()
		{
			Assert ("#1", SecurityElement.IsValidText ("x x")); 
			Assert ("#2", !SecurityElement.IsValidText ("x<x")); 
			Assert ("#3", !SecurityElement.IsValidText ("x>x"));
			Assert ("#4", SecurityElement.IsValidText ("x\"x"));
			Assert ("#5", SecurityElement.IsValidText ("x'x"));
			Assert ("#6", SecurityElement.IsValidText ("x&x"));
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SearchForChildByTag_Null ()
		{
			SecurityElement child = elem.SearchForChildByTag (null);
		}

		[Test]
		public void SearchForChildByTag () 
		{
			SecurityElement	child = elem.SearchForChildByTag ("doesnotexist");
			AssertNull ("#1", child);
			
			child = elem.SearchForChildByTag ("ENDPOINT");
			AssertNull ("#2", child);
			
			child = (SecurityElement) elem.Children [0];
			child = child.SearchForChildByTag ("ENDPOINT");
			AssertEquals ("#3", "All", child.Attribute ("transport"));
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SearchForTextOfTag_Null ()
		{
			string s = elem.SearchForTextOfTag (null);
		}
			
		[Test]
		public void SearchForTextOfTag () 
		{
			string s = elem.SearchForTextOfTag ("ENDPOINT");
			AssertEquals ("SearchForTextOfTag", "some text", s);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Tag_Null () 
		{
			elem.Tag = null;
			AssertNull ("Tag", elem.Tag);
		}

		[Test]
		public void Text_Null () 
		{
			elem.Text = null;
			AssertNull ("Text", elem.Text);
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
			AssertEquals ("ToString()", expected, se.ToString ());
		}

#if NET_2_0
		[Test]
		public void Copy ()
		{
			SecurityElement se = SecurityElement.FromString ("<tag attribute=\"value\"><child attr=\"1\">mono</child><child/></tag>");
			SecurityElement copy = se.Copy ();
			Assert ("se!ReferenceEquals", !Object.ReferenceEquals (se, copy));
			Assert ("c1=ReferenceEquals", Object.ReferenceEquals (se.Children [0], copy.Children [0]));
			Assert ("c2=ReferenceEquals", Object.ReferenceEquals (se.Children [1], copy.Children [1]));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromString_Null () 
		{
			SecurityElement.FromString (null);
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
			SecurityElement se = SecurityElement.FromString ("<tag attribute=\"value\"><child attr=\"1\">mono</child><child/></tag>");
			AssertEquals ("Tag", "tag", se.Tag);
			AssertNull ("Text", se.Text);
			AssertEquals ("Attribute.Count", 1, se.Attributes.Count);
			AssertEquals ("Attribute", "value", se.Attribute ("attribute"));
			AssertEquals ("Children.Count", 2, se.Children.Count);

			SecurityElement child = (SecurityElement) se.Children [0];
			AssertEquals ("Child1.Tag", "child", child.Tag);
			AssertEquals ("Child1.Text", "mono", child.Text);
			AssertEquals ("Child1.Attribute.Count", 1, child.Attributes.Count);
			AssertEquals ("Child1.Attribute", "1", child.Attribute ("attr"));

			child = (SecurityElement) se.Children [1];
			AssertEquals ("Child2.Tag", "child", child.Tag);
			AssertNull ("Child2.Text", child.Text);
			AssertNull ("Child2.Attribute", child.Attributes);
		}
#endif
	}
}
