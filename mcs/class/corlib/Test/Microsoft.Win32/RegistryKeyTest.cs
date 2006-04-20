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

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetValue_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// null value should result in ArgumentNullException
				createdKey.SetValue ("Name", null);
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_Boolean ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Installed"), "#1");
				// create value
				createdKey.SetValue ("Installed", true);
				// get value
				object value = createdKey.GetValue ("Installed");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual (true.ToString (), value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_Byte ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Flags"), "#1");
				// create value
				createdKey.SetValue ("Flags", (byte) 5);
				// get value
				object value = createdKey.GetValue ("Flags");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual ("5", value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_ByteArray ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Flags"), "#1");
				// create value
				createdKey.SetValue ("Flags", new byte[] { 1, 5 });
				// get value
				object value = createdKey.GetValue ("Flags");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (byte[]), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual (new byte[] { 1, 5 }, value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_DateTime ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				object rawValue = DateTime.Now;

				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Path"), "#1");
				// create value
				createdKey.SetValue ("Path", rawValue);
				// get value
				object value = createdKey.GetValue ("Path");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual (rawValue.ToString (), value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_Int32 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("RefCount"), "#1");
				// create value
				createdKey.SetValue ("RefCount", 5);
				// get value
				object value = createdKey.GetValue ("RefCount");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be int
				Assert.AreEqual (typeof (int), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual (5, value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_Int64 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Ticks"), "#1");
				// create value
				createdKey.SetValue ("Ticks", 500L);
				// get value
				object value = createdKey.GetValue ("Ticks");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual ("500", value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_String ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("Path"), "#1");
				// create value
				createdKey.SetValue ("Path", "/usr/lib/whatever");
				// get value
				object value = createdKey.GetValue ("Path");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual ("/usr/lib/whatever", value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_StringArray ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// we created a new subkey, so value should not exist
				Assert.IsNull (createdKey.GetValue ("DependsOnGroup"), "#1");
				// create value
				createdKey.SetValue ("DependsOnGroup", new string[] { "A", "B" });
				// get value
				object value = createdKey.GetValue ("DependsOnGroup");
				// value should exist
				Assert.IsNotNull (value, "#2");
				// type of value should be string
				Assert.AreEqual (typeof (string[]), value.GetType (), "#3");
				// ensure value matches
				Assert.AreEqual (new string[] { "A", "B" }, value, "#4");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}
	}
}
