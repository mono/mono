// ComparerTest

using System;
using System.Collections;
using System.Globalization;
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
		[Category("NotWorking")]
		public void Invariant ()
		{
			Comparer c = Comparer.DefaultInvariant;

			//
			// In Mono we are comparing the ordinal values 
			// of the strings, while it seems that the MS
			// runtime considers lowercase letters like "a"
			// to come before "A".
			//
			// I have not found any documentation on this
			// behavior of the InvariantCulture
			//
			Assert.IsTrue (c.Compare ("a", "A") < 0);
		}

		[Test]
		public void Invariant2 ()
		{
			Assert.IsTrue (CultureInfo.InvariantCulture.CompareInfo.Compare ("a", "A", CompareOptions.Ordinal) > 0);
		}
	}
}
