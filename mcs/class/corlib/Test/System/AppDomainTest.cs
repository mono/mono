//
// AppDomainTest.cs - NUnit Test Cases for AppDomain
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Principal;

namespace MonoTests.System {

	[TestFixture]
	public class AppDomainTest {

		private AppDomain ad;
		private ArrayList files = new ArrayList ();

		[TearDown]
		public void TearDown () 
		{
			if (ad != null) {
				try {
// FIXME: Lots of GC warning when unloading
//					AppDomain.Unload (ad);
					ad = null;
				}
				catch {} // do not affect unit test results in TearDown
			}
			foreach (string fname in files) {
				File.Delete (fname);
			}
			files.Clear ();
		}

		[Test]
		public void SetThreadPrincipal () 
		{
			IIdentity i = new GenericIdentity ("sebastien@ximian.com", "rfc822");
			IPrincipal p = new GenericPrincipal (i, null);
			ad = AppDomain.CreateDomain ("SetThreadPrincipal");
			ad.SetThreadPrincipal (p);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetThreadPrincipalNull ()
		{
			AppDomain.CurrentDomain.SetThreadPrincipal (null);
		}

		[Test]
		[ExpectedException (typeof (PolicyException))]
		public void SetThreadPrincipalTwice () 
		{
			IIdentity i = new GenericIdentity ("sebastien@ximian.com", "rfc822");
			IPrincipal p = new GenericPrincipal (i, null);
			ad = AppDomain.CreateDomain ("SetThreadPrincipalTwice");
			ad.SetThreadPrincipal (p);
			// you only live twice (or so James told me ;-)
			ad.SetThreadPrincipal (p);
		}

		[Test]
		[ExpectedException (typeof (AppDomainUnloadedException))]
		[Ignore ("Unloading cause lots of GC warning")]
		public void SetThreadPrincipalUnloaded () 
		{
			ad = AppDomain.CreateDomain ("Ximian");
			AppDomain.Unload (ad);
			IIdentity i = new GenericIdentity ("sebastien@ximian.com", "rfc822");
			IPrincipal p = new GenericPrincipal (i, null);
			ad.SetThreadPrincipal (p);
		}

		[Test]
		public void SetPrincipalPolicy_NoPrincipal () 
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.NoPrincipal);
		}

		[Test]
		public void SetPrincipalPolicy_UnauthenticatedPrincipal () 
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.UnauthenticatedPrincipal);
		}

		[Test]
		public void SetPrincipalPolicy_WindowsPrincipal () 
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.WindowsPrincipal);
		}

		[Test]
		[ExpectedException (typeof (AppDomainUnloadedException))]
		[Ignore ("Unloading cause lots of GC warning")]
		public void SetPrincipalPolicyUnloaded () 
		{
			ad = AppDomain.CreateDomain ("Ximian");
			AppDomain.Unload (ad);
			ad.SetPrincipalPolicy (PrincipalPolicy.NoPrincipal);
		}

		[Test]
		public void CreateDomain_String ()
		{
			ad = AppDomain.CreateDomain ("CreateDomain_String");
			Assert.IsNotNull (ad.Evidence, "Evidence");
			// Evidence are copied (or referenced?) from default app domain
			// we can't get default so we use the current (which should have copied the default)
			Assert.AreEqual (AppDomain.CurrentDomain.Evidence.Count, ad.Evidence.Count, "Evidence.Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateDomain_String_Null ()
		{
			ad = AppDomain.CreateDomain (null);
		}

		[Test]
		public void CreateDomain_StringEvidence ()
		{
			Evidence e = new Evidence ();
			ad = AppDomain.CreateDomain ("CreateDomain_StringEvidence", e);
			Assert.IsNotNull (ad.Evidence, "Evidence");
			Assert.AreEqual (0, ad.Evidence.Count, "Evidence.Count");

			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.AreEqual (0, ad.Evidence.Count, "Evidence.Count");
			// evidence isn't copied but referenced
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateDomain_StringNullEvidence ()
		{
			ad = AppDomain.CreateDomain (null, new Evidence ());
		}

		[Test]
		public void CreateDomain_StringEvidenceNull ()
		{
			ad = AppDomain.CreateDomain ("CreateDomain_StringEvidenceNull", null);
			Assert.IsNotNull (ad.Evidence, "Evidence");
			// Evidence are copied (or referenced?) from default app domain
			// we can't get default so we use the current (which should have copied the default)
			Evidence e = AppDomain.CurrentDomain.Evidence;
			Assert.AreEqual (e.Count, ad.Evidence.Count, "Evidence.Count-1");
			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.AreEqual (e.Count - 1, ad.Evidence.Count, "Evidence.Count-2");
			// evidence are copied
		}

		[Test]
		public void CreateDomain_StringEvidenceAppDomainSetup ()
		{
			Evidence e = new Evidence ();
			AppDomainSetup info = new AppDomainSetup ();
			info.ApplicationName = "ApplicationName";

			ad = AppDomain.CreateDomain ("CreateDomain_StringEvidenceAppDomainSetup", e, info);
			Assert.IsNotNull (ad.Evidence, "Evidence");
			Assert.AreEqual (0, ad.Evidence.Count, "Evidence.Count");
			Assert.IsNotNull (ad.SetupInformation, "SetupInformation");
			Assert.AreEqual ("ApplicationName", ad.SetupInformation.ApplicationName);

			e.AddHost (new Zone (SecurityZone.MyComputer));
			Assert.AreEqual (0, ad.Evidence.Count, "Evidence.Count");
			// evidence isn't copied but referenced
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateDomain_StringNullEvidenceAppDomainSetup ()
		{
			AppDomainSetup info = new AppDomainSetup ();
			ad = AppDomain.CreateDomain (null, new Evidence (), info);
		}

		[Test]
		public void CreateDomain_StringEvidenceNullAppDomainSetup ()
		{
			AppDomainSetup info = new AppDomainSetup ();
			info.ApplicationName = "ApplicationName";
			ad = AppDomain.CreateDomain ("CreateDomain_StringEvidenceNullAppDomainSetup", null, info);
			Assert.IsNotNull (ad.Evidence, "Evidence");
			// Evidence are copied (or referenced?) from default app domain
			// we can't get default so we use the current (which should have copied the default)
			Assert.AreEqual (AppDomain.CurrentDomain.Evidence.Count, ad.Evidence.Count, "Evidence.Count");
			Assert.AreEqual ("ApplicationName", ad.SetupInformation.ApplicationName, "ApplicationName-1");
			info.ApplicationName = "Test";
			Assert.AreEqual ("Test", info.ApplicationName, "ApplicationName-2");
			Assert.AreEqual ("ApplicationName", ad.SetupInformation.ApplicationName, "ApplicationName-3");
			// copied
		}

		[Test]
		public void CreateDomain_StringEvidenceAppDomainSetupNull ()
		{
			Evidence e = new Evidence ();
			ad = AppDomain.CreateDomain ("CreateDomain_StringEvidenceAppDomainSetupNull", e, null);
			Assert.IsNotNull (ad.Evidence, "Evidence");
			Assert.AreEqual (0, ad.Evidence.Count, "Evidence.Count");
			// SetupInformation is copied from default app domain
			Assert.IsNotNull (ad.SetupInformation, "SetupInformation");
		}

		[Test]
		public void SetAppDomainPolicy ()
		{
			ad = AppDomain.CreateDomain ("SetAppDomainPolicy_Null");
			ad.SetAppDomainPolicy (PolicyLevel.CreateAppDomainLevel ());
			// not much to see
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetAppDomainPolicy_Null ()
		{
			ad = AppDomain.CreateDomain ("SetAppDomainPolicy_Null");
			ad.SetAppDomainPolicy (null);
		}

		[Test]
#if ! NET_2_0
		// MS bug for 2.x ???
		[ExpectedException (typeof (PolicyException))]
#endif
		public void SetAppDomainPolicy_Dual ()
		{
			ad = AppDomain.CreateDomain ("SetAppDomainPolicy_Dual");
			PolicyLevel pl = PolicyLevel.CreateAppDomainLevel ();
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			pl.RootCodeGroup.PolicyStatement = new PolicyStatement (ps);
			ad.SetAppDomainPolicy (pl);

			// only one time!
			pl = PolicyLevel.CreateAppDomainLevel ();
			ps = new PermissionSet (PermissionState.None);
			pl.RootCodeGroup.PolicyStatement = new PolicyStatement (ps);
			ad.SetAppDomainPolicy (pl);
		}

		[Test]
		[ExpectedException (typeof (AppDomainUnloadedException))]
		[Ignore ("Unloading cause lots of GC warning")]
		public void SetAppDomainPolicy_Unloaded ()
		{
			ad = AppDomain.CreateDomain ("SetAppDomainPolicy_Unloaded");
			AppDomain.Unload (ad);
			ad.SetAppDomainPolicy (PolicyLevel.CreateAppDomainLevel ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Ignore ("cause an assertion in mono runtime")]
		public void GetData_Null ()
		{
			AppDomain.CurrentDomain.GetData (null);
		}

		[Test]
		public void SetData ()
		{
			AppDomain.CurrentDomain.SetData ("data", "data");
			Assert.AreEqual ("data", AppDomain.CurrentDomain.GetData ("data"), "GetData");
			AppDomain.CurrentDomain.SetData ("data", null);
			Assert.IsNull (AppDomain.CurrentDomain.GetData ("data"), "GetData-Null");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Ignore ("cause an assertion in mono runtime")]
		public void SetData_Null ()
		{
			AppDomain.CurrentDomain.SetData (null, "data");
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Activate_Null ()
		{
			AppDomain.Activate (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ActivateNewProcess_Null ()
		{
			AppDomain.ActivateNewProcess (null);
		}

		[Test]
		public void ApplyPolicy ()
		{
			ad = AppDomain.CreateDomain ("ApplyPolicy");
			string fullname = Assembly.GetExecutingAssembly ().FullName;
			string result = ad.ApplyPolicy (fullname);
			Assert.AreEqual (fullname, result, "ApplyPolicy");
			// doesn't even requires an assembly name
			Assert.AreEqual ("123", ad.ApplyPolicy ("123"), "Invalid FullName");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ApplyPolicy_Empty ()
		{
			ad = AppDomain.CreateDomain ("ApplyPolicy_Empty");
			ad.ApplyPolicy (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ApplyPolicy_Null ()
		{
			ad = AppDomain.CreateDomain ("ApplyPolicy_Null");
			ad.ApplyPolicy (null);
		}

		[Test]
		public void DomainManager ()
		{
			Assert.IsNull (AppDomain.CurrentDomain.DomainManager, "CurrentDomain.DomainManager");
			ad = AppDomain.CreateDomain ("DomainManager");
			Assert.IsNull (ad.DomainManager, "ad.DomainManager");
		}

		[Test]
		public void IsDefaultAppDomain ()
		{
			ad = AppDomain.CreateDomain ("ReflectionOnlyGetAssemblies");
			Assert.IsFalse (ad.IsDefaultAppDomain (), "IsDefaultAppDomain");
			// we have no public way to get the default appdomain
		}

		[Test]
		public void ReflectionOnlyGetAssemblies ()
		{
			ad = AppDomain.CreateDomain ("ReflectionOnlyGetAssemblies");
			Assembly [] a = ad.ReflectionOnlyGetAssemblies ();
			Assert.IsNotNull (a, "ReflectionOnlyGetAssemblies");
			Assert.AreEqual (0, a.Length, "Count");
		}
#endif
	}
}
