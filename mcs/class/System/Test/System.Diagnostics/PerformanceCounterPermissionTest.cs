//
// PerformanceCounterPermissionTest.cs -
//	NUnit Test Cases for PerformanceCounterPermission
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

#if !MOBILE

using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Diagnostics {

	[TestFixture]
	public class PerformanceCounterPermissionTest {

		static PerformanceCounterPermissionAccess[] AllAccess = {
			PerformanceCounterPermissionAccess.None,
			PerformanceCounterPermissionAccess.Browse,
#if NET_2_0
			PerformanceCounterPermissionAccess.Read,
			PerformanceCounterPermissionAccess.Write,
#endif
			PerformanceCounterPermissionAccess.Instrument,
			PerformanceCounterPermissionAccess.Administer,
		};

		[Test]
		public void PermissionState_None ()
		{
			PermissionState ps = PermissionState.None;
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (ps);
			Assert.AreEqual (0, pcp.PermissionEntries.Count, "PermissionEntries");
			Assert.IsFalse (pcp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = pcp.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes");
			Assert.IsNull (se.Children, "Xml-Children");

			PerformanceCounterPermission copy = (PerformanceCounterPermission)pcp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (pcp, copy), "ReferenceEquals");
			Assert.AreEqual (pcp.PermissionEntries.Count, copy.PermissionEntries.Count, "copy-PermissionEntries");
			Assert.AreEqual (pcp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		public void PermissionState_Unrestricted ()
		{
			PermissionState ps = PermissionState.Unrestricted;
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (ps);
			Assert.AreEqual (0, pcp.PermissionEntries.Count, "PermissionEntries");
			Assert.IsTrue (pcp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = pcp.ToXml ();
			// only class and version are present
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "Xml-Unrestricted");
			Assert.IsNull (se.Children, "Xml-Children");

			PerformanceCounterPermission copy = (PerformanceCounterPermission)pcp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (pcp, copy), "ReferenceEquals");
			Assert.AreEqual (pcp.PermissionEntries.Count, copy.PermissionEntries.Count, "copy-PermissionEntries");
			Assert.AreEqual (pcp.IsUnrestricted (), copy.IsUnrestricted (), "copy-IsUnrestricted ()");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void PermissionState_Bad ()
		{
			PermissionState ps = (PermissionState)77;
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (ps);
			Assert.IsFalse (pcp.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_PermissionEntries_Null ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (null);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void Constructor_MachineName_Null ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PerformanceCounterPermissionAccess.None, null, String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_CategoryName_Null ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PerformanceCounterPermissionAccess.None, "localhost", null);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void PerformanceCounterPermissionAccesss_Bad ()
		{
			PerformanceCounterPermissionAccess pcpa = (PerformanceCounterPermissionAccess)Int32.MinValue;
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (pcpa, "localhost", String.Empty);
			Assert.AreEqual (1, pcp.PermissionEntries.Count, "Count");
			Assert.AreEqual ((PerformanceCounterPermissionAccess)Int32.MinValue, pcp.PermissionEntries [0].PermissionAccess, "PermissionAccess");
		}

		[Test]
		public void PermissionEntries ()
		{
			PerformanceCounterPermissionAccess pcpa = PerformanceCounterPermissionAccess.None;
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (pcpa, "localhost", String.Empty);
			PerformanceCounterPermissionEntryCollection pcpec = pcp.PermissionEntries;
			Assert.AreEqual (1, pcpec.Count, "Count==1");

			PerformanceCounterPermissionEntry pcpe = new PerformanceCounterPermissionEntry (PerformanceCounterPermissionAccess.Browse, "*", String.Empty);
			pcp.PermissionEntries.Add (pcpe);
			Assert.AreEqual (2, pcpec.Count, "Count==2");

			// remove (same instance)
			pcp.PermissionEntries.Remove (pcpe);
			Assert.AreEqual (1, pcpec.Count, "Count==1 (b)");

			// remove different instance (doesn't work)
			pcpe = new PerformanceCounterPermissionEntry (PerformanceCounterPermissionAccess.None, "localhost", String.Empty);
			Assert.AreEqual (1, pcpec.Count, "Count==1");
		}

		[Test]
		public void Copy ()
		{
			foreach (PerformanceCounterPermissionAccess pcpa in AllAccess) {
				PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
				PerformanceCounterPermissionEntry pcpe = new PerformanceCounterPermissionEntry (pcpa, pcpa.ToString (), String.Empty);
				pcp.PermissionEntries.Add (pcpe);
				PerformanceCounterPermission copy = (PerformanceCounterPermission)pcp.Copy ();
				Assert.AreEqual (1, copy.PermissionEntries.Count, "Count==1");
				Assert.AreEqual (pcpa, pcp.PermissionEntries [0].PermissionAccess, pcpa.ToString ());
			}
		}

		[Test]
		public void Intersect_Null ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
			// No intersection with null
			Assert.IsNull (pcp.Intersect (null), "None N null");
		}

		[Test]
		public void Intersect_None ()
		{
			PerformanceCounterPermission pcp1 = new PerformanceCounterPermission (PermissionState.None);
			PerformanceCounterPermission pcp2 = new PerformanceCounterPermission (PermissionState.None);
			// 1. None N None
			PerformanceCounterPermission result = (PerformanceCounterPermission)pcp1.Intersect (pcp2);
			Assert.IsNull (result, "Empty N Empty");
			// 2. None N Entry
			pcp2.PermissionEntries.Add (new PerformanceCounterPermissionEntry (PerformanceCounterPermissionAccess.None, "localhost", String.Empty));
			result = (PerformanceCounterPermission)pcp1.Intersect (pcp2);
			Assert.IsNull (result, "Empty N Entry");
			// 3. Entry N None
			result = (PerformanceCounterPermission)pcp2.Intersect (pcp1);
			Assert.IsNull (result, "Entry N Empty");
		}

		[Test]
		public void Intersect_Unrestricted ()
		{
			// Intersection with unrestricted == Copy
			// a. source (this) is unrestricted
			PerformanceCounterPermission pcp1 = new PerformanceCounterPermission (PermissionState.Unrestricted);
			PerformanceCounterPermission pcp2 = new PerformanceCounterPermission (PermissionState.None);

			// 1. Unrestricted N None
			PerformanceCounterPermission result = (PerformanceCounterPermission)pcp1.Intersect (pcp2);
			Assert.IsFalse (result.IsUnrestricted (), "(Unrestricted N None).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(Unrestricted N None).Count");

			// 2. None N Unrestricted
			result = (PerformanceCounterPermission)pcp2.Intersect (pcp1);
			Assert.IsFalse (result.IsUnrestricted (), "(None N Unrestricted).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(None N Unrestricted).Count");

			// 3. Unrestricted N Unrestricted
			result = (PerformanceCounterPermission)pcp1.Intersect (pcp1);
			Assert.IsTrue (result.IsUnrestricted (), "(Unrestricted N Unrestricted).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(Unrestricted N Unrestricted).Count");

			// 4. Unrestricted N Entry
			pcp2.PermissionEntries.Add (new PerformanceCounterPermissionEntry (PerformanceCounterPermissionAccess.None, "localhost", String.Empty));
			result = (PerformanceCounterPermission)pcp1.Intersect (pcp2);
			Assert.IsFalse (result.IsUnrestricted (), "(Unrestricted N Entry).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Unrestricted N Entry).Count");

			// 5. Entry N Unrestricted
			result = (PerformanceCounterPermission)pcp2.Intersect (pcp1);
			Assert.IsFalse (result.IsUnrestricted (), "(Entry N Unrestricted).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Entry N Unrestricted).Count");

			// 6. Unrestricted N Unrestricted
			pcp1.PermissionEntries.Add (new PerformanceCounterPermissionEntry (PerformanceCounterPermissionAccess.None, "localhost", String.Empty));
			result = (PerformanceCounterPermission)pcp1.Intersect (pcp1);
			Assert.IsTrue (result.IsUnrestricted (), "(Unrestricted N Unrestricted).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Unrestricted N Unrestricted).Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_BadPermission ()
		{
			PerformanceCounterPermission pcp1 = new PerformanceCounterPermission (PermissionState.Unrestricted);
			pcp1.Intersect (new SecurityPermission (SecurityPermissionFlag.Assertion));
		}

		[Test]
		public void IsSubset_Null ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
#if NET_2_0
			Assert.IsTrue (pcp.IsSubsetOf (null), "null");
#else
			Assert.IsFalse (pcp.IsSubsetOf (null), "null");
#endif
		}

		[Test]
		public void IsSubset_None ()
		{
			// IsSubset with none
			// a. source (this) is none -> target is never a subset
			// b. destination (target) is none -> target is always a subset
			PerformanceCounterPermission pcp1 = new PerformanceCounterPermission (PermissionState.None);
			foreach (PerformanceCounterPermissionAccess pcpa in AllAccess) {
				PerformanceCounterPermission pcp2 = new PerformanceCounterPermission (PermissionState.None);
				pcp2.PermissionEntries.Add (new PerformanceCounterPermissionEntry (pcpa, pcpa.ToString (), String.Empty));
				Assert.IsTrue (pcp1.IsSubsetOf (pcp2), "target " + pcpa.ToString ());
				Assert.IsFalse (pcp2.IsSubsetOf (pcp1), "source " + pcpa.ToString ());
			}
		}

		[Test]
		public void IsSubset_Self ()
		{
			foreach (PerformanceCounterPermissionAccess pcpa in AllAccess) {
				PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
				pcp.PermissionEntries.Add (new PerformanceCounterPermissionEntry (pcpa, pcpa.ToString (), String.Empty));
				Assert.IsTrue (pcp.IsSubsetOf (pcp), pcpa.ToString ());
			}
		}

		[Test]
		public void IsSubset_Unrestricted ()
		{
			// IsSubset with unrestricted
			// a. source (this) is unrestricted -> target is never a subset
			// b. destination (target) is unrestricted -> source is always a subset
			PerformanceCounterPermission pcp1 = new PerformanceCounterPermission (PermissionState.Unrestricted);
			foreach (PerformanceCounterPermissionAccess pcpa in AllAccess) {
				PerformanceCounterPermission pcp2 = new PerformanceCounterPermission (PermissionState.None);
				pcp2.PermissionEntries.Add (new PerformanceCounterPermissionEntry (pcpa, pcpa.ToString (), String.Empty));
				Assert.IsFalse (pcp1.IsSubsetOf (pcp2), "target " + pcpa.ToString ());
				Assert.IsTrue (pcp2.IsSubsetOf (pcp1), "source " + pcpa.ToString ());
			}
			Assert.IsTrue (pcp1.IsSubsetOf (pcp1), "Unrestricted.IsSubsetOf(Unrestricted)");
		}

		[Test]
// "special" behavior inherited from ResourceBasePermission
//		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOf_BadPermission ()
		{
			PerformanceCounterPermission pcp1 = new PerformanceCounterPermission (PermissionState.Unrestricted);
			Assert.IsFalse (pcp1.IsSubsetOf (new SecurityPermission (SecurityPermissionFlag.Assertion)));
		}

		[Test]
		public void Union_Null ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
			pcp.PermissionEntries.Add (new PerformanceCounterPermissionEntry (PerformanceCounterPermissionAccess.None, "localhost", String.Empty));
			// Union with null is a simple copy
			PerformanceCounterPermission union = (PerformanceCounterPermission)pcp.Union (null);
			Assert.IsNotNull (pcp.PermissionEntries.Count, "Count");
		}

		[Test]
		public void Union_None ()
		{
			PerformanceCounterPermission pcp1 = new PerformanceCounterPermission (PermissionState.None);
			foreach (PerformanceCounterPermissionAccess pcpa in AllAccess) {
				PerformanceCounterPermission pcp2 = new PerformanceCounterPermission (PermissionState.None);
				pcp2.PermissionEntries.Add (new PerformanceCounterPermissionEntry (pcpa, pcpa.ToString (), String.Empty));
				PerformanceCounterPermission union = (PerformanceCounterPermission)pcp1.Union (pcp2);
				Assert.IsFalse (union.IsUnrestricted (), "target.IsUnrestricted " + pcpa.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "target.Count " + pcpa.ToString ());

				union = (PerformanceCounterPermission)pcp2.Union (pcp1);
				Assert.IsFalse (union.IsUnrestricted (), "source.IsUnrestricted " + pcpa.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "source.Count " + pcpa.ToString ());
			}
		}

		[Test]
		public void Union_Self ()
		{
			foreach (PerformanceCounterPermissionAccess pcpa in AllAccess) {
				PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
				pcp.PermissionEntries.Add (new PerformanceCounterPermissionEntry (pcpa, pcpa.ToString (), String.Empty));
				PerformanceCounterPermission union = (PerformanceCounterPermission)pcp.Union (pcp);
				Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted " + pcpa.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "Count " + pcpa.ToString ());
			}
		}

		[Test]
		public void Union_Unrestricted ()
		{
			// Union with unrestricted is unrestricted
			PerformanceCounterPermission pcp1 = new PerformanceCounterPermission (PermissionState.Unrestricted);
			foreach (PerformanceCounterPermissionAccess pcpa in AllAccess) {
				PerformanceCounterPermission pcp2 = new PerformanceCounterPermission (PermissionState.None);
				pcp2.PermissionEntries.Add (new PerformanceCounterPermissionEntry (pcpa, pcpa.ToString (), String.Empty));
				PerformanceCounterPermission union = (PerformanceCounterPermission)pcp1.Union (pcp2);
				Assert.IsTrue (union.IsUnrestricted (), "target.IsUnrestricted " + pcpa.ToString ());
				Assert.AreEqual (0, union.PermissionEntries.Count, "target.Count " + pcpa.ToString ());

				union = (PerformanceCounterPermission)pcp2.Union (pcp1);
				Assert.IsTrue (union.IsUnrestricted (), "source.IsUnrestricted " + pcpa.ToString ());
				Assert.AreEqual (0, union.PermissionEntries.Count, "source.Count " + pcpa.ToString ());
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_BadPermission ()
		{
			PerformanceCounterPermission pcp1 = new PerformanceCounterPermission (PermissionState.Unrestricted);
			pcp1.Union (new SecurityPermission (SecurityPermissionFlag.Assertion));
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		// Problem inherited from ResourcePermissionBase
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void FromXml_Null ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
			pcp.FromXml (null);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void FromXml_WrongTag ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
			SecurityElement se = pcp.ToXml ();
			se.Tag = "IMono";
			pcp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void FromXml_WrongTagCase ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
			SecurityElement se = pcp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			pcp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
			SecurityElement se = pcp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			pcp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
			SecurityElement se = pcp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			pcp.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
			SecurityElement se = pcp.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			pcp.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			PerformanceCounterPermission pcp = new PerformanceCounterPermission (PermissionState.None);
			SecurityElement se = pcp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			pcp.FromXml (w);
		}
	}
}

#endif
