//
// HybridDictionaryTest.cs - NUnit Test Cases for System.Net.HybridDictionary
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace MonoTests.System.Collections.Specialized
{
	public class HybridDictionaryTest : TestCase
	{
		public HybridDictionaryTest () :
			base ("[MonoTests.System.Net.HybridDictionaryTest]") {}

		public HybridDictionaryTest (string name) : base (name) {}

		protected override void SetUp () {}

		protected override void TearDown () {}

		public static ITest Suite
		{
			get {
				return new TestSuite (typeof (HybridDictionaryTest));
			}
		}

		public void TestAll ()
		{
			HybridDictionary dict = new HybridDictionary (true);
			dict.Add ("CCC", "ccc");
			dict.Add ("BBB", "bbb");
			dict.Add ("fff", "fff");
			dict ["EEE"] = "eee";
			dict ["ddd"] = "ddd";
			
			AssertEquals ("#1", 5, dict.Count);
			AssertEquals ("#2", "eee", dict ["eee"]);
			
			dict.Add ("CCC2", "ccc");
			dict.Add ("BBB2", "bbb");
			dict.Add ("fff2", "fff");
			dict ["EEE2"] = "eee";
			dict ["ddd2"] = "ddd";
			dict ["xxx"] = "xxx";
			dict ["yyy"] = "yyy";
			
			AssertEquals ("#3", 12, dict.Count);
			AssertEquals ("#4", "eee", dict ["eee"]);	
		}
	}        
}