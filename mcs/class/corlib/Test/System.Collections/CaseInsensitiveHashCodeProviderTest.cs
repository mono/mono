// CaseInsensitiveHashCodeProviderTest

using System;
using System.Collections;

using NUnit.Framework;



namespace MonoTests.System.Collections {


	/// <summary>CaseInsensitiveHashCodeProvider test suite.</summary>
	public class CaseInsensitiveHashCodeProviderTest : TestCase {
		protected override void SetUp ()
		{
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
