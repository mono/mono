//
// CookieTest.cs - NUnit Test Cases for System.Net.Cookie
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.Net;

namespace MonoTests.System.Net
{

[TestFixture]
public class CookieTest
{
	[Test]
        public void PublicFields ()
        {
        }

	[Test]
        public void Constructors ()
        {
		Cookie c = new Cookie ("somename", null, null, null);
		try {
			c = new Cookie (null, null, null, null);
			Assertion.Fail ("#1: Name cannot be null");
		} catch (CookieException) {
		}
        }
        
	[Test]
        public void Name ()         
        {
		Cookie c = new Cookie ("SomeName", "SomeValue");
		Assertion.AssertEquals ("#1", c.Name, "SomeName");
		try {
			c.Name = null;
			Assertion.Fail ("#2a");
		} catch (CookieException) {
			Assertion.AssertEquals ("#2b", "SomeName", c.Name);
		}
		try {
			c.Name = "";
			Assertion.Fail ("#2c");
		} catch (CookieException) {
			Assertion.AssertEquals ("#2d", "SomeName", c.Name);			
		}
		try {
			c.Name = " ";
			Assertion.Fail ("#2e");			
		} catch (CookieException) {
			// bah! this fails, yet the name is changed.. 
			// inconsistent with previous test
			Assertion.AssertEquals ("#2f", String.Empty, c.Name);			
		}
		try {
			c.Name = "xxx\r\n";
			Assertion.Fail ("#2g");			
		} catch (CookieException) {
			Assertion.AssertEquals ("#2h", String.Empty, c.Name);			
		}		
		try {
			c.Name = "xxx" + (char) 0x80;
		} catch (CookieException) {
			Assertion.Fail ("#2i");			
		}				
		try {
			c.Name = "$omeName";
			Assertion.Fail ("#3a: Name cannot start with '$' character");
		} catch (CookieException) {
			Assertion.AssertEquals ("#3b", String.Empty, c.Name);
		}
		c.Name = "SomeName$";
		Assertion.AssertEquals ("#4", c.Name, "SomeName$");
		try {
			c.Name = "Some=Name";
			Assertion.Fail ("#5a: Name cannot contain '=' character");
		} catch (CookieException) {
			Assertion.AssertEquals ("#5b", String.Empty, c.Name);
		}		
		c.Name = "domain";
		Assertion.AssertEquals ("#6", c.Name, "domain");
	}
	
	[Test]
	public void Value ()
	{
		// LAMESPEC: According to .Net specs the Value property should not accept 
		// the semicolon and comma characters, yet it does
		/*
		Cookie c = new Cookie("SomeName", "SomeValue");
		try {
			c.Value = "Some;Value";
			Assertion.Fail ("#1: semicolon should not be accepted");
		} catch (CookieException) {
		}
		try {
			c.Value = "Some,Value";
			Assertion.Fail ("#2: comma should not be accepted");
		} catch (CookieException) {
		}
		c.Value = "Some\tValue";
		Assertion.AssertEquals ("#3", c.Value, "Some\tValue");
		*/
	}
	
	[Test]
	public void Port ()
	{
		Cookie c = new Cookie ("SomeName", "SomeValue");
		try {
			c.Port = "123";
			Assertion.Fail ("#1: port must start and end with double quotes");
		} catch (CookieException) {			
		}
		try {
			c.Port = "\"123\"";
		} catch (CookieException) {			
			Assertion.Fail ("#2");
		}
		try {
			c.Port = "\"123;124\"";
			Assertion.Fail ("#3");
		} catch (CookieException) {					
		}
		try {
			c.Port = "\"123,123,124\"";
		} catch (CookieException) {			
			Assertion.Fail ("#4");
		}
		try {
			c.Port = "\"123,124\"";
		} catch (CookieException) {			
			Assertion.Fail ("#5");
		}
	}

	[Test]
        public void Equals ()
        {
		Cookie c1 = new Cookie ("NAME", "VALUE", "PATH", "DOMAIN");
		Cookie c2 = new Cookie ("name", "value", "path", "domain");
		Assertion.Assert("#1", !c1.Equals (c2));
		c2.Value = "VALUE";
		c2.Path = "PATH";
		Assertion.Assert("#2", c1.Equals (c2));
		c2.Version = 1;
		Assertion.Assert("#3", !c1.Equals (c2));
        }
}

}

