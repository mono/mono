using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace MonoTests.System
{
	public class RunBooleanTest : BooleanTest
	{
		protected override void RunTest ()
		{
			TestStrings ();
			TestCompareTo ();
			TestEquals ();
			TestGetHashCode ();
			TestGetType ();
			TestGetTypeCode ();
			TestParse ();
			TestToString ();
		}
	}
}

namespace MonoTests.System
{
	public class RunStringTest : StringTest
	{
		protected override void RunTest ()
		{
			TestLength ();
			TestCompare ();
			TestCompareOrdinal ();
			TestCompareTo ();
			TestConcat ();
			TestCopy ();
			TestCopyTo ();
			TestEndsWith ();
			TestEquals ();
			TestFormat ();
			TestGetEnumerator ();
			TestGetHashCode ();
			TestGetType ();
			TestGetTypeCode ();
			TestIndexOf ();
			TestIndexOfAny ();
			TestInsert ();
			TestIntern ();
			TestIsInterned ();
			TestJoin ();
			TestLastIndexOf ();
			TestLastIndexOfAny ();
			TestPadLeft ();
			TestPadRight ();
			TestRemove ();
			TestReplace ();
			TestSplit ();
			TestStartsWith ();
			TestSubstring ();
			TestToCharArray ();
			TestToLower ();
			TestToString ();
			TestToUpper ();
			TestTrim ();
			TestTrimEnd ();
			TestTrimStart ();
		}
	}
}

namespace MonoTests.System
{
	public class RunTimeSpanTest : TimeSpanTest
	{
		protected override void RunTest ()
		{
			TestCtors ();
			TestProperties ();
			TestAdd ();
			TestCompare ();
			TestNegateAndDuration ();
			TestEquals ();
			TestFromXXXX ();
			TestGetHashCode ();
			TestParse ();
			TestSubstract ();
			TestToString ();
		}
	}
}

namespace MonoTests.System
{
	public class RunDoubleTest : DoubleTest
	{
		protected override void RunTest ()
		{
			TestPublicFields ();
			TestCompareTo ();
			TestEquals ();
			TestTypeCode ();
			TestIsInfinity ();
			TestIsNan ();
			TestIsNegativeInfinity ();
			TestIsPositiveInfinity ();
			TestParse ();
			TestToString ();
		}
	}
}

namespace MonoTests.System
{
	public class RunTimeZoneTest : TimeZoneTest
	{
		protected override void RunTest ()
		{
			TestCtors ();
		}
	}
}

namespace MonoTests.System
{
	public class RunDateTimeTest : DateTimeTest
	{
		protected override void RunTest ()
		{
			TestCtors ();
			TestToString ();
			TestParseExact ();
			TestParse ();
		}
	}
}

namespace MonoTests
{
	public class RunAllTests
	{
		public static void AddAllTests (TestSuite suite)
		{
			suite.AddTest (new MonoTests.System.RunBooleanTest ());
			suite.AddTest (new MonoTests.System.RunStringTest ());
			suite.AddTest (new MonoTests.System.RunTimeSpanTest ());
			suite.AddTest (new MonoTests.System.RunDoubleTest ());
			suite.AddTest (new MonoTests.System.RunTimeZoneTest ());
			suite.AddTest (new MonoTests.System.RunDateTimeTest ());
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

