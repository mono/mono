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
		}

		public static ITest Suite
		{
			get { return new TestSuite (typeof (FileTest)); }
		}

		public void TestExists ()
		{
			Assert ("File filetest/test should exist", File.Exists ("filetest/test"));
		}

		public void TestCreate ()
		{
			File.Create ("filetest/foo");
			Assert ("File should exist", File.Exists ("filetest/foo"));
		}

		public void TestCopy ()
		{
			File.Copy ("filetest/foo", "filetest/bar", false);
			Assert ("File foo should exist", File.Exists ("filetest/foo"));
			Assert ("File bar should exist", File.Exists ("filetest/bar"));
		}
		
		public void TestDelete ()
		{
			File.Delete ("filetest/foo");
			Assert ("File should not exist", !File.Exists ("filetest/foo"));
		}

		public void TestMove ()
		{
			Assert ("File filetest/bar should exist", File.Exists ("filetest/bar"));
			File.Move ("filetest/bar", "filetest/baz");
			Assert ("File filetest/bar should not exist", !File.Exists ("filetest/bar"));
			Assert ("File filetest/baz should exist", File.Exists ("filetest/baz"));
		}
	}
}
