//
// ZoneIdentityPermissionAttributeTest.cs - 
//	NUnit Test Cases for ZoneIdentityPermissionAttribute
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
	public class ZoneIdentityPermissionAttributeTest {

		[Test]
		public void Default () 
		{
			ZoneIdentityPermissionAttribute a = new ZoneIdentityPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.AreEqual (SecurityZone.NoZone, a.Zone, "Zone");

			ZoneIdentityPermission perm = (ZoneIdentityPermission) a.CreatePermission ();
			Assert.AreEqual (SecurityZone.NoZone, perm.SecurityZone, "CreatePermission-SecurityZone");
		}

		[Test]
		public void Action () 
		{
			ZoneIdentityPermissionAttribute a = new ZoneIdentityPermissionAttribute (SecurityAction.Assert);
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
			ZoneIdentityPermissionAttribute a = new ZoneIdentityPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Zone () 
		{
			ZoneIdentityPermissionAttribute a = new ZoneIdentityPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (SecurityZone.NoZone, a.Zone, "Zone=default");
			a.Zone = SecurityZone.Internet;
			Assert.AreEqual (SecurityZone.Internet, a.Zone, "Zone=Internet");
			a.Zone = SecurityZone.Intranet;
			Assert.AreEqual (SecurityZone.Intranet, a.Zone, "Zone=Intranet");
			a.Zone = SecurityZone.MyComputer;
			Assert.AreEqual (SecurityZone.MyComputer, a.Zone, "Zone=MyComputer");
			a.Zone = SecurityZone.NoZone;
			Assert.AreEqual (SecurityZone.NoZone, a.Zone, "Zone=NoZone");
			a.Zone = SecurityZone.Trusted;
			Assert.AreEqual (SecurityZone.Trusted, a.Zone, "Zone=Trusted");
			a.Zone = SecurityZone.Untrusted;
			Assert.AreEqual (SecurityZone.Untrusted, a.Zone, "Zone=Untrusted");
		}

		[Test]
		public void Zone_Invalid ()
		{
			ZoneIdentityPermissionAttribute a = new ZoneIdentityPermissionAttribute (SecurityAction.Assert);
			a.Zone = (SecurityZone)Int32.MinValue;
			// no validation in attribute
		}

		[Test]
		public void TypeId () 
		{
			ZoneIdentityPermissionAttribute a = new ZoneIdentityPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
		}

		[Test]
		[Category ("NotWorking")]
		public void Unrestricted () 
		{
			ZoneIdentityPermissionAttribute a = new ZoneIdentityPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (ZoneIdentityPermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object[] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
