//
// AppDomainCas.cs - CAS unit tests for System.AppDomain
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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;

namespace MonoCasTests.System {

	[TestFixture]
	[Category ("CAS")]
	public class AppDomainCas {

		private AppDomain ad;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// it's safe to create the AppDomain here
			string temp = Path.GetTempPath ();
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationName = "CAS";
			setup.PrivateBinPath = temp;
			setup.DynamicBase = temp;
			ad = AppDomain.CreateDomain ("CAS", null, setup);
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// Partial Trust Tests

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PartialTrust_Deny_Unrestricted ()
		{
			// static
			Assert.IsNotNull (AppDomain.CurrentDomain, "CurrentDomain");
			// instance
			Assert.IsNotNull (ad.FriendlyName, "FriendlyName");
			Assert.IsNotNull (ad.SetupInformation, "SetupInformation");
			Assert.IsFalse (ad.ShadowCopyFiles, "ShadowCopyFiles");
		}

// see http://bugzilla.ximian.com/show_bug.cgi?id=74411
		[Category ("NotWorking")]
		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void BaseDirectory_Deny_FileIOPermission ()
		{
			Assert.IsNotNull (ad.BaseDirectory, "BaseDirectory");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateDomain1_Deny_ControlAppDomain ()
		{
			AppDomain.CreateDomain (null);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateDomain2_Deny_ControlAppDomain ()
		{
			AppDomain.CreateDomain (null, null);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateDomain3_Deny_ControlAppDomain ()
		{
			AppDomain.CreateDomain (null, null, null);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateDomain5_Deny_ControlAppDomain ()
		{
			AppDomain.CreateDomain (null, null, null, null, false);
		}
#if NET_2_0
		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateDomain7_Deny_ControlAppDomain ()
		{
			AppDomain.CreateDomain (null, null, null, null, false, null, null);
		}
#endif
// see http://bugzilla.ximian.com/show_bug.cgi?id=74411
		[Category ("NotWorking")]
		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void DynamicDirectory_Deny_FileIOPermission ()
		{
			Assert.IsNotNull (ad.DynamicDirectory, "DynamicDirectory");
		}

		[Category ("NotWorking")] // check not yet implemented
		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlEvidence = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Evidence_Deny_ControlEvidence ()
		{
			Assert.IsNotNull (ad.Evidence, "Evidence");
		}

// see http://bugzilla.ximian.com/show_bug.cgi?id=74411
		[Category ("NotWorking")]
		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void RelativeSearchPath_Deny_FileIOPermission ()
		{
			Assert.IsNotNull (ad.RelativeSearchPath, "RelativeSearchPath");
		}

// see http://bugzilla.ximian.com/show_bug.cgi?id=74411
		[Category ("NotWorking")]
		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPrincipal = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SetPrincipalPolicy_Deny_ControlPrincipal ()
		{
			ad.SetPrincipalPolicy (PrincipalPolicy.NoPrincipal);
		}

// see http://bugzilla.ximian.com/show_bug.cgi?id=74411
		[Category ("NotWorking")]
		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPrincipal = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SetThreadPrincipal_Deny_ControlPrincipal ()
		{
			ad.SetThreadPrincipal (new GenericPrincipal (new GenericIdentity ("me"), null));
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Unload_Deny_ControlAppDomain ()
		{
			AppDomain.Unload (null);
		}


		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PermitOnly_FileIOPermission ()
		{
			Assert.IsNotNull (ad.BaseDirectory, "BaseDirectory");
			Assert.IsNotNull (ad.DynamicDirectory, "DynamicDirectory");
			Assert.IsNotNull (ad.RelativeSearchPath, "RelativeSearchPath");
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlEvidence = true)]
		public void PermitOnly_ControlEvidence ()
		{
			// other permissions required to get evidence from another domain
			Assert.IsNotNull (AppDomain.CurrentDomain.Evidence, "Evidence");
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPrincipal = true)]
		public void PermitOnly_ControlPrincipal ()
		{
			ad.SetPrincipalPolicy (PrincipalPolicy.NoPrincipal);
			ad.SetThreadPrincipal (new GenericPrincipal (new GenericIdentity ("me"), null));
		}

		// we use reflection to call AppDomain as some methods and events are protected 
		// by LinkDemand (which will be converted into full demand, i.e. a stack walk)
		// when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AppendPrivatePath ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("AppendPrivatePath");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { String.Empty });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ClearPrivatePath ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("ClearPrivatePath");
			mi.Invoke (AppDomain.CurrentDomain, null);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ClearShadowCopyPath ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("ClearShadowCopyPath");
			mi.Invoke (AppDomain.CurrentDomain, null);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SetCachePath ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("SetCachePath");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { String.Empty });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SetData ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("SetData");
			mi.Invoke (AppDomain.CurrentDomain, new object [2] { String.Empty, null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SetShadowCopyFiles ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("SetShadowCopyFiles");
			mi.Invoke (AppDomain.CurrentDomain, null);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SetDynamicBase ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("SetDynamicBase");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { String.Empty });
		}

		// events

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddDomainUnload ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("add_DomainUnload");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void RemoveDomainUnload ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("remove_DomainUnload");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddAssemblyLoad ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("add_AssemblyLoad");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void RemoveAssemblyLoad ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("remove_AssemblyLoad");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddProcessExit ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("add_ProcessExit");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void RemoveProcessExit ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("remove_ProcessExit");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddTypeResolve ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("add_TypeResolve");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void RemoveTypeResolve ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("remove_TypeResolve");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddResourceResolve ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("add_ResourceResolve");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void RemoveResourceResolve ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("remove_ResourceResolve");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddAssemblyResolve ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("add_AssemblyResolve");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void RemoveAssemblyResolve ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("remove_AssemblyResolve");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void AddUnhandledException ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("add_UnhandledException");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlAppDomain = true)]
		[ExpectedException (typeof (SecurityException))]
		public void RemoveUnhandledException ()
		{
			MethodInfo mi = typeof (AppDomain).GetMethod ("remove_UnhandledException");
			mi.Invoke (AppDomain.CurrentDomain, new object [1] { null });
		}
	}
}
