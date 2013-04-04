//
// WebPermissionAttributeTest.cs - NUnit Test Cases for WebPermissionAttribute
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
using System.Net;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace MonoTests.System.Net {

	[TestFixture]
#if MOBILE
	[Ignore ("CAS is not supported and parts will be linked away")]
#endif
	public class WebPermissionAttributeTest {

		[Test]
		public void Default ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			WebPermission wp = (WebPermission)a.CreatePermission ();
			Assert.IsFalse (wp.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void Action ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
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
			WebPermissionAttribute a = new WebPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Unrestricted ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			WebPermission wp = (WebPermission)a.CreatePermission ();
			Assert.IsTrue (wp.IsUnrestricted (), "IsUnrestricted");

			a.Unrestricted = false;
			wp = (WebPermission)a.CreatePermission ();
			Assert.IsFalse (wp.IsUnrestricted (), "!IsUnrestricted");
		}

#if NET_2_0
		[Test]
		public void Accept_Null ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Accept = null; // legal
			Assert.IsNull (a.Accept, "Accept");
			Assert.IsNull (a.Connect, "Connect");
		}
#else

		[Test]
		// Strangely, although you can set Accept to value of null, you cannot
		// then examine the value without throwing a NullRef
		[ExpectedException (typeof (NullReferenceException))]
		public void Accept_Null ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Accept = null; // legal
			Assert.IsNull (a.Connect, "Connect");
		}

		[Test]
		// Strangely, although you can set Accept to value of null, you cannot
		// then examine the value without throwing a NullRef
		[ExpectedException (typeof (NullReferenceException))]
		public void Accept_Null2 ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Accept = null; // legal
			Assert.IsNull (a.Accept, "Accept");
		}

#endif

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Accept_Dual ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Accept = "/";
			a.Accept = "\\";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Accept_Dual_Null ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Accept = "/";
			a.Accept = null;
		}

		[Test]
		public void Accept ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Accept = "/";
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AcceptPattern_Null ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.AcceptPattern = null; 
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AcceptPattern_Dual ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.AcceptPattern = "/";
			a.AcceptPattern = "\\";
		}

#if NET_2_0
		[Test]
		public void AcceptPattern ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.AcceptPattern = "\b(?"; // invalid regex expression
			Assert.AreEqual ("\b(?", a.AcceptPattern, "AcceptPattern");
			Assert.IsNull (a.ConnectPattern, "ConnectPattern");
		}
#endif

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Accept_AcceptPattern ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Accept = "/";
			a.AcceptPattern = "\\";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AcceptPattern_Accept ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.AcceptPattern = "/";
			a.Accept = "\\";
		}

#if NET_2_0
		[Test]
		public void Connect_Null ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Connect = null; // legal
			Assert.IsNull (a.Accept, "Accept");
			Assert.IsNull (a.Connect, "Connect");
		}
#else
		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Connect_Null ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Connect = null; // legal
			Assert.IsNull (a.Accept, "Accept");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Connect_Null2 ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Connect = null; // legal
			Assert.IsNull (a.Connect, "Connect");
		}
#endif

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Connect_Dual ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Connect = "/";
			a.Connect = "\\";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Connect_Dual_Null ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Connect = "/";
			a.Connect = null;
		}

#if NET_2_0
		[Test]
		public void Connect ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Connect = "/";
			Assert.IsNull (a.Accept, "Accept");
			Assert.AreEqual ("/", a.Connect, "Connect");
		}
#endif

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConnectPattern_Null ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.ConnectPattern = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConnectPattern_Dual ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.ConnectPattern = "/";
			a.ConnectPattern = "\\";
		}

#if NET_2_0
		[Test]
		public void ConnectPattern ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.ConnectPattern = "\b(?"; // invalid regex expression
			Assert.IsNull (a.AcceptPattern, "AcceptPattern");
			Assert.AreEqual ("\b(?", a.ConnectPattern, "ConnectPattern");
		}
#endif

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Connect_ConnectPattern ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.Connect = "/";
			a.ConnectPattern = "\\";
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConnectPattern_Accept ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.ConnectPattern = "/";
			a.Connect = "\\";
		}

#if NET_2_0
		[Test]
		public void CreatePermission_InvalidRegex ()
		{
			WebPermissionAttribute a = new WebPermissionAttribute (SecurityAction.Assert);
			a.AcceptPattern = "\b(?"; // invalid regex expression
			a.ConnectPattern = "\b(?"; // invalid regex expression
			WebPermission wp = (WebPermission) a.CreatePermission ();
			Assert.IsNotNull (wp, "CreatePermission");
		}
#endif

		[Test]
		public void Attributes ()
		{
			Type t = typeof (WebPermissionAttribute);
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
