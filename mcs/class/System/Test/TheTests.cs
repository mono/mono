using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace Ximian.Mono.Tests.System
{
	public class RunDnsTest : DnsTest
	{
		protected override void RunTest ()
		{
			TestAsyncGetHostByName ();
			TestAsyncResolve ();
			TestGetHostName ();
			TestGetHostByName ();
			TestGetHostByAddressString ();
			TestGetHostByAddressIPAddress ();
			TestResolve ();
		}
	}
}

namespace Ximian.Mono.Tests.System.Collections.Specialized.Collections.Specialized
{
	public class RunStringCollectionTest : StringCollectionTest
	{
		protected override void RunTest ()
		{
			TestSimpleCount ();
			TestSimpleIsReadOnly ();
			TestSimpleIsSynchronized ();
			TestSimpleItemGet ();
			TestSimpleItemSet ();
			TestSimpleSyncRoot ();
			TestSimpleAdd ();
			TestSimpleAddRange ();
			TestSimpleClear ();
			TestSimpleContains ();
			TestSimpleCopyTo ();
			TestSimpleGetEnumerator ();
			TestSimpleIndexOf ();
			TestSimpleInsert ();
			TestSimpleRemove ();
			TestSimpleRemoveAt ();
		}
	}
}

namespace MonoTests
{
	public class RunAllTests
	{
		public static void AddAllTests (TestSuite suite)
		{
			suite.AddTest (new Ximian.Mono.Tests.System.RunDnsTest ());
			suite.AddTest (new Ximian.Mono.Tests.System.Collections.Specialized.Collections.Specialized.RunStringCollectionTest ());
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

