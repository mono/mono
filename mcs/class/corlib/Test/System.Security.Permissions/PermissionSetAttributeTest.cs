//
// PermissionSetAttributeTest.cs - NUnit Test Cases for PermissionSetAttribute
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
	public class PermissionSetAttributeTest {

		[Test]
		public void Default () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
			Assert.IsFalse (a.UnicodeEncoded, "UnicodeEncoded");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			IPermission perm = a.CreatePermission ();
			Assert.IsNull (perm, "CreatePermission");

			PermissionSet ps = a.CreatePermissionSet ();
			Assert.AreEqual (0, ps.Count, "CreatePermissionSet");
		}

		[Test]
		public void Action ()
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
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
#if NET_2_0
			a.Action = SecurityAction.DemandChoice;
			Assert.AreEqual (SecurityAction.DemandChoice, a.Action, "Action=DemandChoice");
			a.Action = SecurityAction.InheritanceDemandChoice;
			Assert.AreEqual (SecurityAction.InheritanceDemandChoice, a.Action, "Action=InheritanceDemandChoice");
			a.Action = SecurityAction.LinkDemandChoice;
			Assert.AreEqual (SecurityAction.LinkDemandChoice, a.Action, "Action=LinkDemandChoice");
#endif
		}

		[Test]
		public void Action_Invalid ()
		{
			PermissionSetAttribute a = new PermissionSetAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void File () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.File = "mono";
			Assert.AreEqual ("mono", a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
			a.File = null;
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
		}

#if NET_2_0
		[Test]
		public void Hex ()
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			Assert.IsNull (a.Hex, "Hex-1");
			a.Hex = String.Empty;
			Assert.AreEqual (String.Empty, a.Hex, "Hex-2");
			a.Hex = null;
			Assert.IsNull (a.Hex, "Hex-3");
		}
#endif

		[Test]
		public void Name () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.Name = "mono";
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.AreEqual ("mono", a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
			a.Name = null;
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
		}

		[Test]
		public void XML () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.XML = "mono";
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.AreEqual ("mono", a.XML, "XML");
			a.XML = null;
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
		}

		[Test]
		public void UnicodeEncoded () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.UnicodeEncoded = true;
			Assert.IsTrue (a.UnicodeEncoded, "UnicodeEncoded-true");
			a.UnicodeEncoded = false;
			Assert.IsFalse (a.UnicodeEncoded, "UnicodeEncoded-false");
		}

		[Test]
		public void Unrestricted () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");

			PermissionSet ps = a.CreatePermissionSet ();
			Assert.IsTrue (ps.IsUnrestricted (), "CreatePermissionSet.IsUnrestricted");
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (PermissionSetAttribute);
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
