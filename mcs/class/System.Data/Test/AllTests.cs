// Author: Tim Coleman (tim@timcoleman.com)
// Copyright 2002 Tim Coleman

using System;
using NUnit.Framework;

namespace MonoTests.System.Data {
	public class AllTests : TestCase {
		public AllTests (string name) : base (name) {}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite ();
				suite.AddTest (SqlTypes.AllTests.Suite);
				return suite;
			}
		}
	}
}
