//
// StorePermissionAttributeTest.cs -
//	NUnit Test Cases for StorePermissionAttributeTest
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
	public class StorePermissionAttributeTest {

		[Test]
		public void Default ()
		{
			StorePermissionAttribute a = new StorePermissionAttribute (SecurityAction.Assert);
			Assert.IsFalse (a.AddToStore, "AddToStore");
			Assert.IsFalse (a.CreateStore, "CreateStore");
			Assert.IsFalse (a.DeleteStore, "DeleteStore");
			Assert.IsFalse (a.EnumerateCertificates, "EnumerateCertificates");
			Assert.IsFalse (a.EnumerateStores, "EnumerateStores");
			Assert.IsFalse (a.OpenStore, "OpenStore");
			Assert.IsFalse (a.RemoveFromStore, "RemoveFromStore");

			Assert.AreEqual (StorePermissionFlags.NoFlags, a.Flags, "Flags");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");

			StorePermission perm = (StorePermission)a.CreatePermission ();
			Assert.AreEqual (StorePermissionFlags.NoFlags, perm.Flags, "CreatePermission.Flags");
		}

		[Test]
		public void Action ()
		{
			StorePermissionAttribute a = new StorePermissionAttribute (SecurityAction.Assert);
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
			StorePermissionAttribute a = new StorePermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		private StorePermissionAttribute Empty ()
		{
			StorePermissionAttribute a = new StorePermissionAttribute (SecurityAction.Assert);
			a.AddToStore = false;
			a.CreateStore = false;
			a.DeleteStore = false;
			a.EnumerateCertificates = false;
			a.EnumerateStores = false;
			a.OpenStore = false;
			a.RemoveFromStore = false;
			Assert.AreEqual (StorePermissionFlags.NoFlags, a.Flags, "Flags");
			return a;
		}

		[Test]
		public void AddToStore ()
		{
			StorePermissionAttribute a = Empty ();
			a.AddToStore = true;
			Assert.AreEqual (StorePermissionFlags.AddToStore, a.Flags, "Flags=AddToStore");
			a.AddToStore = false;
			Assert.AreEqual (StorePermissionFlags.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void CreateStore ()
		{
			StorePermissionAttribute a = Empty ();
			a.CreateStore = true;
			Assert.AreEqual (StorePermissionFlags.CreateStore, a.Flags, "Flags=CreateStore");
			a.CreateStore = false;
			Assert.AreEqual (StorePermissionFlags.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void DeleteStore ()
		{
			StorePermissionAttribute a = Empty ();
			a.DeleteStore = true;
			Assert.AreEqual (StorePermissionFlags.DeleteStore, a.Flags, "Flags=DeleteStore");
			a.DeleteStore = false;
			Assert.AreEqual (StorePermissionFlags.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void EnumerateCertificates ()
		{
			StorePermissionAttribute a = Empty ();
			a.EnumerateCertificates = true;
			Assert.AreEqual (StorePermissionFlags.EnumerateCertificates, a.Flags, "Flags=EnumerateCertificates");
			a.EnumerateCertificates = false;
			Assert.AreEqual (StorePermissionFlags.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void EnumerateStores ()
		{
			StorePermissionAttribute a = Empty ();
			a.EnumerateStores = true;
			Assert.AreEqual (StorePermissionFlags.EnumerateStores, a.Flags, "Flags=EnumerateStores");
			a.EnumerateStores = false;
			Assert.AreEqual (StorePermissionFlags.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void OpenStore ()
		{
			StorePermissionAttribute a = Empty ();
			a.OpenStore = true;
			Assert.AreEqual (StorePermissionFlags.OpenStore, a.Flags, "Flags=OpenStore");
			a.OpenStore = false;
			Assert.AreEqual (StorePermissionFlags.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void RemoveFromStore ()
		{
			StorePermissionAttribute a = Empty ();
			a.RemoveFromStore = true;
			Assert.AreEqual (StorePermissionFlags.RemoveFromStore, a.Flags, "Flags=RemoveFromStore");
			a.RemoveFromStore = false;
			Assert.AreEqual (StorePermissionFlags.NoFlags, a.Flags, "Flags=NoFlags");
		}

		[Test]
		public void Unrestricted ()
		{
			StorePermissionAttribute a = Empty ();
			a.Unrestricted = true;
			Assert.AreEqual (StorePermissionFlags.NoFlags, a.Flags, "Unrestricted");

			StorePermission perm = (StorePermission)a.CreatePermission ();
			Assert.AreEqual (StorePermissionFlags.AllFlags, perm.Flags, "CreatePermission.Flags");
		}

		[Test]
		public void Flags ()
		{
			StorePermissionAttribute a = Empty ();
			a.Flags = StorePermissionFlags.AddToStore;
			Assert.IsTrue (a.AddToStore, "AddToStore");
			a.Flags |= StorePermissionFlags.CreateStore;
			Assert.IsTrue (a.CreateStore, "CreateStore");
			a.Flags |= StorePermissionFlags.DeleteStore;
			Assert.IsTrue (a.DeleteStore, "DeleteStore");
			a.Flags |= StorePermissionFlags.EnumerateCertificates;
			Assert.IsTrue (a.EnumerateCertificates, "EnumerateCertificates");
			a.Flags |= StorePermissionFlags.EnumerateStores;
			Assert.IsTrue (a.EnumerateStores, "EnumerateStores");
			a.Flags |= StorePermissionFlags.OpenStore;
			Assert.IsTrue (a.OpenStore, "OpenStore");
			a.Flags |= StorePermissionFlags.RemoveFromStore;
			Assert.IsTrue (a.RemoveFromStore, "RemoveFromStore");

			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.AreEqual (StorePermissionFlags.AllFlags, a.Flags, "Flags=AllFlags");

			a.Flags &= ~StorePermissionFlags.AddToStore;
			Assert.IsFalse (a.AddToStore, "AddToStore");
			a.Flags &= ~StorePermissionFlags.CreateStore;
			Assert.IsFalse (a.CreateStore, "CreateStore");
			a.Flags &= ~StorePermissionFlags.DeleteStore;
			Assert.IsFalse (a.DeleteStore, "DeleteStore");
			a.Flags &= ~StorePermissionFlags.EnumerateCertificates;
			Assert.IsFalse (a.EnumerateCertificates, "EnumerateCertificates");
			a.Flags &= ~StorePermissionFlags.EnumerateStores;
			Assert.IsFalse (a.EnumerateStores, "EnumerateStores");
			a.Flags &= ~StorePermissionFlags.OpenStore;
			Assert.IsFalse (a.OpenStore, "OpenStore");
			a.Flags &= ~StorePermissionFlags.RemoveFromStore;
			Assert.IsFalse (a.RemoveFromStore, "RemoveFromStore");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Flags_Invalid ()
		{
			StorePermissionAttribute a = Empty ();
			a.Flags = ((StorePermissionFlags)Int32.MinValue);
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (StorePermissionAttribute);
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
