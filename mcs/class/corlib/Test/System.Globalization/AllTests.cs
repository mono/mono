// System.Globalization/AllTests.cs
//
// (C) 2002 Ulrich Kunitz
//

using System;
using NUnit.Framework;

namespace MonoTests.System.Globalization {

/// <summary>
///   Combines all available unit tests into one test suite.
/// </summary>
public class AllTests : TestCase {
	public AllTests(string name) : base(name) {}
	
	public static ITest Suite 
	{ 
		get 
		{
			TestSuite suite =  new TestSuite();
			suite.AddTest(CalendarTest.Suite);
			return suite;
		}
	}
} // class AllTests

} // namespace MonoTests.System.Globalization
