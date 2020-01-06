//
// SiteIdentityPermissionTest.cs - NUnit Test Cases for SiteIdentityPermission
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
	public class SiteIdentityPermissionTest {

		static string[] GoodSites = {
			"www.mono-project.com",
			"www.novell.com",
			"*.mono-project.com",
			"*.com",
			"*",
		};

		static string[] BadSites = {
			"http://www.mono-project.com:80/",
			"http://www.mono-project.com/",
			"http://www.mono-project.com",
			"www.mono-project.com:80",
			"*www.mono-project.com",
			"*-project.com",
			"www.*.com",
			"www.mono-project.com*"
		};

		[Category ("NotWorking")]
		[Test]
		public void PermissionState_None ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			Assert.AreEqual (String.Empty, sip.Site, "Site");
			SecurityElement se = sip.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes");
			Assert.IsNull (se.Children, "Xml-Children");
			SiteIdentityPermission copy = (SiteIdentityPermission)sip.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (sip, copy), "ReferenceEquals");
		}

		[Category ("NotWorking")]
		[Test]
		public void PermissionStateUnrestricted ()
		{
			// In 2.0 Unrestricted are permitted for identity permissions
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.Unrestricted);
			Assert.AreEqual (String.Empty, sip.Site, "Site");
			SecurityElement se = sip.ToXml ();
			Assert.AreEqual (3, se.Attributes.Count, "Xml-Attributes");
			Assert.IsNull (se.Children, "Xml-Children");
			SiteIdentityPermission copy = (SiteIdentityPermission)sip.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (sip, copy), "ReferenceEquals");
			// and they aren't equals to None
			Assert.IsFalse (sip.Equals (new SiteIdentityPermission (PermissionState.None)));
		}
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionState_Bad ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission ((PermissionState)Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SiteIdentityPermission_NullSite ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SiteIdentityPermission_EmptySite ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (String.Empty);
		}

		[Test]
		public void Site ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			foreach (string s in GoodSites)	{
				sip.Site = s;
				Assert.AreEqual (s, sip.Site, s);
			}
		}

		[Test]
		public void Site_Bad ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			foreach (string s in BadSites) {
				try {
					sip.Site = s;
					Assert.Fail (s + " isn't a bad site!");
				}
				catch (ArgumentException) {
					// expected, continue looping
				}
			}
		}

/*		[Test]
		public void Site_InvalidChars ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			for (int i=0; i < 256; i++) {
				try {
					sip.Site = String.Empty + (char) i;
				}
				catch (ArgumentException) {
					Console.WriteLine ("{0} is bad", i);
				}
			}
		}*/

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Site_Null ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			sip.Site = null;
		}

		[Test]
		public void Copy ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			foreach (string s in GoodSites)	{
				sip.Site = s;
				SiteIdentityPermission copy = (SiteIdentityPermission)sip.Copy ();
				Assert.AreEqual (s, copy.Site, s);
			}
		}

		[Test]
		public void Intersect_Null ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			// No intersection with null
			foreach (string s in GoodSites)	{
				sip.Site = s;
				Assert.IsNull (sip.Intersect (null), s);
			}
		}

		[Test]
		public void Intersect_None ()
		{
			SiteIdentityPermission sip1 = new SiteIdentityPermission (PermissionState.None);
			SiteIdentityPermission sip2 = new SiteIdentityPermission (PermissionState.None);
			SiteIdentityPermission result = (SiteIdentityPermission)sip1.Intersect (sip2);
			Assert.IsNull (result, "None N None");
			foreach (string s in GoodSites)	{
				sip1.Site = s;
				// 1. Intersect None with site
				result = (SiteIdentityPermission)sip1.Intersect (sip2);
				Assert.IsNull (result, "None N " + s);
				// 2. Intersect site with None
				result = (SiteIdentityPermission)sip2.Intersect (sip1);
				Assert.IsNull (result, s + "N None");
			}
		}

		[Test]
		public void Intersect_Self ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			foreach (string s in GoodSites)	{
				sip.Site = s;
				SiteIdentityPermission result = (SiteIdentityPermission)sip.Intersect (sip);
				Assert.AreEqual (s, result.Site, s);
			}
		}

		[Test]
		public void Intersect_Different ()
		{
			SiteIdentityPermission sip1 = new SiteIdentityPermission (GoodSites [0]);
			SiteIdentityPermission sip2 = new SiteIdentityPermission (GoodSites [1]);
			SiteIdentityPermission result = (SiteIdentityPermission)sip1.Intersect (sip2);
			Assert.IsNull (result, "Mono N Novell");
		}

		[Test]
		public void IsSubset_Null ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			Assert.IsTrue (sip.IsSubsetOf (null), "Empty");
			foreach (string s in GoodSites)	{
				sip.Site = s;
				Assert.IsFalse (sip.IsSubsetOf (null), s);
			}
		}

		[Test]
		public void IsSubset_None ()
		{
			// IsSubset with none
			// a. source (this) is none -> target is never a subset
			SiteIdentityPermission sip1 = new SiteIdentityPermission (PermissionState.None);
			SiteIdentityPermission sip2 = new SiteIdentityPermission (PermissionState.None);
			foreach (string s in GoodSites)	{
				sip1.Site = s;
				Assert.IsFalse (sip1.IsSubsetOf (sip2), "target " + s);
			}
			sip1 = new SiteIdentityPermission (PermissionState.None);
			// b. destination (target) is none -> target is always a subset
			foreach (string s in GoodSites)	{
				sip2.Site = s;
				Assert.IsFalse (sip2.IsSubsetOf (sip1), "source " + s);
			}
		}

		[Test]
		public void IsSubset_Self ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			Assert.IsTrue (sip.IsSubsetOf (sip), "None");
			foreach (string s in GoodSites)	{
				sip.Site = s;
				Assert.IsTrue (sip.IsSubsetOf (sip), s);
			}
		}

		[Test]
		public void IsSubset_Different ()
		{
			SiteIdentityPermission sip1 = new SiteIdentityPermission (GoodSites [0]);
			SiteIdentityPermission sip2 = new SiteIdentityPermission (GoodSites [1]);
			Assert.IsFalse (sip1.IsSubsetOf (sip2), "Mono subset Novell");
			Assert.IsFalse (sip2.IsSubsetOf (sip1), "Novell subset Mono");
		}

		[Test]
		public void IsSubset_Wildcard ()
		{
			SiteIdentityPermission sip1 = new SiteIdentityPermission (GoodSites [0]);
			SiteIdentityPermission sip2 = new SiteIdentityPermission ("*.mono-project.com");
			Assert.IsTrue (sip1.IsSubsetOf (sip2), "www.mono-project.com subset *.mono-project.com");
			Assert.IsFalse (sip2.IsSubsetOf (sip1), "*.mono-project.com subset www.mono-project.com");
		}

		[Test]
		public void Union_Null ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			// Union with null is a simple copy
			foreach (string s in GoodSites)	{
				sip.Site = s;
				SiteIdentityPermission union = (SiteIdentityPermission)sip.Union (null);
				Assert.AreEqual (s, union.Site, s);
			}
		}

		[Test]
		public void Union_None ()
		{
			// Union with none is same
			SiteIdentityPermission sip1 = new SiteIdentityPermission (PermissionState.None);
			SiteIdentityPermission sip2 = new SiteIdentityPermission (PermissionState.None);
			// a. source (this) is none
			foreach (string s in GoodSites)	{
				sip1.Site = s;
				SiteIdentityPermission union = (SiteIdentityPermission)sip1.Union (sip2);
				Assert.AreEqual (s, union.Site, s);
			}
			sip1 = new SiteIdentityPermission (PermissionState.None);
			// b. destination (target) is none
			foreach (string s in GoodSites)	{
				sip2.Site = s;
				SiteIdentityPermission union = (SiteIdentityPermission)sip2.Union (sip1);
				Assert.AreEqual (s, union.Site, s);
			}
		}
		[Category ("NotWorking")]
		[Test]
		public void Union_Self ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			SiteIdentityPermission union = (SiteIdentityPermission)sip.Union (sip);
			Assert.IsNull (union, "None U None");
		}
		[Category ("NotWorking")]
		[Test]
		public void Union_Different ()
		{
			SiteIdentityPermission sip1 = new SiteIdentityPermission (GoodSites [0]);
			SiteIdentityPermission sip2 = new SiteIdentityPermission (GoodSites [1]);
			SiteIdentityPermission result = (SiteIdentityPermission)sip1.Union (sip2);
			Assert.IsNotNull (result, "Mono U Novell");
			// new XML format is used to contain more than one site
			SecurityElement se = result.ToXml ();
			Assert.AreEqual (2, se.Children.Count, "Childs");
			Assert.AreEqual ((se.Children [0] as SecurityElement).Attribute ("Site"), sip1.Site, "Site#1");
			Assert.AreEqual ((se.Children [1] as SecurityElement).Attribute ("Site"), sip2.Site, "Site#2");
			// strangely it is still versioned as 'version="1"'.
			Assert.AreEqual ("1", se.Attribute ("version"), "Version");
		}
		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (NotSupportedException))]
		public void Union_Different_Site ()
		{
			SiteIdentityPermission sip1 = new SiteIdentityPermission (GoodSites [0]);
			SiteIdentityPermission sip2 = new SiteIdentityPermission (GoodSites [1]);
			SiteIdentityPermission result = (SiteIdentityPermission)sip1.Union (sip2);
			// it's not possible to return many sites using the Site property so it throws
			Assert.IsNull (result.Site);
		}
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			sip.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			SecurityElement se = sip.ToXml ();
			se.Tag = "IMono";
			sip.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			SecurityElement se = sip.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			sip.FromXml (se);
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			SecurityElement se = sip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			sip.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			SecurityElement se = sip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			sip.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			SecurityElement se = sip.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			sip.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			SiteIdentityPermission sip = new SiteIdentityPermission (PermissionState.None);
			SecurityElement se = sip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			sip.FromXml (w);
		}
	}
}
