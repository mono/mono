//
// ResourcePermissionBaseTest.cs - NUnit Test Cases for ResourcePermissionBase
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

	// the publicizer of the protected ;-)
	public class NonAbstractResourcePermissionBase : ResourcePermissionBase {

		public NonAbstractResourcePermissionBase () : base ()
		{
		}

		public NonAbstractResourcePermissionBase (PermissionState state)
			: base (state) 
		{
		}

		public new Type PermissionAccessType {
			get { return base.PermissionAccessType; }
			set { base.PermissionAccessType = value; }
		}

		public new string [] TagNames {
			get { return base.TagNames; }
			set { base.TagNames = value; }
		}

		public new void AddPermissionAccess (ResourcePermissionBaseEntry entry)
		{
			base.AddPermissionAccess (entry);
		}

		public new void Clear ()
		{
			base.Clear ();
		}

		public new ResourcePermissionBaseEntry [] GetPermissionEntries ()
		{
			return base.GetPermissionEntries ();
		}

		public new void RemovePermissionAccess (ResourcePermissionBaseEntry entry)
		{
			base.RemovePermissionAccess (entry);
		}
	}

	[TestFixture]
	public class ResourcePermissionBaseTest {

		[Test]
		public void Constants ()
		{
			Assert.AreEqual ("*", ResourcePermissionBase.Any, "Any");
			Assert.AreEqual (".", ResourcePermissionBase.Local, "Local");
		}

		private void CheckDefaultValues (string msg, NonAbstractResourcePermissionBase rp, bool unrestricted)
		{
			Assert.IsNull (rp.PermissionAccessType, msg + "-PermissionAccessType");
			Assert.IsNull (rp.TagNames, msg + "-TagNames");
			Assert.AreEqual (unrestricted, rp.IsUnrestricted (), msg + "-IsUnrestricted");
			ResourcePermissionBaseEntry[] entries = rp.GetPermissionEntries ();
			Assert.AreEqual (0, entries.Length, msg + "Count");
		}

		[Test]
		public void Constructor_Empty ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			CheckDefaultValues ("original", rp, false);
			NonAbstractResourcePermissionBase copy = (NonAbstractResourcePermissionBase) rp.Copy ();
			CheckDefaultValues ("copy", rp, false);
		}

		[Test]
		public void Constructor_None ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase (PermissionState.None);
			CheckDefaultValues ("original", rp, false);
			NonAbstractResourcePermissionBase copy = (NonAbstractResourcePermissionBase)rp.Copy ();
			CheckDefaultValues ("copy", rp, false);
		}

		[Test]
		public void Constructor_Unrestricted ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase (PermissionState.Unrestricted);
			CheckDefaultValues ("original", rp, true);
			NonAbstractResourcePermissionBase copy = (NonAbstractResourcePermissionBase)rp.Copy ();
			CheckDefaultValues ("copy", rp, true);
		}

		[Test]
		public void Constructor_Invalid ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ((PermissionState)Int32.MaxValue);
			CheckDefaultValues ("original", rp, false);
			NonAbstractResourcePermissionBase copy = (NonAbstractResourcePermissionBase)rp.Copy ();
			CheckDefaultValues ("copy", rp, false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PermissionAccessType_Null ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.PermissionAccessType = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionAccessType_NonEnum ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.PermissionAccessType = typeof (NonAbstractResourcePermissionBase);
		}

		[Test]
		public void PermissionAccessType ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.PermissionAccessType = typeof (PermissionState);
			Assert.AreEqual (typeof (PermissionState), rp.PermissionAccessType, "PermissionAccessType");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TagNames_Null ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TagNames_Length ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [0];
		}

		[Test]
		public void TagNames ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [2] { "Mono", "Novell" };
			Assert.AreEqual (2, rp.TagNames.Length, "TagNames");
			Assert.AreEqual ("Mono", rp.TagNames [0], "TagNames-1");
			Assert.AreEqual ("Novell", rp.TagNames [1], "TagNames-2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddPermissionAccess_Null ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.AddPermissionAccess (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddPermissionAccess_MismatchTag ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [1] { "Monkeys" };
			rp.AddPermissionAccess (new ResourcePermissionBaseEntry ());
		}

		[Test]
		public void AddPermissionAccess ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [1] { "Monkeys" };
			ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.AddPermissionAccess (entry);

			ResourcePermissionBaseEntry[] entries = rp.GetPermissionEntries ();
			Assert.AreEqual (1, entries.Length, "Count");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddPermissionAccess_Duplicates_SameInstance ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [1] { "Monkeys" };
			ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.AddPermissionAccess (entry);
			rp.AddPermissionAccess (entry);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddPermissionAccess_Duplicates_DifferentInstances ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [1] { "Monkeys" };
			ResourcePermissionBaseEntry entry1 = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.AddPermissionAccess (entry1);
			ResourcePermissionBaseEntry entry2 = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.AddPermissionAccess (entry2);
		}

		[Test]
		public void AddPermissionAccess_SemiDuplicates ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [2] { "Monkeys", "Singes" };
			ResourcePermissionBaseEntry entry1 = new ResourcePermissionBaseEntry (0, new string [2] { "1", "2" });
			rp.AddPermissionAccess (entry1);
			ResourcePermissionBaseEntry entry2 = new ResourcePermissionBaseEntry (0, new string [2] { "2", "1" });
			rp.AddPermissionAccess (entry2);
		}

		[Test]
		public void Clear ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [1] { "Monkeys" };

			ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.AddPermissionAccess (entry);
			ResourcePermissionBaseEntry [] entries = rp.GetPermissionEntries ();
			Assert.AreEqual (1, entries.Length, "Count");

			rp.Clear ();
			entries = rp.GetPermissionEntries ();
			Assert.AreEqual (0, entries.Length, "Count");
		}

		[Test]
		public void Copy ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.PermissionAccessType = typeof (PermissionState);
			rp.TagNames = new string [1] { "Monkeys" };

			ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.AddPermissionAccess (entry);

			NonAbstractResourcePermissionBase copy = (NonAbstractResourcePermissionBase) rp.Copy ();
			Assert.AreEqual (typeof (PermissionState), copy.PermissionAccessType, "PermissionAccessType");
			Assert.AreEqual ("Monkeys", copy.TagNames [0], "TagNames");

			ResourcePermissionBaseEntry [] entries = copy.GetPermissionEntries ();
			Assert.AreEqual (1, entries.Length, "Count");
		}

		[Test]
// MS bug - reported as FDBK15052
//		[ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void FromXml_Null ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.FromXml (null);
		}

		[Test]
		//[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			SecurityElement se = rp.ToXml ();
			se.Tag = "IMono"; // instead of IPermission
			rp.FromXml (se);
		}

		[Test]
		//[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			SecurityElement se = rp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			rp.FromXml (se);
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			SecurityElement se = rp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			rp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			SecurityElement se = rp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			rp.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		//[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			SecurityElement se = rp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			rp.FromXml (w);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			SecurityElement se = rp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			rp.FromXml (w);
		}

		[Test]
		public void GetPermissionEntries ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			ResourcePermissionBaseEntry[] entries = rp.GetPermissionEntries ();
			Assert.AreEqual (0, entries.Length, "Empty");

			rp.PermissionAccessType = typeof (PermissionState);
			rp.TagNames = new string [1] { "Monkeys" };
			ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.AddPermissionAccess (entry);

			entries = rp.GetPermissionEntries ();
			Assert.AreEqual (1, entries.Length, "Count==1");

			rp.Clear ();
			entries = rp.GetPermissionEntries ();
			Assert.AreEqual (0, entries.Length, "Count==0");
		}

		// Intersect

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_DifferentPermissions ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			rp.Intersect (b);
		}

		[Test]
		public void IsSubsetOf_DifferentPermissions ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			// normally (other permissions) this would throw an ArgumentException
			Assert.IsFalse (rp.IsSubsetOf (b));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemovePermissionAccess_Null ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.RemovePermissionAccess (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void RemovePermissionAccess_MismatchTag ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [1] { "Monkeys" };
			rp.RemovePermissionAccess (new ResourcePermissionBaseEntry ());
		}

		[Test]
		public void RemovePermissionAccess ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [1] { "Monkeys" };

			ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.AddPermissionAccess (entry);

			ResourcePermissionBaseEntry[] entries = rp.GetPermissionEntries ();
			Assert.AreEqual (1, entries.Length, "Count==1");

			rp.RemovePermissionAccess (entry);
			entries = rp.GetPermissionEntries ();
			Assert.AreEqual (0, entries.Length, "Count==0");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void RemovePermissionAccess_Unexisting ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [1] { "Monkeys" };

			ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.RemovePermissionAccess (entry);
		}

		private void CheckCommonXml (string msg, SecurityElement se)
		{
			Assert.AreEqual ("IPermission", se.Tag, msg + "Tag");
			Assert.IsTrue (se.Attribute ("class").StartsWith ("MonoTests.System.Security.Permissions.NonAbstractResourcePermissionBase, "), msg + "class");
			Assert.AreEqual ("1", se.Attribute ("version"), msg + "version");
		}

		[Test]
		public void ToXml ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			rp.TagNames = new string [1] { "Monkeys" };
			SecurityElement se = rp.ToXml ();
			CheckCommonXml ("empty", se);
			Assert.AreEqual (2, se.Attributes.Count, "#attributes");
			Assert.IsNull (se.Children, "Children");

			ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.AddPermissionAccess (entry);
			se = rp.ToXml ();
			CheckCommonXml ("one", se);
			Assert.AreEqual (2, se.Attributes.Count, "#attributes");
			Assert.AreEqual (1, se.Children.Count, "1-Children");
			SecurityElement child = (SecurityElement) se.Children [0];
			Assert.AreEqual ("Monkeys", child.Tag, "Monkeys");
			Assert.AreEqual ("Singes", child.Attribute ("name"), "Singes");
		}

		[Test]
		public void ToXml_Unrestricted ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase (PermissionState.Unrestricted);
			rp.TagNames = new string [1] { "Monkeys" };
			SecurityElement se = rp.ToXml ();
			CheckCommonXml ("empty", se);
			Assert.AreEqual (3, se.Attributes.Count, "#attributes");
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "Unrestricted");
			Assert.IsNull (se.Children, "Children");

			ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.AddPermissionAccess (entry);
			se = rp.ToXml ();
			CheckCommonXml ("one", se);
			Assert.AreEqual (3, se.Attributes.Count, "#attributes");
			// not XML output of childrens for unrestricted
			Assert.IsNull (se.Children, "Children");
		}

		[Test]
		public void Union ()
		{
			NonAbstractResourcePermissionBase a = new NonAbstractResourcePermissionBase ();
			NonAbstractResourcePermissionBase z = (NonAbstractResourcePermissionBase) a.Union (null);
			CheckDefaultValues ("Empty U null", z, false);
			Assert.IsFalse (Object.ReferenceEquals (a, z), "!ReferenceEquals1");

			NonAbstractResourcePermissionBase b = new NonAbstractResourcePermissionBase (PermissionState.None);
			z = (NonAbstractResourcePermissionBase) a.Union (b);
			Assert.IsNull (z, "Empty U Empty");

			NonAbstractResourcePermissionBase u = new NonAbstractResourcePermissionBase (PermissionState.Unrestricted);
			z = (NonAbstractResourcePermissionBase) u.Union (b);
			CheckDefaultValues ("Unrestricted U Empty", z, true);
			Assert.IsFalse (Object.ReferenceEquals (u, z), "!ReferenceEquals2");
			Assert.IsFalse (Object.ReferenceEquals (b, z), "!ReferenceEquals3");

			z = (NonAbstractResourcePermissionBase)b.Union (u);
			CheckDefaultValues ("Empty U Unrestricted", z, true);
			Assert.IsFalse (Object.ReferenceEquals (u, z), "!ReferenceEquals4");
			Assert.IsFalse (Object.ReferenceEquals (b, z), "!ReferenceEquals5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_DifferentPermissions ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase ();
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			rp.Union (b);
		}

		[Test]
		public void Unrestricted_AddRemove ()
		{
			NonAbstractResourcePermissionBase rp = new NonAbstractResourcePermissionBase (PermissionState.Unrestricted);
			rp.TagNames = new string [1] { "Monkeys" };

			ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry (0, new string [1] { "Singes" });
			rp.AddPermissionAccess (entry);

			ResourcePermissionBaseEntry [] entries = rp.GetPermissionEntries ();
			Assert.AreEqual (1, entries.Length, "Count==1");

			rp.RemovePermissionAccess (entry);
			entries = rp.GetPermissionEntries ();
			Assert.AreEqual (0, entries.Length, "Count==0");
		}
	}
}
