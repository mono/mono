//
// EnvironmentPermissionTest.cs - NUnit Test Cases for EnvironmentPermission
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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
	public class EnvironmentPermissionTest : Assertion {

		private static string className = "System.Security.Permissions.EnvironmentPermission, ";
		private static string envVariables = "TMP;TEMP";

		[Test]
		public void PermissionStateNone () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			AssertNotNull ("EnvironmentPermission(PermissionState.None)", ep);
			Assert ("IsUnrestricted", !ep.IsUnrestricted ());
			EnvironmentPermission copy = (EnvironmentPermission) ep.Copy ();
			AssertEquals ("Copy.IsUnrestricted", ep.IsUnrestricted (), copy.IsUnrestricted ());
			SecurityElement se = ep.ToXml ();
			Assert ("ToXml-class", (se.Attributes ["class"] as string).StartsWith (className));
			AssertEquals ("ToXml-version", "1", (se.Attributes ["version"] as string));
		}

		[Test]
		public void PermissionStateUnrestricted () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.Unrestricted);
			AssertNotNull ("EnvironmentPermission(PermissionState.Unrestricted)", ep);
			Assert ("IsUnrestricted", ep.IsUnrestricted ());
			EnvironmentPermission copy = (EnvironmentPermission) ep.Copy ();
			AssertEquals ("Copy.IsUnrestricted", ep.IsUnrestricted (), copy.IsUnrestricted ());
			SecurityElement se = ep.ToXml ();
			AssertEquals ("ToXml-Unrestricted", "true", (se.Attributes ["Unrestricted"] as string));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullPathList () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (EnvironmentPermissionAccess.AllAccess, null);
		}

		[Test]
		public void AllAccess () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (EnvironmentPermissionAccess.AllAccess, envVariables);
			Assert ("IsUnrestricted", !ep.IsUnrestricted ());
		}

		[Test]
		public void NoAccess () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (EnvironmentPermissionAccess.NoAccess, envVariables);
			Assert ("IsUnrestricted", !ep.IsUnrestricted ());
		}

		[Test]
		public void ReadAccess () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			Assert ("IsUnrestricted", !ep.IsUnrestricted ());
		}

		[Test]
		public void WriteAccess () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (EnvironmentPermissionAccess.Write, envVariables);
			Assert ("IsUnrestricted", !ep.IsUnrestricted ());
		}

		[Test]
		public void AddPathList () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			ep.AddPathList (EnvironmentPermissionAccess.AllAccess, envVariables);
			// LAMESPEC NoAccess do not remove the TMP from AllAccess
			ep.AddPathList (EnvironmentPermissionAccess.NoAccess, "TMP");
			ep.AddPathList (EnvironmentPermissionAccess.Read, "UID");
			ep.AddPathList (EnvironmentPermissionAccess.Write, "PROMPT");
			SecurityElement se = ep.ToXml ();
			// Note: Debugger can mess results (try to run without stepping)
			AssertEquals ("AddPathList-ToXml-Read", "TMP;TEMP;UID", (se.Attributes ["Read"] as string));
			AssertEquals ("AddPathList-ToXml-Write", "TMP;TEMP;PROMPT", (se.Attributes ["Write"] as string));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetPathListAllAccess () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			ep.AddPathList (EnvironmentPermissionAccess.Read, "UID");
			ep.AddPathList (EnvironmentPermissionAccess.Write, "PROMPT");
			AssertEquals ("GetPathList-AllAccess", "", ep.GetPathList (EnvironmentPermissionAccess.AllAccess));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetPathListNoAccess () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			ep.AddPathList (EnvironmentPermissionAccess.Read, "UID");
			ep.AddPathList (EnvironmentPermissionAccess.Write, "PROMPT");
			AssertEquals ("GetPathList-NoAccess", "", ep.GetPathList (EnvironmentPermissionAccess.NoAccess));
		}

		[Test]
		public void GetPathList () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
#if NET_2_0
			AssertEquals ("GetPathList-Read-Empty", String.Empty, ep.GetPathList (EnvironmentPermissionAccess.Read));
			AssertEquals ("GetPathList-Write-Empty", String.Empty, ep.GetPathList (EnvironmentPermissionAccess.Write));
#else
			AssertNull ("GetPathList-Read-Empty", ep.GetPathList (EnvironmentPermissionAccess.Read));
			AssertNull ("GetPathList-Write-Empty", ep.GetPathList (EnvironmentPermissionAccess.Write));
#endif
			ep.AddPathList (EnvironmentPermissionAccess.Read, "UID");
			ep.AddPathList (EnvironmentPermissionAccess.Write, "PROMPT");
			AssertEquals ("GetPathList-Read", "UID", ep.GetPathList (EnvironmentPermissionAccess.Read));
			AssertEquals ("GetPathList-Write", "PROMPT", ep.GetPathList (EnvironmentPermissionAccess.Write));
		}

		[Test]
		public void SetPathList () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			ep.SetPathList (EnvironmentPermissionAccess.AllAccess, envVariables);
			// LAMESPEC NoAccess do not remove the TMP from AllAccess
			ep.SetPathList (EnvironmentPermissionAccess.NoAccess, "SYSTEMROOT");
			ep.SetPathList (EnvironmentPermissionAccess.Read, "UID");
			ep.SetPathList (EnvironmentPermissionAccess.Write, "PROMPT");
			SecurityElement se = ep.ToXml ();
			AssertEquals ("SetPathList-ToXml-Read", "UID", (se.Attributes ["Read"] as string));
			AssertEquals ("SetPathList-ToXml-Write", "PROMPT", (se.Attributes ["Write"] as string));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlNull () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			ep.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlInvalidPermission () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
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
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
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
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			SecurityElement se = ep.ToXml ();
			AssertNotNull ("ToXml()", se);
			ep.FromXml (se);
			se.AddAttribute ("Read", envVariables);
			ep.FromXml (se);
			AssertEquals ("FromXml-Read", envVariables, ep.GetPathList (EnvironmentPermissionAccess.Read));
			se.AddAttribute ("Write", envVariables);
			ep.FromXml (se);
			AssertEquals ("FromXml-Read", envVariables, ep.GetPathList (EnvironmentPermissionAccess.Read));
			AssertEquals ("FromXml-Write", envVariables, ep.GetPathList (EnvironmentPermissionAccess.Write));
		}

		[Test]
		public void UnionWithNull () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep2 = null;
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Union (ep2);
			AssertEquals ("EP1 U null == EP1", ep1.ToXml ().ToString (), ep3.ToXml ().ToString ());
		}

		[Test]
		public void UnionWithUnrestricted () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (PermissionState.Unrestricted);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Union (ep2);
			Assert ("Unrestricted U EP2 == Unrestricted", ep3.IsUnrestricted ());
			ep3 = (EnvironmentPermission) ep2.Union (ep1);
			Assert ("EP2 U Unrestricted == Unrestricted", ep3.IsUnrestricted ());
		}

		[Test]
		public void Union () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Write, envVariables);
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Union (ep2);
			EnvironmentPermission ep4 = new EnvironmentPermission (EnvironmentPermissionAccess.AllAccess, envVariables);
			AssertEquals ("EP1 U EP2 == EP1+2", ep3.ToXml ().ToString (), ep4.ToXml ().ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnionWithBadPermission () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			FileDialogPermission fdp2 = new FileDialogPermission (PermissionState.Unrestricted);
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Union (fdp2);
		}

		[Test]
		public void IntersectWithNull () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep2 = null;
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Intersect (ep2);
			AssertNull ("EP1 N null == null", ep3);
		}

		[Test]
		public void IntersectWithUnrestricted () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (PermissionState.Unrestricted);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Intersect (ep2);
			Assert ("Unrestricted N EP2 == EP2", !ep3.IsUnrestricted ());
			AssertEquals ("Unrestricted N EP2 == EP2", ep2.ToXml ().ToString (), ep3.ToXml ().ToString ());
			ep3 = (EnvironmentPermission) ep2.Intersect (ep1);
			Assert ("EP2 N Unrestricted == EP2", !ep3.IsUnrestricted ());
			AssertEquals ("EP2 N Unrestricted == EP2", ep2.ToXml ().ToString (), ep3.ToXml ().ToString ());
		}

		[Test]
		public void Intersect () 
		{
			// no intersection
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Write, envVariables);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Intersect (ep2);
			AssertNull ("EP1 N EP2 == null", ep3);
			// intersection in read
			EnvironmentPermission ep4 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, "TMP");		
			ep3 = (EnvironmentPermission) ep4.Intersect (ep2);
			AssertEquals ("Intersect-Read", "TMP", ep3.GetPathList (EnvironmentPermissionAccess.Read));
			// intersection in write
			EnvironmentPermission ep5 = new EnvironmentPermission (EnvironmentPermissionAccess.Write, "TEMP");		
			ep3 = (EnvironmentPermission) ep5.Intersect (ep1);
			AssertEquals ("Intersect-Read", "TEMP", ep3.GetPathList (EnvironmentPermissionAccess.Write));
			// intersection in read and write
			EnvironmentPermission ep6 = new EnvironmentPermission (EnvironmentPermissionAccess.AllAccess, "TEMP");
			EnvironmentPermission ep7 = new EnvironmentPermission (EnvironmentPermissionAccess.AllAccess, envVariables);
			ep3 = (EnvironmentPermission) ep6.Intersect (ep7);
			AssertEquals ("Intersect-AllAccess-Read", "TEMP", ep3.GetPathList (EnvironmentPermissionAccess.Read));
			AssertEquals ("Intersect-AllAccess-Write", "TEMP", ep3.GetPathList (EnvironmentPermissionAccess.Write));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IntersectWithBadPermission () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			FileDialogPermission fdp2 = new FileDialogPermission (PermissionState.Unrestricted);
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Intersect (fdp2);
		}

		[Test]
		public void IsSubsetOfNull () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			Assert ("IsSubsetOf(null)", !ep1.IsSubsetOf (null));
		}

		[Test]
		public void IsSubsetOfUnrestricted () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (PermissionState.Unrestricted);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep3 = new EnvironmentPermission (PermissionState.Unrestricted);
			Assert ("Unrestricted.IsSubsetOf()", !ep1.IsSubsetOf (ep2));
			Assert ("IsSubsetOf(Unrestricted)", ep2.IsSubsetOf (ep1));
			Assert ("Unrestricted.IsSubsetOf(Unrestricted)", ep1.IsSubsetOf (ep3));
		}

		[Test]
		public void IsSubsetOf () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Write, envVariables);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			Assert ("IsSubsetOf(nosubset1)", !ep1.IsSubsetOf (ep2));
			Assert ("IsSubsetOf(nosubset2)", !ep2.IsSubsetOf (ep1));
			EnvironmentPermission ep3 = new EnvironmentPermission (EnvironmentPermissionAccess.Write, "TMP");
			Assert ("IsSubsetOf(TMP)", !ep1.IsSubsetOf (ep3));
			Assert ("TMP.IsSubsetOf()", ep3.IsSubsetOf (ep1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOfBadPermission () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			FileDialogPermission fdp2 = new FileDialogPermission (PermissionState.Unrestricted);
			Assert ("IsSubsetOf(FileDialogPermission)", ep1.IsSubsetOf (fdp2));
		}
	}
}
