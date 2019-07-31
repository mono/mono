//
// UrlTest.cs - NUnit Test Cases for Url
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class UrlTest {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Url_Null () 
		{
			Url u = new Url (null);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Url_Empty () 
		{
			Url u = new Url (String.Empty);
		}

		[Test]
		public void Url_NoProtocol () 
		{
			Url u = new Url ("index.html");
			Assert.AreEqual ("index.html", u.Value, "Value");
		}

		[Test]
		public void Url_WellKnownProtocol () 
		{
			Url u1 = new Url ("file://mono/index.html");
			Url u2 = new Url ("ftp://www.example.com");
			Url u3 = new Url ("http://www.example.com");
			Url u4 = new Url ("https://www.example.com");
			Assert.AreEqual ("file://mono/index.html", u1.Value, "file.Value");
			Assert.AreEqual ("ftp://www.example.com", u2.Value, "ftp.Value");
			Assert.AreEqual ("http://www.example.com", u3.Value, "http.Value");
			Assert.AreEqual ("https://www.example.com", u4.Value, "https.Value");
		}

		[Test]
		public void Url_UnknownProtocol () 
		{
			string url = "mono://www.example.com";
			Url u = new Url (url);
			// Fx 2.0 returns the original url, while 1.0/1.1 adds a '/' at it's end
			Assert.IsTrue (u.Value.StartsWith (url), "mono.Value");
		}

		[Test]
		public void Url_RelativePath () 
		{
			Url u = new Url ("http://www.example.com/path/../newpath/index.html");
			Assert.AreEqual ("http://www.example.com/path/../newpath/index.html", u.Value, "Value");
		}

		[Test]
		public void Url_GoMonoWebUrl () 
		{
			string url = "http://www.example.com";
			Url u = new Url (url);

			Assert.IsTrue (u.Value.StartsWith (url), "Value");
			// no spaces in XML, no ending '/' on url
			Assert.AreEqual ("<System.Security.Policy.Url version=\"1\">" + Environment.NewLine + "<Url>http://www.example.com</Url>" + Environment.NewLine + "</System.Security.Policy.Url>" + Environment.NewLine, u.ToString (), "ToString");
			Url u2 = (Url) u.Copy ();
			Assert.AreEqual (u.Value, u2.Value, "Copy.Value");
			Assert.AreEqual (u.GetHashCode (), u2.GetHashCode (), "Copy.GetHashCode");

			UrlIdentityPermission uip = (UrlIdentityPermission) u.CreateIdentityPermission (null);
			Assert.AreEqual (u.Value, uip.Url, "CreateIdentityPermission");

			Assert.IsTrue (u.Equals (u2), "Equals");
			Url u3 = new Url ("example.com");
			Assert.IsFalse (u.Equals (u3), "!Equals");
		}

		[Test]
		public void Url_InvalidSite () 
		{
			Url u = new Url ("http://www.go-mono.*");
			Assert.AreEqual ("http://www.go-mono.*", u.Value, "Value");
		}

		[Test]
		public void EqualsCaseSensitive () 
		{
			Url u1 = new Url ("http://www.example.com");
			Url u2 = new Url ("http://www.Example.com");
			Assert.IsTrue (u1.Equals (u2), "CaseSensitive");
		}

		[Test]
		public void EqualsPartial () 
		{
			Url u1 = new Url ("http://www.example.com/index.html");
			Url u2 = new Url ("http://www.example.com/*");
			Assert.IsFalse (u1.Equals (u2), "Partial:1-2");
			Assert.IsFalse (u2.Equals (u1), "Partial:2-1");
		}

		[Test]
		public void EqualsNull () 
		{
			Url u = new Url ("http://www.example.com");
			Assert.IsFalse (u.Equals (null), "EqualsNull");
		}

		[Test]
		public void Url_LoneStar () 
		{
			Url u = new Url ("*");
			Assert.AreEqual ("*", u.Value, "Value");
			Assert.AreEqual ("<System.Security.Policy.Url version=\"1\">" + Environment.NewLine + "<Url>*</Url>" + Environment.NewLine + "</System.Security.Policy.Url>" + Environment.NewLine, u.ToString (), "ToString");
			Url u2 = (Url) u.Copy ();
			Assert.AreEqual (u.Value, u2.Value, "Copy.Value");
			Assert.AreEqual (u.GetHashCode (), u2.GetHashCode (), "Copy.GetHashCode");

			UrlIdentityPermission uip = (UrlIdentityPermission) u.CreateIdentityPermission (null);
			Assert.AreEqual (u.Value, uip.Url, "CreateIdentityPermission");

			Assert.IsTrue (u.Equals (u2), "Equals");
			Url u3 = new Url ("index.html");
			Assert.IsFalse (u.Equals (u3), "!Equals(*)");

			u2 = new Url ("file://*");
			Assert.AreEqual ("file://*", u2.Value, "Value-file://*");
			Assert.IsTrue (u.Equals (u2), "Equals-file://*");
		}
	}
}
