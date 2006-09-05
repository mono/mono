//
// DirectoryServicesPermissionAttributeTest.cs -
//	NUnit Test Cases for DirectoryServicesPermissionAttribute
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
using System.DirectoryServices;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.DirectoryServices {

	[TestFixture]
	public class DirectoryServicesPermissionAttributeTest {

		[Test]
		public void Default ()
		{
			DirectoryServicesPermissionAttribute a = new DirectoryServicesPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.AreEqual ("*", a.Path, "Path");
			Assert.AreEqual (DirectoryServicesPermissionAccess.Browse, a.PermissionAccess, "PermissionAccess");

			DirectoryServicesPermission sp = (DirectoryServicesPermission)a.CreatePermission ();
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void Action ()
		{
			DirectoryServicesPermissionAttribute a = new DirectoryServicesPermissionAttribute (SecurityAction.Assert);
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
			DirectoryServicesPermissionAttribute a = new DirectoryServicesPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Unrestricted ()
		{
			DirectoryServicesPermissionAttribute a = new DirectoryServicesPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			DirectoryServicesPermission wp = (DirectoryServicesPermission)a.CreatePermission ();
			Assert.IsTrue (wp.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual ("*", a.Path, "Path");
			Assert.AreEqual (DirectoryServicesPermissionAccess.Browse, a.PermissionAccess, "PermissionAccess");

			a.Unrestricted = false;
			wp = (DirectoryServicesPermission)a.CreatePermission ();
			Assert.IsFalse (wp.IsUnrestricted (), "!IsUnrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Path_Null ()
		{
			DirectoryServicesPermissionAttribute a = new DirectoryServicesPermissionAttribute (SecurityAction.Assert);
			a.Path = null;
		}

		[Test]
		public void Path ()
		{
			DirectoryServicesPermissionAttribute a = new DirectoryServicesPermissionAttribute (SecurityAction.Assert);
			a.Path = String.Empty;
			Assert.AreEqual (String.Empty, a.Path, "Empty");
		}

		[Test]
		public void PermissionAccess ()
		{
			DirectoryServicesPermissionAttribute a = new DirectoryServicesPermissionAttribute (SecurityAction.Assert);
			a.PermissionAccess = DirectoryServicesPermissionAccess.None;
			Assert.AreEqual (DirectoryServicesPermissionAccess.None, a.PermissionAccess, "None");
			a.PermissionAccess = DirectoryServicesPermissionAccess.Browse;
			Assert.AreEqual (DirectoryServicesPermissionAccess.Browse, a.PermissionAccess, "Browse");
			a.PermissionAccess = DirectoryServicesPermissionAccess.Write;
			Assert.AreEqual (DirectoryServicesPermissionAccess.Write, a.PermissionAccess, "Write");
		}

		[Test]
		public void PermissionAccess_Invalid ()
		{
			DirectoryServicesPermissionAttribute a = new DirectoryServicesPermissionAttribute (SecurityAction.Assert);
			a.PermissionAccess = (DirectoryServicesPermissionAccess)Int32.MinValue;
			Assert.AreEqual ((DirectoryServicesPermissionAccess)Int32.MinValue, a.PermissionAccess, "None");
			// no exception thrown
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (DirectoryServicesPermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object [] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Event;
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
