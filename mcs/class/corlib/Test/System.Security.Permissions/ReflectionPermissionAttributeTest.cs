//
// ReflectionPermissionAttributeTest.cs -
//	NUnit Test Cases for ReflectionPermissionAttribute
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
	public class ReflectionPermissionAttributeTest {

		[Test]
		public void Default () 
		{
			ReflectionPermissionAttribute a = new ReflectionPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (ReflectionPermissionFlag.NoFlags, a.Flags, "Flags");
			Assert.IsFalse (a.MemberAccess, "MemberAccess");
			Assert.IsFalse (a.ReflectionEmit, "ReflectionEmit");
			Assert.IsFalse (a.TypeInformation, "TypeInformation");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			ReflectionPermission perm = (ReflectionPermission) a.CreatePermission ();
			Assert.AreEqual (ReflectionPermissionFlag.NoFlags, perm.Flags, "CreatePermission.Flags");
			Assert.IsFalse (perm.IsUnrestricted (), "perm.Unrestricted");
		}

		[Test]
		public void Action () 
		{
			ReflectionPermissionAttribute a = new ReflectionPermissionAttribute (SecurityAction.Assert);
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
			ReflectionPermissionAttribute a = new ReflectionPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Flags () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			attr.Flags = ReflectionPermissionFlag.MemberAccess;
			Assert.IsTrue (attr.MemberAccess, "Flags/MemberAccess=MemberAccess");
			Assert.IsFalse (attr.ReflectionEmit, "Flags/MemberAccess=ReflectionEmit");
			Assert.IsFalse (attr.TypeInformation, "Flags/MemberAccess=TypeInformation");
			attr.Flags |= ReflectionPermissionFlag.ReflectionEmit;
			Assert.IsTrue (attr.MemberAccess, "Flags/ReflectionEmit=MemberAccess");
			Assert.IsTrue (attr.ReflectionEmit, "Flags/ReflectionEmit=ReflectionEmit");
			Assert.IsFalse (attr.TypeInformation, "Flags/ReflectionEmit=TypeInformation");
			attr.Flags |= ReflectionPermissionFlag.TypeInformation;
			Assert.IsTrue (attr.MemberAccess, "Flags/TypeInformation=MemberAccess");
			Assert.IsTrue (attr.ReflectionEmit, "Flags/TypeInformation=ReflectionEmit");
			Assert.IsTrue (attr.TypeInformation, "Flags/TypeInformation=TypeInformation");
			attr.Flags = ReflectionPermissionFlag.NoFlags;
			Assert.IsFalse (attr.MemberAccess, "Flags/NoFlags=MemberAccess");
			Assert.IsFalse (attr.ReflectionEmit, "Flags/NoFlags=ReflectionEmit");
			Assert.IsFalse (attr.TypeInformation, "Flags/NoFlags=TypeInformation");
		}

		[Test]
		public void NoFlags () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (ReflectionPermissionFlag.NoFlags, attr.Flags, "NoFlags.Flags");
			Assert.IsFalse (attr.Unrestricted, "NoFlags.Unrestricted");
			Assert.IsFalse (attr.MemberAccess, "NoFlags=MemberAccess");
			Assert.IsFalse (attr.ReflectionEmit, "NoFlags=ReflectionEmit");
			Assert.IsFalse (attr.TypeInformation, "NoFlags=TypeInformation");
			ReflectionPermission p = (ReflectionPermission) attr.CreatePermission ();
			Assert.AreEqual (ReflectionPermissionFlag.NoFlags, p.Flags, "NoFlags=ReflectionPermission");
		}

		[Test]
		public void Flags_Invalid ()
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			attr.Flags = (ReflectionPermissionFlag)Int32.MinValue;
		}

		[Test]
		public void MemberAccess () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			attr.MemberAccess = true;
			Assert.AreEqual (ReflectionPermissionFlag.MemberAccess, attr.Flags, "MemberAccess.Flags");
			Assert.IsFalse (attr.Unrestricted, "MemberAccess.Unrestricted");
			Assert.IsTrue (attr.MemberAccess, "MemberAccess=MemberAccess");
			Assert.IsFalse (attr.ReflectionEmit, "MemberAccess=ReflectionEmit");
			Assert.IsFalse (attr.TypeInformation, "MemberAccess=TypeInformation");
			ReflectionPermission p = (ReflectionPermission) attr.CreatePermission ();
			Assert.AreEqual (ReflectionPermissionFlag.MemberAccess, p.Flags, "MemberAccess=ReflectionPermission");
		}

		[Test]
		public void ReflectionEmit () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			attr.ReflectionEmit = true;
			Assert.AreEqual (ReflectionPermissionFlag.ReflectionEmit, attr.Flags, "ReflectionEmit.Flags");
			Assert.IsFalse (attr.Unrestricted, "ReflectionEmit.Unrestricted");
			Assert.IsFalse (attr.MemberAccess, "ReflectionEmit=MemberAccess");
			Assert.IsTrue (attr.ReflectionEmit, "ReflectionEmit=ReflectionEmit");
			Assert.IsFalse (attr.TypeInformation, "ReflectionEmit=TypeInformation");
			ReflectionPermission p = (ReflectionPermission) attr.CreatePermission ();
			Assert.AreEqual (ReflectionPermissionFlag.ReflectionEmit, p.Flags, "ReflectionEmit=ReflectionPermission");
		}

		[Test]
		public void TypeInformation () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			attr.TypeInformation = true;
			Assert.AreEqual (ReflectionPermissionFlag.TypeInformation, attr.Flags, "TypeInformation.Flags");
			Assert.IsFalse (attr.Unrestricted, "TypeInformation.Unrestricted");
			Assert.IsFalse (attr.MemberAccess, "TypeInformation=MemberAccess");
			Assert.IsFalse (attr.ReflectionEmit, "TypeInformation=ReflectionEmit");
			Assert.IsTrue (attr.TypeInformation, "TypeInformation=TypeInformation");
			ReflectionPermission p = (ReflectionPermission) attr.CreatePermission ();
			Assert.AreEqual (ReflectionPermissionFlag.TypeInformation, p.Flags, "TypeInformation=TypeInformation");
		}

		[Test]
		public void AllFlags () 
		{
			ReflectionPermissionAttribute attr = new ReflectionPermissionAttribute (SecurityAction.Assert);
			attr.MemberAccess = true;
			attr.ReflectionEmit = true;
			attr.TypeInformation = true;
			Assert.AreEqual (ReflectionPermissionFlag.AllFlags, attr.Flags, "AllFlags.Flags");
			// attribute isn't unrestricted but the created permission is !!!
			Assert.IsFalse (attr.Unrestricted, "AllFlags.Unrestricted");
			Assert.IsTrue (attr.MemberAccess, "AllFlags=MemberAccess");
			Assert.IsTrue (attr.ReflectionEmit, "AllFlags=ReflectionEmit");
			Assert.IsTrue (attr.TypeInformation, "AllFlags=TypeInformation");

			ReflectionPermission p = (ReflectionPermission) attr.CreatePermission ();
			Assert.AreEqual (ReflectionPermissionFlag.AllFlags, p.Flags, "AllFlags=ReflectionPermission");
			Assert.IsTrue (p.IsUnrestricted (), "AllFlags=Unrestricted");
		}

		[Test]
		public void Unrestricted () 
		{
			ReflectionPermissionAttribute a = new ReflectionPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			Assert.AreEqual (ReflectionPermissionFlag.NoFlags, a.Flags, "Unrestricted");

			ReflectionPermission perm = (ReflectionPermission) a.CreatePermission ();
			Assert.AreEqual (ReflectionPermissionFlag.AllFlags, perm.Flags, "CreatePermission.Flags");
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (ReflectionPermissionAttribute);
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
