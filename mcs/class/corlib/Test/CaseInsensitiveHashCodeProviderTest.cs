// CaseInsensitiveHashCodeProviderTest

using System;
using System.Collections;

using NUnit.Framework;



namespace Testsuite.System.Collections {


	/// <summary>CaseInsensitiveHashCodeProvider test suite.</summary>
	public class CaseInsensitiveHashCodeProviderTest {
		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite ("CaseInsensitiveHashCodeProviderTest tests");
				suite.AddTest (CIHashCodeProviderTestCase.Suite);
				return suite;
			}
		}
	}


	public class CIHashCodeProviderTestCase : TestCase {

		public CIHashCodeProviderTestCase(String name) : base(name)
		{
		}

		protected override void SetUp ()
		{
		}

		public static ITest Suite
		{
			get {
				Console.WriteLine("Testing " + (new CaseInsensitiveHashCodeProvider()));
				return new TestSuite(typeof(CIHashCodeProviderTestCase));
			}
		}

		public void TestDefaultInstance ()
		{
			// Make sure the instance returned by Default
			// is really a CaseInsensitiveHashCodeProvider.
			Assert((CaseInsensitiveHashCodeProvider.Default
			        as CaseInsensitiveHashCodeProvider) != null);
		}

		public void TestHashCode () {
			CaseInsensitiveHashCodeProvider cih = new CaseInsensitiveHashCodeProvider ();
			int h1 = cih.GetHashCode ("Test String");
			int h2 = cih.GetHashCode ("test string");
			int h3 = cih.GetHashCode ("TEST STRING");

			Assert("Mixed Case != lower case", h1 == h2);
			Assert("Mixed Case != UPPER CASE", h1 == h3);

			h1 = cih.GetHashCode ("one");
			h2 = cih.GetHashCode ("another");
			// Actually this is quite possible.
			Assert(h1 != h2);
		}
			
	}

}
