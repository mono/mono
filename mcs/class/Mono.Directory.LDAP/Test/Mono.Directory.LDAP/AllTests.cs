//
// MonoTests.System.DirectoryServices.AllTests, System.DirectoryServices.dll
//
// Author:
//   Chris Toshok <toshok@ximian.com>
//

using NUnit.Framework;
using System;
using MonoTests.Directory.LDAP;

namespace MonoTests.Directory.LDAP  {
	
	public class AllTests : TestCase {

		public AllTests (string name) : base (name)
		{
		}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite ();
				suite.AddTest (BindSimpleTest.Suite);
				suite.AddTest (QueryRootDSE.Suite);
				return suite;
			}
		}
	}
}

