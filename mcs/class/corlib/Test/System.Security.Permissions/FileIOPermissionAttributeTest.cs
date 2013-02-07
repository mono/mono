//
// FileIOPermissionAttributeTest.cs -
//	NUnit Test Cases for FileIOPermissionAttribute
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
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
#if MOBILE
	[Ignore]
#endif
	public class FileIOPermissionAttributeTest {

		[Test]
		public void Default () 
		{
			FileIOPermissionAttribute a = new FileIOPermissionAttribute (SecurityAction.Assert);
			Assert.IsNull (a.Append, "Append");
			Assert.IsNull (a.PathDiscovery, "PathDiscovery");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.Write, "Write");
#if NET_2_0
			Assert.IsNotNull (a.AllFiles, "AllFiles");
			Assert.IsNotNull (a.AllLocalFiles, "AllLocalFiles");
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
#endif

			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			FileIOPermission perm = (FileIOPermission) a.CreatePermission ();
			Assert.AreEqual (FileIOPermissionAccess.NoAccess, perm.AllFiles, "CreatePermission-AllFiles");
			Assert.AreEqual (FileIOPermissionAccess.NoAccess, perm.AllLocalFiles, "CreatePermission-AllLocalFiles");
			Assert.IsFalse (perm.IsUnrestricted (), "perm-Unrestricted");
		}

		[Test]
		public void Action () 
		{
			FileIOPermissionAttribute a = new FileIOPermissionAttribute (SecurityAction.Assert);
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
			FileDialogPermissionAttribute a = new FileDialogPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void All () 
		{
			string filename = Assembly.GetCallingAssembly ().Location;
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.All = filename;
			Assert.AreEqual (filename, attr.Append, "All=Append");
			Assert.AreEqual (filename, attr.PathDiscovery, "All=PathDiscovery");
			Assert.AreEqual (filename, attr.Read, "All=Read");
			Assert.AreEqual (filename, attr.Write, "All=Write");
#if NET_2_0
			Assert.IsNotNull (attr.AllFiles, "AllFiles");
			Assert.IsNotNull (attr.AllLocalFiles, "AllLocalFiles");
			Assert.IsNull (attr.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (attr.ViewAccessControl, "ViewAccessControl");
#endif
			FileIOPermission p = (FileIOPermission)attr.CreatePermission ();
			filename = Path.GetFullPath (filename);
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.Append) [0], "All=FileIOPermissionAttribute-Append");
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.PathDiscovery) [0], "All=FileIOPermissionAttribute-PathDiscovery");
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.Read) [0], "All=FileIOPermissionAttribute-Read");
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.Write) [0], "All=FileIOPermissionAttribute-Write");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void All_Get () 
		{
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			string s = attr.All;
		}

		[Test]
		public void Append ()
		{
			string filename = Assembly.GetCallingAssembly ().Location;
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.Append = filename;
			Assert.AreEqual (filename, attr.Append, "Append=Append");
			Assert.IsNull (attr.PathDiscovery, "PathDiscovery=null");
			Assert.IsNull (attr.Read, "Read=null");
			Assert.IsNull (attr.Write, "Write=null");
#if NET_2_0
			Assert.IsNotNull (attr.AllFiles, "AllFiles");
			Assert.IsNotNull (attr.AllLocalFiles, "AllLocalFiles");
			Assert.IsNull (attr.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (attr.ViewAccessControl, "ViewAccessControl");
#endif
			FileIOPermission p = (FileIOPermission)attr.CreatePermission ();
			filename = Path.GetFullPath (filename);
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.Append) [0], "Append=FileIOPermissionAttribute-Append");
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.PathDiscovery), "Append=FileIOPermissionAttribute-PathDiscovery");
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.Read), "Append=FileIOPermissionAttribute-Read");
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.Write), "Append=FileIOPermissionAttribute-Write");
		}

		[Test]
		public void PathDiscovery () 
		{
			string filename = Assembly.GetCallingAssembly ().Location;
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.PathDiscovery = filename;
			Assert.IsNull (attr.Append, "Append=null");
			Assert.AreEqual (filename, attr.PathDiscovery, "PathDiscovery=PathDiscovery");
			Assert.IsNull (attr.Read, "Read=null");
			Assert.IsNull (attr.Write, "Write=null");
#if NET_2_0
			Assert.IsNotNull (attr.AllFiles, "AllFiles");
			Assert.IsNotNull (attr.AllLocalFiles, "AllLocalFiles");
			Assert.IsNull (attr.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (attr.ViewAccessControl, "ViewAccessControl");
#endif
			FileIOPermission p = (FileIOPermission)attr.CreatePermission ();
			filename = Path.GetFullPath (filename);
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.Append), "PathDiscovery=FileIOPermissionAttribute-Append");
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.PathDiscovery) [0], "PathDiscovery=FileIOPermissionAttribute-PathDiscovery");
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.Read), "PathDiscovery=FileIOPermissionAttribute-Read");
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.Write), "PathDiscovery=FileIOPermissionAttribute-Write");
		}

		[Test]
		public void Read () 
		{
			string filename = Assembly.GetCallingAssembly ().Location;
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.Read = filename;
			Assert.IsNull (attr.Append, "Append=null");
			Assert.IsNull (attr.PathDiscovery, "PathDiscovery=null");
			Assert.AreEqual (filename, attr.Read, "Read=Read");
			Assert.IsNull (attr.Write, "Write=null");
#if NET_2_0
			Assert.IsNotNull (attr.AllFiles, "AllFiles");
			Assert.IsNotNull (attr.AllLocalFiles, "AllLocalFiles");
			Assert.IsNull (attr.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (attr.ViewAccessControl, "ViewAccessControl");
#endif
			FileIOPermission p = (FileIOPermission)attr.CreatePermission ();
			filename = Path.GetFullPath (filename);
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.Append), "PathDiscovery=FileIOPermissionAttribute-Append");
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.PathDiscovery), "PathDiscovery=FileIOPermissionAttribute-PathDiscovery");
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.Read) [0], "PathDiscovery=FileIOPermissionAttribute-Read");
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.Write), "PathDiscovery=FileIOPermissionAttribute-Write");
		}

#if NET_2_0
		[Test]
		public void ChangeAccessControl ()
		{
			FileIOPermissionAttribute a = new FileIOPermissionAttribute (SecurityAction.Assert);
			a.ChangeAccessControl = "mono";
			Assert.IsNull (a.Append, "Append");
			Assert.AreEqual ("mono", a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.PathDiscovery, "PathDiscovery");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
			Assert.IsNull (a.Write, "Write");

			a.ChangeAccessControl = null;
			Assert.IsNull (a.Append, "Append");
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.PathDiscovery, "PathDiscovery");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
			Assert.IsNull (a.Write, "Write");
		}

		[Test]
		public void ViewAccessControl ()
		{
			FileIOPermissionAttribute a = new FileIOPermissionAttribute (SecurityAction.Assert);
			a.ViewAccessControl = "mono";
			Assert.IsNull (a.Append, "Append");
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.PathDiscovery, "PathDiscovery");
			Assert.IsNull (a.Read, "Read");
			Assert.AreEqual ("mono", a.ViewAccessControl, "ViewAccessControl");
			Assert.IsNull (a.Write, "Write");

			a.ViewAccessControl = null;
			Assert.IsNull (a.Append, "Append");
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.PathDiscovery, "PathDiscovery");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
			Assert.IsNull (a.Write, "Write");
		}

		[Test]
		public void ViewAndModify_Set ()
		{
			FileIOPermissionAttribute a = new FileIOPermissionAttribute (SecurityAction.Assert);
			a.ViewAndModify = "mono";
			Assert.AreEqual ("mono", a.Append, "Append");
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.AreEqual ("mono", a.PathDiscovery, "PathDiscovery");
			Assert.AreEqual ("mono", a.Read, "Read");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
			Assert.AreEqual ("mono", a.Write, "Write");

			a.ViewAndModify = null;
			Assert.IsNull (a.Append, "Append");
			Assert.IsNull (a.ChangeAccessControl, "ChangeAccessControl");
			Assert.IsNull (a.PathDiscovery, "PathDiscovery");
			Assert.IsNull (a.Read, "Read");
			Assert.IsNull (a.ViewAccessControl, "ViewAccessControl");
			Assert.IsNull (a.Write, "Write");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ViewAndModify_Get ()
		{
			FileIOPermissionAttribute a = new FileIOPermissionAttribute (SecurityAction.Assert);
			a.ViewAndModify = "mono";
			Assert.AreEqual ("ViewAndModify", "mono", a.ViewAndModify);
		}
#endif

		[Test]
		public void Write () 
		{
			string filename = Assembly.GetCallingAssembly ().Location;
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.Write = filename;
			Assert.IsNull (attr.Append, "Append=null");
			Assert.IsNull (attr.PathDiscovery, "PathDiscovery=null");
			Assert.IsNull (attr.Read, "Read=null");
			Assert.AreEqual (filename, attr.Write, "Write=Write");
			FileIOPermission p = (FileIOPermission) attr.CreatePermission ();
			filename = Path.GetFullPath (filename);
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.Append), "PathDiscovery=FileIOPermissionAttribute-Append");
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.PathDiscovery), "PathDiscovery=FileIOPermissionAttribute-PathDiscovery");
			Assert.IsNull (p.GetPathList (FileIOPermissionAccess.Read), "PathDiscovery=FileIOPermissionAttribute-Read");
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.Write) [0], "PathDiscovery=FileIOPermissionAttribute-Write");
		}

		[Test]
		public void Unrestricted () 
		{
			FileIOPermissionAttribute a = new FileIOPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;

			FileIOPermission perm = (FileIOPermission) a.CreatePermission ();
			Assert.IsTrue (perm.IsUnrestricted (), "CreatePermission.IsUnrestricted");
			Assert.AreEqual (FileIOPermissionAccess.AllAccess, perm.AllFiles, "CreatePermission.AllFiles");
			Assert.AreEqual (FileIOPermissionAccess.AllAccess, perm.AllLocalFiles, "CreatePermission.AllLocalFiles");
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (FileDialogPermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object [] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}

		[Test]
		[Category("NotDotNet")]
		public void NonC14NPath ()
		{
			string filename = Path.Combine (Path.GetTempPath (), "test");
			filename = Path.Combine (filename, "..");
			filename = Path.Combine (filename, "here");
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			// attribute class will keep the .. in the path
			attr.All = filename;
			Assert.AreEqual (filename, attr.Append, "All=Append");
			Assert.AreEqual (filename, attr.PathDiscovery, "All=PathDiscovery");
			Assert.AreEqual (filename, attr.Read, "All=Read");
			Assert.AreEqual (filename, attr.Write, "All=Write");
			// but the permission class will c14n it
			filename = Path.GetFullPath (filename);
			FileIOPermission p = (FileIOPermission)attr.CreatePermission ();
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.Append) [0], "All=FileIOPermissionAttribute-Append");
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.PathDiscovery) [0], "All=FileIOPermissionAttribute-PathDiscovery");
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.Read) [0], "All=FileIOPermissionAttribute-Read");
			Assert.AreEqual (filename, p.GetPathList (FileIOPermissionAccess.Write) [0], "All=FileIOPermissionAttribute-Write");
		}
	}
}
