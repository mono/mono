using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace MonoTests.System
{
}

namespace MonoTests
{
	public class RunAllTests
	{
		public static void AddAllTests (TestSuite suite)
		{
		}
	}
}

class MainApp
{
	public static void Main()
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		TestResult result = new TestResult ();
		TestSuite suite = new TestSuite ();
		MonoTests.RunAllTests.AddAllTests (suite);
		suite.Run (result);
		MonoTests.MyTestRunner.Print (result);
	}
}

