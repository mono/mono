//
// RegistryKeyTest.cs - NUnit Test Cases for Microsoft.Win32.RegistryKey
//
// Authors:
//	mei (mei@work.email.ne.jp)
//      Robert Jordan (robertj@gmx.net)
//
// (C) 2005 mei
// (C) 2004, 2005 Novell (http://www.novell.com)
//

using NUnit.Framework;
using Microsoft.Win32;
using System;

namespace MonoTests.Microsoft.Win32
{
	[TestFixture]
	public class RegistryKeyTest
	{
		private const string mimeroot = @"MIME\Database\Content Type";
		[Test]
		[Category("NotWorking")]
		// This will not work on Linux ever
		public void TestGetValue ()
		{
			RegistryKey root = Registry.ClassesRoot;
			RegistryKey key;
			
			key = root.OpenSubKey (mimeroot + @"\audio/wav");
			Assert.AreEqual (".wav", key.GetValue ("Extension"), "GetValue #1");
			key = root.OpenSubKey (mimeroot + @"\text/x-scriptlet");
			Assert.AreEqual (null, key.GetValue ("Extension"), "GetValue #2");
		}

		//
		// Unit test for bug #77212
		//
		[Test]
		public void TestHandle ()
		{
			// this test is for Windows
			int p = (int) Environment.OSVersion.Platform;
			if ((p == 4) || (p == 128))
				return;

			// this regpath always exists under windows
			RegistryKey k = Registry.CurrentUser
				.OpenSubKey ("Software", false)
				.OpenSubKey ("Microsoft", false)
				.OpenSubKey ("Windows", false);
			
			Assert.IsNotNull (k, "#01");
		}

		[Test]
		public void OpenSubKeyTest ()
		{
			RegistryKey key = Registry.LocalMachine;

			// HKEY_LOCAL_MACHINE\software should always exist on Windows
			// and is automatically created on Linux
			Assert.IsNotNull (key.OpenSubKey ("Software"), "#A1");
			Assert.IsNotNull (key.OpenSubKey ("soFtware"), "#A2");

			key = Registry.CurrentUser;

			// HKEY_CURRENT_USER\software should always exist on Windows
			// and is automatically created on Linux
			Assert.IsNotNull (key.OpenSubKey ("Software"), "#B1");
			Assert.IsNotNull (key.OpenSubKey ("soFtware"), "#B2");

			key = Registry.Users;

			// HKEY_USERS\software should not exist on Windows, and should not
			// be created automatically on Linux
			Assert.IsNull (key.OpenSubKey ("Software"), "#C1");
			Assert.IsNull (key.OpenSubKey ("soFtware"), "#C2");
		}

		[Test]
		public void CreateSubKeyTest ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// check if key was successfully created
				Assert.IsNotNull (createdKey, "#1");
				// software subkey should not be created automatically
				Assert.IsNull (createdKey.OpenSubKey ("software"), "#2");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}
	}
}
