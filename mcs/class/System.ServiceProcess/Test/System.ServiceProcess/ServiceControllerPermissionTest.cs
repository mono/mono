//
// ServiceControllerPermissionTest.cs -
//	NUnit Test Cases for ServiceControllerPermission
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
using System.ServiceProcess;

namespace MonoTests.System.ServiceProcess {

	[TestFixture]
	public class ServiceControllerPermissionTest {

		static ServiceControllerPermissionAccess [] AllAccess = {
			ServiceControllerPermissionAccess.None,
			ServiceControllerPermissionAccess.Browse,
			ServiceControllerPermissionAccess.Control,
		};

		[Test]
		public void PermissionState_None ()
		{
			PermissionState ps = PermissionState.None;
			ServiceControllerPermission scp = new ServiceControllerPermission (ps);
			Assert.AreEqual (0, scp.PermissionEntries.Count, "PermissionEntries");
			Assert.IsFalse (scp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = scp.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes");
			Assert.IsNull (se.Children, "Xml-Children");

			ServiceControllerPermission copy = (ServiceControllerPermission)scp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (scp, copy), "ReferenceEquals");
			Assert.AreEqual (scp.PermissionEntries.Count, copy.PermissionEntries.Count, "copy-PermissionEntries");
			Assert.AreEqual (scp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		public void PermissionState_Unrestricted ()
		{
			PermissionState ps = PermissionState.Unrestricted;
			ServiceControllerPermission scp = new ServiceControllerPermission (ps);
			Assert.AreEqual (0, scp.PermissionEntries.Count, "PermissionEntries");
			Assert.IsTrue (scp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = scp.ToXml ();
			// only class and version are present
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "Xml-Unrestricted");
			Assert.IsNull (se.Children, "Xml-Children");

			ServiceControllerPermission copy = (ServiceControllerPermission)scp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (scp, copy), "ReferenceEquals");
			Assert.AreEqual (scp.PermissionEntries.Count, copy.PermissionEntries.Count, "copy-PermissionEntries");
			Assert.AreEqual (scp.IsUnrestricted (), copy.IsUnrestricted (), "copy-IsUnrestricted ()");
		}

		[Test]
// strange as ancestors does the checking (reported as FDBK15131)
//		[ExpectedException (typeof (ArgumentException))]
		public void PermissionState_Bad ()
		{
			PermissionState ps = (PermissionState)77;
			ServiceControllerPermission scp = new ServiceControllerPermission (ps);
			Assert.IsFalse (scp.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void ServiceControllerPermissionAccesss_Bad ()
		{
			ServiceControllerPermissionAccess scpa = (ServiceControllerPermissionAccess)Int32.MinValue;
			ServiceControllerPermission scp = new ServiceControllerPermission (scpa, "localhost", "http");
			Assert.AreEqual (1, scp.PermissionEntries.Count, "Count");
			Assert.AreEqual ((ServiceControllerPermissionAccess)Int32.MinValue, scp.PermissionEntries [0].PermissionAccess, "PermissionAccess");
		}

		[Test]
		public void PermissionEntries ()
		{
			ServiceControllerPermissionAccess scpa = ServiceControllerPermissionAccess.None;
			ServiceControllerPermission scp = new ServiceControllerPermission (scpa, "localhost", "http");
			ServiceControllerPermissionEntryCollection scpec = scp.PermissionEntries;
			Assert.AreEqual (1, scpec.Count, "Count==1");

			ServiceControllerPermissionEntry scpe = new ServiceControllerPermissionEntry (ServiceControllerPermissionAccess.Browse, "*", "ftp");
			scp.PermissionEntries.Add (scpe);
			Assert.AreEqual (2, scpec.Count, "Count==2");

			// remove (same instance)
			scp.PermissionEntries.Remove (scpe);
			Assert.AreEqual (1, scpec.Count, "Count==1 (b)");

			// remove different instance (doesn't work)
			scpe = new ServiceControllerPermissionEntry (ServiceControllerPermissionAccess.None, "localhost", "http");
			Assert.AreEqual (1, scpec.Count, "Count==1");
		}

		[Test]
		public void Copy ()
		{
			foreach (ServiceControllerPermissionAccess scpa in AllAccess) {
				ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
				ServiceControllerPermissionEntry scpe = new ServiceControllerPermissionEntry (scpa, "localhost", scpa.ToString ());
				scp.PermissionEntries.Add (scpe);
				ServiceControllerPermission copy = (ServiceControllerPermission)scp.Copy ();
				Assert.AreEqual (1, copy.PermissionEntries.Count, "Count==1");
				Assert.AreEqual (scpa, scp.PermissionEntries [0].PermissionAccess, scpa.ToString ());
			}
		}

		[Test]
		public void Intersect_Null ()
		{
			ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
			// No intersection with null
			Assert.IsNull (scp.Intersect (null), "None N null");
		}

		[Test]
		public void Intersect_None ()
		{
			ServiceControllerPermission scp1 = new ServiceControllerPermission (PermissionState.None);
			ServiceControllerPermission scp2 = new ServiceControllerPermission (PermissionState.None);
			// 1. None N None
			ServiceControllerPermission result = (ServiceControllerPermission)scp1.Intersect (scp2);
			Assert.IsNull (result, "Empty N Empty");
			// 2. None N Entry
			scp2.PermissionEntries.Add (new ServiceControllerPermissionEntry (ServiceControllerPermissionAccess.None, "localhost", "http"));
			result = (ServiceControllerPermission)scp1.Intersect (scp2);
			Assert.IsNull (result, "Empty N Entry");
			// 3. Entry N None
			result = (ServiceControllerPermission)scp2.Intersect (scp1);
			Assert.IsNull (result, "Entry N Empty");
		}

		[Test]
		public void Intersect_Unrestricted ()
		{
			// Intersection with unrestricted == Copy
			// a. source (this) is unrestricted
			ServiceControllerPermission scp1 = new ServiceControllerPermission (PermissionState.Unrestricted);
			ServiceControllerPermission scp2 = new ServiceControllerPermission (PermissionState.None);

			// 1. Unrestricted N None
			ServiceControllerPermission result = (ServiceControllerPermission)scp1.Intersect (scp2);
			Assert.IsFalse (result.IsUnrestricted (), "(Unrestricted N None).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(Unrestricted N None).Count");

			// 2. None N Unrestricted
			result = (ServiceControllerPermission)scp2.Intersect (scp1);
			Assert.IsFalse (result.IsUnrestricted (), "(None N Unrestricted).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(None N Unrestricted).Count");

			// 3. Unrestricted N Unrestricted
			result = (ServiceControllerPermission)scp1.Intersect (scp1);
			Assert.IsTrue (result.IsUnrestricted (), "(Unrestricted N Unrestricted).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(Unrestricted N Unrestricted).Count");

			// 4. Unrestricted N Entry
			scp2.PermissionEntries.Add (new ServiceControllerPermissionEntry (ServiceControllerPermissionAccess.None, "localhost", "http"));
			result = (ServiceControllerPermission)scp1.Intersect (scp2);
			Assert.IsFalse (result.IsUnrestricted (), "(Unrestricted N Entry).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Unrestricted N Entry).Count");

			// 5. Entry N Unrestricted
			result = (ServiceControllerPermission)scp2.Intersect (scp1);
			Assert.IsFalse (result.IsUnrestricted (), "(Entry N Unrestricted).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Entry N Unrestricted).Count");

			// 6. Unrestricted N Unrestricted
			scp1.PermissionEntries.Add (new ServiceControllerPermissionEntry (ServiceControllerPermissionAccess.None, "localhost", "http"));
			result = (ServiceControllerPermission)scp1.Intersect (scp1);
			Assert.IsTrue (result.IsUnrestricted (), "(Unrestricted N Unrestricted).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Unrestricted N Unrestricted).Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_BadPermission ()
		{
			ServiceControllerPermission scp1 = new ServiceControllerPermission (PermissionState.Unrestricted);
			scp1.Intersect (new SecurityPermission (SecurityPermissionFlag.Assertion));
		}

		[Test]
		public void IsSubset_Null ()
		{
			ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
			Assert.IsTrue (scp.IsSubsetOf (null), "null");
		}

		[Test]
		public void IsSubset_None ()
		{
			// IsSubset with none
			// a. source (this) is none -> target is never a subset
			// b. destination (target) is none -> target is always a subset
			ServiceControllerPermission scp1 = new ServiceControllerPermission (PermissionState.None);
			foreach (ServiceControllerPermissionAccess scpa in AllAccess) {
				ServiceControllerPermission scp2 = new ServiceControllerPermission (PermissionState.None);
				scp2.PermissionEntries.Add (new ServiceControllerPermissionEntry (scpa, "localhost", scpa.ToString ()));
				Assert.IsTrue (scp1.IsSubsetOf (scp2), "target " + scpa.ToString ());
				Assert.IsFalse (scp2.IsSubsetOf (scp1), "source " + scpa.ToString ());
			}
		}

		[Test]
		public void IsSubset_Self ()
		{
			foreach (ServiceControllerPermissionAccess scpa in AllAccess) {
				ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
				scp.PermissionEntries.Add (new ServiceControllerPermissionEntry (scpa, "localhost", scpa.ToString ()));
				Assert.IsTrue (scp.IsSubsetOf (scp), scpa.ToString ());
			}
		}

		[Test]
		public void IsSubset_Unrestricted ()
		{
			// IsSubset with unrestricted
			// a. source (this) is unrestricted -> target is never a subset
			// b. destination (target) is unrestricted -> source is always a subset
			ServiceControllerPermission scp1 = new ServiceControllerPermission (PermissionState.Unrestricted);
			foreach (ServiceControllerPermissionAccess scpa in AllAccess) {
				ServiceControllerPermission scp2 = new ServiceControllerPermission (PermissionState.None);
				scp2.PermissionEntries.Add (new ServiceControllerPermissionEntry (scpa, "localhost", scpa.ToString ()));
				Assert.IsFalse (scp1.IsSubsetOf (scp2), "target " + scpa.ToString ());
				Assert.IsTrue (scp2.IsSubsetOf (scp1), "source " + scpa.ToString ());
			}
			Assert.IsTrue (scp1.IsSubsetOf (scp1), "Unrestricted.IsSubsetOf(Unrestricted)");
		}

		[Test]
// "special" behavior inherited from ResourceBasePermission
//		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOf_BadPermission ()
		{
			ServiceControllerPermission scp1 = new ServiceControllerPermission (PermissionState.Unrestricted);
			Assert.IsFalse (scp1.IsSubsetOf (new SecurityPermission (SecurityPermissionFlag.Assertion)));
		}

		[Test]
		public void Union_Null ()
		{
			ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
			scp.PermissionEntries.Add (new ServiceControllerPermissionEntry (ServiceControllerPermissionAccess.None, "localhost", "http"));
			// Union with null is a simple copy
			ServiceControllerPermission union = (ServiceControllerPermission)scp.Union (null);
			Assert.IsNotNull (scp.PermissionEntries.Count, "Count");
		}

		[Test]
		public void Union_None ()
		{
			ServiceControllerPermission scp1 = new ServiceControllerPermission (PermissionState.None);
			foreach (ServiceControllerPermissionAccess scpa in AllAccess) {
				ServiceControllerPermission scp2 = new ServiceControllerPermission (PermissionState.None);
				scp2.PermissionEntries.Add (new ServiceControllerPermissionEntry (scpa, "localhost", scpa.ToString ()));
				ServiceControllerPermission union = (ServiceControllerPermission)scp1.Union (scp2);
				Assert.IsFalse (union.IsUnrestricted (), "target.IsUnrestricted " + scpa.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "target.Count " + scpa.ToString ());

				union = (ServiceControllerPermission)scp2.Union (scp1);
				Assert.IsFalse (union.IsUnrestricted (), "source.IsUnrestricted " + scpa.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "source.Count " + scpa.ToString ());
			}
		}

		[Test]
		public void Union_Self ()
		{
			foreach (ServiceControllerPermissionAccess scpa in AllAccess) {
				ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
				scp.PermissionEntries.Add (new ServiceControllerPermissionEntry (scpa, "localhost", scpa.ToString ()));
				ServiceControllerPermission union = (ServiceControllerPermission)scp.Union (scp);
				Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted " + scpa.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "Count " + scpa.ToString ());
			}
		}

		[Test]
		public void Union_Unrestricted ()
		{
			// Union with unrestricted is unrestricted
			ServiceControllerPermission scp1 = new ServiceControllerPermission (PermissionState.Unrestricted);
			foreach (ServiceControllerPermissionAccess scpa in AllAccess) {
				ServiceControllerPermission scp2 = new ServiceControllerPermission (PermissionState.None);
				scp2.PermissionEntries.Add (new ServiceControllerPermissionEntry (scpa, "localhost", scpa.ToString ()));
				ServiceControllerPermission union = (ServiceControllerPermission)scp1.Union (scp2);
				Assert.IsTrue (union.IsUnrestricted (), "target.IsUnrestricted " + scpa.ToString ());
				Assert.AreEqual (0, union.PermissionEntries.Count, "target.Count " + scpa.ToString ());

				union = (ServiceControllerPermission)scp2.Union (scp1);
				Assert.IsTrue (union.IsUnrestricted (), "source.IsUnrestricted " + scpa.ToString ());
				Assert.AreEqual (0, union.PermissionEntries.Count, "source.Count " + scpa.ToString ());
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_BadPermission ()
		{
			ServiceControllerPermission scp1 = new ServiceControllerPermission (PermissionState.Unrestricted);
			scp1.Union (new SecurityPermission (SecurityPermissionFlag.Assertion));
		}

		[Test]
// Problem inherited from ResourcePermissionBase
//		[ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void FromXml_Null ()
		{
			ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
			scp.FromXml (null);
		}

		[Test]
		public void FromXml_WrongTag ()
		{
			ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
			SecurityElement se = scp.ToXml ();
			se.Tag = "IMono";
			scp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		public void FromXml_WrongTagCase ()
		{
			ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
			SecurityElement se = scp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			scp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
			SecurityElement se = scp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			scp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
			SecurityElement se = scp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			scp.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
			SecurityElement se = scp.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			scp.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			ServiceControllerPermission scp = new ServiceControllerPermission (PermissionState.None);
			SecurityElement se = scp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			scp.FromXml (w);
		}
	}
}
