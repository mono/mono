// Author: Tim Coleman (tim@timcoleman.com)
// (C) Copyright 2002 Tim Coleman


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

			// property tests

			TestIsNullProperty ();

			// method tests

			TestAdd ();
			TestBitwiseAnd ();
			TestBitwiseOr ();
			TestDivide ();
			TestEquals ();
			TestGreaterThan ();
			TestGreaterThanOrEqual ();
			TestLessThan ();
			TestLessThanOrEqual ();
			TestMod ();
			TestMultiply ();
			TestNotEquals ();
			TestOnesComplement ();
			TestSubtract ();
			TestConversionMethods ();
			TestXor ();
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
