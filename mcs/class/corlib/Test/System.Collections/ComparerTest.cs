// ComparerTest

using System;
using System.Collections;

using NUnit.Framework;



namespace MonoTests.System.Collections {


	/// <summary>Comparer test suite.</summary>
	public class ComparerTest : TestCase {
		protected override void SetUp ()
		{
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

		[Test]
		public void Invariant ()
		{
			Comparer c = Comparer.DefaultInvariant;

			Assert (c.Compare ("a", "A") < 0);
		}
		
				
	}

}
