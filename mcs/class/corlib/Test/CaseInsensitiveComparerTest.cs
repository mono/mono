// CaseInsensitiveComparerTest

using System;
using System.Collections;

using NUnit.Framework;



namespace Testsuite.System.Collections {


	/// <summary>CaseInsensitiveComparer test suite.</summary>
	public class CaseInsensitiveComparerTest {
		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite("CaseInsensitiveComparerTest tests");
				suite.AddTest(CIComparerTestCase.Suite);
				return suite;
			}
		}
	}


	public class CIComparerTestCase : TestCase {

		public CIComparerTestCase (String name) : base(name)
		{
		}

		protected override void SetUp ()
		{
		}

		public static ITest Suite
		{
			get {
				Console.WriteLine("Testing " + (new CaseInsensitiveComparer ()));
				return new TestSuite(typeof(CIComparerTestCase));
			}
		}

		public void TestDefaultInstance ()
		{
			// Make sure the instance returned by Default
			// is really a CaseInsensitiveComparer.
			Assert((CaseInsensitiveComparer.Default
			        as CaseInsensitiveComparer) != null);
		}

		public void TestCompare () {
			CaseInsensitiveComparer cic = new CaseInsensitiveComparer ();

			Assert(cic.Compare ("WILD WEST", "Wild West") == 0);
			Assert(cic.Compare ("WILD WEST", "wild west") == 0);
			Assert(cic.Compare ("Zeus", "Mars") > 0);
			Assert(cic.Compare ("Earth", "Venus") < 0);
		}
			
	}

}
