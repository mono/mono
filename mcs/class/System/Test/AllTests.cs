// MonoTests.AllTests, System.dll
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
	///   Combines all unit tests for the System.dll assembly
	///   into one test suite.
	/// </summary>
	public class AllTests : TestCase
	{
		public AllTests(string name) : base(name) {}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite();
				suite.AddTest (System.AllTests.Suite);
				suite.AddTest (System.Collections.Specialized.AllTests.Suite);
				suite.AddTest (System.ComponentModel.AllTests.Suite);
				suite.AddTest (System.Diagnostics.AllTests.Suite);
                                suite.AddTest (System.Net.AllTests.Suite);
                                suite.AddTest (System.Net.Sockets.AllTests.Suite);
				suite.AddTest (System.Text.RegularExpressions.AllTests.Suite);
				return suite;
			}
		}
	}
}
