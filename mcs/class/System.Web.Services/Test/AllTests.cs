// MonoTests.AllTests, System.Web.Services.dll
//
// Author: 
//     Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002

using NUnit.Framework;

namespace MonoTests {
	public class AllTests : TestCase {
		public AllTests (string name) : base (name) {}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite ();
				suite.AddTest (System.Web.Services.AllTests.Suite);
				return suite;
			}
		}
	}
}
