//
// ZoneIdentityPermissionAttributeTest.cs - NUnit Test Cases for ZoneIdentityPermissionAttribute
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
	public class ZoneIdentityPermissionAttributeTest : Assertion {

		[Test]
		public void Default () 
		{
			ZoneIdentityPermissionAttribute a = new ZoneIdentityPermissionAttribute (SecurityAction.Assert);
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
			Assert ("Unrestricted", !a.Unrestricted);
			AssertEquals ("Zone", SecurityZone.NoZone, a.Zone);

			ZoneIdentityPermission perm = (ZoneIdentityPermission) a.CreatePermission ();
			AssertEquals ("CreatePermission-SecurityZone", SecurityZone.NoZone, perm.SecurityZone);
		}

		[Test]
		public void Action () 
		{
			ZoneIdentityPermissionAttribute a = new ZoneIdentityPermissionAttribute (SecurityAction.Assert);
			AssertEquals ("Action=Assert", SecurityAction.Assert, a.Action);
			a.Action = SecurityAction.Demand;
			AssertEquals ("Action=Demand", SecurityAction.Demand, a.Action);
			a.Action = SecurityAction.Deny;
			AssertEquals ("Action=Deny", SecurityAction.Deny, a.Action);
			a.Action = SecurityAction.InheritanceDemand;
			AssertEquals ("Action=InheritanceDemand", SecurityAction.InheritanceDemand, a.Action);
			a.Action = SecurityAction.LinkDemand;
			AssertEquals ("Action=LinkDemand", SecurityAction.LinkDemand, a.Action);
			a.Action = SecurityAction.PermitOnly;
			AssertEquals ("Action=PermitOnly", SecurityAction.PermitOnly, a.Action);
			a.Action = SecurityAction.RequestMinimum;
			AssertEquals ("Action=RequestMinimum", SecurityAction.RequestMinimum, a.Action);
			a.Action = SecurityAction.RequestOptional;
			AssertEquals ("Action=RequestOptional", SecurityAction.RequestOptional, a.Action);
			a.Action = SecurityAction.RequestRefuse;
			AssertEquals ("Action=RequestRefuse", SecurityAction.RequestRefuse, a.Action);
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
			AssertEquals ("Zone=default", SecurityZone.NoZone, a.Zone);
			a.Zone = SecurityZone.Internet;
			AssertEquals ("Zone=Internet", SecurityZone.Internet, a.Zone);
			a.Zone = SecurityZone.Intranet;
			AssertEquals ("Zone=Intranet", SecurityZone.Intranet, a.Zone);
			a.Zone = SecurityZone.MyComputer;
			AssertEquals ("Zone=MyComputer", SecurityZone.MyComputer, a.Zone);
			a.Zone = SecurityZone.NoZone;
			AssertEquals ("Zone=NoZone", SecurityZone.NoZone, a.Zone);
			a.Zone = SecurityZone.Trusted;
			AssertEquals ("Zone=Trusted", SecurityZone.Trusted, a.Zone);
			a.Zone = SecurityZone.Untrusted;
			AssertEquals ("Zone=Untrusted", SecurityZone.Untrusted, a.Zone);
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
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Unrestricted () 
		{
			ZoneIdentityPermissionAttribute a = new ZoneIdentityPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			IPermission perm = a.CreatePermission ();
		}
	}
}
