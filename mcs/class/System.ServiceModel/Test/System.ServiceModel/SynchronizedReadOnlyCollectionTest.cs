using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;

using ObjectList = System.Collections.Generic.SynchronizedReadOnlyCollection<object>;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class SynchronizedReadOnlyCollectionTest
	{
		[Test, ExpectedException (typeof (ArgumentException))]
		public void TestIListIndexOf ()
		{
			SynchronizedReadOnlyCollection<int> c = new SynchronizedReadOnlyCollection<int> ();

			((IList) c).IndexOf ("foo");
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void TestIListContainsWrongType()
		{
			SynchronizedReadOnlyCollection<int> c = new SynchronizedReadOnlyCollection<int> ();

			((IList) c).Contains ("foo");
		}

		[Test]
		public void TestIListContainsNull ()
		{                        
			ObjectList c = new ObjectList ();
			Assert.IsFalse (((IList) c).Contains (null));

			SynchronizedReadOnlyCollection<ValueType> d = new SynchronizedReadOnlyCollection<ValueType> ();
			Assert.IsFalse (((IList) d).Contains (null));
		}

		[Test]
		public void TestICollectionCopyTo ()
		{
			SynchronizedReadOnlyCollection<int> c = new SynchronizedReadOnlyCollection<int> ();
			Array a = Array.CreateInstance (typeof (String), 10);

			((ICollection) c).CopyTo (a, 0);
		}

		[Test]
		public void TestCtorListArg ()
		{
			object x = new object ();
			object y = new object ();
			ObjectList c = new ObjectList (new object (),
				new object [] {x, y});
			Assert.AreEqual (2, c.Count, "#1");
			// indexer
			Assert.AreEqual (x, c [0], "#2");
			Assert.AreEqual (y, c [1], "#3");
			// GetEnumerator
			IEnumerator<object> ge = c.GetEnumerator ();
			Assert.IsTrue (ge.MoveNext (), "#8");
			Assert.AreEqual (x, ge.Current, "#9");
			Assert.IsTrue (ge.MoveNext (), "#10");
			Assert.AreEqual (y, ge.Current, "#11");
			// IEnumerable.GetEnumerator
			IEnumerable enu = c;
			IEnumerator e = enu.GetEnumerator ();
			Assert.IsTrue (e.MoveNext (), "#4");
			Assert.AreEqual (x, e.Current, "#5");
			Assert.IsTrue (e.MoveNext (), "#6");
			Assert.AreEqual (y, e.Current, "#7");
		}
	}
}
