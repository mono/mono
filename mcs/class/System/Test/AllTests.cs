// Ximian.Mono.Tests.AllTests, System.dll
//
// Author:
//   Mario Martinez (mariom925@home.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;
namespace Ximian.Mono.Tests.System
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
				suite.AddTest (DnsTest.Suite);
				suite.AddTest (Collections.Specialized.NameValueCollectionTest.Suite);
				suite.AddTest (Collections.Specialized.StringCollectionTest.Suite);
				suite.AddTest (Text.RegularExpressions.AllTests.Suite);
        // suite.AddTest (MonoTests.System.Diagnostics.AllTests.Suite);
				return suite;
			}
		}
	}
}
