//
// RegistryKeyTest.cs - NUnit Test Cases for Microsoft.Win32.RegistryKey
//
// Authors:
//	mei (mei@work.email.ne.jp)
//
// (C) 2005 mei
// (C) 2004 Novell (http://www.novell.com)
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
		}
}
