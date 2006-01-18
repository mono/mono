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
	}
}
