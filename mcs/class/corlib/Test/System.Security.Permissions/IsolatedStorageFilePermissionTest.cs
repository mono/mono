//
// IsolatedStorageFilePermissionTest.cs - NUnit Test Cases for IsolatedStorageFilePermission
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
	public class IsolatedStorageFilePermissionTest {

		[Test]
		public void PermissionStateNone ()
		{
			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.None);
			Assert.AreEqual (IsolatedStorageContainment.None, isfp.UsageAllowed, "UsageAllowed");
			Assert.AreEqual (0, isfp.UserQuota, "UserQuota");

			SecurityElement se = isfp.ToXml ();
			// only class and version are present
			Assert.AreEqual ("None", se.Attribute ("Allowed"), "Xml-Allowed");
			Assert.IsNull (se.Children, "Xml-Children");

			IsolatedStorageFilePermission copy = (IsolatedStorageFilePermission)isfp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (isfp, copy), "ReferenceEquals");
			Assert.AreEqual (isfp.UsageAllowed, copy.UsageAllowed, "UsageAllowed");
			Assert.AreEqual (isfp.UserQuota, copy.UserQuota, "UserQuota");
		}

		[Test]
		public void PermissionStateUnrestricted ()
		{
			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.Unrestricted);
			Assert.AreEqual (IsolatedStorageContainment.UnrestrictedIsolatedStorage, isfp.UsageAllowed, "UsageAllowed");
			Assert.AreEqual (Int64.MaxValue, isfp.UserQuota, "UserQuota");

			SecurityElement se = isfp.ToXml ();
			// only class and version are present
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "Xml-Unrestricted");
			Assert.IsNull (se.Children, "Xml-Children");

			IsolatedStorageFilePermission copy = (IsolatedStorageFilePermission)isfp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (isfp, copy), "ReferenceEquals");
			Assert.AreEqual (isfp.UsageAllowed, copy.UsageAllowed, "UsageAllowed");
			Assert.AreEqual (isfp.UserQuota, copy.UserQuota, "UserQuota");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionStateInvalid ()
		{
			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission ((PermissionState)2);
		}

		[Test]
		public void Intersect ()
		{
			IsolatedStorageFilePermission empty = new IsolatedStorageFilePermission (PermissionState.None);
			IsolatedStorageFilePermission intersect = (IsolatedStorageFilePermission)empty.Intersect (null);
			Assert.IsNull (intersect, "empty N null");

			intersect = (IsolatedStorageFilePermission)empty.Intersect (empty);
			Assert.IsNull (intersect, "empty N empty");

			IsolatedStorageFilePermission unrestricted = new IsolatedStorageFilePermission (PermissionState.Unrestricted);
			intersect = (IsolatedStorageFilePermission)unrestricted.Intersect (null);
			Assert.IsNull (intersect, "unrestricted N null");

			intersect = (IsolatedStorageFilePermission)unrestricted.Intersect (empty);
			Assert.IsNotNull (intersect, "unrestricted N empty");

			intersect = (IsolatedStorageFilePermission)unrestricted.Intersect (unrestricted);
			Assert.IsNotNull (intersect, "unrestricted N unrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_DifferentPermissions ()
		{
			IsolatedStorageFilePermission a = new IsolatedStorageFilePermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.Intersect (b);
		}

		[Test]
		public void IsSubsetOf ()
		{
			IsolatedStorageFilePermission empty = new IsolatedStorageFilePermission (PermissionState.None);
			Assert.IsTrue (empty.IsSubsetOf (null), "empty.IsSubsetOf (null)");

			IsolatedStorageFilePermission unrestricted = new IsolatedStorageFilePermission (PermissionState.Unrestricted);
			Assert.IsFalse (unrestricted.IsSubsetOf (null), "unrestricted.IsSubsetOf (null)");
			Assert.IsFalse (unrestricted.IsSubsetOf (empty), "unrestricted.IsSubsetOf (empty)");
			Assert.IsTrue (empty.IsSubsetOf (unrestricted), "empty.IsSubsetOf (unrestricted)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOf_DifferentPermissions ()
		{
			IsolatedStorageFilePermission a = new IsolatedStorageFilePermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.IsSubsetOf (b);
		}

		[Test]
		public void Union ()
		{
			IsolatedStorageFilePermission empty = new IsolatedStorageFilePermission (PermissionState.None);
			IsolatedStorageFilePermission union = (IsolatedStorageFilePermission)empty.Union (null);
			Assert.IsNotNull (union, "empty U null");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-1");
			Assert.IsFalse (Object.ReferenceEquals (empty, union), "ReferenceEquals-1");

			union = (IsolatedStorageFilePermission)empty.Union (empty);
			Assert.IsNotNull (union, "empty U empty");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-2");
			Assert.IsFalse (Object.ReferenceEquals (empty, union), "ReferenceEquals-2");

			IsolatedStorageFilePermission unrestricted = new IsolatedStorageFilePermission (PermissionState.Unrestricted);
			union = (IsolatedStorageFilePermission)unrestricted.Union (null);
			Assert.IsNotNull (union, "unrestricted U null");
			Assert.IsTrue (union.IsUnrestricted (), "IsUnrestricted-3");
			Assert.IsFalse (Object.ReferenceEquals (unrestricted, union), "ReferenceEquals-3");

			union = (IsolatedStorageFilePermission)unrestricted.Union (empty);
			Assert.IsNotNull (union, "unrestricted U empty");
			Assert.IsTrue (union.IsUnrestricted (), "IsUnrestricted-4");
			Assert.IsFalse (Object.ReferenceEquals (unrestricted, union), "ReferenceEquals-4");

			union = (IsolatedStorageFilePermission)unrestricted.Union (unrestricted);
			Assert.IsNotNull (union, "unrestricted U unrestricted");
			Assert.IsTrue (union.IsUnrestricted (), "IsUnrestricted-5");
			Assert.IsFalse (Object.ReferenceEquals (unrestricted, union), "ReferenceEquals-5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_DifferentPermissions ()
		{
			IsolatedStorageFilePermission a = new IsolatedStorageFilePermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.Union (b);
		}

		[Test]
		public void UsageAllowedQuota ()
		{
			IsolatedStorageFilePermission empty = new IsolatedStorageFilePermission (PermissionState.None);
			IsolatedStorageFilePermission small = new IsolatedStorageFilePermission (PermissionState.None);
			small.UsageAllowed = IsolatedStorageContainment.DomainIsolationByUser;
			small.UserQuota = 1;
			IsolatedStorageFilePermission union = (IsolatedStorageFilePermission)empty.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.DomainIsolationByUser, union.UsageAllowed, "DomainIsolationByUser");
			Assert.AreEqual (1, union.UserQuota, "1");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-1");
			Assert.IsTrue (small.IsSubsetOf (union), "IsSubset-1a");
			Assert.IsTrue (empty.IsSubsetOf (union), "IsSubset-1b");
			Assert.IsTrue (union.IsSubsetOf (small), "IsSubset-1c");
			IsolatedStorageFilePermission intersect = (IsolatedStorageFilePermission)union.Intersect (small);
			Assert.AreEqual (small.UsageAllowed, intersect.UsageAllowed, "Intersect-UsageAllowed-1");
			Assert.AreEqual (small.UserQuota, intersect.UserQuota, "Intersect-UserQuota-1");

			small.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser;
			small.UserQuota = 2;
			union = (IsolatedStorageFilePermission)union.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.AssemblyIsolationByUser, union.UsageAllowed, "AssemblyIsolationByUser");
			Assert.AreEqual (2, union.UserQuota, "2");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-2");
			Assert.IsTrue (small.IsSubsetOf (union), "IsSubset-2a");
			Assert.IsTrue (empty.IsSubsetOf (union), "IsSubset-2b");
			Assert.IsTrue (union.IsSubsetOf (small), "IsSubset-2c");
			intersect = (IsolatedStorageFilePermission)union.Intersect (small);
			Assert.AreEqual (small.UsageAllowed, intersect.UsageAllowed, "Intersect-UsageAllowed-2");
			Assert.AreEqual (small.UserQuota, intersect.UserQuota, "Intersect-UserQuota-2");
#if NET_2_0
			small.UsageAllowed = IsolatedStorageContainment.ApplicationIsolationByUser;
			small.UserQuota = 3;
			union = (IsolatedStorageFilePermission)union.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.ApplicationIsolationByUser, union.UsageAllowed, "ApplicationIsolationByUser");
			Assert.AreEqual (3, union.UserQuota, "3");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-3");
			Assert.IsTrue (small.IsSubsetOf (union), "IsSubset-3a");
			Assert.IsTrue (empty.IsSubsetOf (union), "IsSubset-3b");
			Assert.IsTrue (union.IsSubsetOf (small), "IsSubset-3c");
			intersect = (IsolatedStorageFilePermission)union.Intersect (small);
			Assert.AreEqual (small.UsageAllowed, intersect.UsageAllowed, "Intersect-UsageAllowed-3");
			Assert.AreEqual (small.UserQuota, intersect.UserQuota, "Intersect-UserQuota-3");

			small.UsageAllowed = IsolatedStorageContainment.DomainIsolationByMachine;
			small.UserQuota = 4;
			union = (IsolatedStorageFilePermission)union.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.DomainIsolationByMachine, union.UsageAllowed, "DomainIsolationByMachine");
			Assert.AreEqual (4, union.UserQuota, "4");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-4");
			Assert.IsTrue (small.IsSubsetOf (union), "IsSubset-4a");
			Assert.IsTrue (empty.IsSubsetOf (union), "IsSubset-4b");
			Assert.IsTrue (union.IsSubsetOf (small), "IsSubset-4c");
			intersect = (IsolatedStorageFilePermission)union.Intersect (small);
			Assert.AreEqual (small.UsageAllowed, intersect.UsageAllowed, "Intersect-UsageAllowed-4");
			Assert.AreEqual (small.UserQuota, intersect.UserQuota, "Intersect-UserQuota-4");

			small.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByMachine;
			small.UserQuota = 5;
			union = (IsolatedStorageFilePermission)union.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.AssemblyIsolationByMachine, union.UsageAllowed, "AssemblyIsolationByMachine");
			Assert.AreEqual (5, union.UserQuota, "5");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-5");
			Assert.IsTrue (small.IsSubsetOf (union), "IsSubset-5a");
			Assert.IsTrue (empty.IsSubsetOf (union), "IsSubset-5b");
			Assert.IsTrue (union.IsSubsetOf (small), "IsSubset-5c");
			intersect = (IsolatedStorageFilePermission)union.Intersect (small);
			Assert.AreEqual (small.UsageAllowed, intersect.UsageAllowed, "Intersect-UsageAllowed-5");
			Assert.AreEqual (small.UserQuota, intersect.UserQuota, "Intersect-UserQuota-5");

			small.UsageAllowed = IsolatedStorageContainment.ApplicationIsolationByMachine;
			small.UserQuota = 6;
			union = (IsolatedStorageFilePermission)union.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.ApplicationIsolationByMachine, union.UsageAllowed, "ApplicationIsolationByMachine");
			Assert.AreEqual (6, union.UserQuota, "6");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-6");
			Assert.IsTrue (small.IsSubsetOf (union), "IsSubset-6a");
			Assert.IsTrue (empty.IsSubsetOf (union), "IsSubset-6b");
			Assert.IsTrue (union.IsSubsetOf (small), "IsSubset-6c");
			intersect = (IsolatedStorageFilePermission)union.Intersect (small);
			Assert.AreEqual (small.UsageAllowed, intersect.UsageAllowed, "Intersect-UsageAllowed-6");
			Assert.AreEqual (small.UserQuota, intersect.UserQuota, "Intersect-UserQuota-6");
#endif
			small.UsageAllowed = IsolatedStorageContainment.DomainIsolationByRoamingUser;
			small.UserQuota = 7;
			union = (IsolatedStorageFilePermission)union.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.DomainIsolationByRoamingUser, union.UsageAllowed, "DomainIsolationByRoamingUser");
			Assert.AreEqual (7, union.UserQuota, "7a");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-7a");
			Assert.IsTrue (small.IsSubsetOf (union), "IsSubset-7a");
			Assert.IsTrue (empty.IsSubsetOf (union), "IsSubset-7b");
			Assert.IsTrue (union.IsSubsetOf (small), "IsSubset-7c");
			intersect = (IsolatedStorageFilePermission)union.Intersect (small);
			Assert.AreEqual (small.UsageAllowed, intersect.UsageAllowed, "Intersect-UsageAllowed-7");
			Assert.AreEqual (small.UserQuota, intersect.UserQuota, "Intersect-UserQuota-7");

			// can't go back ;-)
			small.UsageAllowed = IsolatedStorageContainment.DomainIsolationByUser;
			small.UserQuota = 1;
			union = (IsolatedStorageFilePermission)union.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.DomainIsolationByRoamingUser, union.UsageAllowed, "DomainIsolationByRoamingUser");
			Assert.AreEqual (7, union.UserQuota, "7b");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-7b");

			small.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByRoamingUser;
			small.UserQuota = 7; // no change
			union = (IsolatedStorageFilePermission)union.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.AssemblyIsolationByRoamingUser, union.UsageAllowed, "AssemblyIsolationByRoamingUser");
			Assert.AreEqual (7, union.UserQuota, "7c");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-7c");
#if NET_2_0
			small.UsageAllowed = IsolatedStorageContainment.ApplicationIsolationByRoamingUser;
			small.UserQuota = 8;
			union = (IsolatedStorageFilePermission)union.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.ApplicationIsolationByRoamingUser, union.UsageAllowed, "ApplicationIsolationByRoamingUser");
			Assert.AreEqual (8, union.UserQuota, "8");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-8");
			Assert.IsTrue (small.IsSubsetOf (union), "IsSubset-8a");
			Assert.IsTrue (empty.IsSubsetOf (union), "IsSubset-8b");
			Assert.IsTrue (union.IsSubsetOf (small), "IsSubset-8c");
			intersect = (IsolatedStorageFilePermission)union.Intersect (small);
			Assert.AreEqual (small.UsageAllowed, intersect.UsageAllowed, "Intersect-UsageAllowed-8");
			Assert.AreEqual (small.UserQuota, intersect.UserQuota, "Intersect-UserQuota-8");
#endif
			small.UsageAllowed = IsolatedStorageContainment.AdministerIsolatedStorageByUser;
			small.UserQuota = 9;
			union = (IsolatedStorageFilePermission)union.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.AdministerIsolatedStorageByUser, union.UsageAllowed, "AdministerIsolatedStorageByUser");
			Assert.AreEqual (9, union.UserQuota, "9");
			Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted-9");
			Assert.IsTrue (small.IsSubsetOf (union), "IsSubset-9a");
			Assert.IsTrue (empty.IsSubsetOf (union), "IsSubset-9b");
			Assert.IsTrue (union.IsSubsetOf (small), "IsSubset-9c");
			intersect = (IsolatedStorageFilePermission)union.Intersect (small);
			Assert.AreEqual (small.UsageAllowed, intersect.UsageAllowed, "Intersect-UsageAllowed-9");
			Assert.AreEqual (small.UserQuota, intersect.UserQuota, "Intersect-UserQuota-9");

			small.UsageAllowed = IsolatedStorageContainment.UnrestrictedIsolatedStorage;
			small.UserQuota = 10;
			union = (IsolatedStorageFilePermission)union.Union (small);
			Assert.AreEqual (IsolatedStorageContainment.UnrestrictedIsolatedStorage, union.UsageAllowed, "UnrestrictedIsolatedStorage");
			Assert.AreEqual (Int64.MaxValue, union.UserQuota, "10");
			Assert.IsTrue (union.IsUnrestricted (), "IsUnrestricted-10");
			Assert.IsTrue (small.IsSubsetOf (union), "IsSubset-10a");
			Assert.IsTrue (empty.IsSubsetOf (union), "IsSubset-10b");
			Assert.IsTrue (union.IsSubsetOf (small), "IsSubset-10c");
			intersect = (IsolatedStorageFilePermission)union.Intersect (small);
			Assert.AreEqual (small.UsageAllowed, intersect.UsageAllowed, "Intersect-UsageAllowed-10");
			Assert.IsFalse ((small.UserQuota == intersect.UserQuota), "Intersect-UserQuota-10");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.None);
			isfp.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.None);
			SecurityElement se = isfp.ToXml ();
			se.Tag = "IMono";
			isfp.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.None);
			SecurityElement se = isfp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			isfp.FromXml (se);
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.None);
			SecurityElement se = isfp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			isfp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.None);
			SecurityElement se = isfp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			isfp.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.None);
			SecurityElement se = isfp.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			isfp.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.None);
			SecurityElement se = isfp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			isfp.FromXml (w);
		}
	}
}
