//
// AppDomainTest.cs - NUnit Test Cases for AppDomain
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Policy;
using System.Security.Principal;

namespace MonoTests.System {

	[TestFixture]
	public class AppDomainTest : Assertion {

		private AppDomain ad;
		private ArrayList files = new ArrayList ();

		[TearDown]
		public void Unload () 
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
			AppDomain ad = AppDomain.CreateDomain ("Ximian");
			AppDomain.Unload (ad);
			ad.SetPrincipalPolicy (PrincipalPolicy.NoPrincipal);
		}
	}
}
