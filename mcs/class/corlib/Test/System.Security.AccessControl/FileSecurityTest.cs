// FileSecurityTest.cs - NUnit Test Cases for FileSecurity
//
// Authors:
//	James Bellinger (jfb@zer7.com)

using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using NUnit.Framework;

namespace MonoTests.System.Security.AccessControl
{
	[TestFixture]
	public class FileSecurityTest
	{
		[Test]
		public void ChangeGroupToEveryone ()
		{
			FileSecurity security;
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			string path = Path.GetTempFileName ();
			try {
				SecurityIdentifier worldSid = new SecurityIdentifier ("WD");

				security = File.GetAccessControl (path);
				security.SetGroup (worldSid);
				File.SetAccessControl (path, security);

				security = File.GetAccessControl (path);
				Assert.AreEqual (worldSid, security.GetGroup (typeof(SecurityIdentifier)));
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void ChangeAccessRules ()
		{
			FileSecurity security;
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			string path = Path.GetTempFileName ();
			try {
				// Add 'Everyone' to the access list.
				SecurityIdentifier worldSid = new SecurityIdentifier ("WD");

				security = File.GetAccessControl (path);
				FileSystemAccessRule rule = new FileSystemAccessRule (worldSid,
				                                                      FileSystemRights.FullControl,
				                                                      AccessControlType.Allow);
				security.AddAccessRule (rule);
				File.SetAccessControl (path, security);

				// Make sure 'Everyone' is *on* the access list.
				// Let's use the SafeHandle overload to check it.
				AuthorizationRuleCollection rules;
				using (FileStream file = File.Open (path, FileMode.Open, FileAccess.Read)) {
					security = file.GetAccessControl ();
					rules = security.GetAccessRules (true, false, typeof (SecurityIdentifier));

					Assert.AreEqual (1, rules.Count);
					Assert.AreEqual (worldSid, rules[0].IdentityReference);
					Assert.AreEqual (InheritanceFlags.None, rules[0].InheritanceFlags);
					Assert.AreEqual (PropagationFlags.None, rules[0].PropagationFlags);
					Assert.IsFalse (rules[0].IsInherited);
				}

				// Remove 'Everyone' from the access list.
				security.RemoveAccessRuleSpecific (rule);
				File.SetAccessControl (path, security);

				// Make sure our non-inherited access control list is now empty.
				security = File.GetAccessControl (path);
				rules = security.GetAccessRules (true, false, typeof (SecurityIdentifier));

				Assert.AreEqual (0, rules.Count);
			} finally {
				File.Delete (path);
			}
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void EveryoneMayNotBeOwner ()
		{
			FileSecurity security;
			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			string path = Path.GetTempFileName ();
			try {
				security = File.GetAccessControl (path);
				security.SetOwner (new SecurityIdentifier ("WD"));
				File.SetAccessControl (path, security);
			} finally {
				File.Delete (path);
			}
		}

	}
}


