using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace MonoTests.System
{
	public class RunArrayTest : ArrayTest
	{
		protected override void RunTest ()
		{
			TestIsFixedSize ();
			TestIsReadOnly ();
			TestIsSynchronized ();
			TestLength ();
			TestRank ();
			TestBinarySearch1 ();
			TestBinarySearch2 ();
			TestClear ();
			TestClone ();
			TestCopy ();
			TestCopy2 ();
			TestCopyTo ();
			TestCreateInstance ();
			TestGetEnumerator ();
			TestGetLength ();
			TestGetLowerBound ();
			TestGetUpperBound ();
			TestGetValue1 ();
			TestGetValue2 ();
			TestGetValue3 ();
			TestGetValueN ();
			TestIndexOf1 ();
			TestIndexOf2 ();
			TestIndexOf3 ();
			TestLastIndexOf1 ();
			TestLastIndexOf2 ();
			TestLastIndexOf3 ();
			TestReverse ();
			TestSetValue1 ();
			TestSetValue2 ();
			TestSetValue3 ();
			TestSetValueN ();
			TestSetValue4 ();
			TestSort ();

		}
	}
}

namespace MonoTests.System
{
	public class RunBitConverterTest : BitConverterTest
	{
		protected override void RunTest ()
		{
			TestIsLittleEndian ();
			TestSingle ();
			TestDouble ();
			TestBool ();
			TestChar ();
			TestInt16 ();
			TestUInt16 ();
			TestInt32 ();
			TestUInt32 ();
			TestInt64 ();
			TestUInt64 ();
			TestToString ();

		}
	}
}

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
	public class RunByteTest : ByteTest
	{
		protected override void RunTest ()
		{
			TestMinMax ();
			TestCompareTo ();
			TestEquals ();
			TestGetHashCode ();
			TestToString ();

		}
	}
}

namespace MonoTests.System
{
	public class RunCharTest : CharTest
	{
		protected override void RunTest ()
		{
			TestCompareTo ();
			TestEquals ();
			TestGetHashValue ();
			TestGetNumericValue ();
			TestGetUnicodeCategory ();
			TestIsControl ();
			TestIsDigit ();
			TestIsLetter ();
			TestIsLetterOrDigit ();
			TestIsLower ();
			TestIsNumber ();
			TestIsPunctuation ();
			TestIsSeparator ();
			TestIsSurrogate ();
			TestIsSymbol ();
			TestIsUpper ();
			TestIsWhiteSpace ();
			TestParse ();
			TestToLower ();
			TestToUpper ();
			TestToString ();
			TestGetTypeCode ();

		}
	}
}

namespace MonoTests.System
{
	public class RunConsoleTest : ConsoleTest
	{
		protected override void RunTest ()
		{
			TestError ();
			TestIn ();
			TestOut ();
			TestOpenStandardError ();
			TestOpenStandardInput ();
			TestOpenStandardOutput ();
			TestRead ();
			TestReadLine ();
			TestSetError ();
			TestSetIn ();
			TestSetOut ();
			TestWrite ();
			TestWriteLine ();

		}
	}
}

namespace MonoTests.System
{
	public class RunEnumTest : EnumTest
	{
		protected override void RunTest ()
		{
			TestCompareTo ();
			TestEquals ();
			TestFormat ();
			TestGetHashCode ();
			TestGetNames ();
			TestGetTypeCode ();
			TestGetUnderlyingType ();
			TestGetValues ();
			TestIsDefined ();
			TestParse1 ();
			TestParse2 ();
			TestToObject ();
			TestToString ();

		}
	}
}

namespace MonoTests.System
{
	public class RunDecimalTest : DecimalTest
	{
		protected override void RunTest ()
		{
			TestToString ();
			TestCurrencyPattern ();
			TestNumberNegativePattern ();
			TestPercentPattern ();
			TestParse ();
			TestConstants ();
			TestConstructInt32 ();
			TestConstructUInt32 ();
			TestConstructInt64 ();
			TestConstructUInt64 ();
			TestConstructSingle ();
			TestConstructSingleRounding ();
			TestConstructDouble ();
			TestConstructDoubleRound ();
			TestNegate ();
			TestPartConstruct ();
			TestFloorTruncate ();
			TestRound ();

		}
	}
}

namespace MonoTests.System
{
	public class RunDecimalTest2 : DecimalTest2
	{
		protected override void RunTest ()
		{
			TestCompare ();
			TestRemainder ();
			TestAdd ();
			TestMult ();
			TestDiv ();

		}
	}
}

namespace MonoTests.System
{
	public class RunGuidTest : GuidTest
	{
		protected override void RunTest ()
		{
			TestCtor1 ();
			TestCtor2 ();
			TestCtor4 ();
			TestCtor5 ();
			TestEmpty ();
			TestNewGuid ();
			TestEqualityOp ();
			TestInequalityOp ();
			TestEquals ();
			TestCompareTo ();
			TestGetHashCode ();
			TestToByteArray ();
			TestToString ();

		}
	}
}

namespace MonoTests.System
{
	public class RunInt16Test : Int16Test
	{
		protected override void RunTest ()
		{
			TestMinMax ();
			TestCompareTo ();
			TestEquals ();
			TestGetHashCode ();
			TestParse ();
			TestToString ();

		}
	}
}

namespace MonoTests.System
{
	public class RunInt32Test : Int32Test
	{
		protected override void RunTest ()
		{
			TestMinMax ();
			TestCompareTo ();
			TestEquals ();
			TestGetHashCode ();
			TestParse ();
			TestToString ();
			TestCustomToString ();

		}
	}
}

namespace MonoTests.System
{
	public class RunInt64Test : Int64Test
	{
		protected override void RunTest ()
		{
			TestMinMax ();
			TestCompareTo ();
			TestEquals ();
			TestGetHashCode ();
			TestRoundTripGeneral ();
			TestRoundTripHex ();
			TestParseNull ();
			TestParse ();
			TestToString ();
			TestUserCurrency ();
			TestUserPercent ();

		}
	}
}

namespace MonoTests.System
{
	public class RunObjectTest : ObjectTest
	{
		protected override void RunTest ()
		{
			TestCtor ();
			TestEquals1 ();
			TestEquals2 ();
			TestGetHashCode ();
			TestGetType ();
			TestReferenceEquals ();
			TestToString ();

		}
	}
}

namespace MonoTests.System
{
	public class RunResolveEventArgsTest : ResolveEventArgsTest
	{
		protected override void RunTest ()
		{
			TestTheWholeThing ();

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
	public class RunUInt16Test : UInt16Test
	{
		protected override void RunTest ()
		{
			TestMinMax ();
			TestCompareTo ();
			TestEquals ();
			TestGetHashCode ();
			TestParse ();
			TestToString ();

		}
	}
}

namespace MonoTests.System
{
	public class RunUInt32Test : UInt32Test
	{
		protected override void RunTest ()
		{
			TestMinMax ();
			TestCompareTo ();
			TestEquals ();
			TestGetHashCode ();
			TestParse ();
			TestToString ();

		}
	}
}

namespace MonoTests.System
{
	public class RunUInt64Test : UInt64Test
	{
		protected override void RunTest ()
		{
			TestMinMax ();
			TestCompareTo ();
			TestEquals ();
			TestGetHashCode ();
			TestParse ();
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
			suite.AddTest (new MonoTests.System.RunArrayTest ());
			suite.AddTest (new MonoTests.System.RunBitConverterTest ());
			suite.AddTest (new MonoTests.System.RunBooleanTest ());
			suite.AddTest (new MonoTests.System.RunByteTest ());
			suite.AddTest (new MonoTests.System.RunCharTest ());
			suite.AddTest (new MonoTests.System.RunConsoleTest ());
			suite.AddTest (new MonoTests.System.RunEnumTest ());
			suite.AddTest (new MonoTests.System.RunDecimalTest ());
			suite.AddTest (new MonoTests.System.RunDecimalTest2 ());
			suite.AddTest (new MonoTests.System.RunGuidTest ());
			suite.AddTest (new MonoTests.System.RunInt16Test ());
			suite.AddTest (new MonoTests.System.RunInt32Test ());
			suite.AddTest (new MonoTests.System.RunInt64Test ());
			suite.AddTest (new MonoTests.System.RunObjectTest ());
			suite.AddTest (new MonoTests.System.RunResolveEventArgsTest ());
			suite.AddTest (new MonoTests.System.RunStringTest ());
			suite.AddTest (new MonoTests.System.RunTimeSpanTest ());
			suite.AddTest (new MonoTests.System.RunUInt16Test ());
			suite.AddTest (new MonoTests.System.RunUInt32Test ());
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

