// MonoTests.System.Web.Services.Configuration.AllTests, System.Web.Services.dll
//
// Author: 
//     Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002

using NUnit.Framework;

namespace MonoTests.System.Web.Services.Configuration {
	public class AllTests : TestCase {
		public AllTests (string name) : base (name) {}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite ();
				suite.AddTest (XmlFormatExtensionAttributeTest.Suite);
				return suite;
			}
		}
	}
}
