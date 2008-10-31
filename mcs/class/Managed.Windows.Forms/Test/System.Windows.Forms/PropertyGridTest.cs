//
// PropertyGridTest.cs: Test cases for PropertyGrid.
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class PropertyGridTest : TestHelper
	{
		[Test]
		public void PropertySort_Valid ()
		{
			PropertyGrid pg;
			EventLogger eventLogger;

			pg = new PropertyGrid ();
			eventLogger = new EventLogger (pg);
			Assert.AreEqual (PropertySort.CategorizedAlphabetical, pg.PropertySort, "#A1");
			Assert.AreEqual (0, eventLogger.EventsRaised, "#A2");
			pg.PropertySort = PropertySort.Alphabetical;
			Assert.AreEqual (PropertySort.Alphabetical, pg.PropertySort, "#A3");
#if NET_2_0
			Assert.AreEqual (1, eventLogger.EventsRaised, "#A4");
			Assert.AreEqual (1, eventLogger.CountEvents ("PropertySortChanged"), "#A5");
#else
			Assert.AreEqual (0, eventLogger.EventsRaised, "#A4");
#endif
			pg.PropertySort = PropertySort.NoSort;
			Assert.AreEqual (PropertySort.NoSort, pg.PropertySort, "#A6");
#if NET_2_0
			Assert.AreEqual (2, eventLogger.EventsRaised, "#A7");
			Assert.AreEqual (2, eventLogger.CountEvents ("PropertySortChanged"), "#A8");
#else
			Assert.AreEqual (0, eventLogger.EventsRaised, "#A7");
#endif
			pg.PropertySort = PropertySort.NoSort;
			Assert.AreEqual (PropertySort.NoSort, pg.PropertySort, "#A9");
#if NET_2_0
			Assert.AreEqual (2, eventLogger.EventsRaised, "#A10");
			Assert.AreEqual (2, eventLogger.CountEvents ("PropertySortChanged"), "#A11");
#else
			Assert.AreEqual (0, eventLogger.EventsRaised, "#A10");
#endif
			pg.PropertySort = PropertySort.CategorizedAlphabetical;
			Assert.AreEqual (PropertySort.CategorizedAlphabetical, pg.PropertySort, "#A12");
#if NET_2_0
			Assert.AreEqual (3, eventLogger.EventsRaised, "#A13");
			Assert.AreEqual (3, eventLogger.CountEvents ("PropertySortChanged"), "#A14");
#else
			Assert.AreEqual (0, eventLogger.EventsRaised, "#A13");
#endif
			pg.PropertySort = PropertySort.Categorized;
			Assert.AreEqual (PropertySort.Categorized, pg.PropertySort, "#A14");
#if NET_2_0
			Assert.AreEqual (3, eventLogger.EventsRaised, "#A15");
			Assert.AreEqual (3, eventLogger.CountEvents ("PropertySortChanged"), "#A16");
#else
			Assert.AreEqual (0, eventLogger.EventsRaised, "#A17");
#endif

			pg = new PropertyGrid ();
			eventLogger = new EventLogger (pg);
			pg.SelectedObject = new Button ();
			Assert.AreEqual (PropertySort.CategorizedAlphabetical, pg.PropertySort, "#B1");
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#B2");
			pg.PropertySort = PropertySort.Alphabetical;
			Assert.AreEqual (PropertySort.Alphabetical, pg.PropertySort, "#B3");
#if NET_2_0
			Assert.AreEqual (1, eventLogger.CountEvents ("PropertySortChanged"), "#B4");
#else
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#B4");
#endif
			pg.PropertySort = PropertySort.NoSort;
			Assert.AreEqual (PropertySort.NoSort, pg.PropertySort, "#B5");
#if NET_2_0
			Assert.AreEqual (2, eventLogger.CountEvents ("PropertySortChanged"), "#B6");
#else
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#B6");
#endif
			pg.PropertySort = PropertySort.CategorizedAlphabetical;
			Assert.AreEqual (PropertySort.CategorizedAlphabetical, pg.PropertySort, "#B7");
#if NET_2_0
			Assert.AreEqual (3, eventLogger.CountEvents ("PropertySortChanged"), "#B8");
#else
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#B8");
#endif
			pg.PropertySort = PropertySort.CategorizedAlphabetical;
			Assert.AreEqual (PropertySort.CategorizedAlphabetical, pg.PropertySort, "#B9");
#if NET_2_0
			Assert.AreEqual (3, eventLogger.CountEvents ("PropertySortChanged"), "#B10");
#else
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#B10");
#endif

			pg.SelectedObject = null;
			Assert.AreEqual (PropertySort.CategorizedAlphabetical, pg.PropertySort, "#C1");
#if NET_2_0
			Assert.AreEqual (3, eventLogger.CountEvents ("PropertySortChanged"), "#C2");
#else
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#C2");
#endif
			pg.PropertySort = PropertySort.Alphabetical;
			Assert.AreEqual (PropertySort.Alphabetical, pg.PropertySort, "#C3");
#if NET_2_0
			Assert.AreEqual (4, eventLogger.CountEvents ("PropertySortChanged"), "#C4");
#else
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#C4");
#endif
			pg.PropertySort = PropertySort.NoSort;
			Assert.AreEqual (PropertySort.NoSort, pg.PropertySort, "#C5");
#if NET_2_0
			Assert.AreEqual (5, eventLogger.CountEvents ("PropertySortChanged"), "#C6");
#else
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#C6");
#endif

			pg.SelectedObject = new Button ();

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (pg);
			form.Show ();

			Assert.AreEqual (PropertySort.NoSort, pg.PropertySort, "#D1");
#if NET_2_0
			Assert.AreEqual (5, eventLogger.CountEvents ("PropertySortChanged"), "#D2");
#else
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#D2");
#endif
			pg.PropertySort = PropertySort.Alphabetical;
			Assert.AreEqual (PropertySort.Alphabetical, pg.PropertySort, "#D3");
#if NET_2_0
			Assert.AreEqual (6, eventLogger.CountEvents ("PropertySortChanged"), "#D4");
#else
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#D4");
#endif
			pg.PropertySort = PropertySort.Categorized;
			Assert.AreEqual (PropertySort.Categorized, pg.PropertySort, "#D5");
#if NET_2_0
			Assert.AreEqual (7, eventLogger.CountEvents ("PropertySortChanged"), "#D6");
#else
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#D6");
#endif
			pg.PropertySort = PropertySort.CategorizedAlphabetical;
			Assert.AreEqual (PropertySort.CategorizedAlphabetical, pg.PropertySort, "#D7");
#if NET_2_0
			Assert.AreEqual (7, eventLogger.CountEvents ("PropertySortChanged"), "#D8");
#else
			Assert.AreEqual (0, eventLogger.CountEvents ("PropertySortChanged"), "#D8");
#endif

			form.Dispose ();
		}

		[Test]
		public void PropertySort_Invalid ()
		{
			PropertyGrid pg = new PropertyGrid ();
#if NET_2_0
			EventLogger eventLogger = new EventLogger (pg);
			try {
				pg.PropertySort = (PropertySort) 666;
				Assert.Fail ("#1");
			} catch (InvalidEnumArgumentException ex) {
				// The value of argument 'value' (666) is invalid
				// for Enum type 'PropertySort'
				Assert.AreEqual (typeof (InvalidEnumArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'value'") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("(" + 666.ToString (CultureInfo.CurrentCulture) + ")") != -1, "#6");
				Assert.IsTrue (ex.Message.IndexOf ("'PropertySort'") != -1, "#7");
				Assert.IsNotNull (ex.ParamName, "#8");
				Assert.AreEqual ("value", ex.ParamName, "#9");

				Assert.AreEqual (0, eventLogger.EventsRaised, "#10");
			}
#else
			pg.PropertySort = (PropertySort) 666;
			Assert.AreEqual ((PropertySort) 666, pg.PropertySort);
#endif
		}

		[Test]
		public void SelectedObject ()
		{
			PropertyGrid pg = new PropertyGrid ();
			Button button1 = new Button ();
			Assert.IsNull (pg.SelectedObject, "#A1");
			Assert.IsNotNull (pg.SelectedObjects, "#A2");
			Assert.AreEqual (0, pg.SelectedObjects.Length, "#A3");
			pg.SelectedObject = button1;
			Assert.IsNotNull (pg.SelectedObject, "#B1");
			Assert.AreSame (button1, pg.SelectedObject, "#B2");
			Assert.IsNotNull (pg.SelectedObjects, "#B3");
			Assert.AreEqual (1, pg.SelectedObjects.Length, "#B4");
			Assert.AreSame (button1, pg.SelectedObjects [0], "#B5");
			Assert.IsNotNull (pg.SelectedGridItem, "#B6");
		}

		[Test] // bug #81796
		public void SelectedObject_NoProperties ()
		{
			PropertyGrid propertyGrid = new PropertyGrid ();
			propertyGrid.SelectedObject = new Button ();
			propertyGrid.SelectedObject = new object ();
			propertyGrid.SelectedObject = new Button ();
		}

		[Test]
		public void SelectedObject_Null ()
		{
			PropertyGrid pg = new PropertyGrid ();
			Assert.IsNull (pg.SelectedObject, "#A1");
			Assert.IsNotNull (pg.SelectedObjects, "#A2");
			Assert.AreEqual (0, pg.SelectedObjects.Length, "#A3");
			pg.SelectedObject = null;
			Assert.IsNull (pg.SelectedObject, "#B1");
			Assert.IsNotNull (pg.SelectedObjects, "#B2");
			Assert.AreEqual (0, pg.SelectedObjects.Length, "#B3");
		}

		[Test]
		public void SelectedObject_Null2 ()
		{
			PropertyGrid pg = new PropertyGrid ();
			EventLogger log = new EventLogger (pg);
			
			Assert.IsNull (pg.SelectedObject, "#A1");
			Assert.IsNotNull (pg.SelectedObjects, "#A2");
			Assert.AreEqual (0, pg.SelectedObjects.Length, "#A3");
			Assert.IsNull (pg.SelectedGridItem, "A4");
			
			pg.SelectedObject = new TextBox ();
			Assert.IsNotNull (pg.SelectedObject, "#B1");
			Assert.IsNotNull (pg.SelectedObjects, "#B2");
			Assert.AreEqual (1, pg.SelectedObjects.Length, "#B3");
			Assert.IsNotNull (pg.SelectedGridItem, "B4");
			Assert.AreEqual (1, log.EventsRaised, "B5");
			Assert.AreEqual ("SelectedObjectsChanged", log.EventsJoined (";"), "B6");

			pg.SelectedObject = null;
			Assert.IsNull (pg.SelectedObject, "#C1");
			Assert.IsNotNull (pg.SelectedObjects, "#C2");
			Assert.AreEqual (0, pg.SelectedObjects.Length, "#C3");
			Assert.IsNull (pg.SelectedGridItem, "C4");
			Assert.AreEqual (2, log.EventsRaised, "C5");
			Assert.AreEqual ("SelectedObjectsChanged;SelectedObjectsChanged", log.EventsJoined (";"), "C6");
		}

		[Test]
		public void SelectedGridItem_Null ()
		{
			PropertyGrid pg = new PropertyGrid ();
			pg.SelectedObject = new TextBox ();
			Assert.IsNotNull (pg.SelectedGridItem, "#1");
			try {
				pg.SelectedGridItem = null;
				Assert.Fail ("#2");
			} catch (ArgumentException ex) {
				// GridItem specified to PropertyGrid.SelectedGridItem must be
				// a valid GridItem
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.IsNull (ex.ParamName, "#6");
			}
		}

		[Test] // bug #79615
		public void SelectedObjects_Multiple ()
		{
			Button button1 = new Button ();
			Button button2 = new Button ();

			PropertyGrid pg = new PropertyGrid ();
			pg.SelectedObjects = new object [] { button1, button2 };
			Assert.IsNotNull (pg.SelectedObjects, "#1");
			Assert.AreEqual (2, pg.SelectedObjects.Length, "#2");
			Assert.AreSame (button1, pg.SelectedObjects [0], "#3");
			Assert.AreSame (button2, pg.SelectedObjects [1], "#4");
			Assert.IsNotNull (pg.SelectedObject, "#5");
			Assert.AreSame (button1, pg.SelectedObject, "#6");
		}

		[Test]
		public void SelectedObjects_Null ()
		{
			PropertyGrid pg = new PropertyGrid ();
			Button button1 = new Button ();
			pg.SelectedObjects = new object [] { button1 };
			Assert.IsNotNull (pg.SelectedObjects, "#A1");
			Assert.AreEqual (1, pg.SelectedObjects.Length, "#A2");
			Assert.AreSame (button1, pg.SelectedObjects [0], "#A3");
			Assert.AreSame (button1, pg.SelectedObject, "#A4");
			pg.SelectedObjects = null;
			Assert.IsNotNull (pg.SelectedObjects, "#B1");
			Assert.AreEqual (0, pg.SelectedObjects.Length, "#B2");
			Assert.IsNull (pg.SelectedObject, "#B3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SelectedObjects_Null_Item ()
		{
			PropertyGrid pg = new PropertyGrid ();
			Button button1 = new Button ();
			pg.SelectedObjects = new object [] { button1, null };
		}

		[Test]
		[Category ("NotWorking")]
		public void PropertyGrid_MergedTest ()
		{
			PropertyGrid pg = new PropertyGrid ();
			pg.SelectedObjects = new object[] { new Button (), new Label () };

			Assert.IsNotNull (pg.SelectedGridItem, "1");
			Assert.AreEqual ("Accessibility", pg.SelectedGridItem.Label, "2");
			Assert.AreEqual (GridItemType.Category, pg.SelectedGridItem.GridItemType, "3");
		}

		[Test]
		[Category ("NotWorking")]
		public void PropertyGrid_MergedRootTest ()
		{
			object[] selected_objects = new object[] { new Button (), new Label () };
			PropertyGrid pg = new PropertyGrid ();

			pg.SelectedObjects = selected_objects;

			Assert.IsNotNull (pg.SelectedGridItem.Parent, "1");
			Assert.AreEqual ("System.Object[]", pg.SelectedGridItem.Parent.Label, "2");
			Assert.AreEqual (GridItemType.Root, pg.SelectedGridItem.Parent.GridItemType, "3");

			Assert.AreEqual (selected_objects, pg.SelectedGridItem.Parent.Value, "4");

			Assert.IsNull (pg.SelectedGridItem.Parent.Parent, "5");
		}

		class ArrayTest_object
		{
			int[] array;
			public ArrayTest_object ()
			{
				array = new int[10];
				for (int i = 0; i < array.Length; i ++)
					array[i] = array.Length - i;
			}
			public int[] Array {
				get { return array; }
			}
		}

		[Test]
		public void PropertyGrid_ArrayTest ()
		{
			PropertyGrid pg = new PropertyGrid ();

			pg.SelectedObject = new ArrayTest_object ();

			// selected object
			Assert.AreEqual ("Array", pg.SelectedGridItem.Label, "1");
			Assert.IsTrue (pg.SelectedGridItem.Value is Array, "2");
			Assert.AreEqual (10, pg.SelectedGridItem.GridItems.Count, "3");
			Assert.AreEqual (GridItemType.Property, pg.SelectedGridItem.GridItemType, "4");
		}

		[Test]
		public void PropertyGrid_ArrayParentTest ()
		{
			PropertyGrid pg = new PropertyGrid ();

			pg.SelectedObject = new ArrayTest_object ();

			// parent
			Assert.IsNotNull (pg.SelectedGridItem.Parent, "1");
			Assert.AreEqual ("Misc", pg.SelectedGridItem.Parent.Label, "2");
			Assert.AreEqual (GridItemType.Category, pg.SelectedGridItem.Parent.GridItemType, "3");
			Assert.AreEqual (1, pg.SelectedGridItem.Parent.GridItems.Count, "4");
		}

		[Test]
		public void PropertyGrid_ArrayRootTest ()
		{
			ArrayTest_object obj = new ArrayTest_object ();
			PropertyGrid pg = new PropertyGrid ();

			pg.SelectedObject = obj;

			// grandparent
			Assert.IsNotNull (pg.SelectedGridItem.Parent.Parent, "1");
			Assert.AreEqual (typeof(ArrayTest_object).ToString(), pg.SelectedGridItem.Parent.Parent.Label, "2");
			Assert.AreEqual (GridItemType.Root, pg.SelectedGridItem.Parent.Parent.GridItemType, "3");
			Assert.AreEqual (1, pg.SelectedGridItem.Parent.Parent.GridItems.Count, "4");
			Assert.AreEqual (obj, pg.SelectedGridItem.Parent.Parent.Value, "5");

			Assert.IsNull (pg.SelectedGridItem.Parent.Parent.Parent, "6");
		}

		[Test]
		public void PropertyGrid_ArrayChildrenTest ()
		{
			PropertyGrid pg = new PropertyGrid ();

			pg.SelectedObject = new ArrayTest_object ();

			// children
			Assert.AreEqual ("[0]", pg.SelectedGridItem.GridItems[0].Label, "1");
			Assert.AreEqual (GridItemType.Property, pg.SelectedGridItem.GridItems[0].GridItemType, "2");
			Assert.AreEqual (10, pg.SelectedGridItem.GridItems[0].Value, "3");
			Assert.AreEqual (0, pg.SelectedGridItem.GridItems[0].GridItems.Count, "4");
		}

		[Test]
		public void PropertyGrid_ItemSelectTest ()
		{
			PropertyGrid pg = new PropertyGrid ();

			pg.SelectedObject = new ArrayTest_object ();

			// the selected grid item is the "Array" property item.
			GridItem array_item = pg.SelectedGridItem;
			GridItem misc_item = array_item.Parent;
			GridItem root_item = misc_item.Parent;

			Assert.AreEqual (array_item, pg.SelectedGridItem, "1");

			Assert.IsTrue (misc_item.Select (), "2");
			Assert.AreEqual (misc_item, pg.SelectedGridItem, "3");

			Assert.IsTrue (array_item.Select (), "4");
			Assert.AreEqual (array_item, pg.SelectedGridItem, "5");

			Assert.IsFalse (root_item.Select (), "6");
			Assert.AreEqual (array_item, pg.SelectedGridItem, "7");
		}
	}
}
