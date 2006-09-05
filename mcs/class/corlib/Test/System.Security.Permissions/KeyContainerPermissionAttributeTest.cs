//
// KeyContainerPermissionAttributeTest.cs -
//	NUnit Test Cases for KeyContainerPermissionAttributeTest
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
	public class KeyContainerPermissionAttributeTest {

		[Test]
		[Category ("NotWorking")]
		public void Default ()
		{
			KeyContainerPermissionAttribute a = new KeyContainerPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (KeyContainerPermissionFlags.NoFlags, a.Flags, "Flags");
			Assert.IsNull (a.KeyContainerName, "KeyContainerName");
			Assert.AreEqual (-1, a.KeySpec, "KeySpec");
			Assert.IsNull (a.KeyStore, "KeyStore");
			Assert.IsNull (a.ProviderName, "ProviderName");
			Assert.AreEqual (-1, a.ProviderType, "ProviderType");

			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");

			KeyContainerPermission perm = (KeyContainerPermission)a.CreatePermission ();
			Assert.AreEqual (KeyContainerPermissionFlags.NoFlags, perm.Flags, "perm.Flags");
			Assert.IsFalse (perm.IsUnrestricted (), "perm.Unrestricted");
		}

		[Test]
		public void Action ()
		{
			KeyContainerPermissionAttribute a = new KeyContainerPermissionAttribute (SecurityAction.Assert);
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
			KeyContainerPermissionAttribute a = new KeyContainerPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		private KeyContainerPermissionAttribute Empty ()
		{
			return new KeyContainerPermissionAttribute (SecurityAction.Assert);
		}

		[Test]
		public void KeyContainerName ()
		{
			KeyContainerPermissionAttribute a = Empty ();
			a.KeyContainerName = "mono";
			Assert.AreEqual ("mono", a.KeyContainerName, "KeyContainerName-1");
			a.KeyContainerName = null;
			Assert.IsNull (a.KeyContainerName, "KeyContainerName-2");
			a.KeyContainerName = String.Empty;
			Assert.AreEqual (String.Empty, a.KeyContainerName, "KeyContainerName-3");
			Assert.AreEqual (KeyContainerPermissionFlags.NoFlags, a.Flags, "Flags");
		}

		[Test]
		public void KeySpec ()
		{
			KeyContainerPermissionAttribute a = Empty ();
			a.KeySpec = Int32.MinValue;
			Assert.AreEqual (Int32.MinValue, a.KeySpec, "KeySpec-1");
			a.KeySpec = 0;
			Assert.AreEqual (0, a.KeySpec, "KeySpec-2");
			a.KeySpec = Int32.MaxValue;
			Assert.AreEqual (Int32.MaxValue, a.KeySpec, "KeySpec-3");
			Assert.AreEqual (KeyContainerPermissionFlags.NoFlags, a.Flags, "Flags");
		}

		[Test]
		public void KeyStore ()
		{
			KeyContainerPermissionAttribute a = Empty ();
			a.KeyStore = "mono";
			Assert.AreEqual ("mono", a.KeyStore, "KeyStore-1");
			a.KeyStore = null;
			Assert.IsNull (a.KeyStore, "KeyStore-2");
			a.KeyStore = String.Empty;
			Assert.AreEqual (String.Empty, a.KeyStore, "KeyStore-3");
			Assert.AreEqual (KeyContainerPermissionFlags.NoFlags, a.Flags, "Flags");
		}

		[Test]
		public void ProviderName ()
		{
			KeyContainerPermissionAttribute a = Empty ();
			a.ProviderName = "mono";
			Assert.AreEqual ("mono", a.ProviderName, "ProviderName-1");
			a.ProviderName = null;
			Assert.IsNull (a.ProviderName, "ProviderName-2");
			a.ProviderName = String.Empty;
			Assert.AreEqual (String.Empty, a.ProviderName, "ProviderName-3");
			Assert.AreEqual (KeyContainerPermissionFlags.NoFlags, a.Flags, "Flags");
		}

		[Test]
		public void ProviderType ()
		{
			KeyContainerPermissionAttribute a = Empty ();
			a.ProviderType = Int32.MinValue;
			Assert.AreEqual (Int32.MinValue, a.ProviderType, "ProviderType-1");
			a.ProviderType = 0;
			Assert.AreEqual (0, a.ProviderType, "ProviderType-2");
			a.ProviderType = Int32.MaxValue;
			Assert.AreEqual (Int32.MaxValue, a.ProviderType, "ProviderType-3");
			Assert.AreEqual (KeyContainerPermissionFlags.NoFlags, a.Flags, "Flags");
		}

		[Test]
		public void Unrestricted ()
		{
			KeyContainerPermissionAttribute a = Empty ();
			a.Unrestricted = true;
			Assert.AreEqual (KeyContainerPermissionFlags.NoFlags, a.Flags, "Flags");

			KeyContainerPermission perm = (KeyContainerPermission)a.CreatePermission ();
			Assert.AreEqual (KeyContainerPermissionFlags.AllFlags, perm.Flags, "CreatePermission.Flags");
		}

		[Test]
		public void Flags ()
		{
			KeyContainerPermissionAttribute a = Empty ();
			a.Flags = KeyContainerPermissionFlags.AllFlags;
			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.AreEqual (KeyContainerPermissionFlags.AllFlags, a.Flags, "Flags");
		}

		[Test]
		public void Flags_Invalid ()
		{
			KeyContainerPermissionAttribute a = Empty ();
			a.Flags = ((KeyContainerPermissionFlags)Int32.MinValue);
			// no validations for flags
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (KeyContainerPermissionAttribute);
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
