// MonoTests.AllTests, Microsoft.VisualBasic.dll
//
// Author:
//   Mario Martinez (mariom925@home.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;

namespace MonoTests
{
	/// <summary>
	///   Combines all unit tests for the Microsoft.VisualBasic.dll assembly
	///   into one test suite.
	/// </summary>
	public class AllTests : TestCase
	{
		public AllTests(string name) : base(name) {}
		public AllTests() : base("Microsoft.VisualBasic.AllTests") {}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite();
				suite.AddTest (Microsoft.VisualBasic.CollectionTest.Suite);
				suite.AddTest (Microsoft.VisualBasic.ConversionTest.Suite);
				suite.AddTest (Microsoft.VisualBasic.DateAndTimeTest.Suite);
				
				return suite;
			}
		}
	}
}
