//
// ZoneIdentityPermissionTest.cs - NUnit Test Cases for ZoneIdentityPermission
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

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class ZoneIdentityPermissionTest	{

		[Test]
		public void PermissionStateNone ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (PermissionState.None);
			Assert.AreEqual (SecurityZone.NoZone, zip.SecurityZone);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionStateUnrestricted ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (PermissionState.Unrestricted);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionStateInvalid ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission ((PermissionState)2);
		}

		private bool Same (ZoneIdentityPermission zip1, ZoneIdentityPermission zip2)
		{
#if NET_2_0
			return zip1.Equals (zip2);
#else
			return (zip1.SecurityZone == zip2.SecurityZone);
#endif
		}

		private ZoneIdentityPermission BasicTestZone (SecurityZone zone, bool special)
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (zone);
			Assert.AreEqual (zone, zip.SecurityZone, "SecurityZone");
			
			ZoneIdentityPermission copy = (ZoneIdentityPermission) zip.Copy ();
			Assert.IsTrue (Same (zip, copy), "Equals-Copy");
			Assert.IsTrue (zip.IsSubsetOf (copy), "IsSubset-1");
			Assert.IsTrue (copy.IsSubsetOf (zip), "IsSubset-2");
			if (special) {
				Assert.IsFalse (zip.IsSubsetOf (null), "IsSubset-Null");
			}
			
			IPermission intersect = zip.Intersect (copy);
			if (special) {
				Assert.IsTrue (intersect.IsSubsetOf (zip), "IsSubset-3");
				Assert.IsFalse (Object.ReferenceEquals (zip, intersect), "!ReferenceEquals1");
				Assert.IsTrue (intersect.IsSubsetOf (copy), "IsSubset-4");
				Assert.IsFalse (Object.ReferenceEquals (copy, intersect), "!ReferenceEquals2");
			}

			Assert.IsNull (zip.Intersect (null), "Intersect with null");

			intersect = zip.Intersect (new ZoneIdentityPermission (PermissionState.None));
			Assert.IsNull (intersect, "Intersect with PS.None");

			// note: can't be tested with PermissionState.Unrestricted

			// XML roundtrip
			SecurityElement se = zip.ToXml ();
			copy.FromXml (se);
			Assert.IsTrue (Same (zip, copy), "Equals-Xml");

			return zip;
		}

		[Test]
		public void SecurityZone_Internet ()
		{
			BasicTestZone (SecurityZone.Internet, true);
		}

		[Test]
		public void SecurityZone_Intranet ()
		{
			BasicTestZone (SecurityZone.Intranet, true);
		}

		[Test]
		public void SecurityZone_MyComputer ()
		{
			BasicTestZone (SecurityZone.MyComputer, true);
		}

		[Test]
		public void SecurityZone_NoZone ()
		{
			ZoneIdentityPermission zip = BasicTestZone (SecurityZone.NoZone, false);
			Assert.IsNull (zip.ToXml ().Attribute ("Zone"), "Zone Attribute");
			Assert.IsTrue (zip.IsSubsetOf (null), "IsSubset-Null");
			IPermission intersect = zip.Intersect (zip);
			Assert.IsNull (intersect, "Intersect with No Zone");
			// NoZone is special as it is a subset of all zones
			ZoneIdentityPermission ss = new ZoneIdentityPermission (SecurityZone.Internet);
			Assert.IsTrue (zip.IsSubsetOf (ss), "IsSubset-Internet");
			ss.SecurityZone = SecurityZone.Intranet;
			Assert.IsTrue (zip.IsSubsetOf (ss), "IsSubset-Intranet");
			ss.SecurityZone = SecurityZone.MyComputer;
			Assert.IsTrue (zip.IsSubsetOf (ss), "IsSubset-MyComputer");
			ss.SecurityZone = SecurityZone.NoZone;
			Assert.IsTrue (zip.IsSubsetOf (ss), "IsSubset-NoZone");
			ss.SecurityZone = SecurityZone.Trusted;
			Assert.IsTrue (zip.IsSubsetOf (ss), "IsSubset-Trusted");
			ss.SecurityZone = SecurityZone.Untrusted;
			Assert.IsTrue (zip.IsSubsetOf (ss), "IsSubset-Untrusted");
		}

		[Test]
		public void SecurityZone_Trusted ()
		{
			BasicTestZone (SecurityZone.Trusted, true);
		}

		[Test]
		public void SecurityZone_Untrusted ()
		{
			BasicTestZone (SecurityZone.Untrusted, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SecurityZone_Invalid ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission ((SecurityZone)128);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_DifferentPermissions ()
		{
			ZoneIdentityPermission a = new ZoneIdentityPermission (SecurityZone.Trusted);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.Intersect (b);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOf_DifferentPermissions ()
		{
			ZoneIdentityPermission a = new ZoneIdentityPermission (SecurityZone.Trusted);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.IsSubsetOf (b);
		}

		[Test]
		public void Union () 
		{
			ZoneIdentityPermission a = new ZoneIdentityPermission (SecurityZone.Trusted);

			ZoneIdentityPermission z = (ZoneIdentityPermission) a.Union (null);
			Assert.IsTrue (Same (a, z), "Trusted+null");
			Assert.IsFalse (Object.ReferenceEquals (a, z), "!ReferenceEquals1");

			z = (ZoneIdentityPermission) a.Union (new ZoneIdentityPermission (PermissionState.None));
			Assert.IsTrue (Same (a, z), "Trusted+PS.None");
			Assert.IsFalse (Object.ReferenceEquals (a, z), "!ReferenceEquals2");

			// note: can't be tested with PermissionState.Unrestricted

			ZoneIdentityPermission n = new ZoneIdentityPermission (SecurityZone.NoZone);
			z = (ZoneIdentityPermission) a.Union (n);
			Assert.IsTrue (Same (a, z), "Trusted+NoZone");
			Assert.IsFalse (Object.ReferenceEquals (a, z), "!ReferenceEquals3");

			z = (ZoneIdentityPermission) n.Union (a);
			Assert.IsTrue (Same (a, z), "NoZone+Trusted");
			Assert.IsFalse (Object.ReferenceEquals (a, z), "!ReferenceEquals4");
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_DifferentIdentities ()
		{
			ZoneIdentityPermission a = new ZoneIdentityPermission (SecurityZone.Trusted);
			ZoneIdentityPermission b = new ZoneIdentityPermission (SecurityZone.Untrusted);
			a.Union (b);
		}
#else
		[Test]
		public void Union_DifferentIdentities ()
		{
			ZoneIdentityPermission a = new ZoneIdentityPermission (SecurityZone.Trusted);
			ZoneIdentityPermission b = new ZoneIdentityPermission (SecurityZone.Untrusted);
			Assert.IsNull (a.Union (b));
		}
#endif
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_DifferentPermissions ()
		{
			ZoneIdentityPermission a = new ZoneIdentityPermission (SecurityZone.Trusted);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.Union (b);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (PermissionState.None);
			zip.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (PermissionState.None);
			SecurityElement se = zip.ToXml ();
			se.Tag = "IMono"; // instead of IPermission
			zip.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (PermissionState.None);
			SecurityElement se = zip.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			zip.FromXml (se);
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (PermissionState.None);
			SecurityElement se = zip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			zip.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (PermissionState.None);
			SecurityElement se = zip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			zip.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (PermissionState.None);
			SecurityElement se = zip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			zip.FromXml (w);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (PermissionState.None);
			SecurityElement se = zip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			zip.FromXml (w);
		}
	}
}
