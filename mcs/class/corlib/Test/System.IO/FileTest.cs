//
// FileTest.cs: Test cases for System.IO.File
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using NUnit.Framework;
using System;
using System.IO;

namespace MonoTests.System.IO
{
	public class FileTest : TestCase
	{
		public FileTest ()
			: base ("System.IO.File testsuite")
		{
		}

		public FileTest (string name)
			: base (name)
		{
		}

		protected override void SetUp ()
		{
		}

		protected override void TearDown ()
		{
		        File.Delete ("resources" + Path.DirectorySeparatorChar + "baz");
		}

		public static ITest Suite
		{
			get { return new TestSuite (typeof (FileTest)); }
		}

		public void TestExists ()
		{
			Assert ("File resources" + Path.DirectorySeparatorChar + "AFile.txt should exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "AFile.txt"));
                        Assert ("File resources" + Path.DirectorySeparatorChar + "doesnotexist should not exist", !File.Exists ("resources" + Path.DirectorySeparatorChar + "doesnotexist"));
		}

		public void TestCreate ()
		{
			FileStream stream = File.Create ("resources" + Path.DirectorySeparatorChar + "foo");
			Assert ("File should exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "foo"));
			stream.Close ();
		}

		public void TestCopy ()
		{
			File.Copy ("resources" + Path.DirectorySeparatorChar + "AFile.txt", "resources" + Path.DirectorySeparatorChar + "bar", false);
			Assert ("File AFile.txt should still exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "AFile.txt"));
			Assert ("File bar should exist after File.Copy", File.Exists ("resources" + Path.DirectorySeparatorChar + "bar"));
		}
		
		public void TestDelete ()
		{
                        Assert ("File resources" + Path.DirectorySeparatorChar + "foo should exist for TestDelete to succeed", File.Exists ("resources" + Path.DirectorySeparatorChar + "foo"));
                        try {
                                File.Delete ("resources" + Path.DirectorySeparatorChar + "foo");
                        } catch (Exception e) {
                                Fail ("Unable to delete resources" + Path.DirectorySeparatorChar + "foo: e=" + e.ToString());
                        }
			Assert ("File resources" + Path.DirectorySeparatorChar + "foo should not exist after File.Delete", !File.Exists ("resources" + Path.DirectorySeparatorChar + "foo"));
		}

		public void TestMove ()
		{
			Assert ("File resources" + Path.DirectorySeparatorChar + "bar should exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "bar"));
			File.Move ("resources" + Path.DirectorySeparatorChar + "bar", "resources" + Path.DirectorySeparatorChar + "baz");
			Assert ("File resources" + Path.DirectorySeparatorChar + "bar should not exist", !File.Exists ("resources" + Path.DirectorySeparatorChar + "bar"));
			Assert ("File resources" + Path.DirectorySeparatorChar + "baz should exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "baz"));
		}

		public void TestOpen ()
		{
                        try {
                                FileStream stream = File.Open ("resources" + Path.DirectorySeparatorChar + "AFile.txt", FileMode.Open);
                        } catch (Exception e) {
                                Fail ("Unable to open resources" + Path.DirectorySeparatorChar + "AFile.txt: e=" + e.ToString());
                        }

                        /* Exception tests */
			try {
				FileStream stream = File.Open ("filedoesnotexist", FileMode.Open);
				Fail ("File 'filedoesnotexist' should not exist");
			} catch (FileNotFoundException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("Unexpect exception caught: e=" + e.ToString());
			}
		}
	}
}
