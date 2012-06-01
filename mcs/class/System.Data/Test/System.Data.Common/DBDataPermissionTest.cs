//
// DBDataPermissionTest.cs - NUnit Test Cases for DBDataPermission
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
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;
using System.IO;
namespace MonoTests.System.Data.Common {

	public class NonAbstractDBDataPermission : DBDataPermission {

#if !NET_2_0
		public NonAbstractDBDataPermission () 
			: base ()
		{
		}

		public NonAbstractDBDataPermission (DBDataPermission permission, bool allowBlankPassword)
			: base (permission)
		{
			AllowBlankPassword = allowBlankPassword;
		}
#else
		// make Copy and CreateInstance work :)
		public NonAbstractDBDataPermission () 
			: base (PermissionState.None)
		{
		}
#endif
		public NonAbstractDBDataPermission (PermissionState state)
			: base (state)
		{
		}

		public NonAbstractDBDataPermission (DBDataPermission permission)
			: base (permission)
		{
		}

		public NonAbstractDBDataPermission (DBDataPermissionAttribute permissionAttribute)
			: base (permissionAttribute)
		{
		}

		public new void Clear ()
		{
			base.Clear ();
		}

		public new DBDataPermission CreateInstance ()
		{
			return base.CreateInstance ();
		}
	}

	[TestFixture]
	public class DBDataPermissionTest {

		private const string defaultConnectString = "Data Source=localhost;Integrated Security=SSPI;Initial Catalog=Northwind;";
		private const string defaultConnectString2 = "Data Source=127.0.0.1;Integrated Security=SSPI;Initial Catalog=Northwind;";

		private void Check (string msg, NonAbstractDBDataPermission dbdp, bool blank, bool unrestricted, int count)
		{
			Assert.AreEqual (blank, dbdp.AllowBlankPassword, msg + ".AllowBlankPassword");
			Assert.AreEqual (unrestricted, dbdp.IsUnrestricted (), msg + ".IsUnrestricted");
			if (count == 0)
				Assert.IsNull (dbdp.ToXml ().Children, msg + ".Count != 0");
			else
				Assert.AreEqual (count, dbdp.ToXml ().Children.Count, msg + ".Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_DBDataPermission_Null ()
		{
			DBDataPermission p = null;
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (p);
		}

		[Test]
		public void Constructor_DBDataPermission ()
		{
			DBDataPermission p = new NonAbstractDBDataPermission (PermissionState.None);
			p.AllowBlankPassword = true;
			p.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);

			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (p);
			Check ("DBDataPermission", dbdp, true, false, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_DBDataPermissionAttribute_Null ()
		{
			DBDataPermissionAttribute a = null;
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (a);
		}

		[Test]
		public void Constructor_DBDataPermissionAttribute ()
		{
			DBDataPermissionAttribute a = new NonAbstractDBDataPermissionAttribute (SecurityAction.Assert);
			a.AllowBlankPassword = true;

			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (a);
			Check ("DBDataPermissionAttribute-0", dbdp, true, false, 0);

			a.Unrestricted = true;
			dbdp = new NonAbstractDBDataPermission (a);
			Check ("DBDataPermissionAttribute-1", dbdp, false, true, 0);
			// Unrestricted "bypass" the AllowBlankPassword (so it report false)

			a.ConnectionString = defaultConnectString;
			dbdp = new NonAbstractDBDataPermission (a);
			Check ("DBDataPermissionAttribute-2", dbdp, false, true, 0);
			// Unrestricted "bypass" the ConnectionString (so it report 0 childs)

			a.Unrestricted = false;
			dbdp = new NonAbstractDBDataPermission (a);
			Check ("DBDataPermissionAttribute-3", dbdp, true, false, 1);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void Constructor_PermissionState_Invalid ()
		{
			PermissionState ps = (PermissionState) Int32.MinValue;
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (ps);
		}

		[Test]
		public void Constructor_PermissionState_None ()
		{
			PermissionState ps = PermissionState.None;
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (ps);
			Check ("PermissionState_None-1", dbdp, false, false, 0);
			dbdp.AllowBlankPassword = true;
			Check ("PermissionState_None-1", dbdp, true, false, 0);
		}

		[Test]
		public void Constructor_PermissionState_Unrestricted ()
		{
			PermissionState ps = PermissionState.Unrestricted;
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (ps);
			Check ("PermissionState_Unrestricted-1", dbdp, false, true, 0);
			dbdp.AllowBlankPassword = true;
			Check ("PermissionState_Unrestricted-2", dbdp, true, true, 0);
		}

		[Test]
		public void AllowBlankPassword ()
		{
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (PermissionState.None);
			Assert.IsFalse (dbdp.AllowBlankPassword, "Default");
			dbdp.AllowBlankPassword = true;
			Assert.IsTrue (dbdp.AllowBlankPassword, "True");
			dbdp.Clear ();
			// clear the connection list - not the permission itself
			Assert.IsTrue (dbdp.AllowBlankPassword, "Clear");
			dbdp.AllowBlankPassword = false;
			Assert.IsFalse (dbdp.AllowBlankPassword, "False");
		}

		[Test]
		public void Add ()
		{
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (PermissionState.None);
			dbdp.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			Assert.AreEqual (1, dbdp.ToXml ().Children.Count, "Count");

			NonAbstractDBDataPermission copy = (NonAbstractDBDataPermission)dbdp.Copy ();
			Assert.AreEqual (1, copy.ToXml ().Children.Count, "Copy.Count");

			dbdp.Clear ();
			Assert.IsNull (dbdp.ToXml ().Children, "Clear");
			Assert.AreEqual (1, copy.ToXml ().Children.Count, "Copy.Count-2");
		}

		[Test]
		public void Add_Duplicates ()
		{
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (PermissionState.None);
			dbdp.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			dbdp.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			// no exception but a single element is kept
			Assert.AreEqual (1, dbdp.ToXml ().Children.Count, "Count");
			dbdp.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.PreventUsage);

			dbdp.Clear ();
			Assert.IsNull (dbdp.ToXml ().Children, "Clear");
		}

		[Test]
		public void Add_Differents ()
		{
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (PermissionState.None);
			dbdp.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			string connectString = "Data Source=127.0.0.1;Integrated Security=SSPI;Initial Catalog=Northwind;";
			dbdp.Add (connectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			Assert.AreEqual (2, dbdp.ToXml ().Children.Count, "Count");

			dbdp.Clear ();
			Assert.IsNull (dbdp.ToXml ().Children, "Clear");
		}

		[Test]
		public void Add_Unrestricted ()
		{
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (PermissionState.Unrestricted);
			Assert.IsTrue (dbdp.IsUnrestricted (), "IsUnrestricted-1");
			// we lose unrestricted by adding an element
			dbdp.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			Assert.IsFalse (dbdp.IsUnrestricted (), "IsUnrestricted-2");
			// removing the element doesn't regain unrestricted status
			dbdp.Clear ();
			Assert.IsFalse (dbdp.IsUnrestricted (), "IsUnrestricted-3");
		}

		[Test]
		public void CreateInstance ()
		{
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (PermissionState.Unrestricted);
			Assert.AreEqual (typeof (NonAbstractDBDataPermission), dbdp.CreateInstance ().GetType (), "Same type"); 
		}


		[Test]
		public void Intersect_Null ()
		{
			NonAbstractDBDataPermission elp = new NonAbstractDBDataPermission (PermissionState.None);
			// No intersection with null
			Assert.IsNull (elp.Intersect (null), "None N null");
		}

		[Test]
		public void Intersect ()
		{
			NonAbstractDBDataPermission dbdp1 = new NonAbstractDBDataPermission (PermissionState.None);
			NonAbstractDBDataPermission dbdp2 = new NonAbstractDBDataPermission (PermissionState.None);
			
			// 1. None N None
			NonAbstractDBDataPermission result = (NonAbstractDBDataPermission) dbdp1.Intersect (dbdp2);
			Assert.IsNull (result, "Empty N Empty");
			
			// 2. None N Entry
			dbdp2.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			result = (NonAbstractDBDataPermission)dbdp1.Intersect (dbdp2);
			Assert.IsNull (result, "Empty N Entry");

			// 3. Entry N None
			result = (NonAbstractDBDataPermission)dbdp2.Intersect (dbdp1);
			Assert.IsNull (result, "Entry N Empty");

			// 4. Unrestricted N None
			NonAbstractDBDataPermission unr = new NonAbstractDBDataPermission (PermissionState.Unrestricted);
			result = (NonAbstractDBDataPermission)unr.Intersect (dbdp1);
			Check ("(Unrestricted N None)", result, false, false, 0);

			// 5. None N Unrestricted
			result = (NonAbstractDBDataPermission)dbdp1.Intersect (unr);
			Check ("(None N Unrestricted)", result, false, false, 0);

			// 6. Unrestricted N Unrestricted
			result = (NonAbstractDBDataPermission)unr.Intersect (unr);
			Check ("(Unrestricted N Unrestricted)", result, false, true, 0);

			// 7. Unrestricted N Entry
			result = (NonAbstractDBDataPermission)unr.Intersect (dbdp2);
			Check ("(Unrestricted N Entry)", result, false, false, 1);

			// 8. Entry N Unrestricted
			result = (NonAbstractDBDataPermission)dbdp2.Intersect (unr);
			Check ("(Entry N Unrestricted)", result, false, false, 1);

			// 9. Entry2 N Entry2
			result = (NonAbstractDBDataPermission)dbdp2.Intersect (dbdp2);
			Check ("(Entry2 N Entry2)", result, false, false, 1);

			// 10. Entry1 N Entry 2
			dbdp1.Add (defaultConnectString2, String.Empty, KeyRestrictionBehavior.PreventUsage);
			result = (NonAbstractDBDataPermission)dbdp1.Intersect (dbdp2);
			Assert.IsNull (result, "(Entry1 N Entry2)");

			// 11. Entry2 N Entry 1
			result = (NonAbstractDBDataPermission)dbdp2.Intersect (dbdp1);
			Assert.IsNull (result, "(Entry2 N Entry1)");
		}

		[Test]
		public void Intersect_AllowBlankPassword ()
		{
			NonAbstractDBDataPermission ptrue = new NonAbstractDBDataPermission (PermissionState.None);
			ptrue.AllowBlankPassword = true;
			ptrue.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			NonAbstractDBDataPermission pfalse = new NonAbstractDBDataPermission (PermissionState.None);
			pfalse.AllowBlankPassword = false;
			pfalse.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);

			NonAbstractDBDataPermission intersect = (NonAbstractDBDataPermission)ptrue.Intersect (ptrue);
			Check ("true N true", intersect, true, false, 1);
			intersect = (NonAbstractDBDataPermission)ptrue.Intersect (pfalse);
			Check ("true N false", intersect, false, false, 1);
			intersect = (NonAbstractDBDataPermission)pfalse.Intersect (ptrue);
			Check ("false N true", intersect, false, false, 1);
			intersect = (NonAbstractDBDataPermission)pfalse.Intersect (pfalse);
			Check ("false N false", intersect, false, false, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_BadPermission ()
		{
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (PermissionState.Unrestricted);
			dbdp.Intersect (new SecurityPermission (SecurityPermissionFlag.Assertion));
		}

		[Test]
		public void IsSubset_Null ()
		{
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (PermissionState.None);
			Assert.IsTrue (dbdp.IsSubsetOf (null), "Empty-null");

			dbdp.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			Assert.IsFalse (dbdp.IsSubsetOf (null), "Element-null");

			dbdp = new NonAbstractDBDataPermission (PermissionState.Unrestricted);
			Assert.IsFalse (dbdp.IsSubsetOf (null), "Unrestricted-null");
		}

		[Test]
		public void IsSubset ()
		{
			NonAbstractDBDataPermission empty = new NonAbstractDBDataPermission (PermissionState.None);
			Assert.IsTrue (empty.IsSubsetOf (empty), "Empty-Empty");

			NonAbstractDBDataPermission dbdp1 = new NonAbstractDBDataPermission (PermissionState.None);
			dbdp1.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			Assert.IsTrue (empty.IsSubsetOf (dbdp1), "Empty-1");
			Assert.IsFalse (dbdp1.IsSubsetOf (empty), "1-Empty");
			Assert.IsTrue (dbdp1.IsSubsetOf (dbdp1), "1-1");

			NonAbstractDBDataPermission dbdp2 = (NonAbstractDBDataPermission)dbdp1.Copy ();
			dbdp2.Add (defaultConnectString2, String.Empty, KeyRestrictionBehavior.AllowOnly);
			Assert.IsTrue (dbdp1.IsSubsetOf (dbdp2), "1-2");
			Assert.IsFalse (dbdp2.IsSubsetOf (dbdp1), "2-1");
			Assert.IsTrue (dbdp2.IsSubsetOf (dbdp2), "2-2");

			NonAbstractDBDataPermission dbdp3 = new NonAbstractDBDataPermission (PermissionState.None);
			dbdp3.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.PreventUsage);
			Assert.IsTrue (dbdp3.IsSubsetOf (dbdp1), "3-1");
			Assert.IsTrue (dbdp1.IsSubsetOf (dbdp3), "1-3");
			Assert.IsTrue (dbdp3.IsSubsetOf (dbdp3), "3-3");

			NonAbstractDBDataPermission unr = new NonAbstractDBDataPermission (PermissionState.Unrestricted);
			Assert.IsTrue (dbdp1.IsSubsetOf (unr), "1-unrestricted");
			Assert.IsFalse (unr.IsSubsetOf (dbdp1), "unrestricted-1");
			Assert.IsTrue (dbdp2.IsSubsetOf (unr), "2-unrestricted");
			Assert.IsFalse (unr.IsSubsetOf (dbdp2), "unrestricted-2");
			Assert.IsTrue (dbdp3.IsSubsetOf (unr), "3-unrestricted");
			Assert.IsFalse (unr.IsSubsetOf (dbdp3), "unrestricted-3");
			Assert.IsTrue (unr.IsSubsetOf (unr), "unrestricted-unrestricted");
		}

		[Test]
		public void IsSubsetOf_AllowBlankPassword ()
		{
			NonAbstractDBDataPermission ptrue = new NonAbstractDBDataPermission (PermissionState.None);
			ptrue.AllowBlankPassword = true;
			ptrue.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			NonAbstractDBDataPermission pfalse = new NonAbstractDBDataPermission (PermissionState.None);
			pfalse.AllowBlankPassword = false;
			pfalse.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);

			Assert.IsTrue (ptrue.IsSubsetOf (ptrue), "true subsetof true");
			Assert.IsFalse (ptrue.IsSubsetOf (pfalse), "true subsetof false");
			Assert.IsTrue (pfalse.IsSubsetOf (ptrue), "false subsetof true");
			Assert.IsTrue (pfalse.IsSubsetOf (pfalse), "false subsetof false");
		}

		[Test]
		public void IsSubsetOf_BothEmpty_KeyRestrictionBehavior ()
		{
			NonAbstractDBDataPermission pAllow = new NonAbstractDBDataPermission (PermissionState.None);
			pAllow.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			NonAbstractDBDataPermission pPrevent = new NonAbstractDBDataPermission (PermissionState.None);
			pPrevent.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.PreventUsage);

			Assert.IsTrue (pAllow.IsSubsetOf (pAllow), "BothEmpty - pAllow subsetof pAllow");
			Assert.IsTrue (pAllow.IsSubsetOf (pPrevent), "BothEmpty - pAllow subsetof pPrevent");
			Assert.IsTrue (pPrevent.IsSubsetOf (pAllow), "BothEmpty - pPrevent subsetof pAllow");
			Assert.IsTrue (pPrevent.IsSubsetOf (pPrevent), "BothEmpty - pPrevent subsetof pPrevent");
		}

		[Test]
		public void IsSubsetOf_EmptyAllow_Prevent_KeyRestrictionBehavior ()
		{
			NonAbstractDBDataPermission pAllow = new NonAbstractDBDataPermission (PermissionState.None);
			pAllow.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			NonAbstractDBDataPermission pPrevent = new NonAbstractDBDataPermission (PermissionState.None);
			pPrevent.Add (defaultConnectString, "password=;", KeyRestrictionBehavior.PreventUsage);

			Assert.IsTrue (pAllow.IsSubsetOf (pAllow), "EmptyAllow_Prevent - pAllow subsetof pAllow");
			Assert.IsTrue (pAllow.IsSubsetOf (pPrevent), "EmptyAllow_Prevent - pAllow subsetof pPrevent");
			Assert.IsTrue (pPrevent.IsSubsetOf (pAllow), "EmptyAllow_Prevent - pPrevent subsetof pAllow");
			Assert.IsTrue (pPrevent.IsSubsetOf (pPrevent), "EmptyAllow_Prevent - pPrevent subsetof pPrevent");
		}

		[Test]
		public void IsSubsetOf_Allow_EmptyPrevent_KeyRestrictionBehavior ()
		{
			NonAbstractDBDataPermission pAllow = new NonAbstractDBDataPermission (PermissionState.None);
			pAllow.Add (defaultConnectString, "data source=;", KeyRestrictionBehavior.AllowOnly);
			NonAbstractDBDataPermission pPrevent = new NonAbstractDBDataPermission (PermissionState.None);
			pPrevent.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.PreventUsage);

			Assert.IsTrue (pAllow.IsSubsetOf (pAllow), "Allow_EmptyPrevent - pAllow subsetof pAllow");
			Assert.IsTrue (pAllow.IsSubsetOf (pPrevent), "Allow_EmptyPrevent - pAllow subsetof pPrevent");
			Assert.IsTrue (pPrevent.IsSubsetOf (pAllow), "Allow_EmptyPrevent - pPrevent subsetof pAllow");
			Assert.IsTrue (pPrevent.IsSubsetOf (pPrevent), "Allow_EmptyPrevent - pPrevent subsetof pPrevent");
		}

		[Test]
		public void IsSubsetOf_AllowPreventSame_KeyRestrictionBehavior ()
		{
			NonAbstractDBDataPermission pAllow = new NonAbstractDBDataPermission (PermissionState.None);
			pAllow.Add (defaultConnectString, "password=;", KeyRestrictionBehavior.AllowOnly);
			NonAbstractDBDataPermission pPrevent = new NonAbstractDBDataPermission (PermissionState.None);
			pPrevent.Add (defaultConnectString, "password=;", KeyRestrictionBehavior.PreventUsage);

			Assert.IsTrue (pAllow.IsSubsetOf (pAllow), "AllowPreventSame - pAllow subsetof pAllow");
			Assert.IsTrue (pAllow.IsSubsetOf (pPrevent), "AllowPreventSame - pAllow subsetof pPrevent");
			Assert.IsTrue (pPrevent.IsSubsetOf (pAllow), "AllowPreventSame - pPrevent subsetof pAllow");
			Assert.IsTrue (pPrevent.IsSubsetOf (pPrevent), "AllowPreventSame - pPrevent subsetof pPrevent");
		}

		[Test]
		public void IsSubsetOf_AllowPreventDifferent_KeyRestrictionBehavior ()
		{
			NonAbstractDBDataPermission pAllow1 = new NonAbstractDBDataPermission (PermissionState.None);
			pAllow1.Add (defaultConnectString, "security=;", KeyRestrictionBehavior.AllowOnly);
			NonAbstractDBDataPermission pAllow2 = new NonAbstractDBDataPermission (PermissionState.None);
			pAllow2.Add (defaultConnectString, "password=;", KeyRestrictionBehavior.AllowOnly);
			NonAbstractDBDataPermission pPrevent1 = new NonAbstractDBDataPermission (PermissionState.None);
			pPrevent1.Add (defaultConnectString, "security=;", KeyRestrictionBehavior.PreventUsage);
			NonAbstractDBDataPermission pPrevent2 = new NonAbstractDBDataPermission (PermissionState.None);
			pPrevent2.Add (defaultConnectString, "password=;", KeyRestrictionBehavior.PreventUsage);

			Assert.IsTrue (pAllow1.IsSubsetOf (pAllow1), "AllowPreventDifferent - pAllow subsetof pAllow");
			Assert.IsTrue (pAllow1.IsSubsetOf (pPrevent2), "AllowPreventDifferent - pAllow subsetof pPrevent");
			Assert.IsTrue (pPrevent1.IsSubsetOf (pAllow2), "AllowPreventDifferent - pPrevent subsetof pAllow");
			Assert.IsTrue (pPrevent1.IsSubsetOf (pPrevent2), "AllowPreventDifferent - pPrevent subsetof pPrevent");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOf_BadPermission ()
		{
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (PermissionState.Unrestricted);
			dbdp.IsSubsetOf (new SecurityPermission (SecurityPermissionFlag.Assertion));
		}

		[Test]
		public void Union_Null ()
		{
			NonAbstractDBDataPermission dbdp = new NonAbstractDBDataPermission (PermissionState.None);
			NonAbstractDBDataPermission union = (NonAbstractDBDataPermission) dbdp.Union (null);
			Check ("Empty U null", dbdp, false, false, 0);

			dbdp.AllowBlankPassword = true;
			dbdp.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			union = (NonAbstractDBDataPermission) dbdp.Union (null);
			Check ("Element U null", dbdp, true, false, 1);

			dbdp = new NonAbstractDBDataPermission (PermissionState.Unrestricted);
			union = (NonAbstractDBDataPermission) dbdp.Union (null);
			Check ("Unrestricted U null", dbdp, false, true, 0);
		}

		[Test]
		public void Union ()
		{
			NonAbstractDBDataPermission empty = new NonAbstractDBDataPermission (PermissionState.None);
			NonAbstractDBDataPermission union = (NonAbstractDBDataPermission) empty.Union (empty);
			Assert.IsNotNull (union, "Empty U Empty");

			NonAbstractDBDataPermission dbdp1 = new NonAbstractDBDataPermission (PermissionState.None);
			dbdp1.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			union = (NonAbstractDBDataPermission) empty.Union (dbdp1);
			Check ("Empty U 1", union, false, false, 1);

			NonAbstractDBDataPermission dbdp2 = (NonAbstractDBDataPermission)dbdp1.Copy ();
			dbdp2.Add (defaultConnectString2, String.Empty, KeyRestrictionBehavior.AllowOnly);
			union = (NonAbstractDBDataPermission) dbdp1.Union (dbdp2);
			Check ("1 U 2", union, false, false, 2);

			NonAbstractDBDataPermission dbdp3 = new NonAbstractDBDataPermission (PermissionState.None);
			dbdp3.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.PreventUsage);
			union = (NonAbstractDBDataPermission) dbdp2.Union (dbdp3);
			Check ("2 U 3", union, false, false, 2);

			NonAbstractDBDataPermission dbdp4 = new NonAbstractDBDataPermission (PermissionState.None);
			dbdp4.Add (defaultConnectString, "Data Source=;", KeyRestrictionBehavior.PreventUsage);
			union = (NonAbstractDBDataPermission) dbdp3.Union (dbdp4);
			Check ("3 U 4", union, false, false, 1);

			NonAbstractDBDataPermission unr = new NonAbstractDBDataPermission (PermissionState.Unrestricted);
			union = (NonAbstractDBDataPermission) unr.Union (empty);
			Check ("unrestricted U empty", union, false, true, 0);

			union = (NonAbstractDBDataPermission)unr.Union (dbdp4);
			Check ("unrestricted U 4", union, false, true, 0);
		}

		[Test]
		public void Union_AllowBlankPassword ()
		{
			NonAbstractDBDataPermission ptrue = new NonAbstractDBDataPermission (PermissionState.None);
			ptrue.AllowBlankPassword = true;
			ptrue.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);
			NonAbstractDBDataPermission pfalse = new NonAbstractDBDataPermission (PermissionState.None);
			pfalse.AllowBlankPassword = false;
			pfalse.Add (defaultConnectString, String.Empty, KeyRestrictionBehavior.AllowOnly);

			NonAbstractDBDataPermission union = (NonAbstractDBDataPermission) ptrue.Union (ptrue);
			Check ("true U true", union, true, false, 1);
			union = (NonAbstractDBDataPermission)ptrue.Union (pfalse);
			Check ("true U false", union, true, false, 1);
			union = (NonAbstractDBDataPermission)pfalse.Union (ptrue);
			Check ("false U true", union, true, false, 1);
			union = (NonAbstractDBDataPermission)pfalse.Union (pfalse);
			Check ("false U false", union, false, false, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_BadPermission ()
		{
			NonAbstractDBDataPermission dbdp1 = new NonAbstractDBDataPermission (PermissionState.Unrestricted);
			dbdp1.Union (new SecurityPermission (SecurityPermissionFlag.Assertion));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			NonAbstractDBDataPermission elp = new NonAbstractDBDataPermission (PermissionState.None);
			elp.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			NonAbstractDBDataPermission elp = new NonAbstractDBDataPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();
			se.Tag = "IMono";
			elp.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			NonAbstractDBDataPermission elp = new NonAbstractDBDataPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			elp.FromXml (se);
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			NonAbstractDBDataPermission elp = new NonAbstractDBDataPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			elp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			NonAbstractDBDataPermission elp = new NonAbstractDBDataPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			elp.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			NonAbstractDBDataPermission elp = new NonAbstractDBDataPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			elp.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			NonAbstractDBDataPermission elp = new NonAbstractDBDataPermission (PermissionState.None);
			SecurityElement se = elp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			elp.FromXml (w);
		}
	}
}
