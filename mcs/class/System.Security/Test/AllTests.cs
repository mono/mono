//
// TestSuite.System.Security.AllTests.cs
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;
using NUnit.Framework;

namespace MonoTests.System.Security{

public class AllTests : TestCase {

	public AllTests (string name) : base (name) {}
        
	public static ITest Suite { 
		get {
			TestSuite suite =  new TestSuite ();
			suite.AddTest (System.Security.Cryptography.Xml.AllTests.Suite);
			return suite;
		}
	}
}

}
