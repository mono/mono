// ComparerTest

using System;
using System.Collections;
using NUnit.Framework;


namespace MonoTests.System.Collections {


	[TestFixture]
	public class ComparerTest {

		[Test]
		public void TestDefaultInstance ()
		{
			// Make sure the instance returned by Default
			// is really a Comparer.
			Assert.IsNotNull (Comparer.Default as Comparer);
		}

		[Test]
		public void TestCompare ()
		{
			Comparer c = Comparer.Default;

			bool thrown = false;

			try {
				c.Compare (new Object (), new Object ());
			} catch (ArgumentException) {
				thrown = true;
			}

			Assert.IsTrue (thrown, "ArgumentException expected");

			Assert.IsTrue (c.Compare (1, 2) < 0, "1,2");
			Assert.IsTrue (c.Compare (2, 2) == 0, "2,2");
			Assert.IsTrue (c.Compare (3, 2) > 0, "3,2");

		}


		[Test]
		
		public void Invariant ()
		{
			Comparer c = Comparer.DefaultInvariant;
			Assert.IsTrue (c.Compare ("a", "A") < 0);
		}
	}
}
