//
// CookieTest.cs - NUnit Test Cases for System.Net.Cookie
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;
using System.Net;

namespace MonoTests.System.Net
{

public class CookieTest : TestCase
{
        public CookieTest () :
                base ("[MonoTests.System.Net.CookieTest]") {}

        public CookieTest (string name) : base (name) {}

        protected override void SetUp () {}

        protected override void TearDown () {}

        public static ITest Suite
        {
                get {
                        return new TestSuite (typeof (CookieTest));
                }
        }

        public void TestPublicFields ()
        {
        }

        public void TestConstructors ()
        {
		Cookie c = new Cookie ("somename", null, null, null);
		try {
			c = new Cookie (null, null, null, null);
			Fail ("#1: Name cannot be null");
		} catch (CookieException) {
		}
        }
        
        public void TestName ()         
        {
		Cookie c = new Cookie ("SomeName", "SomeValue");
		AssertEquals ("#1", c.Name, "SomeName");
		try {
			c.Name = null;
			Fail ("#2a");
		} catch (CookieException) {
			AssertEquals ("#2b", "SomeName", c.Name);
		}
		try {
			c.Name = "";
			Fail ("#2c");
		} catch (CookieException) {
			AssertEquals ("#2d", "SomeName", c.Name);			
		}
		try {
			c.Name = " ";
			Fail ("#2e");			
		} catch (CookieException) {
			// bah! this fails, yet the name is changed.. 
			// inconsistent with previous test
			AssertEquals ("#2f", String.Empty, c.Name);			
		}
		try {
			c.Name = "xxx\r\n";
			Fail ("#2g");			
		} catch (CookieException ttt) {
			AssertEquals ("#2h", String.Empty, c.Name);			
		}		
		try {
			c.Name = "xxx" + (char) 0x80;
		} catch (CookieException) {
			Fail ("#2i");			
		}				
		try {
			c.Name = "$omeName";
			Fail ("#3a: Name cannot start with '$' character");
		} catch (CookieException) {
			AssertEquals ("#3b", String.Empty, c.Name);
		}
		c.Name = "SomeName$";
		AssertEquals ("#4", c.Name, "SomeName$");
		try {
			c.Name = "Some=Name";
			Fail ("#5a: Name cannot contain '=' character");
		} catch (CookieException) {
			AssertEquals ("#5b", String.Empty, c.Name);
		}		
		c.Name = "domain";
		AssertEquals ("#6", c.Name, "domain");
	}
	
	public void TestValue ()
	{
		// LAMESPEC: According to .Net specs the Value property should not accept 
		// the semicolon and comma characters, yet it does
		Cookie c = new Cookie("SomeName", "SomeValue");
		try {
			c.Value = "Some;Value";
			Fail ("#1: semicolon should not be accepted");
		} catch (CookieException) {
		}
		try {
			c.Value = "Some,Value";
			Fail ("#2: comma should not be accepted");
		} catch (CookieException) {
		}
		c.Value = "Some\tValue";
		AssertEquals ("#3", c.Value, "Some\tValue");
	}
	
	public void TestPort ()
	{
		Cookie c = new Cookie ("SomeName", "SomeValue");
		try {
			c.Port = "123";
			Fail ("#1: port must start and end with double quotes");
		} catch (CookieException) {			
		}
		try {
			c.Port = "\"123\"";
		} catch (CookieException) {			
			Fail ("#2");
		}
		try {
			c.Port = "\"123;124\"";
			Fail ("#3");
		} catch (CookieException) {					
		}
		try {
			c.Port = "\"123,123,124\"";
		} catch (CookieException) {			
			Fail ("#4");
		}
		try {
			c.Port = "\"123,124\"";
		} catch (CookieException) {			
			Fail ("#5");
		}
	}

        public void TestEquals ()
        {
		Cookie c1 = new Cookie ("NAME", "VALUE", "PATH", "DOMAIN");
		Cookie c2 = new Cookie ("name", "value", "path", "domain");
		Assert("#1", !c1.Equals (c2));
		c2.Value = "VALUE";
		c2.Path = "PATH";
		Assert("#2", c1.Equals (c2));
		c2.Version = 1;
		Assert("#3", !c1.Equals (c2));
        }
}

}

