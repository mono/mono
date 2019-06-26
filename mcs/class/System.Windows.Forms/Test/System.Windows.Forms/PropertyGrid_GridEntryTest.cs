//
// PropertyGrid_GridEntryTest.cs: Test cases for GridEntry placed in PropertyGrid.
//
// Author:
//   Nikita Voronchev (nikita.voronchev@ru.axxonsoft.com)
//
// (C) 2018 AxxonSoft (http://www.axxonsoft.com)
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class PropertyGrid_GridEntryTest : TestHelper
	{
		public void CheckGridItem(INestedObj ownerObject, string propertyName, GridItem gridItem)
		{
			var context = gridItem as ITypeDescriptorContext;
			Assert.NotNull (gridItem, "gridItem is null (propertyName={0})", propertyName);
			Assert.NotNull (context, "gridItem is not ITypeDescriptorContext (propertyName={0})", propertyName);

			Assert.AreEqual (gridItem.Label, propertyName);

			Assert.AreSame (context.Instance, ownerObject);
			Assert.AreEqual (context.PropertyDescriptor.PropertyType, ownerObject.PropertyAsINestedObj.GetType());
			Assert.AreEqual (context.PropertyDescriptor.Name, propertyName);
		}

		[Test]
		public void ITypeDescriptorContextTest()
		{
			PropertyGrid pg = new PropertyGrid ();

			var rootObj = new NestedObj0 ();
			rootObj.Property1 = new NestedObj1 ();
			rootObj.Property1.Property2 = new NestedObj2 ();
			rootObj.Property1.Property2.Property3 = new NestedObj3 ();
			pg.SelectedObject = rootObj;

			GridItem gridItem_Property1 = pg.GetRootItem ();
			INestedObj ownerOf_Property1 = rootObj;
			CheckGridItem(ownerOf_Property1, "Property1", gridItem_Property1);

			GridItem gridItem_Property2 = gridItem_Property1.GridItems["Property2"];
			INestedObj ownerOf_Property2 = rootObj.Property1;
			CheckGridItem(ownerOf_Property2, "Property2", gridItem_Property2);

			GridItem gridItem_Property3 = gridItem_Property2.GridItems["Property3"];
			INestedObj ownerOf_Property3 = rootObj.Property1.Property2;
			CheckGridItem(ownerOf_Property3, "Property3", gridItem_Property3);
		}

		[Test]
		public void CustomExpandableConverterTest()
		{
			PropertyGrid pg = new PropertyGrid ();

			var rootObj = new ConverterTestRootObject ();
			pg.SelectedObject = rootObj;

			GridItem customExpandableGridItem = pg.GetRootItem ();
			Assert.AreEqual ("CustomExpandableProperty", customExpandableGridItem.Label);

			var substitutedGridItems = customExpandableGridItem.GridItems;
			Assert.AreEqual (1, substitutedGridItems.Count);
			Assert.NotNull (substitutedGridItems["SomeProperty"]);
		}
	}

	public static class PropertyGridExtentions
	{
		// Returns non-Category root `GridItem`.
		public static GridItem GetRootItem (this PropertyGrid pg)
		{
			GridItem gridItem = pg.SelectedGridItem;
			Assert.NotNull(gridItem, "No one GridItem is Selected in the PropertyGrid");

			while (gridItem.Parent != null && gridItem.Parent.GridItemType == GridItemType.Property)
			{
				gridItem = gridItem.Parent;
			}

			return gridItem;
		}
	}

	#region Test Environment: ITypeDescriptorContextTest

	[TypeConverter (typeof (ExpandableObjectConverter))]
	public interface INestedObj
	{
		INestedObj PropertyAsINestedObj { get; }
	}

	// Root object.
	class NestedObj0 : INestedObj
	{
		public NestedObj1 Property1 { get; set; }

		[Browsable (false)]
		public INestedObj PropertyAsINestedObj { get { return Property1; } }
	}

	class NestedObj1 : INestedObj
	{
		public NestedObj2 Property2 { get; set; }

		[Browsable (false)]
		public INestedObj PropertyAsINestedObj { get { return Property2; } }
	}

	class NestedObj2 : INestedObj
	{
		public NestedObj3 Property3 { get; set; }

		[Browsable (false)]
		public INestedObj PropertyAsINestedObj { get { return Property3; } }
	}

	class NestedObj3 : INestedObj
	{
		[Browsable (false)]
		public INestedObj PropertyAsINestedObj { get { return null; } }
	}

	#endregion  // Test Environment: ITypeDescriptorContextTest

	#region Test Environment: CustomExpandableConverter

	[TypeConverter (typeof (ExpandableObjectConverter))]
	public class ConverterTestRootObject
	{
		public ConverterTestPropertiesHolder propertiesHolder = new ConverterTestPropertiesHolder();

		[TypeConverter (typeof (CustomExpandableConverter))]
		public string CustomExpandableProperty { get; set; }

		public ConverterTestRootObject()
		{
			CustomExpandableProperty = String.Empty;
		}
	}

	public class ConverterTestPropertiesHolder
	{
		public string SomeProperty { get; set; }
	}

	public class CustomExpandableConverter : TypeConverter
	{
		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			ConverterTestRootObject testObject = context.Instance as ConverterTestRootObject;
			return TypeDescriptor.GetProperties (testObject.propertiesHolder);
		}
	}

	#endregion  // Test Environment: CustomExpandableConverter
}
