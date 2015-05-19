//
// UrlIdentityPermissionAttributeTest.cs -
//	NUnit Test Cases for UrlIdentityPermissionAttribute
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
	public class UrlIdentityPermissionAttributeTest {

		[Test]
		public void Default () 
		{
			UrlIdentityPermissionAttribute a = new UrlIdentityPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.IsNull (a.Url, "Url");
		}

		[Test]
		public void DefaultPermission () 
		{
			UrlIdentityPermissionAttribute a = new UrlIdentityPermissionAttribute (SecurityAction.Assert);
			Assert.IsNull (a.Url, "Url");
			// UrlIdentityPermission would throw a ArgumentNullException for a null URL ...
			UrlIdentityPermission perm = (UrlIdentityPermission) a.CreatePermission ();
			// ... but this works ...
			Assert.IsNotNull (perm, "CreatePermission(null url)");
			// ... but this doesn't!
			string url = perm.Url;
			Assert.AreEqual (String.Empty, url, "Url-2");
		}

		[Category ("NotWorking")]
		[Test]
		public void Action () 
		{
			UrlIdentityPermissionAttribute a = new UrlIdentityPermissionAttribute (SecurityAction.Assert);
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
			UrlIdentityPermissionAttribute a = new UrlIdentityPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Url () 
		{
			UrlIdentityPermissionAttribute a = new UrlIdentityPermissionAttribute (SecurityAction.Assert);
			a.Url = "mono";
			Assert.AreEqual ("mono", a.Url, "Url-1");
			a.Url = null;
			Assert.IsNull (a.Url, "Url-2");
			a.Url = "mono again";
			Assert.AreEqual ("mono again", a.Url, "Url-3");
		}

		[Test]
		[Category ("NotWorking")]
		public void Unrestricted () 
		{
			UrlIdentityPermissionAttribute a = new UrlIdentityPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			IPermission perm = a.CreatePermission ();
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (UrlIdentityPermissionAttribute);
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
