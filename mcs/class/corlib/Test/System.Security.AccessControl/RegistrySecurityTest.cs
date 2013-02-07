// RegistrySecurityTest.cs - NUnit Test Cases for RegistrySecurity
//
// Authors:
//	James Bellinger (jfb@zer7.com)

#if !MOBILE

using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;
using NUnit.Framework;

namespace MonoTests.System.Security.AccessControl
{
	[TestFixture]
	public class RegistrySecurityTest
	{
		[Test]
		public void ChangeGroupToEveryone ()
		{
			string keyName = @"SOFTWARE\Mono RegistrySecurityTest ChangeGroupToEveryone";

			RegistrySecurity security;
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			try {
				using (RegistryKey key = Registry.CurrentUser.CreateSubKey (keyName)) {
					// Before we begin manipulating this, make sure we're in the right spot.
					Assert.AreEqual (key.Name, @"HKEY_CURRENT_USER\" + keyName);

					// Set the group to Everyone.
					SecurityIdentifier worldSid = new SecurityIdentifier ("WD");

					security = key.GetAccessControl ();
					security.SetGroup (worldSid);
					key.SetAccessControl (security);

					// Make sure it actually became Everyone.
					security = key.GetAccessControl ();
					Assert.AreEqual (worldSid, security.GetGroup (typeof(SecurityIdentifier)));
				}
			} finally {
				Registry.CurrentUser.DeleteSubKey (keyName);
			}
		}

		[Test]
		public void EveryoneCanRead ()
		{
			string keyName = @"Software\Mono RegistrySecurityTest EveryoneCanRead";

			RegistrySecurity security;
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			try {
				using (RegistryKey key = Registry.CurrentUser.CreateSubKey (keyName)) {
					AuthorizationRuleCollection explicitRules, inheritedRules;

					// Before we begin manipulating this, make sure we're in the right spot.
					Assert.AreEqual (key.Name, @"HKEY_CURRENT_USER\" + keyName);

					// Let's add Everyone to the read list.
					SecurityIdentifier worldSid = new SecurityIdentifier ("WD");

					security = key.GetAccessControl ();
					inheritedRules = security.GetAccessRules (false, true, typeof (SecurityIdentifier));
					Assert.AreNotEqual (0, inheritedRules.Count);
					explicitRules = security.GetAccessRules (true, false, typeof (SecurityIdentifier));
					Assert.AreEqual (0, explicitRules.Count);

					security.AddAccessRule (new RegistryAccessRule (worldSid,
					                                                RegistryRights.FullControl,
					                                                AccessControlType.Allow));
					key.SetAccessControl (security);

					// Verify that we have our permission!
					security = key.GetAccessControl ();
					inheritedRules = security.GetAccessRules (false, true, typeof (SecurityIdentifier));
					Assert.AreNotEqual (0, inheritedRules.Count);
					explicitRules = security.GetAccessRules (true, false, typeof (SecurityIdentifier));
					Assert.AreEqual (1, explicitRules.Count);

					RegistryAccessRule rule = (RegistryAccessRule)explicitRules [0];
					Assert.AreEqual (AccessControlType.Allow, rule.AccessControlType);
					Assert.AreEqual (worldSid, rule.IdentityReference);
					Assert.AreEqual (InheritanceFlags.None, rule.InheritanceFlags);
					Assert.AreEqual (PropagationFlags.None, rule.PropagationFlags);
					Assert.AreEqual (RegistryRights.FullControl, rule.RegistryRights);
					Assert.IsFalse (rule.IsInherited);
				}
			} finally {
				Registry.CurrentUser.DeleteSubKey (keyName);
			}
		}
	}
}

#endif

