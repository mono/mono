//
// RegistryKeyTest.cs - NUnit Test Cases for Microsoft.Win32.RegistryKey
//
// Authors:
//	mei (mei@work.email.ne.jp)
//	Robert Jordan (robertj@gmx.net)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005 mei
// (C) 2004, 2005 Novell (http://www.novell.com)
//

using System;
using System.IO;

using Microsoft.Win32;

using NUnit.Framework;

namespace MonoTests.Microsoft.Win32
{
	[TestFixture]
	public class RegistryKeyTest
	{
		private const string mimeroot = @"MIME\Database\Content Type";

		[Test]
		[Category ("NotWorking")] // this will not work on Linux ever
		public void TestGetValue ()
		{
			RegistryKey root = Registry.ClassesRoot;
			RegistryKey key;
			
			key = root.OpenSubKey (mimeroot + @"\audio/wav");
			Assert.AreEqual (".wav", key.GetValue ("Extension"), "GetValue #1");
			key = root.OpenSubKey (mimeroot + @"\text/x-scriptlet");
			Assert.AreEqual (null, key.GetValue ("Extension"), "GetValue #2");
		}

		[Test] // bug #77212
		public void TestHandle ()
		{
			// this test is for Windows only
			if (RunningOnUnix)
				return;

			// this regpath always exists under windows
			RegistryKey k = Registry.CurrentUser
				.OpenSubKey ("Software", false)
				.OpenSubKey ("Microsoft", false)
				.OpenSubKey ("Windows", false);
			
			Assert.IsNotNull (k, "#01");
		}

		[Test]
		public void OpenSubKey ()
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
		public void OpenSubKey_Key_DoesNotExist ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();
			Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName), "#1"); // read-only
			Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName, true), "#2"); // writable
		}

		[Test]
		public void OpenSubKey_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
					RegistryKey subKey = createdKey.CreateSubKey ("monotemp");
					subKey.Close ();
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					Assert.IsNotNull (createdKey, "#2");
					using (RegistryKey subKey = createdKey.OpenSubKey ("monotemp")) {
						Assert.IsNotNull (createdKey, "#3");
					}
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);

					// read-only
					Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName), "#4");
					Assert.IsNull (createdKey.OpenSubKey ("monotemp"), "#5"); // read-only
					// writable
					Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName, true), "#6");
					Assert.IsNull (createdKey.OpenSubKey ("monotemp", true), "#7"); 
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // MS should not allow this
		public void OpenSubKey_Name_Empty ()
		{
			// read-only
			using (RegistryKey emptyKey = Registry.CurrentUser.OpenSubKey (string.Empty)) {
				Assert.IsNotNull (emptyKey, "#1");
			}
			// writable
			using (RegistryKey emptyKey = Registry.CurrentUser.OpenSubKey (string.Empty, true)) {
				Assert.IsNotNull (emptyKey, "#1");
			}
		}

		[Test]
		public void OpenSubKey_Name_MaxLength ()
		{
			string name = new string ('a', 254);

			Assert.IsNull (Registry.CurrentUser.OpenSubKey (name), "#A1");

			name = new string ('a', 255);

			Assert.IsNull (Registry.CurrentUser.OpenSubKey (name), "#B1");

			name = new string ('a', 256);

			try {
				Registry.CurrentUser.OpenSubKey (name);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// 1.x: Registry subkeys should not be
				// greater than or equal to 255 characters
				//
				// 2.x: Registry subkeys should not be
				// greater than 255 characters
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#c4");
				Assert.IsTrue (ex.Message.IndexOf ("255") != -1, "#C5");
				Assert.IsNull (ex.ParamName, "#C6");
			}
		}

		[Test]
		public void OpenSubKey_Name_Null ()
		{
			try {
				Registry.CurrentUser.OpenSubKey (null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("name", ex.ParamName, "#A5");
			}

			try {
				Registry.CurrentUser.OpenSubKey (null, true);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("name", ex.ParamName, "#B5");
			}
		}

		[Test]
		public void Close_Local_Hive ()
		{
			RegistryKey hive = Registry.CurrentUser;
			hive.Close ();

			Assert.IsNotNull (hive.GetSubKeyNames (), "#1");
			Assert.IsNull (hive.GetValue ("doesnotexist"), "#2");
			Assert.IsNotNull (hive.GetValueNames (), "#3");
			Assert.IsNull (hive.OpenSubKey ("doesnotexist"), "#4");
			Assert.IsNotNull (hive.SubKeyCount, "#5");
			Assert.IsNotNull (hive.ToString (), "#6");

			// closing key again does not have any effect
			hive.Close ();
		}

		[Test]
		public void Close_Local_Key ()
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey ("SOFTWARE");
			key.Close ();

			// closing a key twice does not have any effect
			key.Close ();

			try {
				key.CreateSubKey ("a");
				Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteSubKey ("doesnotexist");
				Assert.Fail ("#2");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteSubKeyTree ("doesnotexist");
				Assert.Fail ("#3");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteValue ("doesnotexist");
				Assert.Fail ("#4");
			} catch (ObjectDisposedException) {
			}

			// flushing a closed key does not have any effect
			key.Flush ();

			try {
				key.GetSubKeyNames ();
				Assert.Fail ("#5");
			} catch (ObjectDisposedException) {
			}

			try {
				key.GetValue ("doesnotexist");
				Assert.Fail ("#6");
			} catch (ObjectDisposedException) {
			}

			try {
				key.GetValueNames ();
				Assert.Fail ("#7");
			} catch (ObjectDisposedException) {
			}

			try {
				key.OpenSubKey ("doesnotexist");
				Assert.Fail ("#8");
			} catch (ObjectDisposedException) {
			}

			try {
				key.SetValue ("doesnotexist", "something");
				Assert.Fail ("#9");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = key.SubKeyCount;
				Assert.Fail ("#10:" + x);
			} catch (ObjectDisposedException) {
			}

			try {
				key.ToString ();
				Assert.Fail ("#11");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = key.ValueCount;
				Assert.Fail ("#12:" + x);
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		public void Close_Remote_Hive ()
		{
			// access to registry of remote machines is not implemented on unix
			if (RunningOnUnix)
				return;

			RegistryKey hive = RegistryKey.OpenRemoteBaseKey (
				RegistryHive.CurrentUser, Environment.MachineName);
			hive.Close ();

			// closing a remote hive twice does not have any effect
			hive.Close ();

			try {
				hive.CreateSubKey ("a");
				Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.DeleteSubKey ("doesnotexist");
				Assert.Fail ("#2");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.DeleteSubKeyTree ("doesnotexist");
				Assert.Fail ("#3");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.DeleteValue ("doesnotexist");
				Assert.Fail ("#4");
			} catch (ObjectDisposedException) {
			}

			// flushing a closed hive does not have any effect
			hive.Flush ();

			try {
				hive.GetSubKeyNames ();
				Assert.Fail ("#5");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.GetValue ("doesnotexist");
				Assert.Fail ("#6");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.GetValueNames ();
				Assert.Fail ("#7");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.OpenSubKey ("doesnotexist");
				Assert.Fail ("#8");
			} catch (ObjectDisposedException) {
			}

			try {
				hive.SetValue ("doesnotexist", "something");
				Assert.Fail ("#9");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = hive.SubKeyCount;
				Assert.Fail ("#10:" + x);
			} catch (ObjectDisposedException) {
			}

			try {
				hive.ToString ();
				Assert.Fail ("#11");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = hive.ValueCount;
				Assert.Fail ("#12:" + x);
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		public void Close_Remote_Key ()
		{
			// access to registry of remote machines is not implemented on unix
			if (RunningOnUnix)
				return;

			RegistryKey hive = RegistryKey.OpenRemoteBaseKey (
				RegistryHive.CurrentUser, Environment.MachineName);
			RegistryKey key = hive.OpenSubKey ("SOFTWARE");
			key.Close ();

			// closing a remote key twice does not have any effect
			key.Close ();

			try {
				key.CreateSubKey ("a");
				Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteSubKey ("doesnotexist");
				Assert.Fail ("#2");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteSubKeyTree ("doesnotexist");
				Assert.Fail ("#3");
			} catch (ObjectDisposedException) {
			}

			try {
				key.DeleteValue ("doesnotexist");
				Assert.Fail ("#4");
			} catch (ObjectDisposedException) {
			}

			// flushing a closed key does not have any effect
			key.Flush ();

			try {
				key.GetSubKeyNames ();
				Assert.Fail ("#5");
			} catch (ObjectDisposedException) {
			}

			try {
				key.GetValue ("doesnotexist");
				Assert.Fail ("#6");
			} catch (ObjectDisposedException) {
			}

			try {
				key.GetValueNames ();
				Assert.Fail ("#7");
			} catch (ObjectDisposedException) {
			}

			try {
				key.OpenSubKey ("doesnotexist");
				Assert.Fail ("#8");
			} catch (ObjectDisposedException) {
			}

			try {
				key.SetValue ("doesnotexist", "something");
				Assert.Fail ("#9");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = key.SubKeyCount;
				Assert.Fail ("#10:" + x);
			} catch (ObjectDisposedException) {
			}

			try {
				key.ToString ();
				Assert.Fail ("#11");
			} catch (ObjectDisposedException) {
			}

			try {
				int x = key.ValueCount;
				Assert.Fail ("#12:" + x);
			} catch (ObjectDisposedException) {
			}

			hive.Close ();
		}

		[Test]
		public void CreateSubKey ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					// software subkey should not be created automatically
					Assert.IsNull (createdKey.OpenSubKey ("software"), "#A2");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#B1");
					// software subkey should not be created automatically
					Assert.IsNull (createdKey.OpenSubKey ("software"), "#B2");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						// check if key was successfully created
						Assert.IsNotNull (createdKey, "#C1");
						// software subkey should not be created automatically
						Assert.IsNull (softwareKey.OpenSubKey ("software"), "#C2");
					}

					using (RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName)) {
						// check if key was successfully created
						Assert.IsNotNull (createdKey, "#D1");
						// software subkey should not be created automatically
						Assert.IsNull (softwareKey.OpenSubKey ("software"), "#D2");
					}
				} finally {
					// clean-up
					softwareKey.DeleteSubKeyTree (subKeyName);
				}
			}
		}

		[Test]
		public void CreateSubKey_Key_ReadOnly ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software")) {
				RegistryKey createdKey = null;
				try {
					try {
						createdKey = softwareKey.CreateSubKey (subKeyName);
						Assert.Fail ("#1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
						Assert.IsNotNull (ex.Message, "#3");
						Assert.IsNull (ex.InnerException, "#4");
					}
				} finally {
					if (createdKey != null)
						createdKey.Close ();
				}
			}
		}

		[Test]
		public void CreateSubKey_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						softwareKey.DeleteSubKeyTree (subKeyName);
						Assert.IsNull (softwareKey.OpenSubKey (subKeyName), "#1");
						try {
							createdKey.CreateSubKey ("test");
							Assert.Fail ("#2");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#3");
							Assert.IsNotNull (ex.Message, "#4");
							Assert.IsNull (ex.InnerException, "#5");
						}
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // MS should not allow this
		public void CreateSubKey_Name_Empty ()
		{
			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				using (RegistryKey emptyKey = softwareKey.CreateSubKey (string.Empty)) {
					Assert.IsNotNull (emptyKey, "#1");
					emptyKey.SetValue ("name1", "value1");
				}
			}
		}

		[Test]
		public void CreateSubKey_Name_MaxLength ()
		{
			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				string subKeyName = new string ('a', 254);

				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						Assert.IsNotNull (createdKey, "#A1");
						Assert.IsNotNull (softwareKey.OpenSubKey (subKeyName), "#A2");
					}
				} finally {
					softwareKey.DeleteSubKeyTree (subKeyName);
				}

				subKeyName = new string ('a', 255);

				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						Assert.IsNotNull (createdKey, "#B1");
						Assert.IsNotNull (softwareKey.OpenSubKey (subKeyName), "#B2");
					}
				} finally {
					softwareKey.DeleteSubKey (subKeyName);
				}

				subKeyName = new string ('a', 256);

				try {
					softwareKey.CreateSubKey (subKeyName);
					Assert.Fail ("#C1");
				} catch (ArgumentException ex) {
					// 1.x: Registry subkeys should not be
					// greater than or equal to 255 characters
					//
					// 2.x: Registry subkeys should not be
					// greater than 255 characters
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
					Assert.IsNull (ex.InnerException, "#C3");
					Assert.IsNotNull (ex.Message, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("255") != -1, "#C5");
					Assert.IsNull (ex.ParamName, "#C6");
				}
			}
		}

		[Test]
		public void CreateSubKey_Name_Null ()
		{
			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					softwareKey.CreateSubKey (null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("name", ex.ParamName, "#5");
				}
			}
		}

#if NET_4_0
		// Unfortunately we can't test that the scenario where a volatile
		// key is not alive after a reboot, but we can test other bits.
		[Test]
		public void CreateSubKey_Volatile ()
		{
			RegistryKey key = null;
			RegistryKey subkey = null;
			string subKeyName = "VolatileKey";

			try {
				key = Registry.CurrentUser.CreateSubKey (subKeyName, RegistryKeyPermissionCheck.Default, RegistryOptions.Volatile);
				subkey = key.CreateSubKey ("Child", RegistryKeyPermissionCheck.Default, RegistryOptions.Volatile);
				key.Close ();

				key = Registry.CurrentUser.OpenSubKey (subKeyName);
				subkey = key.OpenSubKey ("Child");
				Assert.AreEqual (true, subkey != null, "#A1");
			} finally {
				if (subkey != null)
					subkey.Close ();
				if (key != null)
					key.Close ();
			}
		}

		[Test]
		public void CreateSubKey_Volatile_Child ()
		{
			RegistryKey key = null;
			RegistryKey subkey = null;
			string subKeyName = "VolatileKey";

			try {
				key = Registry.CurrentUser.CreateSubKey (subKeyName, RegistryKeyPermissionCheck.Default, RegistryOptions.Volatile);
				subkey = key.CreateSubKey ("Child"); // Non volatile child
				Assert.Fail ("#Exc");
			} catch (IOException) {
			} finally {
				if (subkey != null)
					subkey.Close ();
				if (key != null)
					key.Close ();
			}
		}

		[Test]
		public void CreateSubKey_Volatile_Conflict ()
		{
			RegistryKey key = null;
			RegistryKey key2 = null;
			RegistryKey subkey = null;
			string subKeyName = "VolatileKey";

			try {
				// 
				// Create a volatile key and try to open it as a normal one
				//
				key = Registry.CurrentUser.CreateSubKey (subKeyName, RegistryKeyPermissionCheck.Default, RegistryOptions.Volatile);
				key2 = Registry.CurrentUser.CreateSubKey (subKeyName, RegistryKeyPermissionCheck.Default, RegistryOptions.None);
				Assert.AreEqual (key.Name, key2.Name, "A0");

				subkey = key2.CreateSubKey ("Child", RegistryKeyPermissionCheck.Default, RegistryOptions.Volatile);
				Assert.AreEqual (true, key.OpenSubKey ("Child") != null, "#A1");
				Assert.AreEqual (true, key2.OpenSubKey ("Child") != null, "#A2");

				subkey.Close ();
				key.Close ();
				key2.Close ();

				// 
				// Create a non-volatile key and try to open it as a volatile one
				//
				subKeyName = "NonVolatileKey";
				key2 = Registry.CurrentUser.CreateSubKey (subKeyName, RegistryKeyPermissionCheck.Default, RegistryOptions.None);
				key2.SetValue ("Name", "Mono");
				key = Registry.CurrentUser.CreateSubKey (subKeyName, RegistryKeyPermissionCheck.Default, RegistryOptions.Volatile);
				Assert.AreEqual (key.Name, key2.Name, "B0");
				Assert.AreEqual ("Mono", key.GetValue ("Name"), "#B1");
				Assert.AreEqual ("Mono", key2.GetValue ("Name"), "#B2");

				key.CreateSubKey ("Child");
				Assert.AreEqual (true, key.OpenSubKey ("Child") != null, "#B3");
				Assert.AreEqual (true, key2.OpenSubKey ("Child") != null, "#B4");

				// 
				// Close the non-volatile key and try to re-open it as a volatile one
				//
				key.Close ();
				key2.Close ();
				key = Registry.CurrentUser.CreateSubKey (subKeyName, RegistryKeyPermissionCheck.Default, RegistryOptions.Volatile);
				Assert.AreEqual ("Mono", key.GetValue ("Name"), "#C0");
				Assert.AreEqual (true, key.OpenSubKey ("Child") != null, "#C1");
			} finally {
				if (subkey != null)
					subkey.Close ();
				if (key != null)
					key.Close ();
				if (key2 != null)
					key2.Close ();
			}
		}

		[Test]
		public void DeleteSubKey_Volatile ()
		{			
			RegistryKey key = null;
			RegistryKey subkey = null;
			string subKeyName = "VolatileKey";

			try {
				key = Registry.CurrentUser.CreateSubKey (subKeyName, RegistryKeyPermissionCheck.Default, RegistryOptions.Volatile);
				key.CreateSubKey ("VolatileKeyChild", RegistryKeyPermissionCheck.Default, RegistryOptions.Volatile);
				key.SetValue ("Name", "Mono");
				key.Close ();

				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);

				key = Registry.CurrentUser.OpenSubKey (subKeyName);
				Assert.AreEqual (null, key, "#A0");
			} finally {
				if (subkey != null)
					subkey.Close ();
				if (key != null)
					key.Close ();
			}
		}

		// Define a normal key, and create a normal and a volatile key under it, and retrieve their names.
		[Test]
		public void GetSubKeyNames_Volatile ()
		{           
			RegistryKey key = null;
			RegistryKey subkey = null;
			string subKeyName = Guid.NewGuid ().ToString ();
			string volChildKeyName = "volatilechildkey";
			string childKeyName = "childkey";

			try {
				key = Registry.CurrentUser.CreateSubKey (subKeyName);
				key.CreateSubKey (volChildKeyName, RegistryKeyPermissionCheck.Default, RegistryOptions.Volatile);
				key.CreateSubKey (childKeyName, RegistryKeyPermissionCheck.Default, RegistryOptions.None);
				key.Close ();

				key = Registry.CurrentUser.OpenSubKey (subKeyName);
				string [] keyNames = key.GetSubKeyNames ();

				// we can guarantee the order of the child keys, so we sort the two of them
				Array.Sort (keyNames);

				Assert.AreEqual (2, keyNames.Length, "#A0");
				Assert.AreEqual (childKeyName, keyNames [0], "#A1");
				Assert.AreEqual (volChildKeyName, keyNames [1], "#A2");
			} finally {
				if (subkey != null)
					subkey.Close ();
				if (key != null)
					key.Close ();
			}

		}
#endif

		[Test]
		public void DeleteSubKey ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey, "#2");
					Registry.CurrentUser.DeleteSubKey (subKeyName);
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNull (createdKey, "#3");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteSubKey_Key_HasChildKeys ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
					RegistryKey subKey = createdKey.CreateSubKey ("monotemp");
					subKey.Close ();
				}
				try {
					Registry.CurrentUser.DeleteSubKey (subKeyName);
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// Registry key has subkeys and recursive removes are not
					// supported by this method
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.InnerException, "#5");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteSubKey_Key_ReadOnly ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
					RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName);
					createdKey.Close ();
				}

				using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software")) {
					try {
						softwareKey.DeleteSubKey (subKeyName);
						Assert.Fail ("#1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
						Assert.IsNotNull (ex.Message, "#3");
						Assert.IsNull (ex.InnerException, "#4");
					}
				}
			} finally {
				try {
					using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteSubKey_Key_DoesNotExist ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				Registry.CurrentUser.DeleteSubKey (subKeyName);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Cannot delete a subkey tree because the subkey does not exist
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			try {
				Registry.CurrentUser.DeleteSubKey (subKeyName, true);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Cannot delete a subkey tree because the subkey does not exist
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			Registry.CurrentUser.DeleteSubKey (subKeyName, false);
		}

		[Test]
		public void DeleteSubKey_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
					RegistryKey subKey = createdKey.CreateSubKey ("monotemp");
					subKey.Close ();
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					Assert.IsNotNull (createdKey, "#2");
					using (RegistryKey subKey = createdKey.OpenSubKey ("monotemp")) {
						Assert.IsNotNull (createdKey, "#3");
					}
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName), "#4");
					try {
						createdKey.DeleteSubKey ("monotemp");
						Assert.Fail ("#5");
					} catch (ArgumentException ex) {
						// Cannot delete a subkey tree because the subkey does
						// not exist
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#6");
						Assert.IsNull (ex.InnerException, "#7");
						Assert.IsNotNull (ex.Message, "#8");
						Assert.IsNull (ex.ParamName, "#9");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // MS should not allow this
		public void DeleteSubKey_Name_Empty ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName);
					createdKey.DeleteSubKey (string.Empty);
					createdKey.Close ();

					createdKey = softwareKey.OpenSubKey (subKeyName);
					Assert.IsNull (createdKey, "#1");
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null)
							createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					} catch {
					}
				}
			}
		}

		[Test]
		public void DeleteSubKey_Name_MaxLength ()
		{
			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				string subKeyName = new string ('a', 254);

				using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
					createdKey.Close ();
				}
				using (RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName, false)) {
					Assert.IsNotNull (createdKey, "#A1");
				}
				softwareKey.DeleteSubKey (subKeyName);
				using (RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName, false)) {
					Assert.IsNull (createdKey, "#A2");
				}

				subKeyName = new string ('a', 256);

				try {
					softwareKey.DeleteSubKey (subKeyName);
					Assert.Fail ("#B1");
				} catch (ArgumentException ex) {
					// 1.x: Registry subkeys should not be
					// greater than or equal to 255 characters
					//
					// 2.x: Registry subkeys should not be
					// greater than 255 characters
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("255") != -1, "#B5");
					Assert.IsNull (ex.ParamName, "#B6");
				}
			}
		}

		[Test]
		public void DeleteSubKey_Name_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName);
					try {
						createdKey.DeleteSubKey (null);
						Assert.Fail ("#1");
					} catch (ArgumentNullException ex) {
						Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
						Assert.AreEqual ("name", ex.ParamName, "#5");
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null)
							createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					} catch {
					}
				}
			}
		}

		[Test]
		public void DeleteSubKeyTree ()
		{
			// TODO: 
			// - remove key with subkeys
			// - remove key of which some subkeys are marked for deletion
			// - remove key with values
		}

		[Test]
		public void DeleteSubKeyTree_Key_DoesNotExist ()
		{
			// Cannot delete a subkey tree because the subkey does not exist
			string subKeyName = Guid.NewGuid ().ToString ();
			try {
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void DeleteSubKeyTree_Key_ReadOnly ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
					RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName);
					createdKey.Close ();
				}

				using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software")) {
					try {
						softwareKey.DeleteSubKeyTree (subKeyName);
						Assert.Fail ("#1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
						Assert.IsNotNull (ex.Message, "#3");
						Assert.IsNull (ex.InnerException, "#4");
					}
				}
			} finally {
				try {
					using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null)
							createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteSubKeyTree_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
					RegistryKey subKey = createdKey.CreateSubKey ("monotemp");
					subKey.Close ();
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					Assert.IsNotNull (createdKey, "#2");
					using (RegistryKey subKey = createdKey.OpenSubKey ("monotemp")) {
						Assert.IsNotNull (createdKey, "#3");
					}
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName), "#4");
					try {
						createdKey.DeleteSubKeyTree ("monotemp");
						Assert.Fail ("#5");
					} catch (ArgumentException ex) {
						// Cannot delete a subkey tree because the subkey does
						// not exist
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#6");
						Assert.IsNull (ex.InnerException, "#7");
						Assert.IsNotNull (ex.Message, "#8");
						Assert.IsNull (ex.ParamName, "#9");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // MS should not allow this
		public void DeleteSubKeyTree_Name_Empty ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName);
					createdKey.DeleteSubKeyTree (string.Empty);
					createdKey.Close ();

					createdKey = softwareKey.OpenSubKey (subKeyName);
					Assert.IsNull (createdKey, "#1");
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null)
							createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					} catch {
					}
				}
			}
		}

		[Test]
		public void DeleteSubKeyTree_Name_MaxLength ()
		{
			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				string subKeyName = new string ('a', 254);

				using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
					createdKey.Close ();
				}
				using (RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName, false)) {
					Assert.IsNotNull (createdKey, "#A1");
				}
				softwareKey.DeleteSubKeyTree (subKeyName);
				using (RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName, false)) {
					Assert.IsNull (createdKey, "#A2");
				}

#if ONLY_1_1
				subKeyName = new string ('a', 255);
#else
				subKeyName = new string ('a', 256);
#endif

				try {
					softwareKey.DeleteSubKeyTree (subKeyName);
					Assert.Fail ("#B1");
				} catch (ArgumentException ex) {
					// 1.x: Registry subkeys should not be
					// greater than or equal to 255 characters
					//
					// 2.x: Registry subkeys should not be
					// greater than 255 characters
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("255") != -1, "#B5");
					Assert.IsNull (ex.ParamName, "#B6");
				}
			}
		}

		[Test]
		public void DeleteSubKeyTree_Name_Null ()
		{
			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					softwareKey.DeleteSubKeyTree (null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("name", ex.ParamName, "#5");
				}
			}
		}

		[Test]
		public void DeleteValue ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#A2");
					Assert.AreEqual (2, names.Length, "#A3");
					Assert.IsNotNull (names [0], "#A4");
					Assert.AreEqual ("name1", names [0], "#A5");
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#A6");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#A7");
					Assert.AreEqual ("name2", names [1], "#A8");
					Assert.IsNotNull (createdKey.GetValue ("name2"), "#A9");
					Assert.AreEqual ("value2", createdKey.GetValue ("name2"), "#A10");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					Assert.IsNotNull (createdKey, "#B1");
					createdKey.DeleteValue ("name1");
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#B2");
					Assert.AreEqual (1, names.Length, "#B3");
					Assert.IsNotNull (names [0], "#B4");
					Assert.AreEqual ("name2", names [0], "#B5");
					Assert.IsNotNull (createdKey.GetValue ("name2"), "#B6");
					Assert.AreEqual ("value2", createdKey.GetValue ("name2"), "#B7");
					createdKey.DeleteValue (new string ('a', 400), false);
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#C1");
					Assert.AreEqual (1, names.Length, "#C2");
					Assert.IsNotNull (names [0], "#C3");
					Assert.AreEqual ("name2", names [0], "#C4");
					Assert.IsNotNull (createdKey.GetValue ("name2"), "#C5");
					Assert.AreEqual ("value2", createdKey.GetValue ("name2"), "#C6");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteValue_Key_ReadOnly ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					try {
						// deleting value that exists
						createdKey.DeleteValue ("name1");
						Assert.Fail ("#A1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#A2");
						Assert.IsNotNull (ex.Message, "#A3");
						Assert.IsNull (ex.InnerException, "#A4");
					}

					try {
						// deleting value that exists
						createdKey.DeleteValue ("name1", true);
						Assert.Fail ("#B1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#B2");
						Assert.IsNotNull (ex.Message, "#B3");
						Assert.IsNull (ex.InnerException, "#B4");
					}

					try {
						// deleting value that exists
						createdKey.DeleteValue ("name1", false);
						Assert.Fail ("#C1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
					}

					try {
						// deleting value that does not exist
						createdKey.DeleteValue ("name2");
						Assert.Fail ("#D1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#D2");
						Assert.IsNotNull (ex.Message, "#D3");
						Assert.IsNull (ex.InnerException, "#D4");
					}

					try {
						// deleting value that does not exist
						createdKey.DeleteValue ("name2", true);
						Assert.Fail ("#E1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#E2");
						Assert.IsNotNull (ex.Message, "#E3");
						Assert.IsNull (ex.InnerException, "#E4");
					}

					try {
						// deleting value that does not exist
						createdKey.DeleteValue ("name2", false);
						Assert.Fail ("#F1");
					} catch (UnauthorizedAccessException ex) {
						// Cannot write to the registry key
						Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#F2");
						Assert.IsNotNull (ex.Message, "#F3");
						Assert.IsNull (ex.InnerException, "#F4");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteValue_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#1");
					createdKey.SetValue ("name1", "value1");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					Assert.IsNotNull (createdKey, "#2");
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					Assert.IsNull (Registry.CurrentUser.OpenSubKey (subKeyName), "#3");

					createdKey.DeleteValue ("name1");
					createdKey.DeleteValue ("name1", true);
					createdKey.DeleteValue ("name1", false);
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteValue_Value_DoesNotExist ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					createdKey.SetValue ("name1", "value1");

					try {
						createdKey.DeleteValue ("name2");
						Assert.Fail ("#B1");
					} catch (ArgumentException ex) {
						// No value exists with that name
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
						Assert.IsNull (ex.InnerException, "#B3");
						Assert.IsNotNull (ex.Message, "#B4");
						Assert.IsNull (ex.ParamName, "#B5");
					}

					try {
						createdKey.DeleteValue ("name2", true);
						Assert.Fail ("#C1");
					} catch (ArgumentException ex) {
						// No value exists with that name
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
						Assert.IsNull (ex.InnerException, "#C3");
						Assert.IsNotNull (ex.Message, "#C4");
						Assert.IsNull (ex.ParamName, "#C5");
					}

					createdKey.DeleteValue ("name2", false);
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteValue_Name_Empty ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue (string.Empty, "value2");

					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#A1");
					Assert.AreEqual (2, names.Length, "#A2");
					Assert.IsNotNull (names [0], "#A3");
					/*
					Assert.AreEqual ("name1", names [0], "#A4");
					*/
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#A5");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#A6");
					Assert.IsNotNull (names [1], "#A7");
					/*
					Assert.AreEqual (string.Empty, names [1], "#A8");
					*/
					Assert.IsNotNull (createdKey.GetValue (string.Empty), "#A9");
					Assert.AreEqual ("value2", createdKey.GetValue (string.Empty), "#A10");

					createdKey.DeleteValue (string.Empty);

					names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#B1");
					Assert.AreEqual (1, names.Length, "#B2");
					Assert.IsNotNull (names [0], "#B3");
					Assert.AreEqual ("name1", names [0], "#B4");
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#B5");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#B6");

					try {
						createdKey.DeleteValue (string.Empty);
						Assert.Fail ("#C1");
					} catch (ArgumentException ex) {
						// No value exists with that name
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
						Assert.IsNull (ex.InnerException, "#C3");
						Assert.IsNotNull (ex.Message, "#C4");
						Assert.IsNull (ex.ParamName, "#C5");
					}

					try {
						createdKey.DeleteValue (string.Empty, true);
						Assert.Fail ("#D1");
					} catch (ArgumentException ex) {
						// No value exists with that name
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
						Assert.IsNull (ex.InnerException, "#D3");
						Assert.IsNotNull (ex.Message, "#D4");
						Assert.IsNull (ex.ParamName, "#D5");
					}

					createdKey.DeleteValue (string.Empty, false);

					names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#E1");
					Assert.AreEqual (1, names.Length, "#E2");
					Assert.IsNotNull (names [0], "#E3");
					Assert.AreEqual ("name1", names [0], "#E4");
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#E5");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#E6");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void DeleteValue_Name_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue (null, "value2");

					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#A1");
					Assert.AreEqual (2, names.Length, "#A2");
					Assert.IsNotNull (names [0], "#A3");
					/*
					Assert.AreEqual ("name1", names [0], "#A4");
					*/
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#A5");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#A6");
					Assert.IsNotNull (names [1], "#A7");
					/*
					Assert.AreEqual (string.Empty, names [1], "#A8");
					*/
					Assert.IsNotNull (createdKey.GetValue (null), "#A9");
					Assert.AreEqual ("value2", createdKey.GetValue (null), "#A10");

					try {
						createdKey.DeleteValue (null);
						Assert.Fail ("#B1");
					} catch (ArgumentNullException ex) {
						Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
						Assert.IsNull (ex.InnerException, "#B3");
						Assert.IsNotNull (ex.Message, "#B4");
						Assert.AreEqual ("name", ex.ParamName, "#B5");
					}

					try {
						createdKey.DeleteValue (null, true);
						Assert.Fail ("#C1");
					} catch (ArgumentNullException ex) {
						Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
						Assert.IsNull (ex.InnerException, "#C3");
						Assert.IsNotNull (ex.Message, "#C4");
						Assert.AreEqual ("name", ex.ParamName, "#C5");
					}

					try {
						createdKey.DeleteValue (null, false);
						Assert.Fail ("#D1");
					} catch (ArgumentNullException ex) {
						Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#D2");
						Assert.IsNull (ex.InnerException, "#D3");
						Assert.IsNotNull (ex.Message, "#D4");
						Assert.AreEqual ("name", ex.ParamName, "#D5");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValue ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey.GetValue ("name1"), "#1");
					Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#2");
					Assert.IsNotNull (createdKey.GetValue ("name2"), "#3");
					Assert.AreEqual ("value2", createdKey.GetValue ("name2"), "#4");
					Assert.IsNull (createdKey.GetValue ("name3"), "#5");
					Assert.AreEqual ("value3", createdKey.GetValue ("name3", "value3"), "#6");
					Assert.IsNull (createdKey.GetValue ("name3", null), "#7");
					Assert.IsNull (createdKey.GetValue (new string ('a', 400)), "#8");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValue_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);

					Assert.IsNull (createdKey.GetValue ("name1"), "#1");
					Assert.IsNotNull (createdKey.GetValue ("name1", "default"), "#2");
					Assert.AreEqual ("default", createdKey.GetValue ("name1", "default"), "#3");
					Assert.IsNull (createdKey.GetValue ("name3"), "#3");
					Assert.IsNotNull (createdKey.GetValue ("name3", "default"), "#4");
					Assert.AreEqual ("default", createdKey.GetValue ("name3", "default"), "#5");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValue_Name_Empty ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");

					Assert.IsNull (createdKey.GetValue (string.Empty), "#A1");
					Assert.IsNotNull (createdKey.GetValue (string.Empty, "default"), "#A2");
					Assert.AreEqual ("default", createdKey.GetValue (string.Empty, "default"), "#A3");
					Assert.IsNull (createdKey.GetValue (string.Empty, null), "#A4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNull (createdKey.GetValue (string.Empty), "#B1");
					Assert.IsNotNull (createdKey.GetValue (string.Empty, "default"), "#B2");
					Assert.AreEqual ("default", createdKey.GetValue (string.Empty, "default"), "#B3");
					Assert.IsNull (createdKey.GetValue (string.Empty, null), "#B4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					createdKey.SetValue (string.Empty, "value1");
					Assert.IsNotNull (createdKey.GetValue (string.Empty), "#C1");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty), "#C2");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty, "default"), "#C3");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty, null), "#C4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey.GetValue (string.Empty), "#D1");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty), "#D2");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty, "default"), "#D3");
					Assert.AreEqual ("value1", createdKey.GetValue (string.Empty, null), "#D4");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValue_Name_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");

					Assert.IsNull (createdKey.GetValue (null), "#A1");
					Assert.IsNotNull (createdKey.GetValue (null, "default"), "#A2");
					Assert.AreEqual ("default", createdKey.GetValue (null, "default"), "#A3");
					Assert.IsNull (createdKey.GetValue (null, null), "#A4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNull (createdKey.GetValue (null), "#B1");
					Assert.IsNotNull (createdKey.GetValue (null, "default"), "#B2");
					Assert.AreEqual ("default", createdKey.GetValue (null, "default"), "#B3");
					Assert.IsNull (createdKey.GetValue (null, null), "#B4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					createdKey.SetValue (string.Empty, "value1");
					Assert.IsNotNull (createdKey.GetValue (null), "#C1");
					Assert.AreEqual ("value1", createdKey.GetValue (null), "#C2");
					Assert.AreEqual ("value1", createdKey.GetValue (null, "default"), "#C3");
					Assert.AreEqual ("value1", createdKey.GetValue (null, null), "#C4");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey.GetValue (null), "#D1");
					Assert.AreEqual ("value1", createdKey.GetValue (null), "#D2");
					Assert.AreEqual ("value1", createdKey.GetValue (null, "default"), "#D3");
					Assert.AreEqual ("value1", createdKey.GetValue (null, null), "#D4");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValue_Expand ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					Environment.SetEnvironmentVariable ("MONO_TEST1", "123");
					Environment.SetEnvironmentVariable ("MONO_TEST2", "456");

					createdKey.SetValue ("name1", "%MONO_TEST1%/%MONO_TEST2%",
						RegistryValueKind.ExpandString);
					createdKey.SetValue ("name2", "%MONO_TEST1%/%MONO_TEST2%");
					createdKey.SetValue ("name3", "just some text",
						RegistryValueKind.ExpandString);

					Assert.AreEqual ("123/456", createdKey.GetValue ("name1"), "#A1");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2"), "#A2");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3"), "#A3");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name1",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#A4");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#A5");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#A6");
					Assert.AreEqual ("123/456", createdKey.GetValue ("name1",
						null, RegistryValueOptions.None), "#A7");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.None), "#A8");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.None), "#A9");

					Environment.SetEnvironmentVariable ("MONO_TEST1", "789");
					Environment.SetEnvironmentVariable ("MONO_TEST2", "666");

					Assert.AreEqual ("789/666", createdKey.GetValue ("name1"), "#B1");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2"), "#B2");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3"), "#B3");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name1",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#B4");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#B5");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#B6");
					Assert.AreEqual ("789/666", createdKey.GetValue ("name1",
						null, RegistryValueOptions.None), "#B7");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.None), "#B8");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.None), "#B9");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.AreEqual ("789/666", createdKey.GetValue ("name1"), "#C1");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2"), "#C2");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3"), "#C3");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name1",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#C4");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#C5");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#C6");
					Assert.AreEqual ("789/666", createdKey.GetValue ("name1",
						null, RegistryValueOptions.None), "#C7");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.None), "#C8");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.None), "#C9");

					Environment.SetEnvironmentVariable ("MONO_TEST1", "123");
					Environment.SetEnvironmentVariable ("MONO_TEST2", "456");

					Assert.AreEqual ("123/456", createdKey.GetValue ("name1"), "#D1");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2"), "#D2");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3"), "#D3");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name1",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#D4");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#D5");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.DoNotExpandEnvironmentNames), "#D6");
					Assert.AreEqual ("123/456", createdKey.GetValue ("name1",
						null, RegistryValueOptions.None), "#D7");
					Assert.AreEqual ("%MONO_TEST1%/%MONO_TEST2%", createdKey.GetValue ("name2",
						null, RegistryValueOptions.None), "#D8");
					Assert.AreEqual ("just some text", createdKey.GetValue ("name3",
						null, RegistryValueOptions.None), "#D9");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValueNames ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#A1");
					Assert.AreEqual (0, names.Length, "#A2");

					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");
					createdKey.SetValue ("namelong", "value3");
					createdKey.SetValue ("name3", "value4");

					Assert.AreEqual (4, createdKey.ValueCount, "#B1");
					names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#B2");
					Assert.AreEqual (4, names.Length, "#B3");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#C1");
					Assert.AreEqual (4, names.Length, "#C2");

					// Mono's Unix registry API uses a hashtable to store the
					// values (and their names), so names are not returned in
					// order
					//
					// to test whether the names returned by GetValueNames
					// match what we expect, we use these names to remove the
					// the values from the created keys and such we should end
					// up with zero values
					for (int i = 0; i < names.Length; i++) {
						string valueName = names [i];
						createdKey.DeleteValue (valueName);
					}

					// all values should be removed now
					Assert.AreEqual (0, createdKey.ValueCount, "#C3");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void GetValueNames_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.SetValue ("name1", "value1");
					createdKey.SetValue ("name2", "value2");

					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#A1");
					Assert.AreEqual (2, names.Length, "#A2");
				}

				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					string [] names = createdKey.GetValueNames ();
					Assert.IsNotNull (names, "#B1");
					Assert.AreEqual (2, names.Length, "#B2");

					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);

					try {
						createdKey.GetValueNames ();
						Assert.Fail ("#C1");
					} catch (IOException ex) {
						// Illegal operation attempted on a registry key that
						// has been marked for deletion
						Assert.AreEqual (typeof (IOException), ex.GetType (), "#C2");
						Assert.IsNotNull (ex.Message, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test] // bug #78519
		public void GetSubKeyNamesTest ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				// check if key was successfully created
				Assert.IsNotNull (createdKey, "#A");

				RegistryKey subKey = createdKey.CreateSubKey ("foo");
				Assert.IsNotNull (subKey, "#B1");
				Assert.AreEqual (1, createdKey.SubKeyCount, "#B2");
				string[] subKeyNames = createdKey.GetSubKeyNames ();
				Assert.IsNotNull (subKeyNames, "#B3");
				Assert.AreEqual (1, subKeyNames.Length, "#B4");
				Assert.AreEqual ("foo", subKeyNames[0], "#B5");

				subKey = createdKey.CreateSubKey ("longfoo");
				Assert.IsNotNull (subKey, "#C1");
				Assert.AreEqual (2, createdKey.SubKeyCount, "#C2");
				subKeyNames = createdKey.GetSubKeyNames ();
				Assert.IsNotNull (subKeyNames, "#C3");
				Assert.AreEqual (2, subKeyNames.Length, "#C4");
				Assert.AreEqual ("foo", subKeyNames [0], "#C5");
				Assert.AreEqual ("longfoo", subKeyNames [1], "#C6");

				subKey = createdKey.CreateSubKey ("sfoo");
				Assert.IsNotNull (subKey, "#D1");
				Assert.AreEqual (3, createdKey.SubKeyCount, "#D2");
				subKeyNames = createdKey.GetSubKeyNames ();
				Assert.IsNotNull (subKeyNames, "#D3");
				Assert.AreEqual (3, subKeyNames.Length, "#D4");
				Assert.AreEqual ("foo", subKeyNames [0], "#D5");
				Assert.AreEqual ("longfoo", subKeyNames [1], "#D6");
				Assert.AreEqual ("sfoo", subKeyNames [2], "#D7");

				foreach (string name in subKeyNames) {
					createdKey.DeleteSubKeyTree (name);
				}
				Assert.AreEqual (0, createdKey.SubKeyCount, "#E");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void OpenRemoteBaseKey ()
		{
			// access to registry of remote machines is not implemented on unix
			if (RunningOnUnix)
				return;

			RegistryKey hive = RegistryKey.OpenRemoteBaseKey (
				RegistryHive.CurrentUser, Environment.MachineName);
			Assert.IsNotNull (hive, "#1");

			RegistryKey key = hive.OpenSubKey ("SOFTWARE");
			Assert.IsNotNull (key, "#2");
			key.Close ();

			hive.Close ();
		}

		[Test]
		public void OpenRemoteBaseKey_MachineName_Null ()
		{
			try {
				RegistryKey.OpenRemoteBaseKey (RegistryHive.CurrentUser, null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("machineName", ex.ParamName, "#5");
			}
		}

		[Test]
		public void OpenRemoteBaseKey_MachineName_DoesNotExist ()
		{
			// access to registry of remote machines is not implemented on unix
			if (RunningOnUnix)
				return;

			try {
				RegistryKey.OpenRemoteBaseKey (RegistryHive.CurrentUser,
					"DOESNOTEXIST");
				Assert.Fail ("#1");
			} catch (IOException ex) {
				// The network path was not found
				Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.InnerException, "#4");
			}
		}

		[Test] // bug #322839
		public void SetValue1_EntityReferences ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// we created a new subkey, so value should not exist
					Assert.IsNull (createdKey.GetValue ("FirstName&\"<LastName>\""), "#A1");
					// create value
					createdKey.SetValue ("FirstName&\"<LastName>\"", "<'Miguel' & \"de Icaza\">!");
					// get value
					object name = createdKey.GetValue ("FirstName&\"<LastName>\"");
					// value should exist
					Assert.IsNotNull (name, "#A2");
					// type of value should be string
					Assert.AreEqual (typeof (string), name.GetType (), "#A3");
					// ensure value matches
					Assert.AreEqual ("<'Miguel' & \"de Icaza\">!", name, "#A4");

					// we created a new subkey, so value should not exist
					Assert.IsNull (createdKey.GetValue ("Info"), "#B1");
					// create value
					createdKey.SetValue ("Info", new string [] { "Mono&<Novell>!", "<CLR&BCL>" });
					// get value
					object info = createdKey.GetValue ("Info");
					// value should exist
					Assert.IsNotNull (info, "#B2");
					// type of value should be string
					Assert.AreEqual (typeof (string []), info.GetType (), "#B3");
					// ensure value matches
					Assert.AreEqual (new string [] { "Mono&<Novell>!", "<CLR&BCL>" }, info, "#B4");
				}

				using (RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					object name = openedKey.GetValue ("FirstName&\"<LastName>\"");
					Assert.IsNotNull (name, "#C1");
					Assert.AreEqual (typeof (string), name.GetType (), "#C2");
					Assert.AreEqual ("<'Miguel' & \"de Icaza\">!", name, "#C3");

					object info = openedKey.GetValue ("Info");
					Assert.IsNotNull (info, "#D1");
					Assert.AreEqual (typeof (string []), info.GetType (), "#D2");
					Assert.AreEqual (new string [] { "Mono&<Novell>!", "<CLR&BCL>" }, info, "#D3");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_Name_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				createdKey.SetValue (null, "value1");
				string [] names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#A1");
				Assert.AreEqual (1, names.Length, "#A2");
				Assert.IsNotNull (names [0], "#A3");
				Assert.AreEqual (string.Empty, names [0], "#A4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#A5");
				Assert.AreEqual ("value1", createdKey.GetValue (string.Empty), "#A6");
				Assert.IsNotNull (createdKey.GetValue (null), "#A7");
				Assert.AreEqual ("value1", createdKey.GetValue (null), "#A8");

				createdKey.SetValue (string.Empty, "value2");
				names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#B1");
				Assert.AreEqual (1, names.Length, "#B2");
				Assert.IsNotNull (names [0], "#B3");
				Assert.AreEqual (string.Empty, names [0], "#B4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#B5");
				Assert.AreEqual ("value2", createdKey.GetValue (string.Empty), "#B6");
				Assert.IsNotNull (createdKey.GetValue (null), "#B7");
				Assert.AreEqual ("value2", createdKey.GetValue (null), "#B8");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_Name_Empty ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				createdKey.SetValue (string.Empty, "value1");
				string [] names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#A1");
				Assert.AreEqual (1, names.Length, "#A2");
				Assert.IsNotNull (names [0], "#A3");
				Assert.AreEqual (string.Empty, names [0], "#A4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#A5");
				Assert.AreEqual ("value1", createdKey.GetValue (string.Empty), "#A6");
				Assert.IsNotNull (createdKey.GetValue (null), "#A7");
				Assert.AreEqual ("value1", createdKey.GetValue (null), "#A8");

				createdKey.SetValue (null, "value2");
				names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#B1");
				Assert.AreEqual (1, names.Length, "#B2");
				Assert.IsNotNull (names [0], "#B3");
				Assert.AreEqual (string.Empty, names [0], "#B4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#B5");
				Assert.AreEqual ("value2", createdKey.GetValue (string.Empty), "#B6");
				Assert.IsNotNull (createdKey.GetValue (null), "#B7");
				Assert.AreEqual ("value2", createdKey.GetValue (null), "#B8");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_Name_MaxLength ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					string name = new string ('a', 254);

					createdKey.SetValue (name, "value1");
					Assert.IsNotNull (createdKey.GetValue (name), "#A1");
					createdKey.DeleteValue (name);
					Assert.IsNull (createdKey.GetValue (name), "#A2");

					name = new string ('a', 255);

					createdKey.SetValue (name, "value2");
					Assert.IsNotNull (createdKey.GetValue (name), "#B1");
					createdKey.DeleteValue (name);
					Assert.IsNull (createdKey.GetValue (name), "#B2");

					name = new string ('a', 256);

					try {
						createdKey.SetValue (name, "value2");
						Assert.Fail ("#C1");
					} catch (ArgumentException ex) {
						// 1.x: Registry subkeys should not be
						// greater than or equal to 255 characters
						//
						// 2.x: Registry subkeys should not be
						// greater than 255 characters
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
						Assert.IsNull (ex.InnerException, "#C3");
						Assert.IsNotNull (ex.Message, "#C4");
						Assert.IsTrue (ex.Message.IndexOf ("255") != -1, "#C5");
						Assert.IsNull (ex.ParamName, "#C6");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_Value_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				try {
					createdKey.SetValue ("Name", null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("value", ex.ParamName, "#5");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_Boolean ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// we created a new subkey, so value should not exist
					Assert.IsNull (createdKey.GetValue ("Installed"), "#A1");
					// create value
					createdKey.SetValue ("Installed", true);
					// get value
					object value = createdKey.GetValue ("Installed");
					// value should exist
					Assert.IsNotNull (value, "#A2");
					// type of value should be string
					Assert.AreEqual (typeof (string), value.GetType (), "#A3");
					// ensure value matches
					Assert.AreEqual (true.ToString (), value, "#A4");
				}

				using (RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					object value = openedKey.GetValue ("Installed");
					Assert.IsNotNull (value, "#B1");
					Assert.AreEqual (typeof (string), value.GetType (), "#B2");
					Assert.AreEqual (true.ToString (), value, "#B3");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_Byte ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// we created a new subkey, so value should not exist
					Assert.IsNull (createdKey.GetValue ("Flags"), "#A1");
					// create value
					createdKey.SetValue ("Flags", (byte) 5);
					// get value
					object value = createdKey.GetValue ("Flags");
					// value should exist
					Assert.IsNotNull (value, "#A2");
					// type of value should be string
					Assert.AreEqual (typeof (string), value.GetType (), "#A3");
					// ensure value matches
					Assert.AreEqual ("5", value, "#A4");
				}

				using (RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					object value = openedKey.GetValue ("Flags");
					Assert.IsNotNull (value, "#B1");
					Assert.AreEqual (typeof (string), value.GetType (), "#B2");
					Assert.AreEqual ("5", value, "#B3");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_ByteArray ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// we created a new subkey, so value should not exist
					Assert.IsNull (createdKey.GetValue ("Flags"), "#A1");
					// create value
					createdKey.SetValue ("Flags", new byte [] { 1, 5 });
					// get value
					object value = createdKey.GetValue ("Flags");
					// value should exist
					Assert.IsNotNull (value, "#A2");
					// type of value should be string
					Assert.AreEqual (typeof (byte []), value.GetType (), "#3");
					// ensure value matches
					Assert.AreEqual (new byte [] { 1, 5 }, value, "#4");
				}

				using (RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					object value = openedKey.GetValue ("Flags");
					Assert.IsNotNull (value, "#B1");
					Assert.AreEqual (typeof (byte []), value.GetType (), "#B2");
					Assert.AreEqual (new byte [] { 1, 5 }, value, "#B3");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_DateTime ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				object rawValue = DateTime.Now;

				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// we created a new subkey, so value should not exist
					Assert.IsNull (createdKey.GetValue ("Path"), "#A1");
					// create value
					createdKey.SetValue ("Path", rawValue);
					// get value
					object value = createdKey.GetValue ("Path");
					// value should exist
					Assert.IsNotNull (value, "#A2");
					// type of value should be string
					Assert.AreEqual (typeof (string), value.GetType (), "#A3");
					// ensure value matches
					Assert.AreEqual (rawValue.ToString (), value, "#A4");
				}

				using (RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					object value = openedKey.GetValue ("Path");
					Assert.IsNotNull (value, "#B1");
					Assert.AreEqual (typeof (string), value.GetType (), "#B2");
					Assert.AreEqual (rawValue.ToString (), value, "#B3");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SetValue_Int32 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// we created a new subkey, so value should not exist
					Assert.IsNull (createdKey.GetValue ("RefCount"), "#A1");
					// create value
					createdKey.SetValue ("RefCount", 5);
					// get value
					object value = createdKey.GetValue ("RefCount");
					// value should exist
					Assert.IsNotNull (value, "#A2");
					// type of value should be int
					Assert.AreEqual (typeof (int), value.GetType (), "#A3");
					// ensure value matches
					Assert.AreEqual (5, value, "#A4");
				}

				using (RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					object value = openedKey.GetValue ("RefCount");
					Assert.IsNotNull (value, "#B1");
					Assert.AreEqual (typeof (int), value.GetType (), "#B2");
					Assert.AreEqual (5, value, "#B3");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_Int64 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// we created a new subkey, so value should not exist
					Assert.IsNull (createdKey.GetValue ("Ticks"), "#A1");
					// create value
					createdKey.SetValue ("Ticks", 500L);
					// get value
					object value = createdKey.GetValue ("Ticks");
					// value should exist
					Assert.IsNotNull (value, "#A2");
					// type of value should be string
					Assert.AreEqual (typeof (string), value.GetType (), "#A3");
					// ensure value matches
					Assert.AreEqual ("500", value, "#A4");
				}

				using (RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					object value = openedKey.GetValue ("Ticks");
					Assert.IsNotNull (value, "#B1");
					Assert.AreEqual (typeof (string), value.GetType (), "#B2");
					Assert.AreEqual ("500", value, "#B3");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_String ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// we created a new subkey, so value should not exist
					Assert.IsNull (createdKey.GetValue ("Path"), "#A1");
					// create value
					createdKey.SetValue ("Path", "/usr/lib/whatever");
					// get value
					object path = createdKey.GetValue ("Path");
					// value should exist
					Assert.IsNotNull (path, "#A2");
					// type of value should be string
					Assert.AreEqual (typeof (string), path.GetType (), "#A3");
					// ensure value matches
					Assert.AreEqual ("/usr/lib/whatever", path, "#A4");
				}

				using (RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					object path = openedKey.GetValue ("Path");
					Assert.IsNotNull (path, "#B1");
					Assert.AreEqual (typeof (string), path.GetType (), "#B2");
					Assert.AreEqual ("/usr/lib/whatever", path, "#B3");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_StringArray ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// we created a new subkey, so value should not exist
					Assert.IsNull (createdKey.GetValue ("DependsOnGroup"), "#A1");
					// create value
					createdKey.SetValue ("DependsOnGroup", new string [] { "A", "B" });
					// get value
					object value = createdKey.GetValue ("DependsOnGroup");
					// value should exist
					Assert.IsNotNull (value, "#A2");
					// type of value should be string
					Assert.AreEqual (typeof (string []), value.GetType (), "#A3");
					// ensure value matches
					Assert.AreEqual (new string [] { "A", "B" }, value, "#A4");
				}

				using (RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					object value = openedKey.GetValue ("DependsOnGroup");
					Assert.IsNotNull (value, "#B1");
					Assert.AreEqual (typeof (string []), value.GetType (), "#B2");
					Assert.AreEqual (new string [] { "A", "B" }, value, "#B3");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_Key_ReadOnly ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software")) {
				try {
					softwareKey.SetValue ("name1", "value1");
					Assert.Fail ("#1");
				} catch (UnauthorizedAccessException ex) {
					// Cannot write to the registry key
					Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.Message, "#3");
					Assert.IsNull (ex.InnerException, "#4");
				}
			}

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
					}

					using (RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName)) {
						try {
							createdKey.SetValue ("name1", "value1");
							Assert.Fail ("#1");
						} catch (UnauthorizedAccessException ex) {
							// Cannot write to the registry key
							Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
							Assert.IsNotNull (ex.Message, "#3");
							Assert.IsNull (ex.InnerException, "#4");
						}
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test] // SetValue (String, Object)
		public void SetValue1_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						softwareKey.DeleteSubKeyTree (subKeyName);
						Assert.IsNull (softwareKey.OpenSubKey (subKeyName), "#1");
						try {
							createdKey.SetValue ("name1", "value1");
							Assert.Fail ("#2");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#3");
							Assert.IsNotNull (ex.Message, "#4");
							Assert.IsNull (ex.InnerException, "#5");
						}
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test] // SetValue (String, Object, RegistryValueKind)
		public void SetValue2_Key_ReadOnly ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software")) {
				try {
					softwareKey.SetValue ("name1", "value1",
						RegistryValueKind.String);
					Assert.Fail ("#1");
				} catch (UnauthorizedAccessException ex) {
					// Cannot write to the registry key
					Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.Message, "#3");
					Assert.IsNull (ex.InnerException, "#4");
				}
			}

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
					}

					using (RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName)) {
						try {
							createdKey.SetValue ("name1", "value1",
								RegistryValueKind.String);
							Assert.Fail ("#1");
						} catch (UnauthorizedAccessException ex) {
							// Cannot write to the registry key
							Assert.AreEqual (typeof (UnauthorizedAccessException), ex.GetType (), "#2");
							Assert.IsNotNull (ex.Message, "#3");
							Assert.IsNull (ex.InnerException, "#4");
						}
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test] // SetValue (String, Object, RegistryValueKind)
		public void SetValue2_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						softwareKey.DeleteSubKeyTree (subKeyName);
						Assert.IsNull (softwareKey.OpenSubKey (subKeyName), "#1");
						try {
							createdKey.SetValue ("name1", "value1",
								RegistryValueKind.String);
							Assert.Fail ("#2");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#3");
							Assert.IsNotNull (ex.Message, "#4");
							Assert.IsNull (ex.InnerException, "#5");
						}
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test] // SetValue (String, Object, RegistryValueKind)
		public void SetValue2_Name_Empty ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				createdKey.SetValue (string.Empty, "value1",
					RegistryValueKind.String);
				string [] names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#A1");
				Assert.AreEqual (1, names.Length, "#A2");
				Assert.IsNotNull (names [0], "#A3");
				Assert.AreEqual (string.Empty, names [0], "#A4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#A5");
				Assert.AreEqual ("value1", createdKey.GetValue (string.Empty), "#A6");
				Assert.IsNotNull (createdKey.GetValue (null), "#A7");
				Assert.AreEqual ("value1", createdKey.GetValue (null), "#A8");

				createdKey.SetValue (null, "value2",
					RegistryValueKind.String);
				names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#B1");
				Assert.AreEqual (1, names.Length, "#B2");
				Assert.IsNotNull (names [0], "#B3");
				Assert.AreEqual (string.Empty, names [0], "#B4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#B5");
				Assert.AreEqual ("value2", createdKey.GetValue (string.Empty), "#B6");
				Assert.IsNotNull (createdKey.GetValue (null), "#B7");
				Assert.AreEqual ("value2", createdKey.GetValue (null), "#B8");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object, RegistryValueKind)
		public void SetValue2_Name_MaxLength ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					string name = new string ('a', 254);

					createdKey.SetValue (name, "value1",
						RegistryValueKind.String);
					Assert.IsNotNull (createdKey.GetValue (name), "#A1");
					createdKey.DeleteValue (name);
					Assert.IsNull (createdKey.GetValue (name), "#A2");

					name = new string ('a', 255);

					createdKey.SetValue (name, "value2",
						RegistryValueKind.String);
					Assert.IsNotNull (createdKey.GetValue (name), "#B1");
					createdKey.DeleteValue (name);
					Assert.IsNull (createdKey.GetValue (name), "#B2");

					name = new string ('a', 256);

					try {
						createdKey.SetValue (name, "value2",
							RegistryValueKind.String);
						Assert.Fail ("#C1");
					} catch (ArgumentException ex) {
						// Registry subkeys should not be
						// greater than 255 characters
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
						Assert.IsNull (ex.InnerException, "#C3");
						Assert.IsNotNull (ex.Message, "#C4");
						Assert.IsTrue (ex.Message.IndexOf ("255") != -1, "#C5");
						Assert.IsNull (ex.ParamName, "#C6");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test] // SetValue (String, Object, RegistryValueKind)
		public void SetValue2_Name_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				createdKey.SetValue (null, "value1",
					RegistryValueKind.String);
				string [] names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#A1");
				Assert.AreEqual (1, names.Length, "#A2");
				Assert.IsNotNull (names [0], "#A3");
				Assert.AreEqual (string.Empty, names [0], "#A4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#A5");
				Assert.AreEqual ("value1", createdKey.GetValue (string.Empty), "#A6");
				Assert.IsNotNull (createdKey.GetValue (null), "#A7");
				Assert.AreEqual ("value1", createdKey.GetValue (null), "#A8");

				createdKey.SetValue (string.Empty, "value2",
					RegistryValueKind.String);
				names = createdKey.GetValueNames ();
				Assert.IsNotNull (names, "#B1");
				Assert.AreEqual (1, names.Length, "#B2");
				Assert.IsNotNull (names [0], "#B3");
				Assert.AreEqual (string.Empty, names [0], "#B4");
				Assert.IsNotNull (createdKey.GetValue (string.Empty), "#B5");
				Assert.AreEqual ("value2", createdKey.GetValue (string.Empty), "#B6");
				Assert.IsNotNull (createdKey.GetValue (null), "#B7");
				Assert.AreEqual ("value2", createdKey.GetValue (null), "#B8");
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test] // SetValue (String, Object, RegistryValueKind)
		public void SetValue2_Value_Null ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName);
			try {
				try {
					createdKey.SetValue ("Name", null,
						RegistryValueKind.String);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("value", ex.ParamName, "#5");
				}
			} finally {
				// clean-up
				Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
			}
		}

		[Test]
		public void SubKeyCount ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					using (RegistryKey subKey = createdKey.CreateSubKey ("monotemp1")) {
						subKey.Close ();
					}
					Assert.AreEqual (1, createdKey.SubKeyCount, "#A2");
					using (RegistryKey subKey = createdKey.CreateSubKey ("monotemp2")) {
						subKey.Close ();
					}
					Assert.AreEqual (2, createdKey.SubKeyCount, "#A3");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey, "#B1");
					Assert.AreEqual (2, createdKey.SubKeyCount, "#B2");

					using (RegistryKey createdKey2 = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
						Assert.IsNotNull (createdKey2, "#B3");
						Assert.AreEqual (2, createdKey2.SubKeyCount, "#B4");
						createdKey2.DeleteSubKey ("monotemp1");
						Assert.AreEqual (1, createdKey2.SubKeyCount, "#B5");
					}
					Assert.AreEqual (1, createdKey.SubKeyCount, "#B6");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void SubKeyCount_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					using (RegistryKey subKey = createdKey.CreateSubKey ("monotemp1")) {
						subKey.Close ();
					}
					Assert.AreEqual (1, createdKey.SubKeyCount, "#A2");
					using (RegistryKey subKey = createdKey.CreateSubKey ("monotemp2")) {
						subKey.Close ();
					}
					Assert.AreEqual (2, createdKey.SubKeyCount, "#A3");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey, "#B1");
					Assert.AreEqual (2, createdKey.SubKeyCount, "#B2");

					// remove created key
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);

					try {
						Assert.Fail ("#C1: " + createdKey.SubKeyCount);
					} catch (IOException ex) {
						// Illegal operation attempted on a registry key that
						// has been marked for deletion
						Assert.AreEqual (typeof (IOException), ex.GetType (), "#14");
						Assert.IsNotNull (ex.Message, "#15");
						Assert.IsNull (ex.InnerException, "#16");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void ValueCount ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					Assert.AreEqual (0, createdKey.ValueCount, "#A2");
					createdKey.SetValue ("name1", "value1");
					Assert.AreEqual (1, createdKey.ValueCount, "#A3");
					createdKey.SetValue ("name2", "value2");
					Assert.AreEqual (2, createdKey.ValueCount, "#A4");
					createdKey.SetValue ("name2", "value2b");
					Assert.AreEqual (2, createdKey.ValueCount, "#A5");
					createdKey.SetValue ("name3", "value3");
					Assert.AreEqual (3, createdKey.ValueCount, "#A6");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey, "#B1");
					Assert.AreEqual (3, createdKey.ValueCount, "#B2");

					using (RegistryKey createdKey2 = Registry.CurrentUser.OpenSubKey (subKeyName, true)) {
						Assert.IsNotNull (createdKey2, "#B3");
						Assert.AreEqual (3, createdKey2.ValueCount, "#B4");
						createdKey2.DeleteValue ("name2");
						Assert.AreEqual (2, createdKey2.ValueCount, "#B5");
					}
					Assert.AreEqual (2, createdKey.ValueCount, "#B6");
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void ValueCount_Key_Removed ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					// check if key was successfully created
					Assert.IsNotNull (createdKey, "#A1");
					Assert.AreEqual (0, createdKey.ValueCount, "#A2");
					createdKey.SetValue ("name1", "value1");
					Assert.AreEqual (1, createdKey.ValueCount, "#A3");
					createdKey.SetValue ("name2", "value2");
					Assert.AreEqual (2, createdKey.ValueCount, "#A4");
					createdKey.SetValue ("name2", "value2b");
					Assert.AreEqual (2, createdKey.ValueCount, "#A5");
					createdKey.SetValue ("name3", "value3");
					Assert.AreEqual (3, createdKey.ValueCount, "#A6");
				}
				using (RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName)) {
					Assert.IsNotNull (createdKey, "#B1");
					Assert.AreEqual (3, createdKey.ValueCount, "#B2");

					// remove created key
					Registry.CurrentUser.DeleteSubKeyTree (subKeyName);

					try {
						Assert.Fail ("#C1: " + createdKey.ValueCount);
					} catch (IOException ex) {
						// Illegal operation attempted on a registry key that
						// has been marked for deletion
						Assert.AreEqual (typeof (IOException), ex.GetType (), "#14");
						Assert.IsNotNull (ex.Message, "#15");
						Assert.IsNull (ex.InnerException, "#16");
					}
				}
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void bug79051 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						createdKey.SetValue ("test", "whatever");
						createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test]
		public void bug79059 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						using (RegistryKey softwareKey2 = Registry.CurrentUser.OpenSubKey ("software")) {
						}
						createdKey.Close ();
						softwareKey.DeleteSubKeyTree (subKeyName);
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test]
		public void bugnew1 ()
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			using (RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey ("software", true)) {
				try {
					using (RegistryKey createdKey = softwareKey.CreateSubKey (subKeyName)) {
						createdKey.SetValue ("name1", "value1");

						RegistryKey testKey = null;
						try {
							testKey = createdKey.OpenSubKey ("test", true);
							if (testKey == null)
								testKey = createdKey.CreateSubKey ("test");
							testKey.SetValue ("another", "one");
						} finally {
							if (testKey != null)
								testKey.Close ();
						}

						createdKey.SetValue ("name2", "value2");
						Assert.IsNotNull (createdKey.GetValue ("name1"), "#2");
						Assert.AreEqual ("value1", createdKey.GetValue ("name1"), "#3");
						Assert.IsNotNull (createdKey.GetValue ("name2"), "#4");
						Assert.AreEqual ("value2", createdKey.GetValue ("name2"), "#5");

						string [] names = createdKey.GetValueNames ();
						Assert.IsNotNull (names, "#6");
						Assert.AreEqual (2, names.Length, "#7");
						Assert.AreEqual ("name1", names [0], "#8");
						Assert.AreEqual ("name2", names [1], "#9");

						softwareKey.DeleteSubKeyTree (subKeyName);

						using (RegistryKey openedKey = softwareKey.OpenSubKey (subKeyName, true)) {
							Assert.IsNull (openedKey, "#10");
						}

						Assert.IsNull (createdKey.GetValue ("name1"), "#11");
						Assert.IsNull (createdKey.GetValue ("name2"), "#12");

						try {
							createdKey.GetValueNames ();
							Assert.Fail ("#13");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#14");
							Assert.IsNotNull (ex.Message, "#15");
							Assert.IsNull (ex.InnerException, "#16");
						}

						try {
							createdKey.SetValue ("name1", "value1");
							Assert.Fail ("#17");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#18");
							Assert.IsNotNull (ex.Message, "#19");
							Assert.IsNull (ex.InnerException, "#20");
						}

						try {
							createdKey.SetValue ("newname", "value1");
							Assert.Fail ("#21");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#22");
							Assert.IsNotNull (ex.Message, "#23");
							Assert.IsNull (ex.InnerException, "#24");
						}

						Assert.IsNull (createdKey.OpenSubKey ("test"), "#25");
						Assert.IsNull (createdKey.OpenSubKey ("test", true), "#26");
						Assert.IsNull (createdKey.OpenSubKey ("new"), "#27");
						Assert.IsNull (createdKey.OpenSubKey ("new", true), "#28");

						try {
							createdKey.CreateSubKey ("new");
							Assert.Fail ("#29");
						} catch (IOException ex) {
							// Illegal operation attempted on a registry key that
							// has been marked for deletion
							Assert.AreEqual (typeof (IOException), ex.GetType (), "#30");
							Assert.IsNotNull (ex.Message, "#31");
							Assert.IsNull (ex.InnerException, "#32");
						}
					}
				} finally {
					try {
						RegistryKey createdKey = softwareKey.OpenSubKey (subKeyName);
						if (createdKey != null) {
							createdKey.Close ();
							softwareKey.DeleteSubKeyTree (subKeyName);
						}
					} catch {
					}
				}
			}
		}

		[Test]
		public void bugnew2 () // values cannot be written on registry root (hive)
		{
			string [] names = Registry.CurrentUser.GetValueNames ();
			Assert.IsNotNull (names, "#1");
			Registry.CurrentUser.SetValue ("name1", "value1");
			Assert.IsNotNull (Registry.CurrentUser.GetValue ("name1"), "#2");
			Assert.AreEqual ("value1", Registry.CurrentUser.GetValue ("name1"), "#3");
			string [] newNames = Registry.CurrentUser.GetValueNames ();
			Assert.IsNotNull (newNames, "#4");
			Assert.AreEqual (names.Length + 1, newNames.Length, "#5");
			Registry.CurrentUser.DeleteValue ("name1");
		}

		[Test]
		public void bugnew3 () // on Windows, key cannot be closed twice
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.Close ();
				}

				RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName);
				openedKey.Close ();
				openedKey.Close ();
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		[Test]
		public void bugnew4 () // Key cannot be flushed once it has been closed
		{
			string subKeyName = Guid.NewGuid ().ToString ();

			try {
				using (RegistryKey createdKey = Registry.CurrentUser.CreateSubKey (subKeyName)) {
					createdKey.Close ();
				}

				RegistryKey openedKey = Registry.CurrentUser.OpenSubKey (subKeyName);
				openedKey.Close ();
				openedKey.Flush ();
			} finally {
				try {
					RegistryKey createdKey = Registry.CurrentUser.OpenSubKey (subKeyName);
					if (createdKey != null) {
						createdKey.Close ();
						Registry.CurrentUser.DeleteSubKeyTree (subKeyName);
					}
				} catch {
				}
			}
		}

		private bool RunningOnUnix {
			get {
				int p = (int) Environment.OSVersion.Platform;
				return ((p == 4) || (p == 128) || (p == 6));
			}
		}
	}
}
