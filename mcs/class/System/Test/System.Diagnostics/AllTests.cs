//
// MonoTests.System.Diagnostics.AllTests, System.dll
//
// Author:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using NUnit.Framework;
using System;

namespace MonoTests.System.Diagnostics {

	public class AllTests : TestCase {

		public AllTests(string name) : base(name)
		{}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite();
				suite.AddTest (MonoTests.System.Diagnostics.TraceTest.Suite);
				suite.AddTest (MonoTests.System.Diagnostics.SwitchesTest.Suite);
				suite.AddTest (MonoTests.System.Diagnostics.DiagnosticsConfigurationHandlerTest.Suite);
				return suite;
			}
		}
	}
}

