//
// SocketPermissionAttributeTest.cs - NUnit Test Cases for SocketPermissionAttribute
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

#if !MOBILE

using NUnit.Framework;
using System;
using System.Net;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace MonoTests.System.Net {

	[TestFixture]
	public class SocketPermissionAttributeTest {

		[Test]
		public void Default ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.IsNull (a.Access, "Access");
			Assert.IsNull (a.Host, "Host");
			Assert.IsNull (a.Port, "Port");
			Assert.IsNull (a.Transport, "Transport");

			a.Access = "connect";
			a.Host = String.Empty;
			a.Port = "80";
			a.Transport = "tcp";
			SocketPermission sp = (SocketPermission) a.CreatePermission ();
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void Action ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
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
			SocketPermissionAttribute a = new SocketPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Unrestricted ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Access = "connect";
			a.Host = String.Empty;
			a.Port = "80";
			a.Transport = "tcp";
			a.Unrestricted = true;
			SocketPermission wp = (SocketPermission)a.CreatePermission ();
			Assert.IsTrue (wp.IsUnrestricted (), "IsUnrestricted");

			a.Unrestricted = false;
			wp = (SocketPermission)a.CreatePermission ();
			Assert.IsFalse (wp.IsUnrestricted (), "!IsUnrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Access_Null_CreatePermission ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Access = null; // legal (assign)
			SocketPermission sp = (SocketPermission) a.CreatePermission ();
		}

		[Test]
		public void Access_Null ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Access = null; // legal
			Assert.IsNull (a.Access, "Access");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Access_Dual ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Access = "/";
			a.Access = "\\";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Access_Dual_Null ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Access = "/";
			a.Access = null;
		}

		[Test]
		public void Access ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Access = "/";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Host_Null_CreatePermission ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Access = String.Empty;
			a.Host = null; // legal (assign)
			SocketPermission sp = (SocketPermission)a.CreatePermission ();
		}

		[Test]
		public void Host_Null ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Host = null; // legal
			Assert.IsNull (a.Host, "Host");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Host_Dual ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Host = "/";
			a.Host = "\\";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Host_Dual_Null ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Host = "/";
			a.Host = null;
		}

		[Test]
		public void Host ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Host = "www.mono-project.com";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Port_Null_CreatePermission ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Access = String.Empty;
			a.Host = String.Empty;
			a.Port = null; // legal (assign)
			SocketPermission sp = (SocketPermission)a.CreatePermission ();
		}

		[Test]
		public void Port_Null ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Port = null; // legal
			Assert.IsNull (a.Port, "Port");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Port_Dual ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Port = "/";
			a.Port = "\\";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Port_Dual_Null ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Port = "/";
			a.Port = null;
		}

		[Test]
		public void Port ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Port = "80";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Transport_Null_CreatePermission ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Access = String.Empty;
			a.Host = String.Empty;
			a.Port = String.Empty;
			a.Transport = null; // legal (assign)
			SocketPermission sp = (SocketPermission)a.CreatePermission ();
		}

		[Test]
		public void Transport_Null ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Transport = null; // legal
			Assert.IsNull (a.Transport, "Transport");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Transport_Dual ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Transport = "/";
			a.Transport = "\\";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Transport_Dual_Null ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Transport = "/";
			a.Transport = null;
		}

		[Test]
		public void Transport ()
		{
			SocketPermissionAttribute a = new SocketPermissionAttribute (SecurityAction.Assert);
			a.Transport = "http";
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (SocketPermissionAttribute);
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