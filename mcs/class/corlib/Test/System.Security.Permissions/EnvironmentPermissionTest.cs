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
#if MOBILE
	[Ignore]
#endif
	public class EnvironmentPermissionTest {

		private static string className = "System.Security.Permissions.EnvironmentPermission, ";
		private static string envVariables = "TMP;TEMP";

		[Test]
		public void PermissionStateNone () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			Assert.IsNotNull (ep, "EnvironmentPermission(PermissionState.None)");
			Assert.IsTrue (!ep.IsUnrestricted (), "IsUnrestricted");
			EnvironmentPermission copy = (EnvironmentPermission) ep.Copy ();
			Assert.AreEqual (ep.IsUnrestricted (), copy.IsUnrestricted (), "Copy.IsUnrestricted");
			SecurityElement se = ep.ToXml ();
			Assert.IsTrue ((se.Attributes ["class"] as string).StartsWith (className), "ToXml-class");
			Assert.AreEqual ("1", (se.Attributes ["version"] as string), "ToXml-version");
		}

		[Test]
		public void PermissionStateUnrestricted () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.Unrestricted);
			Assert.IsNotNull (ep, "EnvironmentPermission(PermissionState.Unrestricted)");
			Assert.IsTrue (ep.IsUnrestricted (), "IsUnrestricted");
			EnvironmentPermission copy = (EnvironmentPermission) ep.Copy ();
			Assert.AreEqual (ep.IsUnrestricted (), copy.IsUnrestricted (), "Copy.IsUnrestricted");
			SecurityElement se = ep.ToXml ();
			Assert.AreEqual ("true", (se.Attributes ["Unrestricted"] as string), "ToXml-Unrestricted");
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
			Assert.IsTrue (!ep.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void NoAccess () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (EnvironmentPermissionAccess.NoAccess, envVariables);
			Assert.IsTrue (!ep.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void ReadAccess () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			Assert.IsTrue (!ep.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void WriteAccess () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (EnvironmentPermissionAccess.Write, envVariables);
			Assert.IsTrue (!ep.IsUnrestricted (), "IsUnrestricted");
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
			Assert.AreEqual ("TMP;TEMP;UID", (se.Attributes ["Read"] as string), "AddPathList-ToXml-Read");
			Assert.AreEqual ("TMP;TEMP;PROMPT", (se.Attributes ["Write"] as string), "AddPathList-ToXml-Write");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetPathListAllAccess () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			ep.AddPathList (EnvironmentPermissionAccess.Read, "UID");
			ep.AddPathList (EnvironmentPermissionAccess.Write, "PROMPT");
			Assert.AreEqual ("", ep.GetPathList (EnvironmentPermissionAccess.AllAccess), "GetPathList-AllAccess");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetPathListNoAccess () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			ep.AddPathList (EnvironmentPermissionAccess.Read, "UID");
			ep.AddPathList (EnvironmentPermissionAccess.Write, "PROMPT");
			Assert.AreEqual ("", ep.GetPathList (EnvironmentPermissionAccess.NoAccess), "GetPathList-NoAccess");
		}

		[Test]
		public void GetPathList () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
#if NET_2_0
			Assert.AreEqual (String.Empty, ep.GetPathList (EnvironmentPermissionAccess.Read), "GetPathList-Read-Empty");
			Assert.AreEqual (String.Empty, ep.GetPathList (EnvironmentPermissionAccess.Write), "GetPathList-Write-Empty");
#else
			Assert.IsNull (ep.GetPathList (EnvironmentPermissionAccess.Read), "GetPathList-Read-Empty");
			Assert.IsNull (ep.GetPathList (EnvironmentPermissionAccess.Write), "GetPathList-Write-Empty");
#endif
			ep.AddPathList (EnvironmentPermissionAccess.Read, "UID");
			ep.AddPathList (EnvironmentPermissionAccess.Write, "PROMPT");
			Assert.AreEqual ("UID", ep.GetPathList (EnvironmentPermissionAccess.Read), "GetPathList-Read");
			Assert.AreEqual ("PROMPT", ep.GetPathList (EnvironmentPermissionAccess.Write), "GetPathList-Write");
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
			Assert.AreEqual ("UID", (se.Attributes ["Read"] as string), "SetPathList-ToXml-Read");
			Assert.AreEqual ("PROMPT", (se.Attributes ["Write"] as string), "SetPathList-ToXml-Write");
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

#if NET_2_0
		[Category ("NotWorking")]
#endif
		
		[Test]
		public void FromXml () 
		{
			EnvironmentPermission ep = new EnvironmentPermission (PermissionState.None);
			SecurityElement se = ep.ToXml ();
			Assert.IsNotNull (se, "ToXml()");
			ep.FromXml (se);
			se.AddAttribute ("Read", envVariables);
			ep.FromXml (se);
			Assert.AreEqual (envVariables, ep.GetPathList (EnvironmentPermissionAccess.Read), "FromXml-Read");
			se.AddAttribute ("Write", envVariables);
			ep.FromXml (se);
			Assert.AreEqual (envVariables, ep.GetPathList (EnvironmentPermissionAccess.Read), "FromXml-Read");
			Assert.AreEqual (envVariables, ep.GetPathList (EnvironmentPermissionAccess.Write), "FromXml-Write");
		}

		[Test]
		public void UnionWithNull () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep2 = null;
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Union (ep2);
			Assert.AreEqual (ep1.ToXml ().ToString (), ep3.ToXml ().ToString (), "EP1 U null == EP1");
		}

		[Test]
		public void UnionWithUnrestricted () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (PermissionState.Unrestricted);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Union (ep2);
			Assert.IsTrue (ep3.IsUnrestricted (), "Unrestricted U EP2 == Unrestricted");
			ep3 = (EnvironmentPermission) ep2.Union (ep1);
			Assert.IsTrue (ep3.IsUnrestricted (), "EP2 U Unrestricted == Unrestricted");
		}
#if NET_2_0
		[Category ("NotWorking")]
#endif
		[Test]
		public void Union () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Write, envVariables);
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Union (ep2);
			EnvironmentPermission ep4 = new EnvironmentPermission (EnvironmentPermissionAccess.AllAccess, envVariables);
			Assert.AreEqual (ep3.ToXml ().ToString (), ep4.ToXml ().ToString (), "EP1 U EP2 == EP1+2");
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
			Assert.IsNull (ep3, "EP1 N null == null");
		}

		[Test]
		public void IntersectWithUnrestricted () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (PermissionState.Unrestricted);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Intersect (ep2);
			Assert.IsTrue (!ep3.IsUnrestricted (), "Unrestricted N EP2 == EP2");
			Assert.AreEqual (ep2.ToXml ().ToString (), ep3.ToXml ().ToString (), "Unrestricted N EP2 == EP2");
			ep3 = (EnvironmentPermission) ep2.Intersect (ep1);
			Assert.IsTrue (!ep3.IsUnrestricted (), "EP2 N Unrestricted == EP2");
			Assert.AreEqual (ep2.ToXml ().ToString (), ep3.ToXml ().ToString (), "EP2 N Unrestricted == EP2");
		}

		[Test]
		public void Intersect () 
		{
			// no intersection
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Write, envVariables);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep3 = (EnvironmentPermission) ep1.Intersect (ep2);
			Assert.IsNull (ep3, "EP1 N EP2 == null");
			// intersection in read
			EnvironmentPermission ep4 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, "TMP");		
			ep3 = (EnvironmentPermission) ep4.Intersect (ep2);
			Assert.AreEqual ("TMP", ep3.GetPathList (EnvironmentPermissionAccess.Read), "Intersect-Read");
			// intersection in write
			EnvironmentPermission ep5 = new EnvironmentPermission (EnvironmentPermissionAccess.Write, "TEMP");		
			ep3 = (EnvironmentPermission) ep5.Intersect (ep1);
			Assert.AreEqual ("TEMP", ep3.GetPathList (EnvironmentPermissionAccess.Write), "Intersect-Read");
			// intersection in read and write
			EnvironmentPermission ep6 = new EnvironmentPermission (EnvironmentPermissionAccess.AllAccess, "TEMP");
			EnvironmentPermission ep7 = new EnvironmentPermission (EnvironmentPermissionAccess.AllAccess, envVariables);
			ep3 = (EnvironmentPermission) ep6.Intersect (ep7);
			Assert.AreEqual ("TEMP", ep3.GetPathList (EnvironmentPermissionAccess.Read), "Intersect-AllAccess-Read");
			Assert.AreEqual ("TEMP", ep3.GetPathList (EnvironmentPermissionAccess.Write), "Intersect-AllAccess-Write");
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
			Assert.IsTrue (!ep1.IsSubsetOf (null), "IsSubsetOf(null)");
		}

		[Test]
		public void IsSubsetOfUnrestricted () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (PermissionState.Unrestricted);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			EnvironmentPermission ep3 = new EnvironmentPermission (PermissionState.Unrestricted);
			Assert.IsTrue (!ep1.IsSubsetOf (ep2), "Unrestricted.IsSubsetOf()");
			Assert.IsTrue (ep2.IsSubsetOf (ep1), "IsSubsetOf(Unrestricted)");
			Assert.IsTrue (ep1.IsSubsetOf (ep3), "Unrestricted.IsSubsetOf(Unrestricted)");
		}

		[Test]
		public void IsSubsetOf () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Write, envVariables);
			EnvironmentPermission ep2 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			Assert.IsTrue (!ep1.IsSubsetOf (ep2), "IsSubsetOf(nosubset1)");
			Assert.IsTrue (!ep2.IsSubsetOf (ep1), "IsSubsetOf(nosubset2)");
			EnvironmentPermission ep3 = new EnvironmentPermission (EnvironmentPermissionAccess.Write, "TMP");
			Assert.IsTrue (!ep1.IsSubsetOf (ep3), "IsSubsetOf(TMP)");
			Assert.IsTrue (ep3.IsSubsetOf (ep1), "TMP.IsSubsetOf()");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOfBadPermission () 
		{
			EnvironmentPermission ep1 = new EnvironmentPermission (EnvironmentPermissionAccess.Read, envVariables);
			FileDialogPermission fdp2 = new FileDialogPermission (PermissionState.Unrestricted);
			Assert.IsTrue (ep1.IsSubsetOf (fdp2), "IsSubsetOf(FileDialogPermission)");
		}
	}
}
