//
// PermissionRequestEvidenceTest.cs -
//	NUnit Test Cases for PermissionRequestEvidence
//
// Authors:
//	Nick Drochak (ndrochak@gol.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Nick Drochak
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
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class PermissionRequestEvidenceTest {

		private string AdjustNewLine (string s) 
		{
#if NET_2_0
			// no spaces are used in Fx 2.0
			while (s.IndexOf ("\r\n ") != -1)
				s = s.Replace ("\r\n ", "\r\n");
#endif
			if (Environment.NewLine != "\r\n")
				s = s.Replace ("\r\n", Environment.NewLine);
			return s;
		}

		[Test]
		public void NullConstructor ()
		{
			PermissionRequestEvidence pre = new PermissionRequestEvidence (null, null, null);
			Assert.IsNull (pre.RequestedPermissions, "Requested");
			Assert.IsNull (pre.OptionalPermissions, "Optional");
			Assert.IsNull (pre.DeniedPermissions, "Denied");
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\"/>\r\n");
			Assert.AreEqual (expected, pre.ToString (), "ToString");
		}

		[Test]
		public void Constructor1 () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps, ps, ps);
			Assert.IsFalse (pre.RequestedPermissions.IsUnrestricted (), "Requested");
			Assert.IsFalse (pre.OptionalPermissions.IsUnrestricted (), "Optional");
			Assert.IsFalse (pre.DeniedPermissions.IsUnrestricted (), "Denied");
			Assert.IsFalse (Object.ReferenceEquals (ps, pre.RequestedPermissions), "!ReferenceEquals-RequestedPermissions");
			Assert.IsFalse (Object.ReferenceEquals (ps, pre.OptionalPermissions), "!ReferenceEquals-OptionalPermissions");
			Assert.IsFalse (Object.ReferenceEquals (ps, pre.DeniedPermissions), "!ReferenceEquals-DeniedPermissions");
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\">\r\n   <Request>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Request>\r\n   <Optional>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Optional>\r\n   <Denied>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Denied>\r\n</System.Security.Policy.PermissionRequestEvidence>\r\n");
			Assert.AreEqual (expected, pre.ToString (), "ToString");
		}

		[Test]
		public void Constructor2 () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps, ps, ps);
			Assert.IsTrue (pre.RequestedPermissions.IsUnrestricted (), "Requested");
			Assert.IsTrue (pre.OptionalPermissions.IsUnrestricted (), "Optional");
			Assert.IsTrue (pre.DeniedPermissions.IsUnrestricted (), "Denied");
			Assert.IsFalse (Object.ReferenceEquals (ps, pre.RequestedPermissions), "!ReferenceEquals-RequestedPermissions");
			Assert.IsFalse (Object.ReferenceEquals (ps, pre.OptionalPermissions), "!ReferenceEquals-OptionalPermissions");
			Assert.IsFalse (Object.ReferenceEquals (ps, pre.DeniedPermissions), "!ReferenceEquals-DeniedPermissions");
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\">\r\n   <Request>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Request>\r\n   <Optional>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Optional>\r\n   <Denied>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Denied>\r\n</System.Security.Policy.PermissionRequestEvidence>\r\n");
			Assert.AreEqual (expected, pre.ToString (), "ToString");
		}

		[Test]
		public void Constructor3 () 
		{
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.Unrestricted);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps1, ps2, ps1);
			Assert.IsFalse (pre.RequestedPermissions.IsUnrestricted (), "Requested");
			Assert.IsTrue (pre.OptionalPermissions.IsUnrestricted (), "Optional");
			Assert.IsFalse (pre.DeniedPermissions.IsUnrestricted (), "Denied");
			Assert.IsFalse (Object.ReferenceEquals (ps1, pre.RequestedPermissions), "!ReferenceEquals-RequestedPermissions");
			Assert.IsFalse (Object.ReferenceEquals (ps2, pre.OptionalPermissions), "!ReferenceEquals-OptionalPermissions");
			Assert.IsFalse (Object.ReferenceEquals (ps1, pre.DeniedPermissions), "!ReferenceEquals-DeniedPermissions");
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\">\r\n   <Request>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Request>\r\n   <Optional>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Optional>\r\n   <Denied>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Denied>\r\n</System.Security.Policy.PermissionRequestEvidence>\r\n");
			Assert.AreEqual (expected, pre.ToString (), "ToString");
		}

		[Test]
		public void Constructor4 () 
		{
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.Unrestricted);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps2, ps1, ps2);
			Assert.IsTrue (pre.RequestedPermissions.IsUnrestricted (), "Requested");
			Assert.IsFalse (pre.OptionalPermissions.IsUnrestricted (), "Optional");
			Assert.IsTrue (pre.DeniedPermissions.IsUnrestricted (), "Denied");
			Assert.IsFalse (Object.ReferenceEquals (ps2, pre.RequestedPermissions), "!ReferenceEquals-RequestedPermissions");
			Assert.IsFalse (Object.ReferenceEquals (ps1, pre.OptionalPermissions), "!ReferenceEquals-OptionalPermissions");
			Assert.IsFalse (Object.ReferenceEquals (ps2, pre.DeniedPermissions), "!ReferenceEquals-DeniedPermissions");
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\">\r\n   <Request>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Request>\r\n   <Optional>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Optional>\r\n   <Denied>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Denied>\r\n</System.Security.Policy.PermissionRequestEvidence>\r\n");
			Assert.AreEqual (expected, pre.ToString (), "ToString");
		}

		[Test]
		public void Copy () 
		{
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.Unrestricted);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps2, ps1, ps2);
			PermissionRequestEvidence copy = pre.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (pre, copy), "!ReferenceEquals");
			Assert.IsTrue (copy.RequestedPermissions.IsUnrestricted (), "Requested");
			Assert.IsFalse (copy.OptionalPermissions.IsUnrestricted (), "Optional");
			Assert.IsTrue (copy.DeniedPermissions.IsUnrestricted (), "Denied");
			Assert.IsFalse (Object.ReferenceEquals (ps2, copy.RequestedPermissions), "!ReferenceEquals-RequestedPermissions");
			Assert.IsFalse (Object.ReferenceEquals (ps1, copy.OptionalPermissions), "!ReferenceEquals-OptionalPermissions");
			Assert.IsFalse (Object.ReferenceEquals (ps2, copy.DeniedPermissions), "!ReferenceEquals-DeniedPermissions");
			Assert.IsFalse (Object.ReferenceEquals (pre.RequestedPermissions, copy.RequestedPermissions), "!ReferenceEquals-Copy-RequestedPermissions");
			Assert.IsFalse (Object.ReferenceEquals (pre.OptionalPermissions, copy.OptionalPermissions), "!ReferenceEquals-Copy-OptionalPermissions");
			Assert.IsFalse (Object.ReferenceEquals (pre.DeniedPermissions, copy.DeniedPermissions), "!ReferenceEquals-Copy-DeniedPermissions");
			string expected = AdjustNewLine ("<System.Security.Policy.PermissionRequestEvidence version=\"1\">\r\n   <Request>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Request>\r\n   <Optional>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"/>\r\n   </Optional>\r\n   <Denied>\r\n      <PermissionSet class=\"System.Security.PermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"/>\r\n   </Denied>\r\n</System.Security.Policy.PermissionRequestEvidence>\r\n");
			Assert.AreEqual (expected, pre.ToString (), "ToString");
			Assert.AreEqual (pre.ToString (), copy.ToString (), "ToString-Copy");
		}

		[Test]
		public void CopiesButNotReadOnly ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			PermissionRequestEvidence pre = new PermissionRequestEvidence (ps, ps, ps);
			ps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Execution));
			Assert.AreEqual (1, ps.Count, "ps.Count");
			// not a reference
			Assert.AreEqual (0, pre.RequestedPermissions.Count, "Requested.Count");
			Assert.AreEqual (0, pre.OptionalPermissions.Count, "Optional.Count");
			Assert.AreEqual (0, pre.DeniedPermissions.Count, "Denied.Count");
			// and we can still add permissions
			pre.RequestedPermissions.AddPermission (new SecurityPermission (SecurityPermissionFlag.Execution));
			Assert.AreEqual (1, pre.RequestedPermissions.Count, "Requested.Count-2");
			Assert.AreEqual (0, pre.OptionalPermissions.Count, "Optional.Count-2");
			Assert.AreEqual (0, pre.DeniedPermissions.Count, "Denied.Count-2");
		}
	}
}
