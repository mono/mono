//
// SmtpPermissionAttributeTest.cs -
//	NUnit Test Cases for SmtpPermissionAttribute
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Net.Mail;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Net.Mail {

	[TestFixture]
	public class SmtpPermissionAttributeTest {

		[Test]
		public void Default ()
		{
			SmtpPermissionAttribute a = new SmtpPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.IsNull (a.Access, "Access");

			SmtpPermission perm = (SmtpPermission) a.CreatePermission ();
			Assert.IsFalse (a.Unrestricted, "Unrestricted");
		}

		private void Access (SmtpPermissionAttribute a, string s)
		{
			a.Access = s;
			Assert.AreEqual (s, a.Access, s);
		}

		[Test]
		public void Access ()
		{
			SmtpPermissionAttribute a = new SmtpPermissionAttribute (SecurityAction.Assert);
			Assert.IsNull (a.Access, "Null-default");

			Access (a, String.Empty);

			Access (a, "None");
			Access (a, "none");
			Access (a, "NONE");
			Access (a, "nOnE");

			Access (a, "Connect");
			Access (a, "connect");
			Access (a, "CONNECT");
			Access (a, "cOnNeCt");

			a.Access = null;
			Assert.IsNull (a.Access, "Null");
		}

		[Test]
		public void Access_Invalid ()
		{
			SmtpPermissionAttribute a = new SmtpPermissionAttribute (SecurityAction.Assert);
			a.Access = "invalid";
			Assert.AreEqual ("invalid", a.Access, "invalid");
			// no validation in attribute
		}

		[Test]
		public void Action ()
		{
			SmtpPermissionAttribute a = new SmtpPermissionAttribute (SecurityAction.Assert);
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
			SmtpPermissionAttribute a = new SmtpPermissionAttribute ((SecurityAction) Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreatePermission_Invalid ()
		{
			SmtpPermissionAttribute a = new SmtpPermissionAttribute (SecurityAction.Assert);
			a.Access = String.Empty;
			Assert.AreEqual (0, a.Access.Length, "Empty");
			Assert.IsNotNull (a.CreatePermission (), "Empty-Permission");
		}

		[Test]
		public void Unrestricted ()
		{
			SmtpPermissionAttribute a = new SmtpPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			SmtpPermission dp = (SmtpPermission) a.CreatePermission ();
			Assert.IsTrue (dp.IsUnrestricted (), "IsUnrestricted");

			a.Unrestricted = false;
			dp = (SmtpPermission) a.CreatePermission ();
			Assert.IsFalse (dp.IsUnrestricted (), "!IsUnrestricted");
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (SmtpPermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object[] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute) attrs[0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}

#endif
