// Ximian.Mono.Tests.AllTests, System.dll
//
// Author:
//   Mario Martinez (mariom925@home.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;
namespace Ximian.Mono.Tests
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
				TestSuite suite =  new TestSuite();
				suite.AddTest(DnsTest.Suite);
				suite.AddTest(IPHostEntryTest.Suite);
				
				suite.AddTest(Testsuite.System.Collections.Specialized.NameValueCollectionTest.Suite);
				suite.AddTest(new TestSuite(typeof(System.Collections.Specialized.StringCollectionTest)));
				suite.AddTest(MonoTests.System.Text.RegularExpressions.AllTests.Suite);
				return suite;
			}
		}
	}
}
