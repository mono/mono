// EventWaitHandleSecurityTest.cs - NUnit Test Cases for EventWaitHandleSecurity
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
	public class EventWaitHandleSecurityTest
	{
		// TODO: Mono System.Threading.EventWaitHandle does not throw exceptions on failure!
		[Test, ExpectedExceptionAttribute (typeof (UnauthorizedAccessException))]
		public void PermissionsActuallyWork ()
		{
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			bool createdNew; EventWaitHandleSecurity security;
			string name = @"Local\MonoTestWaitHandle";

			using (EventWaitHandle handle = new EventWaitHandle (false, EventResetMode.ManualReset,
			                                                     name, out createdNew)) {
				Assert.IsFalse (handle.SafeWaitHandle.IsInvalid);
				Assert.IsTrue (createdNew);

				// Make sure our later error will be due to permissions and not some sharing bug.
				bool createdAnotherNew;
				using (EventWaitHandle anotherHandle = new EventWaitHandle (false, EventResetMode.ManualReset,
			                                                            	    name, out createdAnotherNew)) {
					Assert.IsFalse (anotherHandle.SafeWaitHandle.IsInvalid);
					Assert.IsFalse (createdAnotherNew);
				}

				// Let's make a deny all.
				security = handle.GetAccessControl ();

				foreach (EventWaitHandleAccessRule rule in security.GetAccessRules
				         (true, false, typeof (SecurityIdentifier))) {
					security.RemoveAccessRuleSpecific (rule);
				}

				Assert.AreEqual (0, security.GetAccessRules (true, false, typeof (SecurityIdentifier)).Count);
				handle.SetAccessControl (security);

				security = handle.GetAccessControl ();
				Assert.AreEqual (0, security.GetAccessRules (true, false, typeof (SecurityIdentifier)).Count);

				// MS.NET will throw on the first line below.
				// For Mono testing the latter verifies the rest until the EventWaitHandle bug is fixed.
				// Also, NUnit 2.4 appears to lacks Assert.Pass ().
				EventWaitHandle badHandle = new EventWaitHandle(false, EventResetMode.ManualReset, name);
				if (badHandle.SafeWaitHandle.IsInvalid)
					throw new UnauthorizedAccessException ();
			}
		}
	}
}

