//
// MonoTests.Microsoft.CSharp.AllTests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved.
//

using System;
using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp  {
	
	public class AllTests : TestCase
	{

		public AllTests (string name) : base (name)
		{
		}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite ();
				return suite;
			}
		}
	}
}

