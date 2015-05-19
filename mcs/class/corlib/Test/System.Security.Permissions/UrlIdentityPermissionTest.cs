//
// UrlIdentityPermissionTest.cs - NUnit Test Cases for UrlIdentityPermission
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

using System.Diagnostics;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class UrlIdentityPermissionTest {

		static string[] GoodUrls = {
			"http://www.mono-project.com:80/",
			"http://www.mono-project.com/",
			"http://www.mono-project.com",
			"www.mono-project.com",
			"www.novell.com",
			"*.mono-project.com",
			"*www.mono-project.com",
			"*-project.com",
			"*.com",
			"*",
		};

		[Test]
		public void PermissionState_None ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			// that cause a NullReferenceException before 2.0
			Assert.AreEqual (String.Empty, uip.Url, "Url");
			SecurityElement se = uip.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes");
			Assert.IsNull (se.Children, "Xml-Children");
			UrlIdentityPermission copy = (UrlIdentityPermission)uip.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (uip, copy), "ReferenceEquals");
		}


		[Test]
		[Category ("NotWorking")]
		public void PermissionStateUnrestricted ()
		{
			// In 2.0 Unrestricted are permitted for identity permissions
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.Unrestricted);
			Assert.AreEqual (String.Empty, uip.Url, "Url");
			SecurityElement se = uip.ToXml ();
			// only class and version are present
			Assert.AreEqual (3, se.Attributes.Count, "Xml-Attributes");
			Assert.IsNull (se.Children, "Xml-Children");
			// and they aren't equals to None
			Assert.IsFalse (uip.Equals (new UrlIdentityPermission (PermissionState.None)));
		}
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionState_Bad ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission ((PermissionState)Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void UrlIdentityPermission_NullUrl ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (null);
		}

		[Test]
		public void Url ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			foreach (string s in GoodUrls) {
				uip.Url = s;
				Assert.AreEqual (s, uip.Url, s);
			}
		}

		[Test]
		public void Url_Null ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			uip.Url = null;
			Assert.AreEqual (String.Empty, uip.Url, "Url");
		}

		[Test]
		public void Copy ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			foreach (string s in GoodUrls) {
				uip.Url = s;
				UrlIdentityPermission copy = (UrlIdentityPermission)uip.Copy ();
				// Fx 1.0/1.1 adds a '/' at the end, while 2.0 keeps the original format
				// so we only compare the start of the url
				Assert.AreEqual (uip.Url, copy.Url, "Url");
			}
		}

		[Test]
		public void Copy_None ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			UrlIdentityPermission copy = (UrlIdentityPermission)uip.Copy ();
		}

		[Test]
		public void Intersect_Null ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			// No intersection with null
			foreach (string s in GoodUrls) {
				uip.Url = s;
				Assert.IsNull (uip.Intersect (null), s);
			}
		}
		[Category ("NotWorking")]
		[Test]
		public void Intersect_None ()
		{
			UrlIdentityPermission uip1 = new UrlIdentityPermission (PermissionState.None);
			UrlIdentityPermission uip2 = new UrlIdentityPermission (PermissionState.None);
			UrlIdentityPermission result = (UrlIdentityPermission)uip1.Intersect (uip2);
			Assert.IsNull (result, "None N None");
			foreach (string s in GoodUrls) {
				uip1.Url = s;
				// 1. Intersect None with Url
				result = (UrlIdentityPermission)uip1.Intersect (uip2);
				Assert.IsNull (result, "None N " + s);
				// 2. Intersect Url with None
				result = (UrlIdentityPermission)uip2.Intersect (uip1);
				Assert.IsNull (result, s + "N None");
			}
		}

		[Test]
		public void Intersect_Self ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			foreach (string s in GoodUrls) {
				uip.Url = s;
				UrlIdentityPermission result = (UrlIdentityPermission)uip.Intersect (uip);
				// Fx 1.0/1.1 adds a '/' at the end, while 2.0 keeps the original format
				// so we only compare the start of the url
				Assert.IsTrue (result.Url.StartsWith (uip.Url), s);
			}
		}

		[Test]
		public void Intersect_Different ()
		{
			UrlIdentityPermission uip1 = new UrlIdentityPermission (GoodUrls [0]);
			UrlIdentityPermission uip2 = new UrlIdentityPermission (GoodUrls [1]);
			UrlIdentityPermission result = (UrlIdentityPermission)uip1.Intersect (uip2);
			Assert.IsNull (result, "Mono N Novell");
		}

		[Test]
		public void IsSubset_Null ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			Assert.IsTrue (uip.IsSubsetOf (null), "Empty");
			foreach (string s in GoodUrls) {
				uip.Url = s;
				Assert.IsFalse (uip.IsSubsetOf (null), s);
			}
		}

		[Category ("NotWorking")]
		[Test]
		public void IsSubset_None ()
		{
			// IsSubset with none
			// a. source (this) is none -> target is never a subset
			UrlIdentityPermission uip1 = new UrlIdentityPermission (PermissionState.None);
			UrlIdentityPermission uip2 = new UrlIdentityPermission (PermissionState.None);
			foreach (string s in GoodUrls) {
				uip1.Url = s;
				Assert.IsFalse (uip1.IsSubsetOf (uip2), "target " + s);
			}
			uip1 = new UrlIdentityPermission (PermissionState.None);
			// b. destination (target) is none -> target is always a subset
			foreach (string s in GoodUrls) {
				uip2.Url = s;
				Assert.IsFalse (uip2.IsSubsetOf (uip1), "source " + s);
			}
		}

		[Test]
		public void IsSubset_Self ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			Assert.IsTrue (uip.IsSubsetOf (uip), "None");
			foreach (string s in GoodUrls) {
				uip.Url = s;
				Assert.IsTrue (uip.IsSubsetOf (uip), s);
			}
		}

		[Test]
		public void IsSubset_Different ()
		{
			UrlIdentityPermission uip1 = new UrlIdentityPermission (GoodUrls [0]);
			UrlIdentityPermission uip2 = new UrlIdentityPermission (GoodUrls [1]);
			Assert.IsFalse (uip1.IsSubsetOf (uip2), "Mono subset Novell");
			Assert.IsFalse (uip2.IsSubsetOf (uip1), "Novell subset Mono");
		}

		[Test]
		public void Union_Null ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			// Union with null is a simple copy
			foreach (string s in GoodUrls) {
				uip.Url = s;
				UrlIdentityPermission union = (UrlIdentityPermission)uip.Union (null);
				// Fx 1.0/1.1 adds a '/' at the end, while 2.0 keeps the original format
				// so we only compare the start of the url
				Assert.IsTrue (union.Url.StartsWith (uip.Url), s);
			}
		}

		[Test]
		public void Union_None ()
		{
			// Union with none is same
			UrlIdentityPermission uip1 = new UrlIdentityPermission (PermissionState.None);
			UrlIdentityPermission uip2 = new UrlIdentityPermission (PermissionState.None);
			// a. source (this) is none
			foreach (string s in GoodUrls) {
				uip1.Url = s;
				UrlIdentityPermission union = (UrlIdentityPermission)uip1.Union (uip2);
				// Fx 1.0/1.1 adds a '/' at the end, while 2.0 keeps the original format
				// so we only compare the start of the url
				Assert.IsTrue (union.Url.StartsWith (uip1.Url), s);
			}
			uip1 = new UrlIdentityPermission (PermissionState.None);
			// b. destination (target) is none
			foreach (string s in GoodUrls) {
				uip2.Url = s;
				UrlIdentityPermission union = (UrlIdentityPermission)uip2.Union (uip1);
				// Fx 1.0/1.1 adds a '/' at the end, while 2.0 keeps the original format
				// so we only compare the start of the url
				Assert.IsTrue (union.Url.StartsWith (uip2.Url), s);
			}
		}

		[Test]
		public void Union_Self ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			UrlIdentityPermission union = (UrlIdentityPermission)uip.Union (uip);
			Assert.IsNull (union, "None U None"); 
			foreach (string s in GoodUrls) {
				uip.Url = s;
				union = (UrlIdentityPermission)uip.Union (uip);
				// Fx 1.0/1.1 adds a '/' at the end, while 2.0 keeps the original format
				// so we only compare the start of the url
				Assert.IsTrue (union.Url.StartsWith (uip.Url), s);
			}
		}
		[Category ("NotWorking")]
		[Test]
		public void Union_Different ()
		{
			UrlIdentityPermission uip1 = new UrlIdentityPermission (GoodUrls [0]);
			UrlIdentityPermission uip2 = new UrlIdentityPermission (GoodUrls [1]);
			UrlIdentityPermission result = (UrlIdentityPermission)uip1.Union (uip2);
			Assert.IsNotNull (result, "Mono U Novell");
			// new XML format is used to contain more than one site
			SecurityElement se = result.ToXml ();
			Assert.AreEqual (2, se.Children.Count, "Childs");
			Assert.AreEqual (GoodUrls [0], (se.Children [0] as SecurityElement).Attribute ("Url"), "Url#1");
			Assert.AreEqual (GoodUrls [1], (se.Children [1] as SecurityElement).Attribute ("Url"), "Url#2");
			// strangely it is still versioned as 'version="1"'.
			Assert.AreEqual ("1", se.Attribute ("version"), "Version");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			uip.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();
			se.Tag = "IMono";
			uip.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			uip.FromXml (se);
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			uip.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			uip.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			uip.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			uip.FromXml (w);
		}
	}
}
