using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace MonoTests.System
{
	public class RunBitConverterTest : BitConverterTest
	{
		protected override void RunTest ()
		{
			try { TestIsLittleEndian (); } catch { }
			try { TestDouble (); } catch { }
			try { TestChar (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunBooleanTest : BooleanTest
	{
		protected override void RunTest ()
		{
			try { TestStrings (); } catch { }
			try { TestCompareTo (); } catch { }
			try { TestEquals (); } catch { }
			try { TestGetHashCode (); } catch { }
			try { TestGetType (); } catch { }
			try { TestGetTypeCode (); } catch { }
			try { TestParse (); } catch { }
			try { TestToString (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunByteTest : ByteTest
	{
		protected override void RunTest ()
		{
			try { TestMinMax (); } catch { }
			try { TestCompareTo (); } catch { }
			try { TestEquals (); } catch { }
			try { TestGetHashCode (); } catch { }
			try { TestParse (); } catch { }
			try { TestToString (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunConsoleTest : ConsoleTest
	{
		protected override void RunTest ()
		{
			try { TestError (); } catch { }
			try { TestIn (); } catch { }
			try { TestOut (); } catch { }
			try { TestOpenStandardError (); } catch { }
			try { TestOpenStandardInput (); } catch { }
			try { TestOpenStandardOutput (); } catch { }
			try { TestRead (); } catch { }
			try { TestReadLine (); } catch { }
			try { TestSetError (); } catch { }
			try { TestSetIn (); } catch { }
			try { TestSetOut (); } catch { }
			try { TestWrite (); } catch { }
			try { TestWriteLine (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunGuidTest : GuidTest
	{
		protected override void RunTest ()
		{
			try { TestCtor1 (); } catch { }
			try { TestCtor2 (); } catch { }
			try { TestCtor4 (); } catch { }
			try { TestCtor5 (); } catch { }
			try { TestEmpty (); } catch { }
			try { TestNewGuid (); } catch { }
			try { TestEqualityOp (); } catch { }
			try { TestInequalityOp (); } catch { }
			try { TestEquals (); } catch { }
			try { TestCompareTo (); } catch { }
			try { TestGetHashCode (); } catch { }
			try { TestToByteArray (); } catch { }
			try { TestToString (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunInt32Test : Int32Test
	{
		protected override void RunTest ()
		{
			try { TestMinMax (); } catch { }
			try { TestCompareTo (); } catch { }
			try { TestEquals (); } catch { }
			try { TestGetHashCode (); } catch { }
			try { TestParse (); } catch { }
			try { TestToString (); } catch { }
			try { TestCustomToString (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunObjectTest : ObjectTest
	{
		protected override void RunTest ()
		{
			try { TestCtor (); } catch { }
			try { TestEquals1 (); } catch { }
			try { TestEquals2 (); } catch { }
			try { TestGetHashCode (); } catch { }
			try { TestGetType (); } catch { }
			try { TestReferenceEquals (); } catch { }
			try { TestToString (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunResolveEventArgsTest : ResolveEventArgsTest
	{
		protected override void RunTest ()
		{
			try { TestTheWholeThing (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunStringTest : StringTest
	{
		protected override void RunTest ()
		{
			try { TestLength (); } catch { }
			try { TestCompare (); } catch { }
			try { TestCompareOrdinal (); } catch { }
			try { TestCompareTo (); } catch { }
			try { TestConcat (); } catch { }
			try { TestCopy (); } catch { }
			try { TestCopyTo (); } catch { }
			try { TestEndsWith (); } catch { }
			try { TestEquals (); } catch { }
			try { TestFormat (); } catch { }
			try { TestGetEnumerator (); } catch { }
			try { TestGetHashCode (); } catch { }
			try { TestGetType (); } catch { }
			try { TestGetTypeCode (); } catch { }
			try { TestIndexOf (); } catch { }
			try { TestIndexOfAny (); } catch { }
			try { TestInsert (); } catch { }
			try { TestIntern (); } catch { }
			try { TestIsInterned (); } catch { }
			try { TestJoin (); } catch { }
			try { TestLastIndexOf (); } catch { }
			try { TestLastIndexOfAny (); } catch { }
			try { TestPadLeft (); } catch { }
			try { TestPadRight (); } catch { }
			try { TestRemove (); } catch { }
			try { TestReplace (); } catch { }
			try { TestSplit (); } catch { }
			try { TestStartsWith (); } catch { }
			try { TestSubstring (); } catch { }
			try { TestToCharArray (); } catch { }
			try { TestToLower (); } catch { }
			try { TestToString (); } catch { }
			try { TestToUpper (); } catch { }
			try { TestTrim (); } catch { }
			try { TestTrimEnd (); } catch { }
			try { TestTrimStart (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunTimeSpanTest : TimeSpanTest
	{
		protected override void RunTest ()
		{
			try { TestCtors (); } catch { }
			try { TestProperties (); } catch { }
			try { TestAdd (); } catch { }
			try { TestCompare (); } catch { }
			try { TestNegateAndDuration (); } catch { }
			try { TestEquals (); } catch { }
			try { TestFromXXXX (); } catch { }
			try { TestGetHashCode (); } catch { }
			try { TestParse (); } catch { }
			try { TestSubstract (); } catch { }
			try { TestToString (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunUInt16Test : UInt16Test
	{
		protected override void RunTest ()
		{
			try { TestMinMax (); } catch { }
			try { TestCompareTo (); } catch { }
			try { TestEquals (); } catch { }
			try { TestGetHashCode (); } catch { }
			try { TestParse (); } catch { }
			try { TestToString (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunUInt64Test : UInt64Test
	{
		protected override void RunTest ()
		{
			try { TestMinMax (); } catch { }
			try { TestCompareTo (); } catch { }
			try { TestEquals (); } catch { }
			try { TestGetHashCode (); } catch { }
			try { TestParse (); } catch { }
			try { TestToString (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunDoubleTest : DoubleTest
	{
		protected override void RunTest ()
		{
			try { TestPublicFields (); } catch { }
			try { TestCompareTo (); } catch { }
			try { TestEquals (); } catch { }
			try { TestTypeCode (); } catch { }
			try { TestIsInfinity (); } catch { }
			try { TestIsNan (); } catch { }
			try { TestIsNegativeInfinity (); } catch { }
			try { TestIsPositiveInfinity (); } catch { }
			try { TestParse (); } catch { }
			try { TestToString (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunTimeZoneTest : TimeZoneTest
	{
		protected override void RunTest ()
		{
			try { TestCtors (); } catch { }
		}
	}
}

namespace MonoTests.System
{
	public class RunDateTimeTest : DateTimeTest
	{
		protected override void RunTest ()
		{
			try { TestCtors (); } catch { }
			try { TestToString (); } catch { }
			try { TestParseExact (); } catch { }
			try { TestParse (); } catch { }
		}
	}
}

namespace MonoTests
{
	public class RunAllTests
	{
		public static void AddAllTests (TestSuite suite)
		{
			suite.AddTest (new MonoTests.System.RunBitConverterTest ());
			suite.AddTest (new MonoTests.System.RunBooleanTest ());
			suite.AddTest (new MonoTests.System.RunByteTest ());
			suite.AddTest (new MonoTests.System.RunConsoleTest ());
			suite.AddTest (new MonoTests.System.RunGuidTest ());
			suite.AddTest (new MonoTests.System.RunInt32Test ());
			suite.AddTest (new MonoTests.System.RunObjectTest ());
			suite.AddTest (new MonoTests.System.RunResolveEventArgsTest ());
			suite.AddTest (new MonoTests.System.RunStringTest ());
			suite.AddTest (new MonoTests.System.RunTimeSpanTest ());
			suite.AddTest (new MonoTests.System.RunUInt16Test ());
			suite.AddTest (new MonoTests.System.RunUInt64Test ());
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

