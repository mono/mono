//
// assembly:	System_test
// namespace:	MonoTests.System.Text.RegularExpressions
// file:	AllTests.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

using System;
using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions {
	
	public class AllTests : TestCase {
		public AllTests (string name) : base (name) { }

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite ();
				suite.AddTest (PerlTest.Suite);

				return suite;
			}
		}
	}
}
