//
// HttpRuntimeCas.cs - CAS unit tests for System.Web.HttpRuntime
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Security.Principal;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpRuntimeCas : AspNetHostingMinimal {

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// static ctor at fulltrust
			new HttpRuntime ();
		}

#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor_Deny_Unrestricted ()
		{
			new HttpRuntime ();
		}
#else
		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Constructor_Deny_UnmanagedCode ()
		{
			new HttpRuntime ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void Constructor_PermitOnly_UnmanagedCode ()
		{
			new HttpRuntime ();
		}
#endif

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StaticProperties ()
		{
			Assert.IsNull (HttpRuntime.AppDomainAppVirtualPath, "AppDomainAppVirtualPath");
			Assert.IsNotNull (HttpRuntime.Cache, "Cache");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AppDomainAppPath_Deny ()
		{
			try {
				Assert.IsNotNull (HttpRuntime.AppDomainAppPath, "AppDomainAppPath");
			}
			catch (ArgumentNullException) {
				Assert.Ignore ("fails before the security check");
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void BinDirectory_Deny ()
		{
			try {
				Assert.IsNotNull (HttpRuntime.BinDirectory, "BinDirectory");
			}
			catch (ArgumentException) {
				Assert.Ignore ("fails before the security check");
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CodegenDir_Deny ()
		{
			try {
				Assert.IsNotNull (HttpRuntime.CodegenDir, "CodegenDir");
			}
			catch (ArgumentNullException) {
				Assert.Ignore ("fails before the security check");
			}
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.High)]
		[ExpectedException (typeof (SecurityException))]
		public void AppDomainAppId_Deny_High ()
		{
			Assert.IsNull (HttpRuntime.AppDomainAppId, "AppDomainAppId");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.High)]
		[ExpectedException (typeof (SecurityException))]
		public void AppDomainId_Deny_High ()
		{
			Assert.IsNull (HttpRuntime.AppDomainId, "AppDomainId");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.High)]
		public void PermitOnly_High ()
		{
			Assert.IsNull (HttpRuntime.AppDomainAppId, "AppDomainAppId");
			Assert.IsNull (HttpRuntime.AppDomainId, "AppDomainId");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Low)]
		[ExpectedException (typeof (SecurityException))]
		public void IsOnUNCShare_Deny_High ()
		{
			Assert.IsFalse (HttpRuntime.IsOnUNCShare, "IsOnUNCShare");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.High)]
		public void IsOnUNCShare_PermitOnly_High ()
		{
			try {
				Assert.IsFalse (HttpRuntime.IsOnUNCShare, "IsOnUNCShare");
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AspInstallDirectory_Deny_FileIOPermission ()
		{
			if (HttpRuntime.AspInstallDirectory == null)
				Assert.Ignore ("null isn't checked for FileIOPermission");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ClrInstallDirectory_Deny_FileIOPermission ()
		{
			Assert.IsNotNull (HttpRuntime.ClrInstallDirectory, "ClrInstallDirectory");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void MachineConfigurationDirectory_Deny_FileIOPermission ()
		{
			Assert.IsNotNull (HttpRuntime.MachineConfigurationDirectory, "MachineConfigurationDirectory");
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PermitOnly_PathDiscovery ()
		{
			string s = HttpRuntime.AspInstallDirectory; // null in unit tests for mono
			Assert.IsNotNull (HttpRuntime.ClrInstallDirectory, "ClrInstallDirectory");
			Assert.IsNotNull (HttpRuntime.MachineConfigurationDirectory, "MachineConfigurationDirectory");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Close_Deny_Unmanaged ()
		{
			HttpRuntime.Close ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void Close_PermitOnly_Unmanaged ()
		{
			HttpRuntime.Close ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Medium)]
		[ExpectedException (typeof (SecurityException))]
		public void ProcessRequest_Deny_Medium ()
		{
			HttpRuntime.ProcessRequest (null);
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Medium)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ProcessRequest_PermitOnly_Medium ()
		{
			HttpRuntime.ProcessRequest (null);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void UnloadAppDomain_Deny_Unmanaged ()
		{
			HttpRuntime.UnloadAppDomain ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void UnloadAppDomain_PermitOnly_Unmanaged ()
		{
			HttpRuntime.UnloadAppDomain ();
		}

		// LinkDemand

		// note: the .ctor also has a LinkDemand for UnmanagedCode (which mess up the results)
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (VoidType);
			Assert.IsNotNull (ci, "default .ctor");
			return ci.Invoke (null);
		}

		public override Type Type {
			get { return typeof (HttpRuntime); }
		}
	}
}
