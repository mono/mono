//
// UrlIdentityPermissionTest.cs - NUnit Test Cases for UrlIdentityPermission
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

		// accepted as Url but fails to work (as expected) in some methods
		static string[] SemiBadUrls = {
			"www.mono-project.com:80",
			String.Empty,
		};

		[Test]
		public void PermissionState_None ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
#if NET_2_0
			Assert.AreEqual (String.Empty, uip.Url, "Url");
#endif
			SecurityElement se = uip.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes");
			Assert.IsNull (se.Children, "Xml-Children");

//			UrlIdentityPermission copy = (UrlIdentityPermission)uip.Copy ();
//			Assert.IsFalse (Object.ReferenceEquals (uip, copy), "ReferenceEquals");
		}

#if !NET_2_0
		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void PermissionState_None_Url ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			Assert.IsNull (uip.Url, "Url");
		}
#endif

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionState_Unrestricted ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.Unrestricted);
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
#if NET_2_0
				Assert.AreEqual (s, uip.Url, s);
#else
				// Fx 1.0/1.1 adds a '/' at the end, while 2.0 keeps the original format
				// so we only compare the start of the url
#endif
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Url_Null ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			uip.Url = null;
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
		public void Intersect_Null ()
		{
			UrlIdentityPermission uip = new UrlIdentityPermission (PermissionState.None);
			// No intersection with null
			foreach (string s in GoodUrls) {
				uip.Url = s;
				Assert.IsNull (uip.Intersect (null), s);
			}
		}

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

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void Union_Different ()
		{
			UrlIdentityPermission uip1 = new UrlIdentityPermission (GoodUrls [0]);
			UrlIdentityPermission uip2 = new UrlIdentityPermission (GoodUrls [1]);
			UrlIdentityPermission result = (UrlIdentityPermission)uip1.Union (uip2);
			Assert.IsNull (result, "Mono U Novell");
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
