// ComparerTest

using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;

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
			Assert.IsTrue (c.Compare (1, 2) < 0, "1,2");
			Assert.IsTrue (c.Compare (2, 2) == 0, "2,2");
			Assert.IsTrue (c.Compare (3, 2) > 0, "3,2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Compare_FirstIComparer ()
		{
			Assert.IsTrue (Comparer.Default.Compare (1, new object ()) > 0, "int,object");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Compare_SecondIComparer ()
		{
			Assert.IsTrue (Comparer.Default.Compare (new object (), 1) > 0, "object,int");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Compare_BothNonIComparer ()
		{
			Comparer.Default.Compare (new object (), new object ());
		}

		[Test]
		[Category ("ManagedCollator")]
		public void Invariant ()
		{
			Comparer c = Comparer.DefaultInvariant;
			Assert.IsTrue (c.Compare ("a", "A") < 0);
		}

		[Test]
		public void Invariant2 ()
		{
			Assert.IsTrue (CultureInfo.InvariantCulture.CompareInfo.Compare ("a", "A", CompareOptions.Ordinal) > 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Null ()
		{
			new Comparer (null);
		}

		[Test]
		[Category ("ManagedCollator")]
		public void Constructor ()
		{
			Comparer c = new Comparer (CultureInfo.InvariantCulture);
			Assert.IsTrue (c.Compare ("a", "A") < 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObjectData_Null ()
		{
			Comparer c = Comparer.DefaultInvariant;
			c.GetObjectData (null, new StreamingContext ());
		}

		[Test]
		public void GetObjectData ()
		{
			Comparer c = Comparer.DefaultInvariant;
			SerializationInfo si = new SerializationInfo (typeof (Comparer), new FormatterConverter ());
			c.GetObjectData (si, new StreamingContext ());
			foreach (SerializationEntry se in si) {
				switch (se.Name) {
				case "CompareInfo":
					CompareInfo ci = (se.Value as CompareInfo);
					Assert.IsNotNull (ci, "CompareInfo");
					Assert.AreEqual (127, ci.LCID, "LCID");
					break;
				default:
					string msg = String.Format ("Unexpected {0} information of type {1} with value '{2}'.",
						se.Name, se.ObjectType, se.Value);
					Assert.Fail (msg);
					break;
				}
			}
		}
	}
}
