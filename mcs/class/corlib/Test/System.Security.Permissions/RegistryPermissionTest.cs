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
	public class RegistryPermissionTest : Assertion	{

		private static string className = "System.Security.Permissions.RegistryPermission, ";
		private static string keyCurrentUser = @"HKEY_CURRENT_USER\Software\Novell iFolder\spouliot\Home";
		private static string keyLocalMachine = @"HKEY_LOCAL_MACHINE\SOFTWARE\Novell\Novell iFolder\1.00.000";

		[Test]
		public void PermissionStateNone ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			AssertNotNull ("RegistryPermission(PermissionState.None)", ep);
			Assert ("IsUnrestricted", !ep.IsUnrestricted ());
			RegistryPermission copy = (RegistryPermission)ep.Copy ();
			AssertEquals ("Copy.IsUnrestricted", ep.IsUnrestricted (), copy.IsUnrestricted ());
			SecurityElement se = ep.ToXml ();
			Assert ("ToXml-class", se.Attribute ("class").StartsWith (className));
			AssertEquals ("ToXml-version", "1", se.Attribute ("version"));
		}

		[Test]
		public void PermissionStateUnrestricted ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.Unrestricted);
			AssertNotNull ("RegistryPermission(PermissionState.Unrestricted)", ep);
			Assert ("IsUnrestricted", ep.IsUnrestricted ());
			RegistryPermission copy = (RegistryPermission)ep.Copy ();
			AssertEquals ("Copy.IsUnrestricted", ep.IsUnrestricted (), copy.IsUnrestricted ());
			SecurityElement se = ep.ToXml ();
			AssertEquals ("ToXml-Unrestricted", "true", se.Attribute ("Unrestricted"));
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
			Assert ("IsUnrestricted", !ep.IsUnrestricted ());
		}

		[Test]
		public void NoAccess ()
		{
			RegistryPermission ep = new RegistryPermission (RegistryPermissionAccess.NoAccess, keyLocalMachine);
			Assert ("IsUnrestricted", !ep.IsUnrestricted ());
		}

		[Test]
		public void CreateAccess ()
		{
			RegistryPermission ep = new RegistryPermission (RegistryPermissionAccess.Create, keyLocalMachine);
			Assert ("IsUnrestricted", !ep.IsUnrestricted ());
		}

		[Test]
		public void ReadAccess ()
		{
			RegistryPermission ep = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			Assert ("IsUnrestricted", !ep.IsUnrestricted ());
		}

		[Test]
		public void WriteAccess ()
		{
			RegistryPermission ep = new RegistryPermission (RegistryPermissionAccess.Write, keyLocalMachine);
			Assert ("IsUnrestricted", !ep.IsUnrestricted ());
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
			AssertEquals ("AddPathList-ToXml-Create", @"HKEY_LOCAL_MACHINE\SOFTWARE\Novell\Novell iFolder\1.00.000", se.Attribute ("Create"));
			AssertEquals ("AddPathList-ToXml-Read", @"HKEY_LOCAL_MACHINE\SOFTWARE\Novell\Novell iFolder\1.00.000;HKEY_CURRENT_USER\Software\Novell iFolder\spouliot\Home", se.Attribute ("Read"));
			AssertEquals ("AddPathList-ToXml-Write", @"HKEY_LOCAL_MACHINE\SOFTWARE\Novell\Novell iFolder\1.00.000;HKEY_CURRENT_USER\Software\Novell iFolder\spouliot\Home", se.Attribute ("Write"));
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
			AssertEquals ("GetPathList-NoAccess", String.Empty, ep.GetPathList (RegistryPermissionAccess.NoAccess));
		}

		[Test]
		public void GetPathList ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
#if NET_2_0
			AssertEquals ("GetPathList-Create-Empty", String.Empty, ep.GetPathList (RegistryPermissionAccess.Create));
			AssertEquals ("GetPathList-Read-Empty", String.Empty, ep.GetPathList (RegistryPermissionAccess.Read));
			AssertEquals ("GetPathList-Write-Empty", String.Empty, ep.GetPathList (RegistryPermissionAccess.Write));
#else
			AssertNull ("GetPathList-Create-Empty", ep.GetPathList (RegistryPermissionAccess.Create));
			AssertNull ("GetPathList-Read-Empty", ep.GetPathList (RegistryPermissionAccess.Read));
			AssertNull ("GetPathList-Write-Empty", ep.GetPathList (RegistryPermissionAccess.Write));
#endif
			ep.AddPathList (RegistryPermissionAccess.Create, keyLocalMachine);
			ep.AddPathList (RegistryPermissionAccess.Create, keyCurrentUser);
			AssertEquals ("GetPathList-Read", keyLocalMachine + ";" + keyCurrentUser, ep.GetPathList (RegistryPermissionAccess.Create));

			ep.AddPathList (RegistryPermissionAccess.Read, keyLocalMachine);
			AssertEquals ("GetPathList-Read", keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Read));

			ep.AddPathList (RegistryPermissionAccess.Write, keyCurrentUser);
			AssertEquals ("GetPathList-Write", keyCurrentUser, ep.GetPathList (RegistryPermissionAccess.Write));
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
			AssertEquals ("SetPathList-ToXml-Read", keyCurrentUser, se.Attribute ("Read"));
			AssertEquals ("SetPathList-ToXml-Write", keyCurrentUser, se.Attribute ("Write"));
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

		[Test]
		public void FromXml ()
		{
			RegistryPermission ep = new RegistryPermission (PermissionState.None);
			SecurityElement se = ep.ToXml ();
			AssertNotNull ("ToXml()", se);
			ep.FromXml (se);
			se.AddAttribute ("Read", keyLocalMachine);
			ep.FromXml (se);
			AssertEquals ("FromXml-Read", keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Read));
			se.AddAttribute ("Write", keyLocalMachine);
			ep.FromXml (se);
			AssertEquals ("FromXml-Read", keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Read));
			AssertEquals ("FromXml-Write", keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Write));
			se.AddAttribute ("Create", keyCurrentUser);
			ep.FromXml (se);
			AssertEquals ("FromXml-Read", keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Read));
			AssertEquals ("FromXml-Write", keyLocalMachine, ep.GetPathList (RegistryPermissionAccess.Write));
			AssertEquals ("FromXml-Create", keyCurrentUser, ep.GetPathList (RegistryPermissionAccess.Create));
		}

		[Test]
		public void UnionWithNull ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep2 = null;
			RegistryPermission ep3 = (RegistryPermission)ep1.Union (ep2);
			AssertEquals ("EP1 U null == EP1", ep1.ToXml ().ToString (), ep3.ToXml ().ToString ());
		}

		[Test]
		public void UnionWithUnrestricted ()
		{
			RegistryPermission ep1 = new RegistryPermission (PermissionState.Unrestricted);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep3 = (RegistryPermission)ep1.Union (ep2);
			Assert ("Unrestricted U EP2 == Unrestricted", ep3.IsUnrestricted ());
			ep3 = (RegistryPermission)ep2.Union (ep1);
			Assert ("EP2 U Unrestricted == Unrestricted", ep3.IsUnrestricted ());
		}

		[Test]
		public void Union ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Write, keyLocalMachine);
			RegistryPermission ep3 = new RegistryPermission (RegistryPermissionAccess.Create, keyLocalMachine);
			RegistryPermission ep4 = (RegistryPermission)ep1.Union (ep2);
			ep4 = (RegistryPermission)ep4.Union (ep3);
			RegistryPermission ep5 = new RegistryPermission (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			AssertEquals ("EP1 U EP2 U EP3 == EP1+2+3", ep4.ToXml ().ToString (), ep5.ToXml ().ToString ());
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
			AssertNull ("EP1 N null == null", ep3);
		}

		[Test]
		public void IntersectWithUnrestricted ()
		{
			RegistryPermission ep1 = new RegistryPermission (PermissionState.Unrestricted);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep3 = (RegistryPermission)ep1.Intersect (ep2);
			Assert ("Unrestricted N EP2 == EP2", !ep3.IsUnrestricted ());
			AssertEquals ("Unrestricted N EP2 == EP2", ep2.ToXml ().ToString (), ep3.ToXml ().ToString ());
			ep3 = (RegistryPermission)ep2.Intersect (ep1);
			Assert ("EP2 N Unrestricted == EP2", !ep3.IsUnrestricted ());
			AssertEquals ("EP2 N Unrestricted == EP2", ep2.ToXml ().ToString (), ep3.ToXml ().ToString ());
		}

		[Test]
		public void Intersect ()
		{
			// no intersection
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Write, keyCurrentUser);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep3 = (RegistryPermission)ep1.Intersect (ep2);
			AssertNull ("EP1 N EP2 == null", ep3);
			// intersection in read
			RegistryPermission ep4 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			ep3 = (RegistryPermission)ep4.Intersect (ep2);
			AssertEquals ("Intersect-Read", keyLocalMachine, ep3.GetPathList (RegistryPermissionAccess.Read));
			// intersection in write
			RegistryPermission ep5 = new RegistryPermission (RegistryPermissionAccess.Write, keyCurrentUser);
			ep3 = (RegistryPermission)ep5.Intersect (ep1);
			AssertEquals ("Intersect-Write", keyCurrentUser, ep3.GetPathList (RegistryPermissionAccess.Write));
			// intersection in read and write
			RegistryPermission ep6 = new RegistryPermission (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			RegistryPermission ep7 = new RegistryPermission (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			ep3 = (RegistryPermission)ep6.Intersect (ep7);
			AssertEquals ("Intersect-AllAccess-Create", keyLocalMachine, ep3.GetPathList (RegistryPermissionAccess.Create));
			AssertEquals ("Intersect-AllAccess-Read", keyLocalMachine, ep3.GetPathList (RegistryPermissionAccess.Read));
			AssertEquals ("Intersect-AllAccess-Write", keyLocalMachine, ep3.GetPathList (RegistryPermissionAccess.Write));
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
			Assert ("IsSubsetOf(null)", !ep1.IsSubsetOf (null));
		}

		[Test]
		public void IsSubsetOfUnrestricted ()
		{
			RegistryPermission ep1 = new RegistryPermission (PermissionState.Unrestricted);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			RegistryPermission ep3 = new RegistryPermission (PermissionState.Unrestricted);
			Assert ("Unrestricted.IsSubsetOf()", !ep1.IsSubsetOf (ep2));
			Assert ("IsSubsetOf(Unrestricted)", ep2.IsSubsetOf (ep1));
			Assert ("Unrestricted.IsSubsetOf(Unrestricted)", ep1.IsSubsetOf (ep3));
		}

		[Test]
		public void IsSubsetOf ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Write, keyLocalMachine);
			RegistryPermission ep2 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			Assert ("IsSubsetOf(nosubset1)", !ep1.IsSubsetOf (ep2));
			Assert ("IsSubsetOf(nosubset2)", !ep2.IsSubsetOf (ep1));
			RegistryPermission ep3 = new RegistryPermission (RegistryPermissionAccess.AllAccess, keyLocalMachine);
			Assert ("Write.IsSubsetOf(All)", ep1.IsSubsetOf (ep3));
			Assert ("All.IsSubsetOf(Write)", !ep3.IsSubsetOf (ep1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOfBadPermission ()
		{
			RegistryPermission ep1 = new RegistryPermission (RegistryPermissionAccess.Read, keyLocalMachine);
			FileDialogPermission fdp2 = new FileDialogPermission (PermissionState.Unrestricted);
			Assert ("IsSubsetOf(FileDialogPermission)", ep1.IsSubsetOf (fdp2));
		}
	}
}
