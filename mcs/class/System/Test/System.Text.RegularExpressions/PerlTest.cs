//
// assembly:	System_test
// namespace:	MonoTests.System.Text.RegularExpressions
// file:	PerlTest.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions {
	
	public class PerlTest : TestCase {
		public static ITest Suite {
			get { return new TestSuite (typeof (PerlTest)); }
		}

		public PerlTest () : this ("System.Text.RegularExpressions Perl testsuite") { }
		public PerlTest (string name) : base (name) { }

		public void TestTrials () {
			foreach (RegexTrial trial in PerlTrials.trials) {
				string actual = trial.Execute ();
				if (actual != trial.Expected) {
					Assertion.Fail (
						trial.ToString () +
						"Expected " + trial.Expected +
						" but got " + actual
					);
				}
			}
		}

		protected override void SetUp () { }
		protected override void TearDown () { }
	}
}
