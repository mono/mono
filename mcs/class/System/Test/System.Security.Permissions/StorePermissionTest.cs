//
// StorePermissionTest.cs - NUnit Test Cases for StorePermissionTest
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class StorePermissionTest {

		[Test]
		public void PermissionState_None ()
		{
			PermissionState ps = PermissionState.None;
			StorePermission sp = new StorePermission (ps);
			Assert.AreEqual (StorePermissionFlags.NoFlags, sp.Flags, "Flags");
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = sp.ToXml ();
			// only class and version are present
			Assert.AreEqual ("NoFlags", se.Attribute ("Flags"), "Xml-Flags");
			Assert.IsNull (se.Children, "Xml-Children");

			StorePermission copy = (StorePermission) sp.Copy ();
			Assert.IsNull (copy, "Copy");
		}

		[Test]
		[Ignore ("2.0 final bug reported as FDBK40928")]
		public void PermissionState_None_Copy ()
		{
			// both will return null under 2.0 final
			// StorePermission sp1 = new StorePermission (PermissionState.None).Copy();
			// StorePermission sp2 = new StorePermission (StorePermissionFlags.NoFlags).Copy ();
			StorePermission sp = new StorePermission (PermissionState.None);

			StorePermission copy = (StorePermission) sp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (sp, copy), "ReferenceEquals");
			Assert.AreEqual (sp.Flags, copy.Flags, "Copy Flags");
			Assert.AreEqual (sp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		public void PermissionState_Unrestricted ()
		{
			PermissionState ps = PermissionState.Unrestricted;
			StorePermission sp = new StorePermission (ps);
			Assert.AreEqual (StorePermissionFlags.AllFlags, sp.Flags, "Flags");
			Assert.IsTrue (sp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = sp.ToXml ();
			Assert.IsNotNull (se.Attribute ("Unrestricted"), "Xml-Unrestricted");
			Assert.IsNull (se.Attribute ("Level"), "Xml-Flags");
			Assert.IsNull (se.Children, "Xml-Children");

			StorePermission copy = (StorePermission) sp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (sp, copy), "ReferenceEquals");
			Assert.AreEqual (sp.Flags, copy.Flags, "Copy Flags");
			Assert.AreEqual (sp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionState_Bad ()
		{
			PermissionState ps = (PermissionState) Int32.MinValue;
			StorePermission sp = new StorePermission (ps);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void StorePermissionFlags_Bad ()
		{
			StorePermissionFlags spf = (StorePermissionFlags) Int32.MinValue;
			StorePermission sp = new StorePermission (spf);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void StorePermissionFlags_BadEight ()
		{
			StorePermissionFlags spf = (StorePermissionFlags) 8; // unassigned
			StorePermission sp = new StorePermission (spf);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Flags_StorePermissionFlags_Bad ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			sp.Flags = (StorePermissionFlags) Int32.MinValue;
		}

		[Test]
		public void Copy ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			// i==0 would return null for MS
			for (int i=1; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp.Flags = (StorePermissionFlags) i;
				StorePermission copy = (StorePermission) sp.Copy ();
				Assert.AreEqual (i, (int) copy.Flags, sp.Flags.ToString ());
			}
		}

		[Test]
		public void Intersect_Null ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			// No intersection with null
			for (int i = 0; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp.Flags = (StorePermissionFlags) i;
				Assert.IsNull (sp.Intersect (null), sp.Flags.ToString ());
			}
		}

		[Test]
		public void Intersect_None ()
		{
			StorePermission sp1 = new StorePermission (PermissionState.None);
			StorePermission sp2 = new StorePermission (PermissionState.None);
			for (int i = 0; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp1.Flags = (StorePermissionFlags) i;
				// 1. Intersect None with ppl
				StorePermission result = (StorePermission) sp1.Intersect (sp2);
				Assert.IsNull (result, "None N " + sp1.Flags.ToString ());
				// 2. Intersect ppl with None
				result = (StorePermission) sp2.Intersect (sp1);
				Assert.IsNull (result, sp1.Flags.ToString () + "N None");
			}
		}

		[Test]
		public void Intersect_Self ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			sp.Flags = StorePermissionFlags.NoFlags; // 0
			StorePermission result = (StorePermission) sp.Intersect (sp);
			Assert.IsNull (result, "NoFlags");
			for (int i = 1; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp.Flags = (StorePermissionFlags) i;
				result = (StorePermission) sp.Intersect (sp);
				Assert.AreEqual (sp.Flags, result.Flags, sp.Flags.ToString ());
			}
		}

		[Test]
		public void Intersect_Unrestricted ()
		{
			// Intersection with unrestricted == Copy
			// a. source (this) is unrestricted
			StorePermission sp1 = new StorePermission (PermissionState.Unrestricted);
			StorePermission sp2 = new StorePermission (PermissionState.None);
			StorePermission result = (StorePermission) sp1.Intersect (sp2);
			Assert.IsNull (result, "target NoFlags");
			for (int i = 1; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp2.Flags = (StorePermissionFlags) i;
				result = (StorePermission) sp1.Intersect (sp2);
				Assert.AreEqual (sp2.Flags, result.Flags, "target " + sp2.Flags.ToString ());
			}
			// b. destination (target) is unrestricted
			sp2.Flags = StorePermissionFlags.NoFlags;
			result = (StorePermission) sp2.Intersect (sp1);
			Assert.IsNull (result, "target NoFlags");
			for (int i = 1; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp2.Flags = (StorePermissionFlags) i;
				result = (StorePermission) sp2.Intersect (sp1);
				Assert.AreEqual (sp2.Flags, result.Flags, "source " + sp2.Flags.ToString ());
			}
		}

		[Test]
		public void IsSubset_Null ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			Assert.IsTrue (sp.IsSubsetOf (null), "NoFlags"); // 0
			for (int i = 1; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp.Flags = (StorePermissionFlags) i;
				Assert.IsFalse (sp.IsSubsetOf (null), sp.Flags.ToString ());
			}
		}

		[Test]
		public void IsSubset_None ()
		{
			// IsSubset with none
			// a. source (this) is none -> target is never a subset
			StorePermission sp1 = new StorePermission (PermissionState.None);
			StorePermission sp2 = new StorePermission (PermissionState.None);
			for (int i = 0; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp2.Flags = (StorePermissionFlags) i;
				Assert.IsTrue (sp1.IsSubsetOf (sp2), "target " + sp2.Flags.ToString ());
			}
			// b. destination (target) is none -> target is always a subset
			for (int i = 1; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp2.Flags = (StorePermissionFlags) i;
				Assert.IsFalse (sp2.IsSubsetOf (sp1), "source " + sp2.Flags.ToString ());
			}
			// exception of NoFlags
			sp2.Flags = StorePermissionFlags.NoFlags;
			Assert.IsTrue (sp2.IsSubsetOf (sp1), "source NoFlags");
		}

		[Test]
		public void IsSubset_Self ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			for (int i = 1; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp.Flags = (StorePermissionFlags) i;
				Assert.IsTrue (sp.IsSubsetOf (sp), sp.Flags.ToString ());
			}
		}

		[Test]
		public void IsSubset_Unrestricted ()
		{
			// IsSubset with unrestricted
			// a. source (this) is unrestricted -> target is never a subset
			StorePermission sp1 = new StorePermission (PermissionState.Unrestricted);
			StorePermission sp2 = new StorePermission (PermissionState.None);
			for (int i = 0; i < (int) StorePermissionFlags.AllFlags - 1; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp2.Flags = (StorePermissionFlags) i;
				Assert.IsFalse (sp1.IsSubsetOf (sp2), "target " + sp2.Flags.ToString ());
			}
			// exception of AllLevel
			sp2.Flags = StorePermissionFlags.AllFlags;
			Assert.IsTrue (sp1.IsSubsetOf (sp2), "target AllLevel");
			// b. destination (target) is unrestricted -> target is always a subset
			for (int i = 0; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp2.Flags = (StorePermissionFlags) i;
				Assert.IsTrue (sp2.IsSubsetOf (sp1), "source " + sp2.Flags.ToString ());
			}
		}

		[Test]
		public void Union_Null ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			// Union with null is a simple copy
			for (int i = 1; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp.Flags = (StorePermissionFlags) i;
				StorePermission union = (StorePermission) sp.Union (null);
				Assert.AreEqual (sp.Flags, union.Flags, sp.Flags.ToString ());
			}
			// except fot NoFlags (which returns null)
			sp.Flags = StorePermissionFlags.NoFlags;
			Assert.IsNull (sp.Union (null), "NoFlags");
		}

		[Test]
		public void Union_None ()
		{
			// Union with none is same
			StorePermission sp1 = new StorePermission (PermissionState.None);
			StorePermission sp2 = new StorePermission (PermissionState.None);

			StorePermission union = (StorePermission) sp1.Union (sp1);
			Assert.IsNull (union, "NoFlags");

			for (int i = 1; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp2.Flags = (StorePermissionFlags) i;

				union = (StorePermission) sp1.Union (sp2);
				Assert.IsFalse (union.IsUnrestricted (), "target.Unrestricted " + sp2.Flags.ToString ());
				Assert.AreEqual (sp2.Flags, union.Flags, "target.Level " + sp2.Flags.ToString ());

				union = (StorePermission) sp2.Union (sp1);
				Assert.IsFalse (union.IsUnrestricted (), "source.Unrestricted " + sp2.Flags.ToString ());
				Assert.AreEqual (sp2.Flags, union.Flags, "source.Level " + sp2.Flags.ToString ());
			}

			sp2.Flags = StorePermissionFlags.AllFlags;
			union = (StorePermission) sp1.Union (sp2);
			Assert.IsTrue (union.IsUnrestricted (), "target.Unrestricted Unrestricted");
			Assert.AreEqual (StorePermissionFlags.AllFlags, union.Flags, "target.Level Unrestricted");

			union = (StorePermission) sp2.Union (sp1);
			Assert.IsTrue (union.IsUnrestricted (), "source.Unrestricted Unrestricted");
			Assert.AreEqual (StorePermissionFlags.AllFlags, union.Flags, "source.Level Unrestricted");
		}

		[Test]
		public void Union_Self ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			StorePermission union = (StorePermission) sp.Union (sp);
			Assert.IsNull (union, "NoFlags");
			for (int i = 1; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp.Flags = (StorePermissionFlags) i;
				union = (StorePermission) sp.Union (sp);
				Assert.AreEqual (sp.Flags, union.Flags, sp.Flags.ToString ());
			}
		}

		[Test]
		public void Union_Unrestricted ()
		{
			// Union with unrestricted is unrestricted
			StorePermission sp1 = new StorePermission (PermissionState.Unrestricted);
			StorePermission sp2 = new StorePermission (PermissionState.None);
			// a. source (this) is unrestricted
			for (int i = 0; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp2.Flags = (StorePermissionFlags) i;
				StorePermission union = (StorePermission) sp1.Union (sp2);
				Assert.IsTrue (union.IsUnrestricted (), "target " + sp2.Flags.ToString ());
			}
			// b. destination (target) is unrestricted
			for (int i = 0; i < (int) StorePermissionFlags.AllFlags; i++) {
				// 8 isn't a valid value (so we exclude it from the rest of the loop)
				if ((i & 8) == 8)
					continue;
				sp2.Flags = (StorePermissionFlags) i;
				StorePermission union = (StorePermission) sp2.Union (sp1);
				Assert.IsTrue (union.IsUnrestricted (), "source " + sp2.Flags.ToString ());
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			sp.FromXml (null);
		}

		[Test]
		public void FromXml_WrongTag ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();
			se.Tag = "IMono";
			sp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		public void FromXml_WrongTagCase ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			sp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			sp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_NoClass ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			sp.FromXml (w);
			// note: normally IPermission classes (in corlib) DO NOT care about
			// attribute "class" name presence in the XML
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			sp.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			StorePermission sp = new StorePermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			sp.FromXml (w);
			// version is optional (in this case)
		}
	}
}

#endif
