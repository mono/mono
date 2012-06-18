//
// MetaColumnTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell Inc. http://novell.com
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;
using System.Web.Routing;

using NUnit.Framework;
using NUnit.Mocks;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using MonoTests.Common;
using MonoTests.ModelProviders;

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;

namespace MonoTests.System.Web.DynamicData
{
	// IMPORTANT
	//
	// ALL tests which make use of RouteTable.Routes _MUST_ clear the collection before running
	//
	[TestFixture]
	public class MetaColumnTest
	{
		DynamicDataContainerModelProvider <TestDataContext> dynamicModelProvider;
		DynamicDataContainerModelProvider<TestDataContext2> dynamicModelProviderNoScaffold;

		[TestFixtureSetUp]
		public void SetUp ()
		{
			dynamicModelProvider = new DynamicDataContainerModelProvider <TestDataContext> ();
			Utils.RegisterContext (dynamicModelProvider, new ContextConfiguration () { ScaffoldAllTables = true });

			dynamicModelProviderNoScaffold = new DynamicDataContainerModelProvider <TestDataContext2> ();
			Utils.RegisterContext (dynamicModelProviderNoScaffold, new ContextConfiguration () { ScaffoldAllTables = false }, false);
		}

		[Test]
		public void ApplyFormattingInEditMode ()
		{
			MetaModel m = Utils.CommonInitialize ();
			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("ColumnNoAttributes");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (false, mc.ApplyFormatInEditMode, "#A1-1");

			mc = t.GetColumn ("ColumnFormatInEditMode");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual (true, mc.ApplyFormatInEditMode, "#A2-1");

			t = m.Tables[TestDataContext.TableFooWithMetadataType];
			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#C1");
			Assert.AreEqual (true, mc.ApplyFormatInEditMode, "#C1-1");

			mc = t.GetColumn ("Column2");
			Assert.IsNotNull (mc, "#C2");
			Assert.AreEqual (true, mc.ApplyFormatInEditMode, "#C2-1");
		}

		[Test]
		public void Attributes ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("ColumnNoAttributes");
			Assert.IsNotNull (mc, "#A1");
			Assert.IsNotNull (mc.Attributes, "#A2");
			Assert.AreEqual (6, mc.Attributes.Count, "#A3");
			Assert.IsTrue (mc.IsRequired, "#A3-1");

			mc = t.GetColumn ("ColumnFormatInEditMode");
			Assert.IsNotNull (mc, "#A4");
			Assert.IsNotNull (mc.Attributes, "#A4-1");
			Assert.AreEqual (7, mc.Attributes.Count, "#A4-2");
			Assert.AreEqual (1, mc.Attributes.OfType <DisplayFormatAttribute> ().Count (), "#A4-3");

			mc = t.GetColumn ("ColumnWithDataType");
			Assert.IsNotNull (mc, "#A5");
			Assert.IsNotNull (mc.Attributes, "#A5-1");
			Assert.AreEqual (7, mc.Attributes.Count, "#A5-2");
			Assert.AreEqual (1, mc.Attributes.OfType<DataTypeAttribute> ().Count (), "#A5-3");

			t = m.Tables[TestDataContext.TableFooWithMetadataType];
			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#B1");
			Assert.IsNotNull (mc.Attributes, "#B1-1");
			Assert.AreEqual (9, mc.Attributes.Count, "#B1-2");
			Assert.AreEqual (1, mc.Attributes.OfType<DisplayFormatAttribute> ().Count (), "#B1-3");

			mc = t.GetColumn ("Column2");
			Assert.IsNotNull (mc, "#B2");
			Assert.IsNotNull (mc.Attributes, "#B2-1");
			Assert.AreEqual (8, mc.Attributes.Count, "#B2-2");
			Assert.AreEqual (1, mc.Attributes.OfType<DataTypeAttribute> ().Count (), "#B2-3");
		}

		[Test]
		public void ColumnType ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("ColumnNoAttributes");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (typeof (string), mc.ColumnType, "#A3");

			mc = t.GetColumn ("ColumnFormatInEditMode");
			Assert.IsNotNull (mc, "#A4");
			Assert.IsNotNull (mc.Attributes, "#A4-1");
			Assert.AreEqual (typeof (string), mc.ColumnType, "#A4-2");

			mc = t.GetColumn ("ColumnWithDataType");
			Assert.IsNotNull (mc, "#A5");
			Assert.IsNotNull (mc.Attributes, "#A5-1");
			Assert.AreEqual (typeof (string), mc.ColumnType, "#A5-2");

			t = m.Tables[TestDataContext.TableFooWithMetadataType];
			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#B1");
			Assert.IsNotNull (mc.Attributes, "#B1-1");
			Assert.AreEqual (typeof (string), mc.ColumnType, "#B1-2");

			mc = t.GetColumn ("Column2");
			Assert.IsNotNull (mc, "#B2");
			Assert.IsNotNull (mc.Attributes, "#B2-1");
			Assert.AreEqual (typeof (string), mc.ColumnType, "#B2-2");

			mc = t.GetColumn ("Column3");
			Assert.IsNotNull (mc, "#C2");
			Assert.IsNotNull (mc.Attributes, "#C2-1");
			Assert.AreEqual (typeof (string), mc.ColumnType, "#C2-2");
		}

		[Test]
		public void ConvertEmptyStringToNull ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("ColumnNoAttributes");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (true, mc.ConvertEmptyStringToNull, "#A1-1");

			mc = t.GetColumn ("ColumnFormatInEditMode");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual (true, mc.ConvertEmptyStringToNull, "#A2-1");

			t = m.Tables[TestDataContext.TableFooWithMetadataType];
			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#C1");
			Assert.AreEqual (true, mc.ConvertEmptyStringToNull, "#C1-1");

			mc = t.GetColumn ("Column2");
			Assert.IsNotNull (mc, "#C2");
			Assert.AreEqual (true, mc.ConvertEmptyStringToNull, "#C2-1");
		}

		[Test]
		public void DataFormatString ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("ColumnNoAttributes");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (String.Empty, mc.DataFormatString, "#A1-1");

			mc = t.GetColumn ("ColumnFormatInEditMode");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual ("Item: {0}", mc.DataFormatString, "#A2-1");

			t = m.Tables[TestDataContext.TableFooWithMetadataType];
			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#C1");
			Assert.AreEqual ("Item: {0}", mc.DataFormatString, "#C1-1");

			mc = t.GetColumn ("Column2");
			Assert.IsNotNull (mc, "#C2");
			Assert.AreEqual ("{0:t}", mc.DataFormatString, "#C2-1");
		}

		[Test]
		public void DataTypeAttribute ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("ColumnNoAttributes");

			Assert.IsNotNull (mc, "#A1");
			Assert.IsNotNull (mc.DataTypeAttribute, "#A1-1");

			mc = t.GetColumn ("ColumnWithDataType");
			Assert.IsNotNull (mc, "#A2");
			Assert.IsNotNull (mc.DataTypeAttribute, "#A2-1");
			Assert.AreEqual (DataType.EmailAddress, mc.DataTypeAttribute.DataType, "#A2-2");

			t = m.Tables[TestDataContext.TableFooWithMetadataType];
			mc = t.GetColumn ("Column2");
			Assert.IsNotNull (mc, "#C1");
			Assert.IsNotNull (mc.DataTypeAttribute, "#C1-1");
			Assert.AreEqual (DataType.Time, mc.DataTypeAttribute.DataType, "#C1-2");

			mc = t.GetColumn ("Column3");
			Assert.IsNotNull (mc, "#C2");
			Assert.IsNotNull (mc.DataTypeAttribute, "#C2-1");
			Assert.AreEqual (DataType.Currency, mc.DataTypeAttribute.DataType, "#C2-2");

			// Check default types for columns not decorated with the attribute
			t = m.Tables[TestDataContext.TableBazDataTypeDefaultTypes];
			mc = t.GetColumn ("Char_Column");
			Assert.IsNotNull (mc, "#D1");
			Assert.IsNull (mc.DataTypeAttribute, "#D1-1");

			mc = t.GetColumn ("Byte_Column");
			Assert.IsNotNull (mc, "#D2");
			Assert.IsNull (mc.DataTypeAttribute, "#D2-1");

			mc = t.GetColumn ("Int_Column");
			Assert.IsNotNull (mc, "#D3");
			Assert.IsNull (mc.DataTypeAttribute, "#D3-1");

			mc = t.GetColumn ("Long_Column");
			Assert.IsNotNull (mc, "#D4");
			Assert.IsNull (mc.DataTypeAttribute, "#D4-1");

			mc = t.GetColumn ("Bool_Column");
			Assert.IsNotNull (mc, "#D5");
			Assert.IsNull (mc.DataTypeAttribute, "#D5-1");

			mc = t.GetColumn ("String_Column");
			Assert.IsNotNull (mc, "#D6");
			Assert.IsNotNull (mc.DataTypeAttribute, "#D6-1");
			Assert.AreEqual (DataType.Text, mc.DataTypeAttribute.DataType, "#D6-2");

			mc = t.GetColumn ("DateTime_Column");
			Assert.IsNotNull (mc, "#D7");
			Assert.IsNull (mc.DataTypeAttribute, "#D7-1");

			mc = t.GetColumn ("FooEmpty_Column");
			Assert.IsNotNull (mc, "#D7");
			Assert.IsNull (mc.DataTypeAttribute, "#D7-1");
		}

		[Test]
		public void DefaultValue ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("ColumnFormatInEditMode");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (null, mc.DefaultValue, "#A1-1");

			mc = t.GetColumn ("ColumnWithDefaultStringValue");
			Assert.IsNotNull (mc, "#A2");
			Assert.IsNotNull (mc.DefaultValue, "#A2-1");
			Assert.AreEqual (typeof (string), mc.DefaultValue.GetType (), "#A2-2");
			Assert.AreEqual ("Value", mc.DefaultValue, "#A2-3");

			mc = t.GetColumn ("ColumnWithDefaultLongValue");
			Assert.IsNotNull (mc, "#A3");
			Assert.IsNotNull (mc.DefaultValue, "#A3-1");
			Assert.AreEqual (typeof (long), mc.DefaultValue.GetType (), "#A3-2");
			Assert.AreEqual (12345, mc.DefaultValue, "#A3-3");

			t = m.Tables[TestDataContext.TableFooWithMetadataType];
			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#B1");
			Assert.IsNotNull (mc.DefaultValue, "#B1-1");
			Assert.AreEqual (typeof (string), mc.DefaultValue.GetType (), "#B1-2");
			Assert.AreEqual ("Value", mc.DefaultValue, "#B1-3");

			mc = t.GetColumn ("Column2");
			Assert.IsNotNull (mc, "#B2");
			Assert.IsNotNull (mc.DefaultValue, "#B2-1");
			Assert.AreEqual (typeof (string), mc.DefaultValue.GetType (), "#B2-2");
			Assert.AreEqual ("Value", mc.DefaultValue, "#B2-3");

			mc = t.GetColumn ("Column3");
			Assert.IsNotNull (mc, "#B3");
			Assert.IsNotNull (mc.DefaultValue, "#B3-1");
			Assert.AreEqual (typeof (int), mc.DefaultValue.GetType (), "#B3-2");
			Assert.AreEqual (123, mc.DefaultValue, "#B3-3");
		}

		[Test]
		public void Description ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("ColumnWithDescription");
			Assert.IsNotNull (mc, "#A1");
			Assert.IsNotNull (mc.Description, "#A1-1");
			Assert.AreEqual ("Description", mc.Description, "#A1-2");

			t = m.Tables[TestDataContext.TableFooWithMetadataType];
			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#B1");
			Assert.IsNotNull (mc.Description, "#B1-1");
			Assert.AreEqual ("Description", mc.Description, "#B1-2");

			mc = t.GetColumn ("Column3");
			Assert.IsNotNull (mc, "#B2");
			Assert.IsNotNull (mc.Description, "#B2-1");
			Assert.AreEqual ("Description", mc.Description, "#B2-2");
		}

		[Test]
		public void DisplayName ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("ColumnWithDisplayName");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual ("Display Name", mc.DisplayName, "#A1-1");

			mc = t.GetColumn ("ColumnNoAttributes");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual ("ColumnNoAttributes", mc.DisplayName, "#A2-1");

			t = m.Tables[TestDataContext.TableFooWithMetadataType];
			mc = t.GetColumn ("Column3");
			Assert.IsNotNull (mc, "#B1");
			Assert.AreEqual ("Column three", mc.DisplayName, "#B1-1");

			mc = t.GetColumn ("Column4");
			Assert.IsNotNull (mc, "#B2");
			Assert.AreEqual ("Column four", mc.DisplayName, "#B2-1");
		}

		[Test]
		public void EntityTypeProperty ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableFooWithMetadataType];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.IsNotNull (mc.EntityTypeProperty, "#A2");
			Assert.AreEqual ("Column1", mc.EntityTypeProperty.Name, "#A3");
			Assert.AreEqual (mc.Provider.EntityTypeProperty, mc.EntityTypeProperty, "#A4");
		}

		[Test]
		public void HtmlEncode ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableFooWithMetadataType];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (true, mc.HtmlEncode, "#A1-1");

			mc = t.GetColumn ("Column2");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual (true, mc.HtmlEncode, "#A2-1");

			mc = t.GetColumn ("Column3");
			Assert.IsNotNull (mc, "#A3");
			Assert.AreEqual (true, mc.HtmlEncode, "#A3-1");

			mc = t.GetColumn ("Column4");
			Assert.IsNotNull (mc, "#A4");
			Assert.AreEqual (true, mc.HtmlEncode, "#A4-1");
		}

		[Test]
		public void IsBinaryData ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazDataTypeDefaultTypes];
			MetaColumn mc = t.GetColumn ("Char_Column");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (false, mc.IsBinaryData, "#A2");

			mc = t.GetColumn ("Byte_Column");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual (false, mc.IsBinaryData, "#A2-1");

			mc = t.GetColumn ("Int_Column");
			Assert.IsNotNull (mc, "#A3");
			Assert.AreEqual (false, mc.IsBinaryData, "#A3-1");

			mc = t.GetColumn ("Long_Column");
			Assert.IsNotNull (mc, "#A4");
			Assert.AreEqual (false, mc.IsBinaryData, "#A4-1");

			mc = t.GetColumn ("Bool_Column");
			Assert.IsNotNull (mc, "#A5");
			Assert.AreEqual (false, mc.IsBinaryData, "#A5-1");

			mc = t.GetColumn ("String_Column");
			Assert.IsNotNull (mc, "#A6");
			Assert.AreEqual (false, mc.IsBinaryData, "#A6-1");

			mc = t.GetColumn ("DateTime_Column");
			Assert.IsNotNull (mc, "#A7");
			Assert.AreEqual (false, mc.IsBinaryData, "#A7-1");

			mc = t.GetColumn ("FooEmpty_Column");
			Assert.IsNotNull (mc, "#A8");
			Assert.AreEqual (false, mc.IsBinaryData, "#A8-1");

			mc = t.GetColumn ("Object_Column");
			Assert.IsNotNull (mc, "#A9");
			Assert.AreEqual (false, mc.IsBinaryData, "#A9-1");

			mc = t.GetColumn ("ByteArray_Column");
			Assert.IsNotNull (mc, "#A10");
			Assert.AreEqual (true, mc.IsBinaryData, "#A10-1");

			mc = t.GetColumn ("IntArray_Column");
			Assert.IsNotNull (mc, "#A11");
			Assert.AreEqual (false, mc.IsBinaryData, "#A11-1");

			mc = t.GetColumn ("StringArray_Column");
			Assert.IsNotNull (mc, "#A12");
			Assert.AreEqual (false, mc.IsBinaryData, "#A12-1");

			mc = t.GetColumn ("ObjectArray_Column");
			Assert.IsNotNull (mc, "#A13");
			Assert.AreEqual (false, mc.IsBinaryData, "#A13-1");

			mc = t.GetColumn ("StringList_Column");
			Assert.IsNotNull (mc, "#A14");
			Assert.AreEqual (false, mc.IsBinaryData, "#A14-1");

			mc = t.GetColumn ("Dictionary_Column");
			Assert.IsNotNull (mc, "#A15");
			Assert.AreEqual (false, mc.IsBinaryData, "#A15-1");

			mc = t.GetColumn ("ICollection_Column");
			Assert.IsNotNull (mc, "#A16");
			Assert.AreEqual (false, mc.IsBinaryData, "#A16-1");

			mc = t.GetColumn ("IEnumerable_Column");
			Assert.IsNotNull (mc, "#A17");
			Assert.AreEqual (false, mc.IsBinaryData, "#A17-1");

			mc = t.GetColumn ("ICollectionByte_Column");
			Assert.IsNotNull (mc, "#A18");
			Assert.AreEqual (false, mc.IsBinaryData, "#A18-1");

			mc = t.GetColumn ("IEnumerableByte_Column");
			Assert.IsNotNull (mc, "#A19");
			Assert.AreEqual (false, mc.IsBinaryData, "#A19-1");

			mc = t.GetColumn ("ByteMultiArray_Column");
			Assert.IsNotNull (mc, "#A20");
			Assert.AreEqual (false, mc.IsBinaryData, "#A20-1");

			mc = t.GetColumn ("BoolArray_Column");
			Assert.IsNotNull (mc, "#A21");
			Assert.AreEqual (false, mc.IsBinaryData, "#A21-1");
		}

		[Test]
		public void IsCustomProperty ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("CustomPropertyColumn1");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (true, mc.IsCustomProperty, "#A1-1");
			Assert.AreEqual (mc.Provider.IsCustomProperty, mc.IsCustomProperty, "#A1-2");

			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#B1");
			Assert.AreEqual (false, mc.IsCustomProperty, "#B1-1");
			Assert.AreEqual (mc.Provider.IsCustomProperty, mc.IsCustomProperty, "#B1-2");
		}

		[Test]
		public void IsFloatingPoint ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazDataTypeDefaultTypes];
			MetaColumn mc = t.GetColumn ("Int_Column");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (false, mc.IsFloatingPoint, "#A1-1");

			mc = t.GetColumn ("Long_Column");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual (false, mc.IsFloatingPoint, "#A2-1");

			mc = t.GetColumn ("Bool_Column");
			Assert.IsNotNull (mc, "#A3");
			Assert.AreEqual (false, mc.IsFloatingPoint, "#A3-1");

			mc = t.GetColumn ("Single_Column");
			Assert.IsNotNull (mc, "#A4");
			Assert.AreEqual (true, mc.IsFloatingPoint, "#A4-1");

			mc = t.GetColumn ("Float_Column");
			Assert.IsNotNull (mc, "#A5");
			Assert.AreEqual (true, mc.IsFloatingPoint, "#A5-1");

			mc = t.GetColumn ("Double_Column");
			Assert.IsNotNull (mc, "#A6");
			Assert.AreEqual (true, mc.IsFloatingPoint, "#A6-1");

			mc = t.GetColumn ("Decimal_Column");
			Assert.IsNotNull (mc, "#A7");
			Assert.AreEqual (true, mc.IsFloatingPoint, "#A7-1");
		}

		[Test]
		public void IsForeignKeyComponent ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (false, mc.IsForeignKeyComponent, "#A1-1");

			t = m.Tables[TestDataContext.TableAssociatedFoo];
			mc = t.GetColumn ("ForeignKeyColumn1");
			Assert.IsNotNull (mc, "#B1");
			Assert.AreEqual (true, mc.IsForeignKeyComponent, "#B1-1");

			mc = t.GetColumn ("PrimaryKeyColumn1");
			Assert.IsNotNull (mc, "#B2");
			Assert.AreEqual (true, mc.IsForeignKeyComponent, "#B2-1");

			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#B3");
			Assert.AreEqual (false, mc.IsForeignKeyComponent, "#B3-1");
		}

		[Test]
		public void IsGenerated ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (false, mc.IsGenerated, "#A1-1");

			mc = t.GetColumn ("GeneratedColumn1");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual (true, mc.IsGenerated, "#A2-1");
		}

		[Test]
		public void IsInteger ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazDataTypeDefaultTypes];
			MetaColumn mc = t.GetColumn ("Char_Column");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (false, mc.IsInteger, "#A1-1");

			mc = t.GetColumn ("Byte_Column");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual (true, mc.IsInteger, "#A2-1");

			mc = t.GetColumn ("SByte_Column");
			Assert.IsNotNull (mc, "#A3");
			Assert.AreEqual (false, mc.IsInteger, "#A3-1");

			mc = t.GetColumn ("SByte_Column");
			Assert.IsNotNull (mc, "#A3");
			Assert.AreEqual (false, mc.IsInteger, "#A3-1");

			mc = t.GetColumn ("Int_Column");
			Assert.IsNotNull (mc, "#A4");
			Assert.AreEqual (true, mc.IsInteger, "#A4-1");

			mc = t.GetColumn ("UInt_Column");
			Assert.IsNotNull (mc, "#A5");
			Assert.AreEqual (false, mc.IsInteger, "#A5-1");

			mc = t.GetColumn ("Long_Column");
			Assert.IsNotNull (mc, "#A6");
			Assert.AreEqual (true, mc.IsInteger, "#A6-1");

			mc = t.GetColumn ("ULong_Column");
			Assert.IsNotNull (mc, "#A7");
			Assert.AreEqual (false, mc.IsInteger, "#A7-1");

			mc = t.GetColumn ("Short_Column");
			Assert.IsNotNull (mc, "#A8");
			Assert.AreEqual (true, mc.IsInteger, "#A8-1");

			mc = t.GetColumn ("UShort_Column");
			Assert.IsNotNull (mc, "#A9");
			Assert.AreEqual (false, mc.IsInteger, "#A9-1");

			mc = t.GetColumn ("DateTime_Column");
			Assert.IsNotNull (mc, "#A10");
			Assert.AreEqual (false, mc.IsInteger, "#A10-1");

			mc = t.GetColumn ("Float_Column");
			Assert.IsNotNull (mc, "#A11");
			Assert.AreEqual (false, mc.IsInteger, "#A11-1");
		}

		[Test]
		public void IsLongString ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazDataTypeDefaultTypes];
			MetaColumn mc = t.GetColumn ("String_Column");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (true, mc.IsString, "#A1-1");
			Assert.AreEqual (false, mc.IsLongString, "#A1-2");
			Assert.AreEqual (0, mc.MaxLength, "#A1-3");

			mc = t.GetColumn ("MaximumLength_Column1");
			Assert.IsNotNull (mc, "#B1");
			Assert.AreEqual (true, mc.IsString, "#B1-1");
			Assert.AreEqual (true, mc.IsLongString, "#B1-2");
			Assert.AreEqual (Int32.MaxValue, mc.MaxLength, "#B1-3");

			// It appears .NET allows for negative maximum lengths...
			mc = t.GetColumn ("MaximumLength_Column2");
			Assert.IsNotNull (mc, "#C1");
			Assert.AreEqual (true, mc.IsString, "#C1-1");
			Assert.AreEqual (false, mc.IsLongString, "#C1-2");
			Assert.AreEqual (Int32.MinValue, mc.MaxLength, "#C1-3");

			// This is the highest length at which string is considered to be short
			mc = t.GetColumn ("MaximumLength_Column3");
			Assert.IsNotNull (mc, "#D1");
			Assert.AreEqual (true, mc.IsString, "#D1-1");
			Assert.AreEqual (false, mc.IsLongString, "#D1-2");
			Assert.AreEqual ((Int32.MaxValue / 2) - 5, mc.MaxLength, "#D1-3");

			// This is the lowest length at which string is considered to be short
			mc = t.GetColumn ("MaximumLength_Column4");
			Assert.IsNotNull (mc, "#E1");
			Assert.AreEqual (true, mc.IsString, "#E1-1");
			Assert.AreEqual (true, mc.IsLongString, "#E1-2");
			Assert.AreEqual ((Int32.MaxValue / 2) - 4, mc.MaxLength, "#E1-3");
		}

		[Test]
		public void IsPrimaryKey ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (false, mc.IsPrimaryKey, "#A1-1");
			Assert.AreEqual (mc.Provider.IsPrimaryKey, mc.IsPrimaryKey, "#A1-2");

			mc = t.GetColumn ("PrimaryKeyColumn1");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual (true, mc.IsPrimaryKey, "#A2-1");
			Assert.AreEqual (mc.Provider.IsPrimaryKey, mc.IsPrimaryKey, "#A2-2");

			t = m.Tables[TestDataContext.TableAssociatedFoo];
			mc = t.GetColumn ("ForeignKeyColumn1");
			Assert.IsNotNull (mc, "#B1");
			Assert.AreEqual (false, mc.IsPrimaryKey, "#B1-1");
			Assert.AreEqual (mc.Provider.IsPrimaryKey, mc.IsPrimaryKey, "#B1-2");

			mc = t.GetColumn ("PrimaryKeyColumn1");
			Assert.IsNotNull (mc, "#B2");
			Assert.AreEqual (true, mc.IsPrimaryKey, "#B2-1");
			Assert.AreEqual (mc.Provider.IsPrimaryKey, mc.IsPrimaryKey, "#B2-2");
		}

		[Test]
		public void IsReadOnly ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("ReadOnlyColumn");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (true, mc.IsReadOnly, "#A1-1");

			// Apparently it value passed to ReadOnlyAttribute's constructor doesn't matter.
			// The only presence of it marks the column as read-only
			mc = t.GetColumn ("ReadWriteColumn");
			Assert.IsNotNull (mc, "#B1");
			Assert.AreEqual (true, mc.IsReadOnly, "#B1-1");

			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#C1");
			Assert.AreEqual (false, mc.IsReadOnly, "#C1-1");
		}

		[Test]
		public void IsRequired ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableFooWithMetadataType];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.IsTrue (mc.IsRequired, "#A1-1");
			Assert.AreEqual (1, mc.Attributes.OfType<RequiredAttribute> ().Count (), "#A1-2");

			mc = t.GetColumn ("Column5");
			Assert.IsNotNull (mc, "#B1");
			Assert.IsTrue (mc.IsRequired, "#B1-1");
			Assert.AreEqual (1, mc.Attributes.OfType<RequiredAttribute> ().Count (), "#B1-2");

			// This one is made nullable by the test column provider
			mc = t.GetColumn ("Column6");
			Assert.IsNotNull (mc, "#C1");
			Assert.IsFalse (mc.IsRequired, "#C1-1");
			Assert.AreEqual (0, mc.Attributes.OfType<RequiredAttribute> ().Count (), "#C1-2");

			mc = t.GetColumn ("Column7");
			Assert.IsNotNull (mc, "#D1");
			Assert.IsTrue (mc.IsRequired, "#D1-1");
			Assert.AreEqual (1, mc.Attributes.OfType<RequiredAttribute> ().Count (), "#D1-2");

			mc = t.GetColumn ("Column2");
			Assert.IsNotNull (mc, "#E1");
			Assert.IsTrue (mc.IsRequired, "#F1-1");
			Assert.AreEqual (1, mc.Attributes.OfType<RequiredAttribute> ().Count (), "#E1-2");
		}

		[Test]
		public void IsString ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazDataTypeDefaultTypes];
			MetaColumn mc = t.GetColumn ("Char_Column");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (false, mc.IsString, "#A1-1");

			mc = t.GetColumn ("Int_Column");
			Assert.IsNotNull (mc, "#B1");
			Assert.AreEqual (false, mc.IsString, "#B1-1");

			mc = t.GetColumn ("String_Column");
			Assert.IsNotNull (mc, "#C1");
			Assert.AreEqual (true, mc.IsString, "#C1-1");
		}

		[Test]
		public void MaxLength ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazDataTypeDefaultTypes];
			MetaColumn mc = t.GetColumn ("Char_Column");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (0, mc.MaxLength, "#A1-1");

			mc = t.GetColumn ("String_Column");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual (0, mc.MaxLength, "#A2-1");

			mc = t.GetColumn ("MaximumLength_Column1");
			Assert.IsNotNull (mc, "#A3");
			Assert.AreEqual (Int32.MaxValue, mc.MaxLength, "#A3-1");

			mc = t.GetColumn ("MaximumLength_Column2");
			Assert.IsNotNull (mc, "#A4");
			Assert.AreEqual (Int32.MinValue, mc.MaxLength, "#A4-1");

			mc = t.GetColumn ("MaximumLength_Column3");
			Assert.IsNotNull (mc, "#A5");
			Assert.AreEqual ((Int32.MaxValue / 2) - 5, mc.MaxLength, "#A5-1");

			mc = t.GetColumn ("MaximumLength_Column4");
			Assert.IsNotNull (mc, "#A6");
			Assert.AreEqual ((Int32.MaxValue / 2) - 4, mc.MaxLength, "#A6-1");

			mc = t.GetColumn ("MaximumLength_Column5");
			Assert.IsNotNull (mc, "#A7");
			Assert.AreEqual (255, mc.MaxLength, "#A7-1");
			Assert.AreEqual (512, mc.Provider.MaxLength, "#A7-2");
			Assert.IsTrue (mc.MaxLength != mc.Provider.MaxLength, "#A7-3");
		}

		[Test]
		public void Model ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.IsTrue (mc.Model == m, "#A2");
		}

		[Test]
		public void Name ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual ("Column1", mc.Name, "#A2");
		}

		[Test]
		public void NullDisplayText ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (String.Empty, mc.NullDisplayText, "#A1-1");

			mc = t.GetColumn ("NullDisplayTextColumn");
			Assert.IsNotNull (mc, "#A2");
			Assert.AreEqual ("Text", mc.NullDisplayText, "#A2-1");
		}

		[Test]
		public void Provider ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.IsNotNull (mc.Provider, "#A2");
		}

		[Test]
		public void RequiredErrorMessage ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
//			Assert.IsFalse (String.IsNullOrEmpty (mc.RequiredErrorMessage), "#A2");

			mc = t.GetColumn ("ErrorMessageColumn1");
			Assert.IsNotNull (mc, "#B1");
			Assert.IsFalse (String.IsNullOrEmpty (mc.RequiredErrorMessage), "#B2");
			Assert.AreEqual ("Custom error message", mc.RequiredErrorMessage, "#B3");

			mc = t.GetColumn ("ErrorMessageColumn2");
			Assert.IsNotNull (mc, "#C1");
			Assert.IsFalse (String.IsNullOrEmpty (mc.RequiredErrorMessage), "#C2");

			t = m.Tables[TestDataContext.TableFooWithMetadataType];
			mc = t.GetColumn ("Column6");
			Assert.IsNotNull (mc, "#D1");
			Assert.IsFalse (mc.IsRequired, "#D1-1");
			Assert.AreEqual (0, mc.Attributes.OfType<RequiredAttribute> ().Count (), "#D1-2");
			Assert.AreEqual (String.Empty, mc.RequiredErrorMessage, "#D1-3");
		}

		[Test]
		public void Scaffold ()
		{
			//
			// ScaffoldAllTables=true is in effect for this model
			//
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazColumnAttributes];
			MetaColumn mc = t.GetColumn ("NoScaffoldColumn");
			Assert.IsNotNull (mc, "#1");
			Assert.AreEqual (false, mc.Scaffold, "#1-1");

			mc = t.GetColumn ("ScaffoldAttributeColumn");
			Assert.IsNotNull (mc, "#2");
			Assert.AreEqual (true, mc.Scaffold, "#2-1");

			mc = t.GetColumn ("ColumnNoAttributes");
			Assert.IsNotNull (mc, "#3");
			Assert.AreEqual (true, mc.Scaffold, "#3-1");
			
			// No attribute cases
			mc = t.GetColumn ("UIHintColumn");
			Assert.IsNotNull (mc, "#4");
			Assert.AreEqual (true, mc.Scaffold, "#4-1");

			t = m.Tables[TestDataContext.TableFooNoScaffold];
			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#5");
			Assert.AreEqual (true, mc.Scaffold, "#5-1");

			t = m.Tables[TestDataContext.TableBaz];
			mc = t.GetColumn ("GeneratedColumn1");
			Assert.IsNotNull (mc, "#6");
			Assert.AreEqual (false, mc.Scaffold, "#6-1");

			mc = t.GetColumn ("GeneratedColumn2");
			Assert.IsNotNull (mc, "#7");
			Assert.AreEqual (true, mc.Scaffold, "#7-1");

			mc = t.GetColumn ("CustomPropertyColumn1");
			Assert.IsNotNull (mc, "#8");
			Assert.AreEqual (false, mc.Scaffold, "#8-1");

			mc = t.GetColumn ("CustomPropertyColumn2");
			Assert.IsNotNull (mc, "#9");
			Assert.AreEqual (true, mc.Scaffold, "#9-1");

			t = m.Tables[TestDataContext.TableAssociatedFoo];
			mc = t.GetColumn ("ForeignKeyColumn1");
			Assert.IsNotNull (mc, "#10");
			Assert.AreEqual (true, mc.IsForeignKeyComponent, "#10-1");
			Assert.AreEqual (true, mc.Scaffold, "#11-2");

			m = MetaModel.GetModel (dynamicModelProviderNoScaffold.ContextType);
			t = m.Tables[TestDataContext2.TableFooBarNoScaffold];
			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#12");
			Assert.AreEqual (true, mc.Scaffold, "#12-1");

			mc = t.GetColumn ("CustomPropertyColumn1");
			Assert.IsNotNull (mc, "#13");
			Assert.AreEqual (false, mc.Scaffold, "#13-1");

			mc = t.GetColumn ("CustomPropertyColumn2");
			Assert.IsNotNull (mc, "#14");
			Assert.AreEqual (true, mc.Scaffold, "#14-1");
			
			mc = t.GetColumn ("GeneratedColumn1");
			Assert.IsNotNull (mc, "#15");
			Assert.AreEqual (false, mc.Scaffold, "#15-1");

			mc = t.GetColumn ("GeneratedColumn2");
			Assert.IsNotNull (mc, "#16");
			Assert.AreEqual (true, mc.Scaffold, "#16-1");

			mc = t.GetColumn ("ForeignKeyColumn1");
			Assert.IsNotNull (mc, "#17");
			Assert.AreEqual (true, mc.IsForeignKeyComponent, "#17-1");
			Assert.AreEqual (true, mc.Scaffold, "#17-2");
		}

		[Test]
		public void SortExpression ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (false, mc.Provider.IsSortable, "#A1-1");
			Assert.AreEqual (String.Empty, mc.SortExpression, "#A1-2");

			mc = t.GetColumn ("SortableColumn1");
			Assert.IsNotNull (mc, "#B1");
			Assert.AreEqual (true, mc.Provider.IsSortable, "#B1-1");
			Assert.AreEqual ("SortableColumn1", mc.SortExpression, "#B1-2");
		}

		[Test]
		public void Table ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#A1");
			Assert.IsTrue (mc.Table == t, "#A2");
		}

		[Test]
		public void TypeCodeTest ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBazDataTypeDefaultTypes];
			MetaColumn mc = t.GetColumn ("Char_Column");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (typeof (char), mc.ColumnType, "#A1-1");
			Assert.AreEqual (TypeCode.Object, mc.TypeCode, "#A1-2");

			mc = t.GetColumn ("String_Column");
			Assert.IsNotNull (mc, "#B1");
			Assert.AreEqual (typeof (string), mc.ColumnType, "#B1-1");
			Assert.AreEqual (TypeCode.String, mc.TypeCode, "#B1-2");

			mc = t.GetColumn ("Byte_Column");
			Assert.IsNotNull (mc, "#C1");
			Assert.AreEqual (typeof (byte), mc.ColumnType, "#C1-1");
			Assert.AreEqual (TypeCode.Byte, mc.TypeCode, "#C1-2");

			mc = t.GetColumn ("SByte_Column");
			Assert.IsNotNull (mc, "#D1");
			Assert.AreEqual (typeof (sbyte), mc.ColumnType, "#d1-1");
			Assert.AreEqual (TypeCode.Object, mc.TypeCode, "#D1-2");

			mc = t.GetColumn ("Int_Column");
			Assert.IsNotNull (mc, "#E1");
			Assert.AreEqual (typeof (int), mc.ColumnType, "#E1-1");
			Assert.AreEqual (TypeCode.Int32, mc.TypeCode, "#E1-2");

			mc = t.GetColumn ("UInt_Column");
			Assert.IsNotNull (mc, "#F1");
			Assert.AreEqual (typeof (uint), mc.ColumnType, "#F1-1");
			Assert.AreEqual (TypeCode.Object, mc.TypeCode, "#F1-2");

			mc = t.GetColumn ("Long_Column");
			Assert.IsNotNull (mc, "#G1");
			Assert.AreEqual (typeof (long), mc.ColumnType, "#G1-1");
			Assert.AreEqual (TypeCode.Int64, mc.TypeCode, "#G1-2");

			mc = t.GetColumn ("Float_Column");
			Assert.IsNotNull (mc, "#H1");
			Assert.AreEqual (typeof (float), mc.ColumnType, "#H1-1");
			Assert.AreEqual (TypeCode.Single, mc.TypeCode, "#H1-2");

			mc = t.GetColumn ("Double_Column");
			Assert.IsNotNull (mc, "#I1");
			Assert.AreEqual (typeof (double), mc.ColumnType, "#I1-1");
			Assert.AreEqual (TypeCode.Double, mc.TypeCode, "#I1-2");

			mc = t.GetColumn ("Decimal_Column");
			Assert.IsNotNull (mc, "#J1");
			Assert.AreEqual (typeof (decimal), mc.ColumnType, "#J1-1");
			Assert.AreEqual (TypeCode.Decimal, mc.TypeCode, "#J1-2");

			mc = t.GetColumn ("StringArray_Column");
			Assert.IsNotNull (mc, "#K1");
			Assert.AreEqual (typeof (string[]), mc.ColumnType, "#K1-1");
			Assert.AreEqual (TypeCode.Object, mc.TypeCode, "#K1-2");
		}

		[Test]
		public void UIHint ()
		{
			MetaModel m = Utils.CommonInitialize ();

			MetaTable t = m.Tables[TestDataContext.TableBaz];
			MetaColumn mc = t.GetColumn ("CustomPropertyColumn1");
			Assert.IsNotNull (mc, "#A1");
			Assert.AreEqual (null, mc.UIHint, "#A2");

			mc = t.GetColumn ("Column1");
			Assert.IsNotNull (mc, "#B1");
			Assert.AreEqual (null, mc.UIHint, "#B2");

			mc = t.GetColumn ("CustomPropertyColumn2");
			Assert.IsNotNull (mc, "#C1");
			Assert.AreEqual ("UI Hint", mc.UIHint, "#C2");

			mc = t.GetColumn ("EmptyHintColumn");
			Assert.IsNotNull (mc, "#D1");
			Assert.AreEqual (String.Empty, mc.UIHint, "#D2");
		}
	}
}
