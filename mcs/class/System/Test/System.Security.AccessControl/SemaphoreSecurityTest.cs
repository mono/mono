// SemaphoreSecurityTest.cs - NUnit Test Cases for SemaphoreSecurity
//
// Authors:
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2012 James Bellinger

using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.Security.AccessControl
{
	[TestFixture]
	public class SemaphoreSecurityTest
	{
		// TODO: Mono System.Threading.Semaphore does not throw exceptions on failure (except in OpenExisting).
		[Test, ExpectedExceptionAttribute (typeof (UnauthorizedAccessException))]
		public void PermissionsActuallyWork ()
		{
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			bool createdNew; SemaphoreSecurity security;
			string name = @"Local\MonoTestSemaphore";

			using (Semaphore semaphore = new Semaphore (1, 1, name, out createdNew)) {
				Assert.IsFalse (semaphore.SafeWaitHandle.IsInvalid);
				Assert.IsTrue (createdNew);

				// Make sure our later error will be due to permissions and not some sharing bug.
				bool createdAnotherNew;
				using (Semaphore anotherSemaphore = new Semaphore (1, 1, name, out createdAnotherNew)) {
					Assert.IsFalse (anotherSemaphore.SafeWaitHandle.IsInvalid);
					Assert.IsFalse (createdAnotherNew);
				}

				// Let's make a deny all.
				security = semaphore.GetAccessControl ();

				foreach (SemaphoreAccessRule rule in security.GetAccessRules
				         (true, false, typeof (SecurityIdentifier))) {
					security.RemoveAccessRuleSpecific (rule);
				}

				Assert.AreEqual (0, security.GetAccessRules (true, false, typeof (SecurityIdentifier)).Count);
				semaphore.SetAccessControl (security);

				security = semaphore.GetAccessControl ();
				Assert.AreEqual (0, security.GetAccessRules (true, false, typeof (SecurityIdentifier)).Count);

				// MS.NET will throw on the first line below.
				// For Mono testing the latter verifies the rest until the Semaphore bug is fixed.
				// Also, NUnit 2.4 appears to lacks Assert.Pass ().
				Semaphore badSemaphore = new Semaphore (1, 1, name);
				if (badSemaphore.SafeWaitHandle.IsInvalid)
					throw new UnauthorizedAccessException ();
			}
		}
	}
}

