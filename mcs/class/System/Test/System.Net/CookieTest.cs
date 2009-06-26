//
// CookieTest.cs - NUnit Test Cases for System.Net.Cookie
//
// Authors:
//	Lawrence Pit (loz@cable.a2000.nl)
//	Martin Willemoes Hansen (mwh@sysrq.dk)
//	Daniel Nauck    (dna(at)mono-project(dot)de)
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
			Cookie c = new Cookie ();
			Assert.AreEqual (string.Empty, c.Name, "#A1");
			Assert.AreEqual (string.Empty, c.Value, "#A2");
			Assert.AreEqual (string.Empty, c.Domain, "#A3");
			Assert.AreEqual (string.Empty, c.Port, "#A4");
			Assert.AreEqual (string.Empty, c.Comment, "#A5");
			Assert.AreEqual (null, c.CommentUri, "#A6");
			Assert.IsFalse (c.Discard, "#A7");
			Assert.IsFalse (c.Expired, "#A8");
			Assert.AreEqual (DateTime.MinValue, c.Expires, "#A9");
#if NET_2_0
			Assert.IsFalse (c.HttpOnly, "#A10");
#endif
			Assert.AreEqual (string.Empty, c.Path, "#A11");
			Assert.IsFalse (c.Secure, "#A12");
			Assert.AreEqual (0, c.Version, "#A13");
			Assert.AreEqual (string.Empty, c.ToString (), "#A14");

			c.Expires = DateTime.Now;
			Assert.IsTrue (c.Expired, "#A15");

			c.Port = null;
			Assert.AreEqual (string.Empty, c.Port, "#A16");

			c.Value = null;
			Assert.AreEqual (string.Empty, c.Value, "#A17");
		}

		[Test]
		public void Constructors ()
		{
			Cookie c = new Cookie ("somename", null, null, null);
			try
			{
				c = new Cookie (null, null, null, null);
				Assert.Fail ("#1: Name cannot be null");
			}
			catch (CookieException)
			{
			}
		}

		[Test]
		public void Name ()
		{
			Cookie c = new Cookie ("SomeName", "SomeValue");
			Assert.AreEqual (c.Name, "SomeName", "#1");
			try
			{
				c.Name = null;
				Assert.Fail ("#2a");
			}
			catch (CookieException)
			{
				Assert.AreEqual ("SomeName", c.Name, "#2b");
			}
			try
			{
				c.Name = "";
				Assert.Fail ("#2c");
			}
			catch (CookieException)
			{
				Assert.AreEqual ("SomeName", c.Name, "#2d");
			}
			try
			{
				c.Name = " ";
				Assert.Fail ("#2e");
			}
			catch (CookieException)
			{
				// bah! this fails, yet the name is changed.. 
				// inconsistent with previous test
				Assert.AreEqual (String.Empty, c.Name, "#2f");
			}
			try
			{
				c.Name = "xxx\r\n";
				Assert.Fail ("#2g");
			}
			catch (CookieException)
			{
				Assert.AreEqual (String.Empty, c.Name, "#2h");
			}
			try
			{
				c.Name = "xxx" + (char)0x80;
			}
			catch (CookieException)
			{
				Assert.Fail ("#2i");
			}
			try
			{
				c.Name = "$omeName";
				Assert.Fail ("#3a: Name cannot start with '$' character");
			}
			catch (CookieException)
			{
				Assert.AreEqual (String.Empty, c.Name, "#3b");
			}
			c.Name = "SomeName$";
			Assert.AreEqual (c.Name, "SomeName$", "#4");
			try
			{
				c.Name = "Some=Name";
				Assert.Fail ("#5a: Name cannot contain '=' character");
			}
			catch (CookieException)
			{
				Assert.AreEqual (String.Empty, c.Name, "#5b");
			}
			c.Name = "domain";
			Assert.AreEqual (c.Name, "domain", "#6");
		}

		[Test]
		public void Path ()
		{
			Cookie c = new Cookie ();
			c.Path = "/Whatever";
			Assert.AreEqual ("/Whatever", c.Path, "#1");
			c.Path = null;
			Assert.AreEqual (string.Empty, c.Path, "#2");
			c.Path = "ok";
			Assert.AreEqual ("ok", c.Path, "#3");
			c.Path = string.Empty;
			Assert.AreEqual (string.Empty, c.Path, "#4");
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
				Assert.Fail ("#1: semicolon should not be accepted");
			} catch (CookieException) {
			}
			try {
				c.Value = "Some,Value";
				Assert.Fail ("#2: comma should not be accepted");
			} catch (CookieException) {
			}
			c.Value = "Some\tValue";
			Assert.AreEqual (c.Value, "Some\tValue", "#3");
			*/
		}

		[Test]
		public void Port ()
		{
			Cookie c = new Cookie ("SomeName", "SomeValue");
			try
			{
				c.Port = "123";
				Assert.Fail ("#1: port must start and end with double quotes");
			}
			catch (CookieException)
			{
			}
			try
			{
				Assert.AreEqual (0, c.Version, "#6.1");
				c.Port = "\"123\"";
				Assert.AreEqual (1, c.Version, "#6.2");
			}
			catch (CookieException)
			{
				Assert.Fail ("#2");
			}
			try
			{
				c.Port = "\"123;124\"";
				Assert.Fail ("#3");
			}
			catch (CookieException)
			{
			}
			try
			{
				c.Port = "\"123,123,124\"";
			}
			catch (CookieException)
			{
				Assert.Fail ("#4");
			}
			try
			{
				c.Port = "\"123,124\"";
			}
			catch (CookieException)
			{
				Assert.Fail ("#5");
			}
		}

		[Test]
		public void Equals ()
		{
			Cookie c1 = new Cookie ("NAME", "VALUE", "PATH", "DOMAIN");
			Cookie c2 = new Cookie ("name", "value", "path", "domain");
			Assert.IsTrue (!c1.Equals (c2), "#1");
			c2.Value = "VALUE";
			c2.Path = "PATH";
			Assert.IsTrue (c1.Equals (c2), "#2");
			c2.Version = 1;
			Assert.IsTrue (!c1.Equals (c2), "#3");
		}

		[Test]
		public void ToStringTest ()
		{
			Cookie c1 = new Cookie ("NAME", "VALUE", "/", "example.com");
			Assert.AreEqual ("NAME=VALUE", c1.ToString (), "#A1");

			Cookie c2 = new Cookie ();
			Assert.AreEqual (string.Empty, c2.ToString (), "#A2");

			Cookie c3 = new Cookie("NAME", "VALUE");
			Assert.AreEqual ("NAME=VALUE", c3.ToString (), "#A3");

			Cookie c4 = new Cookie ("NAME", "VALUE", "/", "example.com");
			c4.Version = 1;
			Assert.AreEqual ("$Version=1; NAME=VALUE; $Path=/; $Domain=example.com", c4.ToString (), "#A4");

			Cookie c5 = new Cookie ("NAME", "VALUE", "/", "example.com");
			c5.Port = "\"8080\"";
			Assert.AreEqual ("$Version=1; NAME=VALUE; $Path=/; $Domain=example.com; $Port=\"8080\"", c5.ToString (), "#A5");

			Cookie c6 = new Cookie ("NAME", "VALUE");
			c6.Version = 1;
			Assert.AreEqual ("$Version=1; NAME=VALUE", c6.ToString (), "#A6");
		}
	}
}