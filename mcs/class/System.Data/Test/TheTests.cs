using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace MonoTests.System.Data.SqlTypes
{
	public class RunSqlInt32Test : SqlInt32Test
	{
		protected override void RunTest ()
		{
			TestCreate ();
		}
	}
}

namespace MonoTests
{
	public class RunAllTests
	{
		public static void AddAllTests (TestSuite suite)
		{
			suite.AddTest (new MonoTests.System.Data.SqlTypes.RunSqlInt32Test());
		}
	}
}

class MainApp
{
	public static void Main ()
	{
		TestResult result = new TestResult ();
		TestSuite suite = new TestSuite ();
		MonoTests.RunAllTests.AddAllTests (suite);
		suite.Run (result);
		MonoTests.MyTestRunner.Print (result);
	}
}
