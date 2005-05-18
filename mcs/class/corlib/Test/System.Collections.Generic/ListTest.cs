#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.Collections.Generic {

	[TestFixture]
	public class ListTest : Assertion {

		[SetUp]
		public void SetUp ()
		{
		}

		[Test]  // This was for bug #74980
		public void TestInsertion ()
		{
			List<string> test = new List<string>();
			test.Insert(0, "a");
			test.Insert(0, "b");
		}

		[Test]
		public void TestOutOfRange ()
		{
			List<int> l = new List<int> (4);

			bool errorThrown = false;
			try {
				l.IndexOf (0, 0, 4);
			} catch (ArgumentNullException){
				errorThrown = true;
			}
			Assert ("Out of range count exception not thrown", errorThrown);
		}

		[Test]
		public void TestIndexOf ()
		{
			List<int> l = new List<int>();

			l.Add (100);
			l.Add (200);

			Assert ("Could not find value", l.IndexOf (200) == 1);
		}
	}
}
#endif
