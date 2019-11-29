//
// System.ComponentModel.EventDescriptorCollection test cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2008 Gert Driesen
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using CategoryAttribute = System.ComponentModel.CategoryAttribute;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class EventDescriptorCollectionTests
	{
		private CultureInfo originalCulture;

		[SetUp]
		public void SetUp ()
		{
			originalCulture = Thread.CurrentThread.CurrentCulture;
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

		[Test]
		public void Empty ()
		{
			EventDescriptorCollection descriptors = EventDescriptorCollection.Empty;
			Assert.AreEqual (0, descriptors.Count);
			AssertReadOnly (descriptors, "Empty");
		}

		[Test]
		public void Find ()
		{
			EventDescriptor descA = new MockEventDescriptor ("hehe_\u0061\u030a", null);
			EventDescriptor descB = new MockEventDescriptor ("heh_\u00e5", null);
			EventDescriptor descC = new MockEventDescriptor ("Foo", null);
			EventDescriptor descD = new MockEventDescriptor ("FOo", null);
			EventDescriptor descE = new MockEventDescriptor ("Aim", null);
			EventDescriptor descF = new MockEventDescriptor ("Bar", null);

			EventDescriptorCollection col = new EventDescriptorCollection (
				new EventDescriptor [] { descA, descB, descC, descD, descE, descF });

			Assert.IsNull (col.Find ("heh_\u0061\u030a", false), "#1");
			Assert.IsNull (col.Find ("hehe_\u00e5", false), "#2");
			Assert.AreSame (descA, col.Find ("hehe_\u0061\u030a", false), "#3");
			Assert.AreSame (descB, col.Find ("heh_\u00e5", false), "#4");
			Assert.IsNull (col.Find ("foo", false), "#5");
			Assert.AreSame (descC, col.Find ("foo", true), "#6");
			Assert.AreSame (descD, col.Find ("FOo", false), "#7");
			Assert.AreSame (descC, col.Find ("FOo", true), "#8");
			Assert.IsNull (col.Find ("fOo", false), "#9");
			Assert.AreSame (descC, col.Find ("fOo", true), "#10");
			Assert.IsNull (col.Find ("AIm", false), "#11");
			Assert.AreSame (descE, col.Find ("AIm", true), "#12");
			Assert.IsNull (col.Find ("AiM", false), "#13");
			Assert.AreSame (descE, col.Find ("AiM", true), "#14");
			Assert.AreSame (descE, col.Find ("Aim", false), "#15");
			Assert.AreSame (descE, col.Find ("Aim", true), "#16");
		}

		[Test]
		public void Find_Key_Null ()
		{
			EventDescriptorCollection descriptors = new EventDescriptorCollection (
				new EventDescriptor[] { new MockEventDescriptor ("A", "X"),
					new MockEventDescriptor ("b", "Y")});

			Assert.IsNull (descriptors.Find (null, false), "#1");
			Assert.IsNull (descriptors.Find (null, true), "#2");
		}

		[Test]
		public void IList ()
		{
			IList list = ((IList) new EventDescriptorCollection (null));

			Assert.AreEqual (0, list.Count, "#1");
			Assert.IsFalse (list.IsFixedSize, "#2");
			Assert.IsFalse (list.IsReadOnly, "#3");
			Assert.IsFalse (list.IsSynchronized, "#4");
			Assert.IsNull (list.SyncRoot, "#5");
		}

		[Test]
		public void IList_Add_Null ()
		{
			IList list = ((IList) new EventDescriptorCollection (null));
			Assert.AreEqual (0, list.Count, "#1");
			list.Add (null);
			Assert.AreEqual (1, list.Count, "#2");
		}

		[Test]
		public void IList_Add_NoEventDescriptor ()
		{
			IList list = ((IList) new EventDescriptorCollection (null));
			try {
				list.Add (5);
				Assert.Fail ("#1");
			} catch (InvalidCastException) {
			}
		}

		[Test] // this [String]
		public void Indexer2 ()
		{
			EventDescriptor descA = new MockEventDescriptor ("hehe_\u0061\u030a", null);
			EventDescriptor descB = new MockEventDescriptor ("heh_\u00e5", null);
			EventDescriptor descC = new MockEventDescriptor ("Foo", null);
			EventDescriptor descD = new MockEventDescriptor ("FOo", null);
			EventDescriptor descE = new MockEventDescriptor ("Aim", null);
			EventDescriptor descF = new MockEventDescriptor ("Bar", null);

			EventDescriptorCollection col = new EventDescriptorCollection (
				new EventDescriptor [] { descA, descB, descC, descD, descE, descF });

			Assert.IsNull (col ["heh_\u0061\u030a"], "#1");
			Assert.IsNull (col ["hehe_\u00e5"], "#2");
			Assert.AreSame (descA, col ["hehe_\u0061\u030a"], "#3");
			Assert.AreSame (descB, col ["heh_\u00e5"], "#4");
			Assert.IsNull (col ["foo"], "#5");
			Assert.AreSame (descD, col ["FOo"], "#6");
			Assert.IsNull (col ["fOo"], "#7");
			Assert.IsNull (col ["AIm"], "#8");
			Assert.IsNull (col ["AiM"], "#9");
			Assert.AreSame (descE, col ["Aim"], "#10");
			Assert.IsNull (col [(string) null], "#11");
		}

		public void ReadOnly ()
		{
			EventDescriptorCollection descriptors = new EventDescriptorCollection (
				(EventDescriptor []) null, true);
			AssertReadOnly (descriptors, "ReadOnly");
		}

		[Test] // Sort ()
#if WASM
		[Ignore ("WASM CompareInfo - https://github.com/mono/mono/issues/17837")]
#endif
		public void Sort1 ()
		{
			EventDescriptorCollection descriptors;
			EventDescriptorCollection sorted;

			EventDescriptor descA = new MockEventDescriptor ("Foo", "B");
			EventDescriptor descB = new MockEventDescriptor ("Aim", "C");
			EventDescriptor descC = new MockEventDescriptor ("Bim", "A");
			EventDescriptor descD = new MockEventDescriptor ("AIm", "E");
			EventDescriptor descE = new MockEventDescriptor ("Boo", "D");
			EventDescriptor descF = new MockEventDescriptor ("FOo", "F");

			EventDescriptor [] props = new EventDescriptor [] {
				descA, descB, descC, descD, descE, descF };
			descriptors = new EventDescriptorCollection (props);

			Assert.AreSame (descA, descriptors [0], "#A1");
			Assert.AreSame (descB, descriptors [1], "#A2");
			Assert.AreSame (descC, descriptors [2], "#A3");
			Assert.AreSame (descD, descriptors [3], "#A4");
			Assert.AreSame (descE, descriptors [4], "#A5");
			Assert.AreSame (descF, descriptors [5], "#A6");

			sorted = descriptors.Sort ();

			Assert.AreSame (descA, descriptors [0], "#B1");
			Assert.AreSame (descB, descriptors [1], "#B2");
			Assert.AreSame (descC, descriptors [2], "#B3");
			Assert.AreSame (descD, descriptors [3], "#B4");
			Assert.AreSame (descE, descriptors [4], "#B5");
			Assert.AreSame (descF, descriptors [5], "#B6");

			Assert.AreSame (descB, sorted [0], "#C1");
			Assert.AreSame (descD, sorted [1], "#C2");
			Assert.AreSame (descC, sorted [2], "#C3");
			Assert.AreSame (descE, sorted [3], "#C4");
			Assert.AreSame (descA, sorted [4], "#C5");
			Assert.AreSame (descF, sorted [5], "#C6");
		}

		[Test] // Sort (String [])
#if WASM
		[Ignore ("WASM CompareInfo - https://github.com/mono/mono/issues/17837")]
#endif
		public void Sort2 ()
		{
			EventDescriptorCollection descriptors;
			EventDescriptorCollection sorted;

			EventDescriptor descA = new MockEventDescriptor ("Foo", "B");
			EventDescriptor descB = new MockEventDescriptor ("Aim", "C");
			EventDescriptor descC = new MockEventDescriptor ("Bim", "A");
			EventDescriptor descD = new MockEventDescriptor ("AIm", "E");
			EventDescriptor descE = new MockEventDescriptor ("Boo", "D");
			EventDescriptor descF = new MockEventDescriptor ("FOo", "F");

			EventDescriptor [] props = new EventDescriptor [] {
				descA, descB, descC, descD, descE, descF };
			descriptors = new EventDescriptorCollection (props);

			Assert.AreSame (descA, descriptors [0], "#A1");
			Assert.AreSame (descB, descriptors [1], "#A2");
			Assert.AreSame (descC, descriptors [2], "#A3");
			Assert.AreSame (descD, descriptors [3], "#A4");
			Assert.AreSame (descE, descriptors [4], "#A5");
			Assert.AreSame (descF, descriptors [5], "#A5");

			sorted = descriptors.Sort (new string [] { "B", "Foo", null, "A", "Boo" });

			Assert.AreSame (descA, descriptors [0], "#B1");
			Assert.AreSame (descB, descriptors [1], "#B2");
			Assert.AreSame (descC, descriptors [2], "#B3");
			Assert.AreSame (descD, descriptors [3], "#B4");
			Assert.AreSame (descE, descriptors [4], "#B5");
			Assert.AreSame (descF, descriptors [5], "#B6");

			Assert.AreSame (descA, sorted [0], "#C1");
			Assert.AreSame (descE, sorted [1], "#C2");
			Assert.AreSame (descB, sorted [2], "#C3");
			Assert.AreSame (descD, sorted [3], "#C4");
			Assert.AreSame (descC, sorted [4], "#C5");
			Assert.AreSame (descF, sorted [5], "#C6");

			sorted = descriptors.Sort ((string []) null);

			Assert.AreSame (descA, descriptors [0], "#D1");
			Assert.AreSame (descB, descriptors [1], "#D2");
			Assert.AreSame (descC, descriptors [2], "#D3");
			Assert.AreSame (descD, descriptors [3], "#D4");
			Assert.AreSame (descE, descriptors [4], "#D5");
			Assert.AreSame (descF, descriptors [5], "#D6");

			Assert.AreSame (descB, sorted [0], "#E1");
			Assert.AreSame (descD, sorted [1], "#E2");
			Assert.AreSame (descC, sorted [2], "#E3");
			Assert.AreSame (descE, sorted [3], "#E4");
			Assert.AreSame (descA, sorted [4], "#E5");
			Assert.AreSame (descF, sorted [5], "#E6");
		}

		[Test] // Sort (IComparer)
#if WASM
		[Ignore ("WASM CompareInfo - https://github.com/mono/mono/issues/17837")]
#endif
		public void Sort3 ()
		{
			EventDescriptorCollection descriptors;
			EventDescriptorCollection sorted;

			EventDescriptor descA = new MockEventDescriptor ("Foo", "B");
			EventDescriptor descB = new MockEventDescriptor ("Aim", "C");
			EventDescriptor descC = new MockEventDescriptor ("Bim", "A");
			EventDescriptor descD = new MockEventDescriptor ("AIm", "E");
			EventDescriptor descE = new MockEventDescriptor ("Boo", "D");
			EventDescriptor descF = new MockEventDescriptor ("FOo", "F");

			EventDescriptor [] props = new EventDescriptor [] {
				descA, descB, descC, descD, descE, descF };
			descriptors = new EventDescriptorCollection (props);

			Assert.AreSame (descA, descriptors [0], "#A1");
			Assert.AreSame (descB, descriptors [1], "#A2");
			Assert.AreSame (descC, descriptors [2], "#A3");
			Assert.AreSame (descD, descriptors [3], "#A4");
			Assert.AreSame (descE, descriptors [4], "#A5");
			Assert.AreSame (descF, descriptors [5], "#A6");

			sorted = descriptors.Sort (new CategoryComparer ());

			Assert.AreSame (descA, descriptors [0], "#B1");
			Assert.AreSame (descB, descriptors [1], "#B2");
			Assert.AreSame (descC, descriptors [2], "#B3");
			Assert.AreSame (descD, descriptors [3], "#B4");
			Assert.AreSame (descE, descriptors [4], "#B5");
			Assert.AreSame (descF, descriptors [5], "#B6");

			Assert.AreSame (descC, sorted [0], "#C1");
			Assert.AreSame (descA, sorted [1], "#C2");
			Assert.AreSame (descB, sorted [2], "#C3");
			Assert.AreSame (descE, sorted [3], "#C4");
			Assert.AreSame (descD, sorted [4], "#C5");
			Assert.AreSame (descF, sorted [5], "#C6");

			sorted = descriptors.Sort ((Comparer) null);

			Assert.AreSame (descA, descriptors [0], "#D1");
			Assert.AreSame (descB, descriptors [1], "#D2");
			Assert.AreSame (descC, descriptors [2], "#D3");
			Assert.AreSame (descD, descriptors [3], "#D4");
			Assert.AreSame (descE, descriptors [4], "#D5");
			Assert.AreSame (descF, descriptors [5], "#D6");

			Assert.AreSame (descB, sorted [0], "#E1");
			Assert.AreSame (descD, sorted [1], "#E2");
			Assert.AreSame (descC, sorted [2], "#E3");
			Assert.AreSame (descE, sorted [3], "#E4");
			Assert.AreSame (descA, sorted [4], "#E5");
			Assert.AreSame (descF, sorted [5], "#E6");
		}

		[Test] // Sort (String [], IComparer)
#if WASM
		[Ignore ("WASM CompareInfo - https://github.com/mono/mono/issues/17837")]
#endif
		public void Sort4 ()
		{
			EventDescriptorCollection descriptors;
			EventDescriptorCollection sorted;

			EventDescriptor descA = new MockEventDescriptor ("Foo", "B");
			EventDescriptor descB = new MockEventDescriptor ("Aim", "C");
			EventDescriptor descC = new MockEventDescriptor ("Bim", "A");
			EventDescriptor descD = new MockEventDescriptor ("AIm", "E");
			EventDescriptor descE = new MockEventDescriptor ("Boo", "D");
			EventDescriptor descF = new MockEventDescriptor ("FOo", "F");

			EventDescriptor [] props = new EventDescriptor [] {
				descA, descB, descC, descD, descE, descF };
			descriptors = new EventDescriptorCollection (props);

			Assert.AreSame (descA, descriptors [0], "#A1");
			Assert.AreSame (descB, descriptors [1], "#A2");
			Assert.AreSame (descC, descriptors [2], "#A3");
			Assert.AreSame (descD, descriptors [3], "#A4");
			Assert.AreSame (descE, descriptors [4], "#A5");
			Assert.AreSame (descF, descriptors [5], "#A6");

			sorted = descriptors.Sort (new string [] { "B", "Foo", null, "A", "Boo" },
				new CategoryComparer ());

			Assert.AreSame (descA, descriptors [0], "#B1");
			Assert.AreSame (descB, descriptors [1], "#B2");
			Assert.AreSame (descC, descriptors [2], "#B3");
			Assert.AreSame (descD, descriptors [3], "#B4");
			Assert.AreSame (descE, descriptors [4], "#B5");
			Assert.AreSame (descF, descriptors [5], "#B6");

			Assert.AreSame (descA, sorted [0], "#C1");
			Assert.AreSame (descE, sorted [1], "#C2");
			Assert.AreSame (descC, sorted [2], "#C3");
			Assert.AreSame (descB, sorted [3], "#C4");
			Assert.AreSame (descD, sorted [4], "#C5");
			Assert.AreSame (descF, sorted [5], "#C6");

			sorted = descriptors.Sort ((string []) null, new CategoryComparer ());

			Assert.AreSame (descA, descriptors [0], "#D1");
			Assert.AreSame (descB, descriptors [1], "#D2");
			Assert.AreSame (descC, descriptors [2], "#D3");
			Assert.AreSame (descD, descriptors [3], "#D4");
			Assert.AreSame (descE, descriptors [4], "#D5");
			Assert.AreSame (descF, descriptors [5], "#D6");

			Assert.AreSame (descC, sorted [0], "#E1");
			Assert.AreSame (descA, sorted [1], "#E2");
			Assert.AreSame (descB, sorted [2], "#E3");
			Assert.AreSame (descE, sorted [3], "#E4");
			Assert.AreSame (descD, sorted [4], "#E5");
			Assert.AreSame (descF, sorted [5], "#E6");

			sorted = descriptors.Sort (new string [] { "B", "Foo", null, "A", "Boo" },
				(Comparer) null);

			Assert.AreSame (descA, descriptors [0], "#F1");
			Assert.AreSame (descB, descriptors [1], "#F2");
			Assert.AreSame (descC, descriptors [2], "#F3");
			Assert.AreSame (descD, descriptors [3], "#F4");
			Assert.AreSame (descE, descriptors [4], "#F5");
			Assert.AreSame (descF, descriptors [5], "#F6");

			Assert.AreSame (descA, sorted [0], "#G1");
			Assert.AreSame (descE, sorted [1], "#G2");
			Assert.AreSame (descB, sorted [2], "#G3");
			Assert.AreSame (descD, sorted [3], "#G4");
			Assert.AreSame (descC, sorted [4], "#G5");
			Assert.AreSame (descF, sorted [5], "#G6");

			sorted = descriptors.Sort ((string []) null, (Comparer) null);

			Assert.AreSame (descA, descriptors [0], "#H1");
			Assert.AreSame (descB, descriptors [1], "#H2");
			Assert.AreSame (descC, descriptors [2], "#H3");
			Assert.AreSame (descD, descriptors [3], "#H4");
			Assert.AreSame (descE, descriptors [4], "#H5");
			Assert.AreSame (descF, descriptors [5], "#H6");

			Assert.AreSame (descB, sorted [0], "#I1");
			Assert.AreSame (descD, sorted [1], "#I2");
			Assert.AreSame (descC, sorted [2], "#I3");
			Assert.AreSame (descE, sorted [3], "#I4");
			Assert.AreSame (descA, sorted [4], "#I5");
			Assert.AreSame (descF, sorted [5], "#I6");
		}

		private void AssertReadOnly (EventDescriptorCollection descriptors, string testCase)
		{
			MockEventDescriptor desc = new MockEventDescriptor (
				"Date", "NOW");

			try {
				descriptors.Add (desc);
				Assert.Fail (testCase + "#1");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			// ensure read-only check if performed before value is checked
			try {
				descriptors.Add (null);
				Assert.Fail (testCase + "#2");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			try {
				descriptors.Clear ();
				Assert.Fail (testCase + "#3");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			try {
				descriptors.Insert (0, desc);
				Assert.Fail (testCase + "#4");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			// ensure read-only check if performed before value is checked
			try {
				descriptors.Insert (0, null);
				Assert.Fail (testCase + "#5");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			try {
				descriptors.Remove (desc);
				Assert.Fail (testCase + "#6");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			// ensure read-only check if performed before value is checked
			try {
				descriptors.Remove (null);
				Assert.Fail (testCase + "#7");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			try {
				descriptors.RemoveAt (0);
				Assert.Fail (testCase + "#8");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			IList list = (IList) descriptors;
			Assert.IsTrue (((IList) descriptors).IsReadOnly, testCase + "#9");
			Assert.IsTrue (((IList) descriptors).IsFixedSize, testCase + "#10");

			try {
				list.Add (desc);
				Assert.Fail (testCase + "#11");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			// ensure read-only check if performed before value is checked
			try {
				list.Add (null);
				Assert.Fail (testCase + "#12");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			try {
				list.Clear ();
				Assert.Fail (testCase + "#13");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			try {
				list.Insert (0, desc);
				Assert.Fail (testCase + "#14");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			// ensure read-only check if performed before value is checked
			try {
				list.Insert (0, null);
				Assert.Fail (testCase + "#15");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			try {
				list.Remove (desc);
				Assert.Fail (testCase + "#16");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			// ensure read-only check if performed before value is checked
			try {
				list.Remove (null);
				Assert.Fail (testCase + "#17");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			try {
				list.RemoveAt (0);
				Assert.Fail (testCase + "#18");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			try {
				list[0] = desc;
				Assert.Fail (testCase + "#19");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			// ensure read-only check if performed before value is checked
			try {
				list[0] = null;
				Assert.Fail (testCase + "#20");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}
		}

		private class MockEventDescriptor : EventDescriptor
		{
			public MockEventDescriptor (string name, string category)
				: base (name, new Attribute [] { new CategoryAttribute (category) })
			{
			}

			public override Type ComponentType {
				get {
					return null;
				}
			}

			public override Type EventType {
				get {
					return null;
				}
			}

			public override bool IsMulticast {
				get {
					return false;
				}
			}

			public override void AddEventHandler (object component, Delegate value)
			{
			}

			public override void RemoveEventHandler (object component, Delegate value)
			{
			}
		}

		class CategoryComparer : IComparer
		{
			public int Compare (object x, object y)
			{
				EventDescriptor descX = x as EventDescriptor;
				EventDescriptor descY = y as EventDescriptor;

				if (descX == null && descY == null)
					return 0;
				if (descX == null)
					return -1;
				if (descY == null)
					return 1;

				return string.Compare (descX.Category, descY.Category, false, CultureInfo.CurrentCulture);
			}
		}
	}
}
