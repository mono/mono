// DirectorySecurityTest.cs - NUnit Test Cases for DirectorySecurity
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
	public class DirectorySecurityTest
	{
		[Test]
		public void InheritedPermissions ()
		{
			AuthorizationRuleCollection rules;
			DirectorySecurity dirSecurity; FileSecurity fileSecurity;
			SecurityIdentifier usersSid = new SecurityIdentifier ("BU");
			SecurityIdentifier worldSid = new SecurityIdentifier ("WD");
			FileSystemAccessRule worldDirFullControl = new FileSystemAccessRule
				(worldSid, FileSystemRights.FullControl,
				 InheritanceFlags.ObjectInherit, PropagationFlags.None,
				 AccessControlType.Allow);

			if (PlatformID.Win32NT != Environment.OSVersion.Platform) {
				Assert.Ignore (); return;
			}

			string dirpath = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			string dirpath2 = null;
			string filepath = null;
			DirectoryInfo dirinfo = Directory.CreateDirectory (dirpath);

			try {
				// Set Full Control to Everyone.
				dirSecurity = dirinfo.GetAccessControl ();
				dirSecurity.SetGroup (usersSid);
				dirSecurity.AddAccessRule (worldDirFullControl);
				Directory.SetAccessControl (dirpath, dirSecurity);

				// Did the rule store on the directory?
				dirSecurity = Directory.GetAccessControl (dirpath);
				rules = dirSecurity.GetAccessRules (true, false, typeof (SecurityIdentifier ));
				Assert.AreEqual (usersSid, dirSecurity.GetGroup (typeof(SecurityIdentifier)));
				Assert.AreEqual (1, rules.Count);
				Assert.AreEqual (worldSid, rules[0].IdentityReference);
				Assert.AreEqual (InheritanceFlags.ObjectInherit, rules[0].InheritanceFlags);
				Assert.AreEqual (PropagationFlags.None, rules[0].PropagationFlags);
				Assert.IsFalse (rules[0].IsInherited);

				// Create a file. It will have no explicit rules.
				filepath = Path.Combine (dirpath, Path.GetRandomFileName ());
				using (FileStream file = new FileStream (filepath, FileMode.Create, FileAccess.ReadWrite)) {
					fileSecurity = file.GetAccessControl ();

					rules = fileSecurity.GetAccessRules (true, false, typeof (SecurityIdentifier));
					Assert.AreEqual (0, rules.Count);
				}

				// Make sure the file has inherited the Full Control access rule.
				FileInfo fileInfo = new FileInfo (filepath);
				fileSecurity = fileInfo.GetAccessControl ();

				rules = fileSecurity.GetAccessRules (false, true, typeof (SecurityIdentifier));
				bool fileInheritedRule = false;
				foreach (FileSystemAccessRule rule in rules) {
					if (rule.AccessControlType == AccessControlType.Allow &&
					    rule.FileSystemRights == FileSystemRights.FullControl &&
					    rule.IdentityReference == worldSid &&
					    rule.IsInherited &&
					    rule.InheritanceFlags == InheritanceFlags.None &&
					    rule.PropagationFlags == PropagationFlags.None) // only containers get non-None flags
						fileInheritedRule = true;
				}
				Assert.IsTrue (fileInheritedRule);

				// ContainerInherit not being set, create a directory.
				// Its inherited rule will have propagation flags to indicate only its children are affected.
				dirpath2 = Path.Combine (dirpath, Path.GetRandomFileName ());
				dirinfo = Directory.CreateDirectory (dirpath2);
				dirSecurity = dirinfo.GetAccessControl ();

				rules = dirSecurity.GetAccessRules (false, true, typeof (SecurityIdentifier));
				bool dirInheritedRule = false;
				foreach (FileSystemAccessRule rule in rules) {
					if (rule.AccessControlType == AccessControlType.Allow &&
					    rule.FileSystemRights == FileSystemRights.FullControl &&
					    rule.IdentityReference == worldSid &&
					    rule.IsInherited &&
					    rule.InheritanceFlags == InheritanceFlags.ObjectInherit &&
					    rule.PropagationFlags == PropagationFlags.InheritOnly) // <-- key difference
						dirInheritedRule = true;
				}
				Assert.IsTrue (dirInheritedRule);

			} finally {
				if (null != filepath) File.Delete (filepath);
				if (null != dirpath2) Directory.Delete (dirpath2);
				Directory.Delete (dirpath);
			}
		}
	}
}


