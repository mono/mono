//
// HybridDictionaryTest.cs - NUnit Test Cases for System.Net.HybridDictionary
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace MonoTests.System.Collections.Specialized
{
	[TestFixture]
	public class HybridDictionaryTest
	{
		[Test]
		public void All ()
		{
			HybridDictionary dict = new HybridDictionary (true);
			dict.Add ("CCC", "ccc");
			dict.Add ("BBB", "bbb");
			dict.Add ("fff", "fff");
			dict ["EEE"] = "eee";
			dict ["ddd"] = "ddd";
			
			Assertion.AssertEquals ("#1", 5, dict.Count);
			Assertion.AssertEquals ("#2", "eee", dict ["eee"]);
			
			dict.Add ("CCC2", "ccc");
			dict.Add ("BBB2", "bbb");
			dict.Add ("fff2", "fff");
			dict ["EEE2"] = "eee";
			dict ["ddd2"] = "ddd";
			dict ["xxx"] = "xxx";
			dict ["yyy"] = "yyy";
			
			Assertion.AssertEquals ("#3", 12, dict.Count);
			Assertion.AssertEquals ("#4", "eee", dict ["eee"]);	
		}
	}        
}
