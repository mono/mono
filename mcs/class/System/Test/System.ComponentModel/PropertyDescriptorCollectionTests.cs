//
// System.ComponentModel.PropertyDescriptorCollection test cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2005 Novell, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class PropertyDescriptorCollectionTests
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
			PropertyDescriptorCollection descriptors = PropertyDescriptorCollection.Empty;
			Assert.AreEqual (0, descriptors.Count);
			AssertReadOnly (descriptors, "Empty");
		}

		[Test]
		public void Find ()
		{
			PropertyDescriptor descA = new MockPropertyDescriptor ("hehe_\u0061\u030a", 2);
			PropertyDescriptor descB = new MockPropertyDescriptor ("heh_\u00e5", 3);
			PropertyDescriptor descC = new MockPropertyDescriptor ("Foo", 5);
			PropertyDescriptor descD = new MockPropertyDescriptor ("FOo", 6);
			PropertyDescriptor descE = new MockPropertyDescriptor ("Aim", 1);
			PropertyDescriptor descF = new MockPropertyDescriptor ("Bar", 4);

			PropertyDescriptorCollection col = new PropertyDescriptorCollection (
				new PropertyDescriptor [] { descA, descB, descC, descD, descE, descF });

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
		public void Find_Name_Null ()
		{
			PropertyDescriptorCollection descriptors;
			
			descriptors = new PropertyDescriptorCollection (
				new PropertyDescriptor[] { new MockPropertyDescriptor ("A", 1),
					new MockPropertyDescriptor ("b", 2)});

			try {
				descriptors.Find (null, false);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
			}

			try {
				descriptors.Find (null, true);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
			}

			descriptors = PropertyDescriptorCollection.Empty;

			try {
				descriptors.Find (null, false);
				Assert.Fail ("#C1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
			}

			try {
				descriptors.Find (null, true);
				Assert.Fail ("#D1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNotNull (ex.ParamName, "#D5");
			}
		}

		[Test]
		public void IList ()
		{
			IList list = ((IList) new PropertyDescriptorCollection (null));

			Assert.AreEqual (0, list.Count, "#1");
			Assert.IsFalse (list.IsFixedSize, "#2");
			Assert.IsFalse (list.IsReadOnly, "#3");
			Assert.IsFalse (list.IsSynchronized, "#4");
			Assert.IsNull (list.SyncRoot, "#5");
		}

		[Test]
		public void IList_Add_Null ()
		{
			IList list = ((IList) new PropertyDescriptorCollection (null));
			Assert.AreEqual (0, list.Count, "#1");
			list.Add (null);
			Assert.AreEqual (1, list.Count, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void IList_Add_NoPropertyDescriptor ()
		{
			IList list = ((IList) new PropertyDescriptorCollection (null));
			list.Add (5);
		}

		[Test]
		public void IDictionary ()
		{
			IDictionary dictionary = ((IDictionary) new PropertyDescriptorCollection (null));

			Assert.AreEqual (0, dictionary.Count, "#1");
			Assert.IsFalse (dictionary.IsFixedSize, "#2");
			Assert.IsFalse (dictionary.IsReadOnly, "#3");
			Assert.IsFalse (dictionary.IsSynchronized, "#4");
			Assert.IsNull (dictionary.SyncRoot, "#5");
		}


		[Test]
		[ExpectedException (typeof(ArgumentException))]
		public void IDictionary_Add_Null ()
		{
			IDictionary dictionary = ((IDictionary) new PropertyDescriptorCollection (null));
			dictionary.Add ("whatever", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IDictionary_Add_NoPropertyDescriptor ()
		{
			IDictionary dictionary = ((IDictionary) new PropertyDescriptorCollection (null));
			dictionary.Add ("whatever", 5);
		}

		[Test] // this [String]
		public void Indexer2 ()
		{
			PropertyDescriptor descA = new MockPropertyDescriptor ("hehe_\u0061\u030a", 2);
			PropertyDescriptor descB = new MockPropertyDescriptor ("heh_\u00e5", 3);
			PropertyDescriptor descC = new MockPropertyDescriptor ("Foo", 5);
			PropertyDescriptor descD = new MockPropertyDescriptor ("FOo", 6);
			PropertyDescriptor descE = new MockPropertyDescriptor ("Aim", 1);
			PropertyDescriptor descF = new MockPropertyDescriptor ("Bar", 4);

			PropertyDescriptorCollection col = new PropertyDescriptorCollection (
				new PropertyDescriptor [] { descA, descB, descC, descD, descE, descF });

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
		}

		[Test]
		public void Indexer2_Name_Null ()
		{
			PropertyDescriptorCollection descriptors;

			descriptors = new PropertyDescriptorCollection (
				new PropertyDescriptor [] { new MockPropertyDescriptor ("A", 1),
					new MockPropertyDescriptor ("b", 2)});

			try {
				PropertyDescriptor desc = descriptors [(string) null];
				Assert.Fail ("#A1:" + desc);
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
			}

			descriptors = PropertyDescriptorCollection.Empty;

			try {
				PropertyDescriptor desc = descriptors [(string) null];
				Assert.Fail ("#B1:" + desc);
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
			}
		}

		public void ReadOnly ()
		{
			PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(null, true);
			AssertReadOnly (descriptors, "ReadOnly");
		}

		[Test] // Sort ()
#if WASM
		[Ignore ("WASM CompareInfo - https://github.com/mono/mono/issues/17837")]
#endif
		public void Sort1 ()
		{
			PropertyDescriptorCollection descriptors;
			PropertyDescriptorCollection sorted;

			PropertyDescriptor descA = new MockPropertyDescriptor("Foo", 2);
			PropertyDescriptor descB = new MockPropertyDescriptor ("Aim", 3);
			PropertyDescriptor descC = new MockPropertyDescriptor ("Bim", 1);
			PropertyDescriptor descD = new MockPropertyDescriptor("AIm", 5);
			PropertyDescriptor descE = new MockPropertyDescriptor("Boo", 4);
			PropertyDescriptor descF = new MockPropertyDescriptor ("FOo", 6);

			PropertyDescriptor [] props = new  PropertyDescriptor [] {
				descA, descB, descC, descD, descE, descF };
			descriptors = new PropertyDescriptorCollection (props);

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
			PropertyDescriptorCollection descriptors;
			PropertyDescriptorCollection sorted;

			PropertyDescriptor descA = new MockPropertyDescriptor ("Foo", 2);
			PropertyDescriptor descB = new MockPropertyDescriptor ("Aim", 3);
			PropertyDescriptor descC = new MockPropertyDescriptor ("Bim", 1);
			PropertyDescriptor descD = new MockPropertyDescriptor ("AIm", 5);
			PropertyDescriptor descE = new MockPropertyDescriptor ("Boo", 4);
			PropertyDescriptor descF = new MockPropertyDescriptor ("FOo", 6);

			PropertyDescriptor [] props = new PropertyDescriptor [] {
				descA, descB, descC, descD, descE, descF };
			descriptors = new PropertyDescriptorCollection (props);

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
			PropertyDescriptorCollection descriptors;
			PropertyDescriptorCollection sorted;

			PropertyDescriptor descA = new MockPropertyDescriptor ("Foo", 2);
			PropertyDescriptor descB = new MockPropertyDescriptor ("Aim", 3);
			PropertyDescriptor descC = new MockPropertyDescriptor ("Bim", 1);
			PropertyDescriptor descD = new MockPropertyDescriptor ("AIm", 5);
			PropertyDescriptor descE = new MockPropertyDescriptor ("Boo", 4);
			PropertyDescriptor descF = new MockPropertyDescriptor ("FOo", 6);

			PropertyDescriptor [] props = new PropertyDescriptor [] {
				descA, descB, descC, descD, descE, descF };
			descriptors = new PropertyDescriptorCollection (props);

			Assert.AreSame (descA, descriptors [0], "#A1");
			Assert.AreSame (descB, descriptors [1], "#A2");
			Assert.AreSame (descC, descriptors [2], "#A3");
			Assert.AreSame (descD, descriptors [3], "#A4");
			Assert.AreSame (descE, descriptors [4], "#A5");
			Assert.AreSame (descF, descriptors [5], "#A6");

			sorted = descriptors.Sort (new ComparableComparer ());

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
			PropertyDescriptorCollection descriptors;
			PropertyDescriptorCollection sorted;

			PropertyDescriptor descA = new MockPropertyDescriptor ("Foo", 2);
			PropertyDescriptor descB = new MockPropertyDescriptor ("Aim", 3);
			PropertyDescriptor descC = new MockPropertyDescriptor ("Bim", 1);
			PropertyDescriptor descD = new MockPropertyDescriptor ("AIm", 5);
			PropertyDescriptor descE = new MockPropertyDescriptor ("Boo", 4);
			PropertyDescriptor descF = new MockPropertyDescriptor ("FOo", 6);

			PropertyDescriptor [] props = new PropertyDescriptor [] {
				descA, descB, descC, descD, descE, descF };
			descriptors = new PropertyDescriptorCollection (props);

			Assert.AreSame (descA, descriptors [0], "#A1");
			Assert.AreSame (descB, descriptors [1], "#A2");
			Assert.AreSame (descC, descriptors [2], "#A3");
			Assert.AreSame (descD, descriptors [3], "#A4");
			Assert.AreSame (descE, descriptors [4], "#A5");
			Assert.AreSame (descF, descriptors [5], "#A6");

			sorted = descriptors.Sort (new string [] { "B", "Foo", null, "A", "Boo" },
				new ComparableComparer ());

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

			sorted = descriptors.Sort ((string []) null, new ComparableComparer ());

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

		private void AssertReadOnly (PropertyDescriptorCollection descriptors, string testCase)
		{
			MockPropertyDescriptor mockPropertyDescr = new MockPropertyDescriptor (
				"Date", DateTime.Now);

			try {
				descriptors.Add (mockPropertyDescr);
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
				descriptors.Insert (0, mockPropertyDescr);
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
				descriptors.Remove (mockPropertyDescr);
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
				list.Add (mockPropertyDescr);
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
				list.Insert (0, mockPropertyDescr);
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
				list.Remove (mockPropertyDescr);
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
				list[0] = mockPropertyDescr;
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

			IDictionary dictionary = (IDictionary) descriptors;
			Assert.IsTrue (dictionary.IsReadOnly, testCase + "#21");
			Assert.IsTrue (dictionary.IsFixedSize, testCase + "#22");

			try {
				dictionary.Add ("test", mockPropertyDescr);
				Assert.Fail (testCase + "#23");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			// value is checked before read-only check
			try {
				dictionary.Add ("test", null);
				Assert.Fail (testCase + "#24");
			} catch (ArgumentException) {
				// read-only collection cannot be modified
			}

			try {
				dictionary.Clear ();
				Assert.Fail (testCase + "#25");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			try {
				dictionary[0] = mockPropertyDescr;
				Assert.Fail (testCase + "#26");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			// ensure read-only check if performed before value is checked
			try {
				dictionary[0] = null;
				Assert.Fail (testCase + "#27");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}
		}

		private class MockPropertyDescriptor : PropertyDescriptor
		{
			private object _value;

			public MockPropertyDescriptor (string name, object value) : base (name, null)
			{
				_value = value;
			}

			public override bool CanResetValue (object component)
			{
				return true;
			}

			public override object GetValue (object component)
			{
				return _value;
			}

			public override void ResetValue (object component)
			{
				_value = null;
			}

			public override void SetValue (object component, object value)
			{
				_value = value;
			}

			public override bool ShouldSerializeValue (object component)
			{
				return false;
			}

			public override Type ComponentType {
				get {
					if (_value != null) {
						return _value.GetType ();
					}
					return null;
				}
			}

			public override bool IsReadOnly {
				get {
					return false;
				}
			}

			public override Type PropertyType {
				get {
					return ComponentType;
				}
			}
 		}

		class ComparableComparer : IComparer
		{
			public int Compare (object x, object y)
			{
				PropertyDescriptor descX = x as PropertyDescriptor;
				PropertyDescriptor descY = y as PropertyDescriptor;

				if (descX == null && descY == null)
					return 0;
				if (descX == null)
					return -1;
				if (descY == null)
					return 1;

				IComparable compX = descX.GetValue (null) as IComparable;
				IComparable compY = descY.GetValue (null) as IComparable;

				if (compX == null && compY == null)
					return 0;
				if (compX == null)
					return -1;
				if (compY == null)
					return 1;
				return compX.CompareTo (compY);
			}
		}
	}
}
