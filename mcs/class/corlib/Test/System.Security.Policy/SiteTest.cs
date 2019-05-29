//
// SiteTest.cs - NUnit Test Cases for Site
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class SiteTest  {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Site_Null () 
		{
			Site s = new Site (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_Empty () 
		{
			Site s = new Site (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_FileUrl () 
		{
			Site s = new Site ("file://mono/index.html");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_AllGoMono () 
		{
			Site s = new Site ("http://*.example.com");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_FullUrlWithPort () 
		{
			Site s = new Site ("http://www.example.com:8080/index.html");
		}

		[Test]
		public void Site_GoMonoWebSite () 
		{
			Site s = new Site ("www.example.com");
			Assert.AreEqual ("www.example.com", s.Name, "Name");
			Assert.AreEqual ("<System.Security.Policy.Site version=\"1\">" + Environment.NewLine + "<Name>www.example.com</Name>" + Environment.NewLine + "</System.Security.Policy.Site>" + Environment.NewLine, s.ToString (), "ToString");
			Site s2 = (Site) s.Copy ();
			Assert.AreEqual (s.Name, s2.Name, "Copy.Name");
			Assert.AreEqual (s.GetHashCode (), s2.GetHashCode (), "Copy.GetHashCode");

			SiteIdentityPermission sip = (SiteIdentityPermission) s.CreateIdentityPermission (null);
			Assert.AreEqual (s.Name, sip.Site, "CreateIdentityPermission");

			Assert.IsTrue (s.Equals (s2), "Equals");
			Site s3 = new Site ("example.com");
			Assert.IsTrue (!s.Equals (s3), "!Equals");
		}

		[Test]
		public void Site_AllGoMonoSite () 
		{
			Site s = new Site ("*.example.com");
			Assert.AreEqual ("*.example.com", s.Name, "Name");
			Assert.AreEqual ("<System.Security.Policy.Site version=\"1\">" + Environment.NewLine + "<Name>*.example.com</Name>" + Environment.NewLine + "</System.Security.Policy.Site>" + Environment.NewLine, s.ToString (), "ToString");
			Site s2 = (Site) s.Copy ();
			Assert.AreEqual (s.Name, s2.Name, "Copy.Name");
			Assert.AreEqual (s.GetHashCode (), s2.GetHashCode (), "Copy.GetHashCode");

			SiteIdentityPermission sip = (SiteIdentityPermission) s.CreateIdentityPermission (null);
			Assert.AreEqual (s.Name, sip.Site, "CreateIdentityPermission");

			Assert.IsTrue (s.Equals (s2), "Equals");
			Site s3 = new Site ("example.com");
			Assert.IsTrue (!s.Equals (s3), "!Equals");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_GoMonoAllTLD () 
		{
			Site s = new Site ("www.go-mono.*");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_TwoStars () 
		{
			Site s = new Site ("*.*.example.com");
		}

		[Test]
		public void EqualsCaseSensitive () {
			Site s1 = new Site ("*.example.com");
			Site s2 = new Site ("*.Example.com");
			Assert.IsTrue (s1.Equals (s2), "CaseSensitive");
		}

		[Test]
		public void EqualsPartial () 
		{
			Site s1 = new Site ("www.example.com");
			Site s2 = new Site ("*.example.com");
			Assert.IsTrue (!s1.Equals (s2), "Partial:1-2");
			Assert.IsTrue (!s2.Equals (s1), "Partial:2-1");
		}

		[Test]
		public void EqualsNull () 
		{
			Site s1 = new Site ("*.example.com");
			Assert.IsTrue (!s1.Equals (null), "EqualsNull");
		}

		[Test]
		public void Site_LoneStar () 
		{
			Site s = new Site ("*");
			Assert.AreEqual ("*", s.Name, "Name");
			Assert.AreEqual ("<System.Security.Policy.Site version=\"1\">" + Environment.NewLine + "<Name>*</Name>" + Environment.NewLine + "</System.Security.Policy.Site>" + Environment.NewLine, s.ToString (), "ToString");
			Site s2 = (Site) s.Copy ();
			Assert.AreEqual (s.Name, s2.Name, "Copy.Name");
			Assert.AreEqual (s.GetHashCode (), s2.GetHashCode (), "Copy.GetHashCode");

			SiteIdentityPermission sip = (SiteIdentityPermission) s.CreateIdentityPermission (null);
			Assert.AreEqual (s.Name, sip.Site, "CreateIdentityPermission");

			Assert.IsTrue (s.Equals (s2), "Equals");
			Site s3 = new Site ("example.com");
			Assert.IsTrue (!s.Equals (s3), "!Equals");
		}

		[Test]
		public void AllChars () 
		{
			for (int i=1; i < 256; i++) {
				bool actual = false;
				char c = Convert.ToChar (i);
				try {
					Site s = new Site (Convert.ToString (c));
					actual = true;
					// Console.WriteLine ("GOOD: {0} - {1}", i, c);
				}
				catch {
					// Console.WriteLine ("FAIL: {0} - {1}", i, c);
				}
				bool result = ((i == 45)		// -
					|| (i == 33)			// !
					|| (i >= 35 && i <= 42)		// #$%&'()*
					|| (i >= 48 && i <= 57)		// 0-9
					|| (i >= 94 && i <= 95)		// ^_
					|| (i >= 97 && i <= 123)	// a-z{
					|| (i >= 125 && i <= 126)	// }~
					|| (i >= 64 && i <= 90));	// @,A-Z
				Assert.IsTrue ((actual == result), "#"+i);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateFromUrl_Null ()
		{
			Site.CreateFromUrl (null);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void CreateFromUrl_Empty ()
		{
			Site.CreateFromUrl (String.Empty);
		}

		string[] valid_urls = {
			"http://www.example.com",
			"http://*.example.com",
			"http://www.example.com:8080/index.html",
		};

		[Test]
		public void CreateFromUrl_Valid () 
		{
			foreach (string url in valid_urls) {
				Site s = Site.CreateFromUrl (url);
				Assert.IsTrue ((s.Name.ToUpper (CultureInfo.InvariantCulture).IndexOf ("EXAMPLE") != -1), s.Name);
			}
		}

		string[] invalid_urls = {
			"file://mono/index.html",	// file:// isn't supported as a site (2.0)
		};

		[Test]
		public void CreateFromUrl_Invalid ()
		{
			string msg = null;
			foreach (string url in invalid_urls) {
				try {
					Site.CreateFromUrl (url);
					msg = String.Format ("Expected ArgumentException for {0} but got none", url);
				}
				catch (ArgumentException) {
				}
				catch (Exception e) {
					msg = String.Format ("Expected ArgumentException for {0} but got: {1}", url, e);
				}
				finally {
					if (msg != null) {
						Assert.Fail (msg);
						msg = null;
					}
				}
			}
		}
	}
}
