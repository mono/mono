//
// ServiceControllerPermissionAttributeTest.cs -
//	NUnit Test Cases for ServiceControllerPermissionAttribute
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.ServiceProcess;

namespace MonoTests.System.ServiceProcess {

	[TestFixture]
	public class ServiceControllerPermissionAttributeTest {

		[Test]
		public void Default ()
		{
			ServiceControllerPermissionAttribute a = new ServiceControllerPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.AreEqual (".", a.MachineName, "MachineName");
			Assert.AreEqual (ServiceControllerPermissionAccess.Browse, a.PermissionAccess, "PermissionAccess");

			ServiceControllerPermission sp = (ServiceControllerPermission)a.CreatePermission ();
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void Action ()
		{
			ServiceControllerPermissionAttribute a = new ServiceControllerPermissionAttribute (SecurityAction.Assert);
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
			new ServiceControllerPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Unrestricted ()
		{
			ServiceControllerPermissionAttribute a = new ServiceControllerPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			ServiceControllerPermission wp = (ServiceControllerPermission)a.CreatePermission ();
			Assert.IsTrue (wp.IsUnrestricted (), "IsUnrestricted");

			a.Unrestricted = false;
			wp = (ServiceControllerPermission)a.CreatePermission ();
			Assert.IsFalse (wp.IsUnrestricted (), "!IsUnrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MachineName_Null ()
		{
			ServiceControllerPermissionAttribute a = new ServiceControllerPermissionAttribute (SecurityAction.Assert);
			a.MachineName = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MachineName_Empty ()
		{
			ServiceControllerPermissionAttribute a = new ServiceControllerPermissionAttribute (SecurityAction.Assert);
			a.MachineName = String.Empty;
		}

		[Test]
		public void MachineName ()
		{
			ServiceControllerPermissionAttribute a = new ServiceControllerPermissionAttribute (SecurityAction.Assert);
			for (int i = 0; i < 256; i++) {
				try {
					a.MachineName = Convert.ToChar (i).ToString ();
					Assert.AreEqual (i, (int)a.MachineName [0], i.ToString ());
				}
				catch {
					switch (i) {
						case 9:
						case 10:
						case 11:
						case 12:
						case 13:
						case 32:
						case 92:
#if NET_2_0
						case 133:
#endif
						case 160:
							// known invalid chars
							break;
						default:
							Assert.Fail (i.ToString ());
							break;
					}
				}
			}
			// all first 256 characters seems to be valid
			// is there other rules ?
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ServiceName_Null ()
		{
			ServiceControllerPermissionAttribute a = new ServiceControllerPermissionAttribute (SecurityAction.Assert);
			a.ServiceName = null;
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void ServiceName_Empty ()
		{
			ServiceControllerPermissionAttribute a = new ServiceControllerPermissionAttribute (SecurityAction.Assert);
			a.ServiceName = String.Empty;
		}

		[Test]
		public void ServiceName ()
		{
			ServiceControllerPermissionAttribute a = new ServiceControllerPermissionAttribute (SecurityAction.Assert);
			for (int i = 0; i < 256; i++) {
				try {
					a.ServiceName = Convert.ToChar (i).ToString ();
					Assert.AreEqual (i, (int)a.ServiceName [0], i.ToString ());
				}
				catch {
					switch (i) {
						case 47:
						case 92:
							// known invalid chars
							break;
						default:
							Assert.Fail (i.ToString ());
							break;
					}
				}
			}
			// all first 256 characters seems to be valid
			// is there other rules ?
		}

		[Test]
		public void PermissionAccess ()
		{
			ServiceControllerPermissionAttribute a = new ServiceControllerPermissionAttribute (SecurityAction.Assert);
			a.PermissionAccess = ServiceControllerPermissionAccess.None;
			Assert.AreEqual (ServiceControllerPermissionAccess.None, a.PermissionAccess, "None");
			a.PermissionAccess = ServiceControllerPermissionAccess.Browse;
			Assert.AreEqual (ServiceControllerPermissionAccess.Browse, a.PermissionAccess, "Browse");
			a.PermissionAccess = ServiceControllerPermissionAccess.Control;
			Assert.AreEqual (ServiceControllerPermissionAccess.Control, a.PermissionAccess, "Control");
		}

		[Test]
		public void PermissionAccess_Invalid ()
		{
			ServiceControllerPermissionAttribute a = new ServiceControllerPermissionAttribute (SecurityAction.Assert);
			a.PermissionAccess = (ServiceControllerPermissionAccess)Int32.MinValue;
			Assert.AreEqual ((ServiceControllerPermissionAccess)Int32.MinValue, a.PermissionAccess, "None");
			// no exception thrown
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (ServiceControllerPermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object [] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Event);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
