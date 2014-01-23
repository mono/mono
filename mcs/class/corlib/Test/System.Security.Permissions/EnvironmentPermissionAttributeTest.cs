//
// EnvironmentPermissionAttributeTest.cs -
//	NUnit Test Cases for EnvironmentPermissionAttribute
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
#if MOBILE
	[Ignore]
#endif
	public class EnvironmentPermissionAttributeTest {

		private static string envar = "TMP";

		[Test]
		public void Default () 
		{
			EnvironmentPermissionAttribute a = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			EnvironmentPermission p = (EnvironmentPermission) a.CreatePermission ();
#if NET_2_0
			Assert.AreEqual (String.Empty, p.GetPathList (EnvironmentPermissionAccess.Read), "GetPathList(Read)");
			Assert.AreEqual (String.Empty, p.GetPathList (EnvironmentPermissionAccess.Write), "GetPathList(Write)");
#else
			Assert.IsNull (p.GetPathList (EnvironmentPermissionAccess.Read), "GetPathList(Read)");
			Assert.IsNull (p.GetPathList (EnvironmentPermissionAccess.Write), "GetPathList(Write)");
#endif
			Assert.IsFalse (p.IsUnrestricted (), "CreatePermission-IsUnrestricted");
		}

		[Test]
		public void Action () 
		{
			EnvironmentPermissionAttribute a = new EnvironmentPermissionAttribute (SecurityAction.Assert);
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
			EnvironmentPermissionAttribute a = new EnvironmentPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void All () 
		{
			EnvironmentPermissionAttribute attr = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			attr.All = envar;
			Assert.AreEqual (envar, attr.Read, "All=Read");
			Assert.AreEqual (envar, attr.Write, "All=Write");
			EnvironmentPermission p = (EnvironmentPermission) attr.CreatePermission ();
			Assert.AreEqual (envar, p.GetPathList (EnvironmentPermissionAccess.Read), "All=EnvironmentPermission-Read");
			Assert.AreEqual (envar, p.GetPathList (EnvironmentPermissionAccess.Write), "All=EnvironmentPermission-Write");
		}
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void All_Get () 
		{
			EnvironmentPermissionAttribute attr = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			string s = attr.All;
		}

		[Test]
		public void Read () 
		{
			EnvironmentPermissionAttribute attr = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			attr.Read = envar;
			Assert.AreEqual (envar, attr.Read, "Read=Read");
			Assert.IsNull (attr.Write, "Write=null");
			EnvironmentPermission p = (EnvironmentPermission) attr.CreatePermission ();
			Assert.AreEqual (envar, p.GetPathList (EnvironmentPermissionAccess.Read), "Read=EnvironmentPermission-Read");
#if NET_2_0
			Assert.AreEqual (String.Empty, p.GetPathList (EnvironmentPermissionAccess.Write), "Read=EnvironmentPermission-Write");
#else
			Assert.IsNull (p.GetPathList (EnvironmentPermissionAccess.Write), "Read=EnvironmentPermission-Write");
#endif
		}

		[Test]
		public void Write ()
		{
			EnvironmentPermissionAttribute attr = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			attr.Write = envar;
			Assert.IsNull (attr.Read, "Read=null");
			Assert.AreEqual (envar, attr.Write, "Write=Write");
			EnvironmentPermission p = (EnvironmentPermission) attr.CreatePermission ();
#if NET_2_0
			Assert.AreEqual (String.Empty, p.GetPathList (EnvironmentPermissionAccess.Read), "Write=EnvironmentPermission-Read");
#else
			Assert.IsNull (p.GetPathList (EnvironmentPermissionAccess.Read), "Write=EnvironmentPermission-Read");
#endif
			Assert.AreEqual (envar, p.GetPathList (EnvironmentPermissionAccess.Write), "Write=EnvironmentPermission-Write");
		}

		[Test]
		public void Unrestricted () 
		{
			EnvironmentPermissionAttribute a = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;

			EnvironmentPermission perm = (EnvironmentPermission) a.CreatePermission ();
			Assert.IsTrue (perm.IsUnrestricted (), "CreatePermission.IsUnrestricted");
			Assert.AreEqual (String.Empty, perm.GetPathList (EnvironmentPermissionAccess.Read), "GetPathList(Read)");
			Assert.AreEqual (String.Empty, perm.GetPathList (EnvironmentPermissionAccess.Write), "GetPathList(Write)");
		}

		[Test]
		public void Attributes ()
		{
			EnvironmentPermissionAttribute a = new EnvironmentPermissionAttribute (SecurityAction.Assert);
			Type t = typeof (EnvironmentPermissionAttribute);
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
