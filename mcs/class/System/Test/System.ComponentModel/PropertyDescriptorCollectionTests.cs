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

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class PropertyDescriptorCollectionTests
	{
		[Test]
#if TARGET_JVM
		[Ignore ("TD BUG ID: 7229")]
#endif		
		public void Empty ()
		{
			PropertyDescriptorCollection descriptors = PropertyDescriptorCollection.Empty;
			AssertReadOnly (descriptors, "Empty");
		}

		[Test]
		public void Find ()
		{
			PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection (
				new PropertyDescriptor[] { new MockPropertyDescriptor("A", 1), 
					new MockPropertyDescriptor("b", 2)});

			Assert.IsNotNull (descriptors.Find ("A", false), "#1");
			Assert.IsNotNull (descriptors.Find ("b", false), "#2");
			Assert.IsNull (descriptors.Find ("a", false), "#3");
			Assert.IsNotNull (descriptors.Find ("a", true), "#4");
		}

		[Test]
		[ExpectedException (typeof(ArgumentNullException))]
		public void Find_NullKey ()
		{
			PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection (
				new PropertyDescriptor[] { new MockPropertyDescriptor("A", 1), 
					new MockPropertyDescriptor("b", 2)});
			descriptors.Find (null, false);
		}

		[Test]
		public void IList ()
		{
			IList list = ((IList) new PropertyDescriptorCollection (null));

			Assert.AreEqual (0, list.Count, "#1");
#if NET_2_0
			Assert.IsFalse (list.IsFixedSize, "#2");
#else
			Assert.IsTrue (list.IsFixedSize, "#2");
#endif
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
#if NET_2_0
			Assert.IsFalse (dictionary.IsFixedSize, "#2");
#else
			Assert.IsTrue (dictionary.IsFixedSize, "#2");
#endif
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

#if NET_2_0
		public void ReadOnly ()
		{
			PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(null, true);
			AssertReadOnly (descriptors, "ReadOnly");
		}
#endif

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
#if NET_2_0
			Assert.IsTrue (((IList) descriptors).IsFixedSize, testCase + "#10");
#else
			Assert.IsFalse (((IList) descriptors).IsFixedSize, testCase + "#10");
#endif

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
#if NET_2_0
			Assert.IsTrue (dictionary.IsFixedSize, testCase + "#22");
#else
			Assert.IsFalse (dictionary.IsFixedSize, testCase + "#22");
#endif

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
	}
}

