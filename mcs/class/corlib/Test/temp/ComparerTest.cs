// ComparerTest

using System;
using System.Collections;

using NUnit.Framework;



namespace Testsuite.System.Collections {


	/// <summary>Comparer test suite.</summary>
	public class ComparerTest {
		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite ("Comparer tests");
				suite.AddTest (ComparerTestCase.Suite);
				return suite;
			}
		}
	}


	public class ComparerTestCase : TestCase {

		public ComparerTestCase (String name) : base(name)
		{
		}

		protected override void SetUp ()
		{
		}

		public static ITest Suite
		{
			get {
				Console.WriteLine("Testing " + Comparer.Default);
				return new TestSuite(typeof(ComparerTestCase));
			}
		}

		public void TestDefaultInstance ()
		{
			// Make sure the instance returned by Default
			// is really a Comparer.
			Assert((Comparer.Default as Comparer) != null);
		}

		public void TestCompare ()
		{
			Comparer c = Comparer.Default;

			bool thrown = false;

			try {
				c.Compare (new Object (), new Object ());
			} catch (ArgumentException) {
				thrown = true;
			}

			Assert("ArgumentException expected", thrown);

			Assert(c.Compare (1, 2) < 0);
			Assert(c.Compare (2, 2) == 0);
			Assert(c.Compare (3, 2) > 0);

		}
			
	}

}
