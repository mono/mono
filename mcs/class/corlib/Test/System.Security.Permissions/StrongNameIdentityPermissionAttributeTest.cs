//
// StrongNameIdentityPermissionAttributeTest.cs -
//	NUnit Test Cases for StrongNameIdentityPermissionAttribute
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
	public class StrongNameIdentityPermissionAttributeTest {

		static string pk = "00240000048000009400000006020000002400005253413100040000010001003DBD7208C62B0EA8C1C058072B635F7C9ABDCB22DB20B2A9DADAEFE800642F5D8DEB7802F7A5367728D7558D1468DBEB2409D02B131B926E2E59544AAC18CFC909023F4FA83E94001FC2F11A27477D1084F514B861621A0C66ABD24C4B9FC90F3CD8920FF5FFCED76E5C6FB1F57DD356F96727A4A5485B079344004AF8FFA4CB";

		[Test]
		public void Default () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.PublicKey, "PublicKey");
			Assert.IsNull (a.Version, "Version");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsTrue (!a.Unrestricted, "Unrestricted");

			StrongNameIdentityPermission perm = (StrongNameIdentityPermission) a.CreatePermission ();
			Assert.AreEqual (String.Empty, perm.Name, "CreatePermission.Name");
			Assert.IsNull (perm.PublicKey, "CreatePermission.PublicKey");
			Assert.AreEqual ("0.0", perm.Version.ToString (), "CreatePermission.Version");
		}

		[Test]
		public void Action () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (SecurityAction.Assert, a.Action, "Action=Assert");
			a.Action = SecurityAction.Demand;
			Assert.AreEqual (SecurityAction.Demand, a.Action, "Action=Demand");
			a.Action = SecurityAction.Deny;
			Assert.AreEqual (SecurityAction.Deny, a.Action, "Action=Deny");
			a.Action = SecurityAction.InheritanceDemand;
			Assert.AreEqual (SecurityAction.InheritanceDemand, a.Action, "Action=InheritanceDemand");
			a.Action = SecurityAction.LinkDemand;
			Assert.AreEqual (SecurityAction.LinkDemand, a.Action, "Action=LinkDemand");
			a.Action = SecurityAction.PermitOnly;
			Assert.AreEqual (SecurityAction.PermitOnly, a.Action, "Action=PermitOnly");
			a.Action = SecurityAction.RequestMinimum;
			Assert.AreEqual (SecurityAction.RequestMinimum, a.Action, "Action=RequestMinimum");
			a.Action = SecurityAction.RequestOptional;
			Assert.AreEqual (SecurityAction.RequestOptional, a.Action, "Action=RequestOptional");
			a.Action = SecurityAction.RequestRefuse;
			Assert.AreEqual (SecurityAction.RequestRefuse, a.Action, "Action=RequestRefuse");
		}

		[Test]
		public void Action_Invalid ()
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Name () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Name = "mono";
			Assert.AreEqual ("mono", a.Name, "Name");
			a.Name = null;
			Assert.IsNull (a.Name, "Name-null");
			a.Name = String.Empty;
			Assert.AreEqual (String.Empty, a.Name, "Name-empty");
		}

		[Test]
		public void PublicKey () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.PublicKey = pk;
			Assert.AreEqual (pk, a.PublicKey, "PublicKey");
			a.PublicKey = null;
			Assert.IsNull (a.PublicKey, "PublicKey");
		}

		[Test]
		public void Version () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Version = "1.2.3.4";
			Assert.AreEqual ("1.2.3.4", a.Version, "Version");
			a.Version = null;
			Assert.IsNull (a.Version, "Version");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreatePermission_OnlyName () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Name = "mono";
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		public void CreatePermission_OnlyPublicKey () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.PublicKey = pk;
			StrongNameIdentityPermission p = (StrongNameIdentityPermission)a.CreatePermission ();
			Assert.IsNull (p.Name, "Name");
			Assert.AreEqual (pk, p.PublicKey.ToString (), "PublicKey");
			Assert.IsNull (p.Version, "Version");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreatePermission_OnlyVersion () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Version = "1.2.3.4";
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		public void CreatePermission_NamePublicKey () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Name = "mono";
			a.PublicKey = pk;
			StrongNameIdentityPermission p = (StrongNameIdentityPermission) a.CreatePermission ();
			Assert.AreEqual ("mono", p.Name, "Name");
			Assert.AreEqual (pk, p.PublicKey.ToString (), "PublicKey");
			Assert.IsNull (p.Version, "Version");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreatePermission_NameVersion () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Name = "mono";
			a.Version = "1.2.3.4";
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		public void CreatePermission_PublicKeyVersion () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.PublicKey = pk;
			a.Version = "1.2.3.4";
			IPermission perm = a.CreatePermission ();
			StrongNameIdentityPermission p = (StrongNameIdentityPermission)a.CreatePermission ();
			Assert.IsNull (p.Name, "Name");
			Assert.AreEqual (pk, p.PublicKey.ToString (), "PublicKey");
			Assert.AreEqual ("1.2.3.4", p.Version.ToString (), "Version");
		}

		[Test]
		public void CreatePermission () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Name = "mono";
			a.PublicKey = pk;
			a.Version = "1.2.3.4";
			StrongNameIdentityPermission p = (StrongNameIdentityPermission)a.CreatePermission ();
			Assert.AreEqual ("mono", p.Name, "Name");
			Assert.AreEqual (pk, p.PublicKey.ToString (), "PublicKey");
			Assert.AreEqual ("1.2.3.4", p.Version.ToString (), "Version");
		}

		[Test]
#if !NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#else
		[Category ("NotWorking")]
#endif
		public void Unrestricted () 
		{
			StrongNameIdentityPermissionAttribute a = new StrongNameIdentityPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (StrongNameIdentityPermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object [] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
