//
// SecurityElementTest.cs - NUnit Test Cases for System.Security.SecurityElement
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Security;

namespace MonoTests.System.Security
{

public class SecurityElementTest : TestCase
{
	SecurityElement elem;
	
        protected override void SetUp () 
        {
		elem = CreateElement ();
	}

        protected override void TearDown () {}
        
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

        public void TestConstructors ()
        {
		
	}
	
	public void TestAddAttribute ()
	{
		try {
			elem.AddAttribute (null, "valid");
			Fail ("#1");
		} catch (ArgumentNullException) { }
		try {
			elem.AddAttribute ("valid", null);
			Fail ("#2");
		} catch (ArgumentNullException) { }
		try {
			elem.AddAttribute ("<invalid>", "valid");
			Fail ("#3");
		} catch (ArgumentException) { }
		try {
			elem.AddAttribute ("valid", "invalid\"");
			Fail ("#4");
		} catch (ArgumentException) { }
		try {
			elem.AddAttribute ("valid", "valid\'");			
		} catch (ArgumentException) { Fail ("#5"); }
		try {			
			elem.AddAttribute ("valid", "valid&");
			Fail ("#6");
			// in xml world this is actually not considered valid
			// but it is by MS.Net
		} catch (ArgumentException) { }
		try {
			elem.AddAttribute ("valid", "<invalid>");
			Fail ("#7"); 
		} catch (ArgumentException) { }
	}
	
	public void TestAttributes ()
	{
		Hashtable h = elem.Attributes;
		
		/*
		// this will result in an InvalidCastException on MS.Net
		// I have no clue why
		
		h.Add ("<invalid>", "valid");
		try {
			elem.Attributes = h;
			Fail ("#1");
		} catch (ArgumentException) { }
                */
                
		h = elem.Attributes;
		h.Add ("valid", "\"invalid\"");
		try {
			elem.Attributes = h;
			Fail ("#2");
		} catch (ArgumentException) { }
		
		h = elem.Attributes;
		h.Add ("foo", "bar");
		Assert ("#3", elem.Attributes.Count != h.Count);
		
		elem.Attributes = h;
		Assert ("#4", elem.Attribute ("foo") != null);
	}
	
	public void TestEqual ()
	{
		int iTest = 0;
		try {
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
		} catch (Exception e) {
			Fail ("Unexpected Exception at iTest = " + iTest + ". e = " + e);
		}
	}
	
	public void TestEscape ()
	{
		AssertEquals ("#1", SecurityElement.Escape ("foo<>\"'& bar"), "foo&lt;&gt;&quot;&apos;&amp; bar");
	}
	
	public void TestIsValidAttributeName ()
	{
		Assert ("#1", !SecurityElement.IsValidAttributeName ("x x")); 
		Assert ("#2", !SecurityElement.IsValidAttributeName ("x<x")); 
		Assert ("#3", !SecurityElement.IsValidAttributeName ("x>x"));
		Assert ("#4", SecurityElement.IsValidAttributeName ("x\"x"));
		Assert ("#5", SecurityElement.IsValidAttributeName ("x'x"));
		Assert ("#6", SecurityElement.IsValidAttributeName ("x&x"));			
	}

	public void TestIsValidAttributeValue ()
	{
		Assert ("#1", SecurityElement.IsValidAttributeValue ("x x")); 
		Assert ("#2", !SecurityElement.IsValidAttributeValue ("x<x")); 
		Assert ("#3", !SecurityElement.IsValidAttributeValue ("x>x"));
		Assert ("#4", !SecurityElement.IsValidAttributeValue ("x\"x"));
		Assert ("#5", SecurityElement.IsValidAttributeValue ("x'x"));
		Assert ("#6", SecurityElement.IsValidAttributeValue ("x&x"));		
	}

	public void TestIsValidTag ()
	{
		Assert ("#1", !SecurityElement.IsValidTag ("x x")); 
		Assert ("#2", !SecurityElement.IsValidTag ("x<x")); 
		Assert ("#3", !SecurityElement.IsValidTag ("x>x"));
		Assert ("#4", SecurityElement.IsValidTag ("x\"x"));
		Assert ("#5", SecurityElement.IsValidTag ("x'x"));
		Assert ("#6", SecurityElement.IsValidTag ("x&x"));
	}

	public void TestIsValidText ()
	{
		Assert ("#1", SecurityElement.IsValidText ("x x")); 
		Assert ("#2", !SecurityElement.IsValidText ("x<x")); 
		Assert ("#3", !SecurityElement.IsValidText ("x>x"));
		Assert ("#4", SecurityElement.IsValidText ("x\"x"));
		Assert ("#5", SecurityElement.IsValidText ("x'x"));
		Assert ("#6", SecurityElement.IsValidText ("x&x"));
	}
	
	public void TestSearchForChildByTag ()
	{
		SecurityElement child = null;
		try {
			child = elem.SearchForChildByTag (null);
			Fail ("#1 should have thrown an ArgumentNullException");
		} catch (ArgumentNullException) { }

		child = elem.SearchForChildByTag ("doesnotexist");
		AssertEquals ("#2", child, null);
		
		child = elem.SearchForChildByTag ("ENDPOINT");
		AssertEquals ("#3", child, null);
		
		child = (SecurityElement) elem.Children [0];
		child = child.SearchForChildByTag ("ENDPOINT");
		AssertEquals ("#4", child.Attribute ("transport"), "All");
	}
	
	public void TestSearchForTextOfTag ()
	{
		try {
			string t1 = elem.SearchForTextOfTag (null);
			Fail ("#1 should have thrown an ArgumentNullException");
		} catch (ArgumentNullException) { }
		
		string t2 = elem.SearchForTextOfTag ("ENDPOINT");
		AssertEquals ("#2", t2, "some text");
	}
}

}

