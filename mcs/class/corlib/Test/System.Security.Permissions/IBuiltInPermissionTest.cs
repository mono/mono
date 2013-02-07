//
// IBuiltInPermissionTest.cs - NUnit Test Cases for IBuiltInPermission
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class IBuiltInPermissionTest {

		// IBuiltInPermission is internal but we can test it's values
		// using reflection.
		private int GetTokenIndex (IPermission p)
		{
			Type t = p.GetType ();
			int result = (int) t.InvokeMember ("System.Security.Permissions.IBuiltInPermission.GetTokenIndex", 
				BindingFlags.InvokeMethod | BindingFlags.NonPublic |  BindingFlags.Instance,
				null, p, null);
			return result;
		}

		[Test]
		public void Environment ()
		{
			IPermission p = (IPermission) new EnvironmentPermission (PermissionState.None);
			Assert.AreEqual (0, GetTokenIndex (p));
		}

		[Test]
		public void FileDialog ()
		{
			IPermission p = (IPermission) new FileDialogPermission (PermissionState.None);
			Assert.AreEqual (1, GetTokenIndex (p));
		}

		[Test]
		public void FileIO ()
		{
			IPermission p = (IPermission) new FileIOPermission (PermissionState.None);
			Assert.AreEqual (2, GetTokenIndex (p));
		}

		[Test]
		public void IsolatedStorageFile ()
		{
			IPermission p = (IPermission) new IsolatedStorageFilePermission (PermissionState.None);
			Assert.AreEqual (3, GetTokenIndex (p));
		}

		[Test]
		public void Reflection ()
		{
			IPermission p = (IPermission) new ReflectionPermission (PermissionState.None);
			Assert.AreEqual (4, GetTokenIndex (p));
		}

		[Test]
		public void Registry ()
		{
			IPermission p = (IPermission) new RegistryPermission (PermissionState.None);
			Assert.AreEqual (5, GetTokenIndex (p));
		}

		[Test]
		public void Security ()
		{
			IPermission p = (IPermission) new SecurityPermission (PermissionState.None);
			Assert.AreEqual (6, GetTokenIndex (p));
		}

		[Test]
		public void UI ()
		{
			IPermission p = (IPermission) new UIPermission (PermissionState.None);
			Assert.AreEqual (7, GetTokenIndex (p));
		}

		[Test]
		public void Principal ()
		{
			IPermission p = (IPermission) new PrincipalPermission (PermissionState.None);
			Assert.AreEqual (8, GetTokenIndex (p));
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		public void HostProtection ()
		{
			HostProtectionAttribute hpa = new HostProtectionAttribute ();
			// internal permission
			IPermission p = hpa.CreatePermission ();
			Assert.AreEqual (9, GetTokenIndex (p));
		}

		[Test]
		public void PublisherIdentity ()
		{
			IPermission p = (IPermission) new PublisherIdentityPermission (PermissionState.None);
#if NET_2_0
			Assert.AreEqual (10, GetTokenIndex (p));
#else
			Assert.AreEqual (9, GetTokenIndex (p));
#endif
		}

		[Test]
		public void SiteIdentity ()
		{
			IPermission p = (IPermission) new SiteIdentityPermission (PermissionState.None);
#if NET_2_0
			Assert.AreEqual (11, GetTokenIndex (p));
#else
			Assert.AreEqual (10, GetTokenIndex (p));
#endif
		}

		[Test]
		public void StrongNameIdentity ()
		{
			IPermission p = (IPermission) new StrongNameIdentityPermission (PermissionState.None);
#if NET_2_0
			Assert.AreEqual (12, GetTokenIndex (p));
#else
			Assert.AreEqual (11, GetTokenIndex (p));
#endif
		}

		[Test]
		public void UrlIdentity ()
		{
			IPermission p = (IPermission) new UrlIdentityPermission (PermissionState.None);
#if NET_2_0
			Assert.AreEqual (13, GetTokenIndex (p));
#else
			Assert.AreEqual (12, GetTokenIndex (p));
#endif
		}

		[Test]
		public void ZoneIdentity ()
		{
			IPermission p = (IPermission) new ZoneIdentityPermission (PermissionState.None);
#if NET_2_0
			Assert.AreEqual (14, GetTokenIndex (p));
#else
			Assert.AreEqual (13, GetTokenIndex (p));
#endif
		}

#if NET_2_0
		[Test]
		public void GacIdentity ()
		{
			IPermission p = (IPermission) new GacIdentityPermission (PermissionState.None);
			Assert.AreEqual (15, GetTokenIndex (p));
		}

		[Test]
		public void KeyContainer ()
		{
			IPermission p = (IPermission)new KeyContainerPermission (PermissionState.None);
			Assert.AreEqual (16, GetTokenIndex (p));
		}
#endif
	}
}
