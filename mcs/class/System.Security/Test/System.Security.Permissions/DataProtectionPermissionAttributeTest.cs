//
// DataProtectionPermissionAttributeTest.cs -
//	NUnit Test Cases for DataProtectionPermissionAttributeTest
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class DataProtectionPermissionAttributeTest {

		[Test]
		public void Default ()
		{
			DataProtectionPermissionAttribute a = new DataProtectionPermissionAttribute (SecurityAction.Assert);
			Assert.IsFalse (a.ProtectData, "ProtectData");
			Assert.IsFalse (a.UnprotectData, "UnprotectData");
			Assert.IsFalse (a.ProtectMemory, "ProtectMemory");
			Assert.IsFalse (a.UnprotectMemory, "UnprotectMemory");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			Assert.AreEqual (DataProtectionPermissionFlags.NoFlags, a.Flags, "Flags");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");

			DataProtectionPermission perm = (DataProtectionPermission)a.CreatePermission ();
			Assert.AreEqual (DataProtectionPermissionFlags.NoFlags, perm.Flags, "CreatePermission.Flags");
		}

		[Test]
		public void Action ()
		{
			DataProtectionPermissionAttribute a = new DataProtectionPermissionAttribute (SecurityAction.Assert);
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
			DataProtectionPermissionAttribute a = new DataProtectionPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		private DataProtectionPermissionAttribute Empty ()
		{
			DataProtectionPermissionAttribute a = new DataProtectionPermissionAttribute (SecurityAction.Assert);
			a.ProtectData = false;
			a.UnprotectData = false;
			a.ProtectMemory = false;
			a.UnprotectMemory = false;
			Assert.AreEqual (DataProtectionPermissionFlags.NoFlags, a.Flags, "Flags");
			return a;
		}

		[Test]
		public void ProtectData ()
		{
			DataProtectionPermissionAttribute a = Empty ();
			a.ProtectData = true;
			Assert.AreEqual (DataProtectionPermissionFlags.ProtectData, a.Flags, "Flags=ProtectData");
			a.ProtectData = false;
			Assert.AreEqual (DataProtectionPermissionFlags.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void UnprotectData ()
		{
			DataProtectionPermissionAttribute a = Empty ();
			a.UnprotectData = true;
			Assert.AreEqual (DataProtectionPermissionFlags.UnprotectData, a.Flags, "Flags=UnprotectData");
			a.UnprotectData = false;
			Assert.AreEqual (DataProtectionPermissionFlags.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void ProtectMemory ()
		{
			DataProtectionPermissionAttribute a = Empty ();
			a.ProtectMemory = true;
			Assert.AreEqual (DataProtectionPermissionFlags.ProtectMemory, a.Flags, "Flags=ProtectMemory");
			a.ProtectMemory = false;
			Assert.AreEqual (DataProtectionPermissionFlags.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void UnprotectMemory ()
		{
			DataProtectionPermissionAttribute a = Empty ();
			a.UnprotectMemory = true;
			Assert.AreEqual (DataProtectionPermissionFlags.UnprotectMemory, a.Flags, "Flags=UnprotectMemory");
			a.UnprotectMemory = false;
			Assert.AreEqual (DataProtectionPermissionFlags.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void Unrestricted ()
		{
			DataProtectionPermissionAttribute a = Empty ();
			a.Unrestricted = true;
			Assert.AreEqual (DataProtectionPermissionFlags.NoFlags, a.Flags, "Unrestricted");

			DataProtectionPermission perm = (DataProtectionPermission)a.CreatePermission ();
			Assert.AreEqual (DataProtectionPermissionFlags.AllFlags, perm.Flags, "CreatePermission.Flags");
		}

		[Test]
		public void Flags ()
		{
			DataProtectionPermissionAttribute a = Empty ();
			a.Flags = DataProtectionPermissionFlags.ProtectData;
			Assert.IsTrue (a.ProtectData, "ProtectData");
			a.Flags |= DataProtectionPermissionFlags.ProtectMemory;
			Assert.IsTrue (a.ProtectMemory, "ProtectMemory");
			a.Flags |= DataProtectionPermissionFlags.UnprotectData;
			Assert.IsTrue (a.UnprotectData, "UnprotectData");
			a.Flags |= DataProtectionPermissionFlags.UnprotectMemory;
			Assert.IsTrue (a.UnprotectMemory, "UnprotectMemory");

			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.AreEqual (DataProtectionPermissionFlags.AllFlags, a.Flags, "Flags=AllFlags");

			a.Flags &= ~DataProtectionPermissionFlags.ProtectData;
			Assert.IsFalse (a.ProtectData, "ProtectData");
			a.Flags &= ~DataProtectionPermissionFlags.ProtectMemory;
			Assert.IsFalse (a.ProtectMemory, "ProtectMemory");
			a.Flags &= ~DataProtectionPermissionFlags.UnprotectData;
			Assert.IsFalse (a.UnprotectData, "UnprotectData");
			a.Flags &= ~DataProtectionPermissionFlags.UnprotectMemory;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Flags_Invalid ()
		{
			DataProtectionPermissionAttribute a = Empty ();
			a.Flags = ((DataProtectionPermissionFlags)Int32.MinValue);
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (DataProtectionPermissionAttribute);
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

#endif
