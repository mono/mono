// Author: Tim Coleman (tim@timcoleman.com)
// Copyright 2002 Tim Coleman

using System;
using System.Data;
using System.Data.SqlTypes;
using NUnit.Framework;

namespace MonoTests {
	public class AllTests : TestCase {
		public AllTests (string name) : base (name) {}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite ();
				suite.AddTest (System.Data.SqlTypes.AllTests.Suite);
				suite.AddTest (System.Data.Xml.AllTests.Suite);
				return suite;
			}
		}
	}
}
