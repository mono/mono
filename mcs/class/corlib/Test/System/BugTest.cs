// BugTest.cs - NUnit Test Case that exibits a NullReferenceException
//
// Nick Drochak (ndrochak@gol.com)
// 
// (C) Nick Drochak
// 
// Delete this file after bug is fixed


using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System
{
	public class BugTest : TestCase
	{
		public BugTest() : base ("MonoTests.System.BugTest testsuite") {}
		public BugTest(string name) : base(name) {}

// seem to need two value types here
		DateTime tryDT;
		decimal tryDec;
// and you need this here
		object tryObj;
		
		protected override void SetUp() {
// have to leave this assignment here
			tryDec = 1234.2345m;
		}
		protected override void TearDown() {}

		public static ITest Suite {
			get { 
				return new TestSuite(typeof(BugTest)); 
			}
		}

// the second just runs so that SetUp() is called again (thus doing something with a value type Decimal)
		public void TestChangeType() {
		}		

// the first test to run must fail
		public void TestGetTypeCode() {
			Fail ("No Reason");
		}
	}
}