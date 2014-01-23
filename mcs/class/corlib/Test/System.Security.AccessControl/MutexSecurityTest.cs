// MutexSecurityTest.cs - NUnit Test Cases for MutexSecurity
//
// Authors:
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2012 James Bellinger

#if !MOBILE

using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.Security.AccessControl
{
	[TestFixture]
	public class MutexSecurityTest
	{
		[Test, ExpectedException (typeof (WaitHandleCannotBeOpenedException))]
		public void FailsForNonexistantMutex ()
		{
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			new MutexSecurity (@"Local\NonexistantMutex", AccessControlSections.Access);
		}

		[Test]
		public void SucceedsForExistingMutex ()
		{
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			bool createdNew;
			string name = @"Local\MonoTestMutex";

			// Let's be sure the mutex destroys on Close() as well.
			// Otherwise, many of our tests will fail as they will be accessing the wrong mutex.
			using (Mutex mutex = new Mutex (false, name, out createdNew)) {
				Assert.IsTrue (createdNew);
				new MutexSecurity (name, AccessControlSections.Access);
			}

			using (Mutex mutex = new Mutex (false, name, out createdNew)) {
				Assert.IsTrue (createdNew);
				new MutexSecurity (name, AccessControlSections.Access);
			}
		}

		[Test]
		public void CanSetAndGetMutexSecurity ()
		{
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			MutexAccessRule rule; SecurityIdentifier sid;
			AuthorizationRuleCollection rulesA, rulesB, rulesC;
			bool createdNew; MutexSecurity security;
			string name = @"Local\MonoTestMutex";

			using (Mutex mutex = new Mutex(false, name, out createdNew)) {
				Assert.IsTrue (createdNew);

				security = mutex.GetAccessControl ();
				rulesA = security.GetAccessRules (true, false, typeof (SecurityIdentifier));
				Assert.AreNotEqual (0, rulesA.Count);

				// Contrary to what you'd expect, these classes only try to persist sections that
				// that were *changed*. Awful, eh? To be fair, if you retrieve and modify it's fine.
				security = new MutexSecurity ();
				mutex.SetAccessControl (security);

				security = mutex.GetAccessControl ();
				rulesB = security.GetAccessRules (true, false, typeof (SecurityIdentifier));
				Assert.AreEqual (rulesA.Count, rulesB.Count);

				// And here's our dummy change. Observe...
				sid = new SecurityIdentifier( "S-1-5-12-3456-7890");
				rule = new MutexAccessRule (sid, MutexRights.Synchronize, AccessControlType.Allow);

				security = new MutexSecurity ();
				security.RemoveAccessRuleSpecific (rule);
				mutex.SetAccessControl (security);

				security = mutex.GetAccessControl ();
				rulesC = security.GetAccessRules (true, false, typeof (SecurityIdentifier));
				Assert.AreEqual (0, rulesC.Count);
			}
		}

		// TODO: Mono System.Threading.Mutex does not throw exceptions on failure!
		[Test, ExpectedExceptionAttribute (typeof (UnauthorizedAccessException))]
		public void PermissionsActuallyWork ()
		{
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			bool createdNew; MutexSecurity security;
			string name = @"Local\MonoTestMutex";

			using (Mutex mutex = new Mutex (false, name, out createdNew)) {
				Assert.IsFalse (mutex.SafeWaitHandle.IsInvalid);
				Assert.IsTrue (createdNew);

				// Make sure our later error will be due to permissions and not some sharing bug.
				bool createdAnotherNew;
				using (Mutex anotherMutex = new Mutex (false, name, out createdAnotherNew)) {
					Assert.IsFalse (mutex.SafeWaitHandle.IsInvalid);
					Assert.IsFalse (createdAnotherNew);
				}

				// Let's make a deny all.
				security = mutex.GetAccessControl ();

				foreach (MutexAccessRule rule in security.GetAccessRules
				         (true, false, typeof (SecurityIdentifier))) {
					security.RemoveAccessRuleSpecific (rule);
				}

				Assert.AreEqual (0, security.GetAccessRules (true, false, typeof (SecurityIdentifier)).Count);
				mutex.SetAccessControl (security);

				security = mutex.GetAccessControl ();
				Assert.AreEqual (0, security.GetAccessRules (true, false, typeof (SecurityIdentifier)).Count);

				// MS.NET will throw on the first line below.
				// For Mono testing the latter verifies the rest until the Mutex bug is fixed.
				// Also, NUnit 2.4 appears to lacks Assert.Pass ().
				Mutex badMutex = new Mutex(false, name);
				if (badMutex.SafeWaitHandle.IsInvalid)
					throw new UnauthorizedAccessException ();
			}
		}
	}
}

#endif

