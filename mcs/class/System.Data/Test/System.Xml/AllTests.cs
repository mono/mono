// Author:
//   Ville Palo (vi64pa@koti.soon.fi)
//
// (C) Copyright 2002 Ville Palo
//

using NUnit.Framework;

namespace MonoTests.System.Data.Xml
{
	public class AllTests : TestCase
	{
		public AllTests (string name) : base (name) {}

		public static ITest Suite {
			get {
				TestSuite suite =  new TestSuite ();
				suite.AddTest (new TestSuite (typeof (XmlDataDocumentTest)));
			        return suite;
			}
		}
	}
}
