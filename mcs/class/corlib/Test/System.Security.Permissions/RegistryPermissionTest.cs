//
// RegistryPermissionTest.cs - NUnit Test Cases for RegistryPermission
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
	public class RegistryPermissionTest	{

		private static string className = "System.Security.Permissions.RegistryPermission, ";
		private static string keyCurrentUser = @"HKEY_CURRENT_USER\Software\Novell iFolder\spouliot\Home";
		private static string keyCurrentUserSubset = @"HKEY_CURRENT_USER\Software\Novell iFolder\";
		private static string keyLocalMachine = @"HKEY_LOCAL_MACHINE\SOFTWARE\Novell\Novell iFolder\1.00.000";
		private static string keyLocalMachineSubset = @"HKEY_LOCAL_MACHINE\SOFTWARE\Novell\";

		[Test]
		public void PermissionStateNone ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			Assert.IsNotNull (ep, "RegistryPermission(PermissionState.None)");
			Assert.IsTrue (!ep.IsUnrestricted (), "IsUnrestricted");
			RegistryPermission copy = (RegistryPermission)ep.Copy ();
			Assert.AreEqual (ep.IsUnrestricted (), copy.IsUnrestricted (), "Copy.IsUnrestricted");
			SecurityElement se = ep.ToXml ();
			Assert.IsTrue (se.Attribute ("class").StartsWith (className), "ToXml-class");
			Assert.AreEqual ("1", se.Attribute ("version"), "ToXml-version");
		}

		[Test]
		public void PermissionStateUnrestricted ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.Unrestricted);
			Assert.IsNotNull (ep, "RegistryPermission(PermissionState.Unrestricted)");
			Assert.IsTrue (ep.IsUnrestricted (), "IsUnrestricted");
			RegistryPermission copy = (RegistryPermission)ep.Copy ();
			Assert.AreEqual (ep.IsUnrestricted (), copy.IsUnrestricted (), "Copy.IsUnrestricted");
			SecurityElement se = ep.ToXml ();
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "ToXml-Unrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullPathList ()
		{
			RegistryPermission ep = new RegistryPermission (RegistryPermissionAccess.AllAccess, null);
		}

		[Test]
		public void AllAccess ()
		{
			RegistryPermission ep = new RegistryPermission (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			Assert.IsTrue (!ep.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void NoAccess ()
		{
			RegistryPermission ep = new RegistryPermission (RegistryPermissionAccess.NoAccess, keyLocalMachine);
			Assert.IsTrue (!ep.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void CreateAccess ()
		{
			RegistryPermission ep = new RegistryPermission (RegistryPermissionAccess.Create, keyLocalMachine);
			Assert.IsTrue (!ep.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void ReadAccess ()
		{
			RegistryPermission ep = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			Assert.IsTrue (!ep.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void WriteAccess ()
		{
			RegistryPermission ep = new RegistryPermission (RegistryPermissionAccess.Write, keyLocalMachine);
			Assert.IsTrue (!ep.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void AddPathList ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			ep.AddPathList (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			// LAMESPEC NoAccess do not remove the keyLocalMachine from AllAccess
			ep.AddPathList (RegistryPermissionAccess.NoAccess, keyLocalMachine);
			ep.AddPathList (RegistryPermissionAccess.Read, keyCurrentUser);
			ep.AddPathList (RegistryPermissionAccess.Write, keyCurrentUser);
			SecurityElement se = ep.ToXml ();
			// Note: Debugger can mess results (try to run without stepping)
			Assert.AreEqual (@"HKEY_LOCAL_MACHINE\SOFTWARE\Novell\Novell iFolder\1.00.000", se.Attribute ("Create"), "AddPathList-ToXml-Create");
			Assert.AreEqual (@"HKEY_LOCAL_MACHINE\SOFTWARE\Novell\Novell iFolder\1.00.000;HKEY_CURRENT_USER\Software\Novell iFolder\spouliot\Home", se.Attribute ("Read"), "AddPathList-ToXml-Read");
			Assert.AreEqual (@"HKEY_LOCAL_MACHINE\SOFTWARE\Novell\Novell iFolder\1.00.000;HKEY_CURRENT_USER\Software\Novell iFolder\spouliot\Home", se.Attribute ("Write"), "AddPathList-ToXml-Write");
		}

		[Test]
		public void AddPathList_Subset ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			ep.AddPathList (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			ep.AddPathList (RegistryPermissionAccess.AllAccess, keyLocalMachineSubset);
			SecurityElement se = ep.ToXml ();
			Assert.AreEqual (keyLocalMachineSubset, se.Attribute ("Create"), "AddPathList-ToXml-Create");
			Assert.AreEqual (keyLocalMachineSubset, se.Attribute ("Read"), "AddPathList-ToXml-Read");
			Assert.AreEqual (keyLocalMachineSubset, se.Attribute ("Write"), "AddPathList-ToXml-Write");

			ep = new RegistryPermission (PermissionState.None);
			ep.AddPathList (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			ep.AddPathList (RegistryPermissionAccess.Create, keyLocalMachineSubset);
			ep.AddPathList (RegistryPermissionAccess.Read, keyCurrentUser);
			se = ep.ToXml ();
			Assert.AreEqual (keyLocalMachineSubset, se.Attribute ("Create"), "AddPathList-ToXml-Create");
			Assert.AreEqual (keyLocalMachine + ";" + keyCurrentUser, se.Attribute ("Read"), "AddPathList-ToXml-Read");
			Assert.AreEqual (keyLocalMachine, se.Attribute ("Write"), "AddPathList-ToXml-Write");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetPathListAllAccess ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			ep.GetPathList (RegistryPermissionAccess.AllAccess);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetPathListNoAccess ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			ep.AddPathList (RegistryPermissionAccess.Read, keyCurrentUser);
			ep.AddPathList (RegistryPermissionAccess.Write, keyLocalMachine);
			Assert.AreEqual (String.Empty, ep.GetPathList (RegistryPermissionAccess.NoAccess), "GetPathList-NoAccess");
		}

		[Test]
		public void GetPathList ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
#if NET_2_0
			Assert.AreEqual (String.Empty, ep.GetPathList (RegistryPermissionAccess.Create), "GetPathList-Create-Empty");
			Assert.AreEqual (String.Empty, ep.GetPathList (RegistryPermissionAccess.Read), "GetPathList-Read-Empty");
			Assert.AreEqual (String.Empty, ep.GetPathList (RegistryPermissionAccess.Write), "GetPathList-Write-Empty");
#else
			Assert.IsNull (ep.GetPathList (RegistryPermissionAccess.Create), "GetPathList-Create-Empty");
			Assert.IsNull (ep.GetPathList (RegistryPermissionAccess.Read), "GetPathList-Read-Empty");
			Assert.IsNull (ep.GetPathList (RegistryPermissionAccess.Write), "GetPathList-Write-Empty");
#endif
			ep.AddPathList (RegistryPermissionAccess.Create, keyLocalMachine);
			ep.AddPathList (RegistryPermissionAccess.Create, keyCurrentUser);
			Assert.AreEqual (keyLocalMachine + ";" + keyCurrentUser, ep.GetPathList (RegistryPermissionAccess.Create), "GetPathList-Read");

			ep.AddPathList (RegistryPermissionAccess.Read, keyLocalMachine);
			Assert.AreEqual (keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Read), "GetPathList-Read");

			ep.AddPathList (RegistryPermissionAccess.Write, keyCurrentUser);
			Assert.AreEqual (keyCurrentUser, ep.GetPathList (RegistryPermissionAccess.Write), "GetPathList-Write");
		}

		[Test]
		public void SetPathList ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			ep.SetPathList (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			// LAMESPEC NoAccess do not remove the TMP from AllAccess
			ep.SetPathList (RegistryPermissionAccess.NoAccess, keyLocalMachine);
			ep.SetPathList (RegistryPermissionAccess.Read, keyCurrentUser);
			ep.SetPathList (RegistryPermissionAccess.Write, keyCurrentUser);
			SecurityElement se = ep.ToXml ();
			Assert.AreEqual (keyCurrentUser, se.Attribute ("Read"), "SetPathList-ToXml-Read");
			Assert.AreEqual (keyCurrentUser, se.Attribute ("Write"), "SetPathList-ToXml-Write");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlNull ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			ep.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlInvalidPermission ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			SecurityElement se = ep.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement ("IInvalidPermission", se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", se.Attribute ("version"));
			ep.FromXml (se2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlWrongVersion ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			SecurityElement se = ep.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement (se.Tag, se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", "2");
			ep.FromXml (se2);
		}

#if NET_2_0
		[Category ("NotWorking")]
#endif
		[Test]
		public void FromXml ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			SecurityElement se = ep.ToXml ();
			Assert.IsNotNull (se, "ToXml()");
			ep.FromXml (se);
			se.AddAttribute ("Read", keyLocalMachine);
			ep.FromXml (se);
			Assert.AreEqual (keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Read), "FromXml-Read");
			se.AddAttribute ("Write", keyLocalMachine);
			ep.FromXml (se);
			Assert.AreEqual (keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Read), "FromXml-Read");
			Assert.AreEqual (keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Write), "FromXml-Write");
			se.AddAttribute ("Create", keyCurrentUser);
			ep.FromXml (se);
			Assert.AreEqual (keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Read), "FromXml-Read");
			Assert.AreEqual (keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Write), "FromXml-Write");
			Assert.AreEqual (keyCurrentUser, ep.GetPathList (RegistryPermissionAccess.Create), "FromXml-Create");
		}

		[Test]
		public void UnionWithNull ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep2 = null;
			RegistryPermission ep3 = (RegistryPermission)ep1.Union (ep2);
			Assert.AreEqual (ep1.ToXml ().ToString (), ep3.ToXml ().ToString (), "EP1 U null == EP1");
		}

		[Test]
		public void UnionWithUnrestricted ()
		{
			RegistryPermission ep1 = new RegistryPermission (PermissionState.Unrestricted);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep3 = (RegistryPermission)ep1.Union (ep2);
			Assert.IsTrue (ep3.IsUnrestricted (), "Unrestricted U EP2 == Unrestricted");
			ep3 = (RegistryPermission)ep2.Union (ep1);
			Assert.IsTrue (ep3.IsUnrestricted (), "EP2 U Unrestricted == Unrestricted");
		}

#if NET_2_0
		[Category ("NotWorking")]
#endif
		[Test]
		public void Union ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Write, keyLocalMachine);
			RegistryPermission ep3 = new RegistryPermission (RegistryPermissionAccess.Create, keyLocalMachine);
			RegistryPermission ep4 = (RegistryPermission)ep1.Union (ep2);
			ep4 = (RegistryPermission)ep4.Union (ep3);
			RegistryPermission ep5 = new RegistryPermission (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			Assert.AreEqual (ep4.ToXml ().ToString (), ep5.ToXml ().ToString (), "EP1 U EP2 U EP3 == EP1+2+3");
		}

#if NET_2_0
		[Category ("NotWorking")]
#endif
		[Test]
		public void Union_Subset ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Create, keyLocalMachineSubset);
			RegistryPermission ep3 = (RegistryPermission)ep1.Union (ep2);
			Assert.AreEqual (keyLocalMachineSubset, ep3.GetPathList (RegistryPermissionAccess.Create), "Create");
			Assert.AreEqual (keyLocalMachine, ep3.GetPathList (RegistryPermissionAccess.Read), "Read");
			Assert.AreEqual (keyLocalMachine, ep3.GetPathList (RegistryPermissionAccess.Write), "Write");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnionWithBadPermission ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			FileDialogPermission fdp2 = new FileDialogPermission (PermissionState.Unrestricted);
			RegistryPermission ep3 = (RegistryPermission)ep1.Union (fdp2);
		}

		[Test]
		public void IntersectWithNull ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep2 = null;
			RegistryPermission ep3 = (RegistryPermission)ep1.Intersect (ep2);
			Assert.IsNull (ep3, "EP1 N null == null");
		}

		[Test]
		public void IntersectWithUnrestricted ()
		{
			RegistryPermission ep1 = new RegistryPermission (PermissionState.Unrestricted);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep3 = (RegistryPermission)ep1.Intersect (ep2);
			Assert.IsTrue (!ep3.IsUnrestricted (), "Unrestricted N EP2 == EP2");
			Assert.AreEqual (ep2.ToXml ().ToString (), ep3.ToXml ().ToString (), "Unrestricted N EP2 == EP2");
			ep3 = (RegistryPermission)ep2.Intersect (ep1);
			Assert.IsTrue (!ep3.IsUnrestricted (), "EP2 N Unrestricted == EP2");
			Assert.AreEqual (ep2.ToXml ().ToString (), ep3.ToXml ().ToString (), "EP2 N Unrestricted == EP2");
		}

		[Test]
		public void Intersect ()
		{
			// no intersection
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Write, keyCurrentUser);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep3 = (RegistryPermission)ep1.Intersect (ep2);
			Assert.IsNull (ep3, "EP1 N EP2 == null");
			// intersection in read
			RegistryPermission ep4 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			ep3 = (RegistryPermission)ep4.Intersect (ep2);
			Assert.AreEqual (keyLocalMachine, ep3.GetPathList (RegistryPermissionAccess.Read), "Intersect-Read");
			// intersection in write
			RegistryPermission ep5 = new RegistryPermission (RegistryPermissionAccess.Write, keyCurrentUser);
			ep3 = (RegistryPermission)ep5.Intersect (ep1);
			Assert.AreEqual (keyCurrentUser, ep3.GetPathList (RegistryPermissionAccess.Write), "Intersect-Write");
			// intersection in read and write
			RegistryPermission ep6 = new RegistryPermission (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			RegistryPermission ep7 = new RegistryPermission (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			ep3 = (RegistryPermission)ep6.Intersect (ep7);
			Assert.AreEqual (keyLocalMachine, ep3.GetPathList (RegistryPermissionAccess.Create), "Intersect-AllAccess-Create");
			Assert.AreEqual (keyLocalMachine, ep3.GetPathList (RegistryPermissionAccess.Read), "Intersect-AllAccess-Read");
			Assert.AreEqual (keyLocalMachine, ep3.GetPathList (RegistryPermissionAccess.Write), "Intersect-AllAccess-Write");
		}

#if NET_2_0
		[Category ("NotWorking")]
#endif
		[Test]
		public void Intersect_Subset ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Create, keyLocalMachineSubset);
			RegistryPermission ep3 = (RegistryPermission)ep1.Intersect (ep2);
			Assert.AreEqual (keyLocalMachine, ep3.GetPathList (RegistryPermissionAccess.Create), "Create");
			Assert.IsNull (ep3.GetPathList (RegistryPermissionAccess.Read), "Read");
			Assert.IsNull (ep3.GetPathList (RegistryPermissionAccess.Write), "Write");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IntersectWithBadPermission ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			FileDialogPermission fdp2 = new FileDialogPermission (PermissionState.Unrestricted);
			RegistryPermission ep3 = (RegistryPermission)ep1.Intersect (fdp2);
		}

		[Test]
		public void IsSubsetOfNull ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			Assert.IsTrue (!ep1.IsSubsetOf (null), "IsSubsetOf(null)");
		}

		[Test]
		public void IsSubsetOfUnrestricted ()
		{
			RegistryPermission ep1 = new RegistryPermission (PermissionState.Unrestricted);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep3 = new RegistryPermission (PermissionState.Unrestricted);
			Assert.IsTrue (!ep1.IsSubsetOf (ep2), "Unrestricted.IsSubsetOf()");
			Assert.IsTrue (ep2.IsSubsetOf (ep1), "IsSubsetOf(Unrestricted)");
			Assert.IsTrue (ep1.IsSubsetOf (ep3), "Unrestricted.IsSubsetOf(Unrestricted)");
		}

		[Test]
		public void IsSubsetOf ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Write, keyLocalMachine);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			Assert.IsTrue (!ep1.IsSubsetOf (ep2), "IsSubsetOf(nosubset1)");
			Assert.IsTrue (!ep2.IsSubsetOf (ep1), "IsSubsetOf(nosubset2)");
			RegistryPermission ep3 = new RegistryPermission (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			Assert.IsTrue (ep1.IsSubsetOf (ep3), "Write.IsSubsetOf(All)");
			Assert.IsTrue (!ep3.IsSubsetOf (ep1), "All.IsSubsetOf(Write)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOfBadPermission ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			FileDialogPermission fdp2 = new FileDialogPermission (PermissionState.Unrestricted);
			Assert.IsTrue (ep1.IsSubsetOf (fdp2), "IsSubsetOf(FileDialogPermission)");
		}
	}
}
