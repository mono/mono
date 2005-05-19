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
			} catch (ArgumentOutOfRangeException){
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

		static List<int> MakeList()
		{
			List<int> l = new List<int> ();

			l.Add (55);
			l.Add (50);
			l.Add (22);
			l.Add (80);
			l.Add (56);
			l.Add (52);
			l.Add (40);
			l.Add (63);

			return l;
		}
			
		[Test]
		public void TestGetRange ()
		{
			List<int> l = MakeList ();

			List<int> r = l.GetRange (2, 4);
			AssertEquals ("Size is not correct", 4, r.Count);
			AssertEquals ("Data failure", 22, r [0]);
			AssertEquals ("Data failure", 80, r [1]);
			AssertEquals ("Data failure", 56, r [2]);
			AssertEquals ("Data failure", 52, r [3]);
		}
	}
}
#endif
