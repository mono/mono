//
// DirectoryServicesPermissionTest.cs -
//	NUnit Test Cases for DirectoryServicesPermission
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
using System.DirectoryServices;
using System.Security;
using System.Security.Permissions;

using System.Diagnostics;

namespace MonoTests.System.DirectoryServices {

	[TestFixture]
	public class DirectoryServicesPermissionTest {

		static DirectoryServicesPermissionAccess[] AllAccess = {
			DirectoryServicesPermissionAccess.None,
			DirectoryServicesPermissionAccess.Browse,
			DirectoryServicesPermissionAccess.Write,
			DirectoryServicesPermissionAccess.Browse | DirectoryServicesPermissionAccess.Write,
		};

		[Test]
		public void PermissionState_None ()
		{
			PermissionState ps = PermissionState.None;
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (ps);
			Assert.AreEqual (0, dsp.PermissionEntries.Count, "PermissionEntries");
			Assert.IsFalse (dsp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = dsp.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes");
			Assert.IsNull (se.Children, "Xml-Children");

			DirectoryServicesPermission copy = (DirectoryServicesPermission)dsp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (dsp, copy), "ReferenceEquals");
			Assert.AreEqual (dsp.PermissionEntries.Count, copy.PermissionEntries.Count, "copy-PermissionEntries");
			Assert.AreEqual (dsp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		public void PermissionState_Unrestricted ()
		{
			PermissionState ps = PermissionState.Unrestricted;
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (ps);
			Assert.AreEqual (0, dsp.PermissionEntries.Count, "PermissionEntries");
			Assert.IsTrue (dsp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = dsp.ToXml ();
			// only class and version are present
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "Xml-Unrestricted");
			Assert.IsNull (se.Children, "Xml-Children");

			DirectoryServicesPermission copy = (DirectoryServicesPermission)dsp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (dsp, copy), "ReferenceEquals");
			Assert.AreEqual (dsp.PermissionEntries.Count, copy.PermissionEntries.Count, "copy-PermissionEntries");
			Assert.AreEqual (dsp.IsUnrestricted (), copy.IsUnrestricted (), "copy-IsUnrestricted ()");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void PermissionState_Bad ()
		{
			PermissionState ps = (PermissionState)77;
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (ps);
			Assert.IsFalse (dsp.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void DirectoryServicesPermissionAccesss_Bad ()
		{
			DirectoryServicesPermissionAccess dspa = (DirectoryServicesPermissionAccess) Int32.MinValue;
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (dspa, String.Empty);
			Assert.AreEqual (1, dsp.PermissionEntries.Count, "Count");
			Assert.AreEqual ((DirectoryServicesPermissionAccess)Int32.MinValue, dsp.PermissionEntries [0].PermissionAccess, "PermissionAccess");
			Assert.AreEqual (String.Empty, dsp.PermissionEntries [0].Path, "Path");
		}

		[Test]
		public void PermissionEntries () 
		{
			DirectoryServicesPermissionAccess dspa = DirectoryServicesPermissionAccess.None;
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (dspa, String.Empty);
			DirectoryServicesPermissionEntryCollection dspec = dsp.PermissionEntries;
			Assert.AreEqual (1, dspec.Count, "Count==1");

			DirectoryServicesPermissionEntry dspe = new DirectoryServicesPermissionEntry (DirectoryServicesPermissionAccess.Browse, "*");
			dsp.PermissionEntries.Add (dspe);
			Assert.AreEqual (2, dspec.Count, "Count==2");

			// remove (same instance)
			dsp.PermissionEntries.Remove (dspe);
			Assert.AreEqual (1, dspec.Count, "Count==1 (b)");

			// remove different instance (doesn't work)
			dspe = new DirectoryServicesPermissionEntry (DirectoryServicesPermissionAccess.None, String.Empty);
			Assert.AreEqual (1, dspec.Count, "Count==1");
		}

		[Test]
		public void Copy ()
		{
			foreach (DirectoryServicesPermissionAccess ppl in AllAccess) {
				DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
				DirectoryServicesPermissionEntry dspe = new DirectoryServicesPermissionEntry (ppl, ppl.ToString ());
				dsp.PermissionEntries.Add (dspe);
				DirectoryServicesPermission copy = (DirectoryServicesPermission)dsp.Copy ();
				Assert.AreEqual (1, copy.PermissionEntries.Count, "Count==1");
				Assert.AreEqual (ppl, dsp.PermissionEntries [0].PermissionAccess, ppl.ToString ());
				Assert.AreEqual (ppl.ToString (), dsp.PermissionEntries [0].Path, ppl.ToString () + "-Path");
			}
		}

		[Test]
		public void Intersect_Null ()
		{
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
			// No intersection with null
			Assert.IsNull (dsp.Intersect (null), "None N null");
		}

		[Test]
		public void Intersect_None ()
		{
			DirectoryServicesPermission dsp1 = new DirectoryServicesPermission (PermissionState.None);
			DirectoryServicesPermission dsp2 = new DirectoryServicesPermission (PermissionState.None);
			// 1. None N None
			DirectoryServicesPermission result = (DirectoryServicesPermission) dsp1.Intersect (dsp2);
			Assert.IsNull (result, "Empty N Empty");
			// 2. None N Entry
			dsp2.PermissionEntries.Add (new DirectoryServicesPermissionEntry (DirectoryServicesPermissionAccess.None, String.Empty));
			result = (DirectoryServicesPermission) dsp1.Intersect (dsp2);
			Assert.IsNull (result, "Empty N Entry");
			// 3. Entry N None
			result = (DirectoryServicesPermission) dsp2.Intersect (dsp1);
			Assert.IsNull (result, "Entry N Empty");
		}

		[Test]
		public void Intersect_Unrestricted ()
		{
			// Intersection with unrestricted == Copy
			// a. source (this) is unrestricted
			DirectoryServicesPermission dsp1 = new DirectoryServicesPermission (PermissionState.Unrestricted);
			DirectoryServicesPermission dsp2 = new DirectoryServicesPermission (PermissionState.None);

			// 1. Unrestricted N None
			DirectoryServicesPermission result = (DirectoryServicesPermission) dsp1.Intersect (dsp2);
			Assert.IsFalse (result.IsUnrestricted (), "(Unrestricted N None).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(Unrestricted N None).Count");

			// 2. None N Unrestricted
			result = (DirectoryServicesPermission) dsp2.Intersect (dsp1);
			Assert.IsFalse (result.IsUnrestricted (), "(None N Unrestricted).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(None N Unrestricted).Count");

			// 3. Unrestricted N Unrestricted
			result = (DirectoryServicesPermission) dsp1.Intersect (dsp1);
			Assert.IsTrue (result.IsUnrestricted (), "(Unrestricted N Unrestricted).IsUnrestricted");
			Assert.AreEqual (0, result.PermissionEntries.Count, "(Unrestricted N Unrestricted).Count");

			// 4. Unrestricted N Entry
			dsp2.PermissionEntries.Add (new DirectoryServicesPermissionEntry (DirectoryServicesPermissionAccess.None, String.Empty));
			result = (DirectoryServicesPermission)dsp1.Intersect (dsp2);
			Assert.IsFalse (result.IsUnrestricted (), "(Unrestricted N Entry).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Unrestricted N Entry).Count");

			// 5. Entry N Unrestricted
			result = (DirectoryServicesPermission)dsp2.Intersect (dsp1);
			Assert.IsFalse (result.IsUnrestricted (), "(Entry N Unrestricted).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Entry N Unrestricted).Count");

			// 6. Unrestricted N Unrestricted
			dsp1.PermissionEntries.Add (new DirectoryServicesPermissionEntry (DirectoryServicesPermissionAccess.None, String.Empty));
			result = (DirectoryServicesPermission)dsp1.Intersect (dsp1);
			Assert.IsTrue (result.IsUnrestricted (), "(Unrestricted N Unrestricted).IsUnrestricted");
			Assert.AreEqual (1, result.PermissionEntries.Count, "(Unrestricted N Unrestricted).Count");
		}

		[Test]
		public void IsSubset_Null ()
		{
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
#if NET_2_0
			Assert.IsTrue (dsp.IsSubsetOf (null), "null");
#else
			Assert.IsFalse (dsp.IsSubsetOf (null), "null");
#endif
		}

		[Test]
		public void IsSubset_None ()
		{
			// IsSubset with none
			// a. source (this) is none -> target is never a subset
			// b. destination (target) is none -> target is always a subset
			DirectoryServicesPermission dsp1 = new DirectoryServicesPermission (PermissionState.None);
			foreach (DirectoryServicesPermissionAccess ppl in AllAccess) {
				DirectoryServicesPermission dsp2 = new DirectoryServicesPermission (PermissionState.None);
				dsp2.PermissionEntries.Add (new DirectoryServicesPermissionEntry (ppl, ppl.ToString ()));
				Assert.IsTrue (dsp1.IsSubsetOf (dsp2), "target " + ppl.ToString ());
				Assert.IsFalse (dsp2.IsSubsetOf (dsp1), "source " + ppl.ToString ());
			}
		}

		[Test]
		public void IsSubset_Self ()
		{
			foreach (DirectoryServicesPermissionAccess ppl in AllAccess) {
				DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
				dsp.PermissionEntries.Add (new DirectoryServicesPermissionEntry (ppl, ppl.ToString ()));
				Assert.IsTrue (dsp.IsSubsetOf (dsp), ppl.ToString ());
			}
		}

		[Test]
		public void IsSubset_Unrestricted ()
		{
			// IsSubset with unrestricted
			// a. source (this) is unrestricted -> target is never a subset
			// b. destination (target) is unrestricted -> source is always a subset
			DirectoryServicesPermission dsp1 = new DirectoryServicesPermission (PermissionState.Unrestricted);
			foreach (DirectoryServicesPermissionAccess ppl in AllAccess) {
				DirectoryServicesPermission dsp2 = new DirectoryServicesPermission (PermissionState.None);
				dsp2.PermissionEntries.Add (new DirectoryServicesPermissionEntry (ppl, ppl.ToString ()));
				Assert.IsFalse (dsp1.IsSubsetOf (dsp2), "target " + ppl.ToString ());
				Assert.IsTrue (dsp2.IsSubsetOf (dsp1), "source " + ppl.ToString ());
			}
			Assert.IsTrue (dsp1.IsSubsetOf (dsp1), "Unrestricted.IsSubsetOf(Unrestricted)");
		}

		[Test]
		public void Union_Null ()
		{
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
			dsp.PermissionEntries.Add (new DirectoryServicesPermissionEntry (DirectoryServicesPermissionAccess.None, String.Empty));
			// Union with null is a simple copy
			DirectoryServicesPermission union = (DirectoryServicesPermission)dsp.Union (null);
			Assert.IsNotNull (dsp.PermissionEntries.Count, "Count");
		}

		[Test]
		public void Union_None ()
		{
			DirectoryServicesPermission dsp1 = new DirectoryServicesPermission (PermissionState.None);
			foreach (DirectoryServicesPermissionAccess ppl in AllAccess) {
				DirectoryServicesPermission dsp2 = new DirectoryServicesPermission (PermissionState.None);
				dsp2.PermissionEntries.Add (new DirectoryServicesPermissionEntry (ppl, ppl.ToString ()));
				DirectoryServicesPermission union = (DirectoryServicesPermission) dsp1.Union (dsp2);
				Assert.IsFalse (union.IsUnrestricted (), "target.IsUnrestricted " + ppl.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "target.Count " + ppl.ToString ());

				union = (DirectoryServicesPermission) dsp2.Union (dsp1);
				Assert.IsFalse (union.IsUnrestricted (), "source.IsUnrestricted " + ppl.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "source.Count " + ppl.ToString ());
			}
		}

		[Test]
		public void Union_Self ()
		{
			foreach (DirectoryServicesPermissionAccess ppl in AllAccess) {
				DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
				dsp.PermissionEntries.Add (new DirectoryServicesPermissionEntry (ppl, ppl.ToString ()));
				DirectoryServicesPermission union = (DirectoryServicesPermission)dsp.Union (dsp);
				Assert.IsFalse (union.IsUnrestricted (), "IsUnrestricted " + ppl.ToString ());
				Assert.AreEqual (1, union.PermissionEntries.Count, "Count " + ppl.ToString ());
			}
		}

		[Test]
		public void Union_Unrestricted ()
		{
			// Union with unrestricted is unrestricted
			DirectoryServicesPermission dsp1 = new DirectoryServicesPermission (PermissionState.Unrestricted);
			foreach (DirectoryServicesPermissionAccess ppl in AllAccess) {
				DirectoryServicesPermission dsp2 = new DirectoryServicesPermission (PermissionState.None);
				dsp2.PermissionEntries.Add (new DirectoryServicesPermissionEntry (ppl, ppl.ToString ()));
				DirectoryServicesPermission union = (DirectoryServicesPermission)dsp1.Union (dsp2);
				Assert.IsTrue (union.IsUnrestricted (), "target.IsUnrestricted " + ppl.ToString ());
				Assert.AreEqual (0, union.PermissionEntries.Count, "target.Count " + ppl.ToString ());

				union = (DirectoryServicesPermission)dsp2.Union (dsp1);
				Assert.IsTrue (union.IsUnrestricted (), "source.IsUnrestricted " + ppl.ToString ());
				Assert.AreEqual (0, union.PermissionEntries.Count, "source.Count " + ppl.ToString ());
			}
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
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
			dsp.FromXml (null);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void FromXml_WrongTag ()
		{
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
			SecurityElement se = dsp.ToXml ();
			se.Tag = "IMono";
			dsp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void FromXml_WrongTagCase ()
		{
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
			SecurityElement se = dsp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			dsp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
			SecurityElement se = dsp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			dsp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
			SecurityElement se = dsp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			dsp.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
			SecurityElement se = dsp.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			dsp.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			DirectoryServicesPermission dsp = new DirectoryServicesPermission (PermissionState.None);
			SecurityElement se = dsp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			dsp.FromXml (w);
		}
	}
}
