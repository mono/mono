using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using MonoTests.Common;
using MonoTests.DataSource;
using MonoTests.DataObjects;

namespace MonoTests.Common
{
	sealed class FieldTemplatePathTables
	{
		public static readonly List<FieldTemplateTestDescription> FieldTemplateReadOnlyColumns = new List<FieldTemplateTestDescription> ()
		{
			new FieldTemplateTestDescription ("Char_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Byte_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Int_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Long_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Bool_Column", "~/DynamicData/FieldTemplates/Boolean.ascx"),
			new FieldTemplateTestDescription ("String_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Float_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Single_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Double_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Decimal_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("SByte_Column"),
			new FieldTemplateTestDescription ("UInt_Column"),
			new FieldTemplateTestDescription ("ULong_Column"),
			new FieldTemplateTestDescription ("Short_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("UShort_Column"),
			new FieldTemplateTestDescription ("DateTime_Column", "~/DynamicData/FieldTemplates/DateTime.ascx"),
			new FieldTemplateTestDescription ("FooEmpty_Column"),
			new FieldTemplateTestDescription ("Object_Column"),
			new FieldTemplateTestDescription ("ByteArray_Column"),
			new FieldTemplateTestDescription ("IntArray_Column"),
			new FieldTemplateTestDescription ("StringArray_Column"),
			new FieldTemplateTestDescription ("ObjectArray_Column"),
			new FieldTemplateTestDescription ("StringList_Column"),
			new FieldTemplateTestDescription ("Dictionary_Column"),
			new FieldTemplateTestDescription ("ICollection_Column"),
			new FieldTemplateTestDescription ("IEnumerable_Column"),
			new FieldTemplateTestDescription ("ICollectionByte_Column"),
			new FieldTemplateTestDescription ("IEnumerableByte_Column"),
			new FieldTemplateTestDescription ("ByteMultiArray_Column"),
			new FieldTemplateTestDescription ("BoolArray_Column"),
			new FieldTemplateTestDescription ("MaximumLength_Column4", "~/DynamicData/FieldTemplates/Text.ascx"),
		};

		public static readonly List<FieldTemplateTestDescription> FieldTemplateEditColumns = new List<FieldTemplateTestDescription> ()
		{
			new FieldTemplateTestDescription ("Char_Column", "~/DynamicData/FieldTemplates/Text_Edit.ascx"),
			new FieldTemplateTestDescription ("Byte_Column", "~/DynamicData/FieldTemplates/Integer_Edit.ascx"),
			new FieldTemplateTestDescription ("Int_Column", "~/DynamicData/FieldTemplates/Integer_Edit.ascx"),
			new FieldTemplateTestDescription ("Long_Column", "~/DynamicData/FieldTemplates/Integer_Edit.ascx"),
			new FieldTemplateTestDescription ("Bool_Column", "~/DynamicData/FieldTemplates/Boolean_Edit.ascx"),
			new FieldTemplateTestDescription ("String_Column", "~/DynamicData/FieldTemplates/Text_Edit.ascx"),
			new FieldTemplateTestDescription ("Float_Column", "~/DynamicData/FieldTemplates/Decimal_Edit.ascx"),
			new FieldTemplateTestDescription ("Single_Column", "~/DynamicData/FieldTemplates/Decimal_Edit.ascx"),
			new FieldTemplateTestDescription ("Double_Column", "~/DynamicData/FieldTemplates/Decimal_Edit.ascx"),
			new FieldTemplateTestDescription ("Decimal_Column", "~/DynamicData/FieldTemplates/Decimal_Edit.ascx"),
			new FieldTemplateTestDescription ("SByte_Column"),
			new FieldTemplateTestDescription ("UInt_Column"),
			new FieldTemplateTestDescription ("ULong_Column"),
			new FieldTemplateTestDescription ("Short_Column", "~/DynamicData/FieldTemplates/Integer_Edit.ascx"),
			new FieldTemplateTestDescription ("UShort_Column"),
			new FieldTemplateTestDescription ("DateTime_Column", "~/DynamicData/FieldTemplates/DateTime_Edit.ascx"),
			new FieldTemplateTestDescription ("FooEmpty_Column"),
			new FieldTemplateTestDescription ("Object_Column"),
			new FieldTemplateTestDescription ("ByteArray_Column"),
			new FieldTemplateTestDescription ("IntArray_Column"),
			new FieldTemplateTestDescription ("StringArray_Column"),
			new FieldTemplateTestDescription ("ObjectArray_Column"),
			new FieldTemplateTestDescription ("StringList_Column"),
			new FieldTemplateTestDescription ("Dictionary_Column"),
			new FieldTemplateTestDescription ("ICollection_Column"),
			new FieldTemplateTestDescription ("IEnumerable_Column"),
			new FieldTemplateTestDescription ("ICollectionByte_Column"),
			new FieldTemplateTestDescription ("IEnumerableByte_Column"),
			new FieldTemplateTestDescription ("ByteMultiArray_Column"),
			new FieldTemplateTestDescription ("BoolArray_Column"),
			new FieldTemplateTestDescription ("MaximumLength_Column4", "~/DynamicData/FieldTemplates/MultilineText_Edit.ascx"),
		};

		public static readonly List<string> NonDefaultFullTypeNameTemplates = new List<string> () {
			"System.Char.ascx",
			"System.Char.ascx.cs",
			"System.Byte.ascx",
			"System.Byte.ascx.cs",
			"System.Boolean.ascx",
			"System.Boolean.ascx.cs",
			"System.Int16.ascx",
			"System.Int16.ascx.cs",
			"System.Int32.ascx",
			"System.Int32.ascx.cs",
			"System.Int64.ascx",
			"System.Int64.ascx.cs",
			"System.String.ascx",
			"System.String.ascx.cs",
			"System.UInt16.ascx",
			"System.UInt16.ascx.cs",
			"System.UInt32.ascx",
			"System.UInt32.ascx.cs",
			"System.UInt64.ascx",
			"System.UInt64.ascx.cs",
			"System.SByte.ascx",
			"System.SByte.ascx.cs",
			"System.Object.ascx",
			"System.Object.ascx.cs",
			"System.Byte[].ascx",
			"System.Byte[].ascx.cs",
			"System.Collections.Generic.List`1[System.String].ascx",
			"System.Collections.Generic.List`1[System.String].ascx.cs",
			"MonoTests.Common.FooEmpty.ascx",
			"MonoTests.Common.FooEmpty.ascx.cs",
			"System.Collections.ICollection.ascx",
			"System.Collections.ICollection.ascx.cs",
		};

		public static readonly List<FieldTemplateTestDescription> FieldTemplateNonDefaultColumns = new List<FieldTemplateTestDescription> ()
		{
			new FieldTemplateTestDescription ("Char_Column", "~/DynamicData/FieldTemplates/System.Char.ascx"),
			new FieldTemplateTestDescription ("Byte_Column", "~/DynamicData/FieldTemplates/System.Byte.ascx"),
			new FieldTemplateTestDescription ("Int_Column", "~/DynamicData/FieldTemplates/System.Int32.ascx"),
			new FieldTemplateTestDescription ("Long_Column", "~/DynamicData/FieldTemplates/System.Int64.ascx"),
			new FieldTemplateTestDescription ("Bool_Column", "~/DynamicData/FieldTemplates/System.Boolean.ascx"),
			new FieldTemplateTestDescription ("String_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Float_Column", "~/DynamicData/FieldTemplates/System.String.ascx"),
			new FieldTemplateTestDescription ("Single_Column", "~/DynamicData/FieldTemplates/System.String.ascx"),
			new FieldTemplateTestDescription ("Double_Column", "~/DynamicData/FieldTemplates/System.String.ascx"),
			new FieldTemplateTestDescription ("Decimal_Column", "~/DynamicData/FieldTemplates/System.String.ascx"),
			new FieldTemplateTestDescription ("SByte_Column", "~/DynamicData/FieldTemplates/System.SByte.ascx"),
			new FieldTemplateTestDescription ("UInt_Column", "~/DynamicData/FieldTemplates/System.UInt32.ascx"),
			new FieldTemplateTestDescription ("ULong_Column", "~/DynamicData/FieldTemplates/System.UInt64.ascx"),
			new FieldTemplateTestDescription ("Short_Column", "~/DynamicData/FieldTemplates/System.Int16.ascx"),
			new FieldTemplateTestDescription ("UShort_Column", "~/DynamicData/FieldTemplates/System.UInt16.ascx"),
			new FieldTemplateTestDescription ("DateTime_Column", "~/DynamicData/FieldTemplates/DateTime.ascx"),
			new FieldTemplateTestDescription ("FooEmpty_Column", "~/DynamicData/FieldTemplates/MonoTests.Common.FooEmpty.ascx"),
			new FieldTemplateTestDescription ("Object_Column", "~/DynamicData/FieldTemplates/System.Object.ascx"),
			new FieldTemplateTestDescription ("ByteArray_Column", "~/DynamicData/FieldTemplates/System.Byte[].ascx"),
			new FieldTemplateTestDescription ("IntArray_Column"),
			new FieldTemplateTestDescription ("StringArray_Column"),
			new FieldTemplateTestDescription ("ObjectArray_Column"),
			new FieldTemplateTestDescription ("StringList_Column"),

			// Doesn't work for some reason
			//new FieldTemplateTestDescription ("StringList_Column", "~/DynamicData/FieldTemplates/System.Collections.Generic.List`1[System.String].ascx"),
			new FieldTemplateTestDescription ("Dictionary_Column"),
			new FieldTemplateTestDescription ("ICollection_Column", "~/DynamicData/FieldTemplates/System.Collections.ICollection.ascx"),
			new FieldTemplateTestDescription ("IEnumerable_Column"),
			new FieldTemplateTestDescription ("ICollectionByte_Column"),
			new FieldTemplateTestDescription ("IEnumerableByte_Column"),
			new FieldTemplateTestDescription ("ByteMultiArray_Column"),
			new FieldTemplateTestDescription ("BoolArray_Column"),
			new FieldTemplateTestDescription ("MaximumLength_Column4", "~/DynamicData/FieldTemplates/System.String.ascx"),
		};

		public static readonly List<string> NonDefaultShortTypeNameTemplates = new List<string> () {
			"Char.ascx",
			"Char.ascx.cs",
			"Byte.ascx",
			"Byte.ascx.cs",
			"Int16.ascx",
			"Int16.ascx.cs",
			"Int32.ascx",
			"Int32.ascx.cs",
			"Int64.ascx",
			"Int64.ascx.cs",
			"String.ascx",
			"String.ascx.cs",
			"UInt16.ascx",
			"UInt16.ascx.cs",
			"UInt32.ascx",
			"UInt32.ascx.cs",
			"UInt64.ascx",
			"UInt64.ascx.cs",
			"SByte.ascx",
			"SByte.ascx.cs",
			"Object.ascx",
			"Object.ascx.cs",
			"Byte[].ascx",
			"Byte[].ascx.cs",
			"FooEmpty.ascx",
			"FooEmpty.ascx.cs",
			"ICollection.ascx",
			"ICollection.ascx.cs",
		};

		public static readonly List<FieldTemplateTestDescription> FieldTemplateNonDefaultShortColumns = new List<FieldTemplateTestDescription> ()
		{
			new FieldTemplateTestDescription ("FooEmpty_Column", "~/DynamicData/FieldTemplates/FooEmpty.ascx"),
			new FieldTemplateTestDescription ("Char_Column", "~/DynamicData/FieldTemplates/Char.ascx"),
			new FieldTemplateTestDescription ("Byte_Column", "~/DynamicData/FieldTemplates/Byte.ascx"),
			new FieldTemplateTestDescription ("Int_Column", "~/DynamicData/FieldTemplates/Int32.ascx"),
			new FieldTemplateTestDescription ("Long_Column", "~/DynamicData/FieldTemplates/Int64.ascx"),
			new FieldTemplateTestDescription ("Bool_Column", "~/DynamicData/FieldTemplates/Boolean.ascx"),
			new FieldTemplateTestDescription ("String_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Float_Column", "~/DynamicData/FieldTemplates/String.ascx"),
			new FieldTemplateTestDescription ("Single_Column", "~/DynamicData/FieldTemplates/String.ascx"),
			new FieldTemplateTestDescription ("Double_Column", "~/DynamicData/FieldTemplates/String.ascx"),
			new FieldTemplateTestDescription ("Decimal_Column", "~/DynamicData/FieldTemplates/String.ascx"),
			new FieldTemplateTestDescription ("SByte_Column", "~/DynamicData/FieldTemplates/SByte.ascx"),
			new FieldTemplateTestDescription ("UInt_Column", "~/DynamicData/FieldTemplates/UInt32.ascx"),
			new FieldTemplateTestDescription ("ULong_Column", "~/DynamicData/FieldTemplates/UInt64.ascx"),
			new FieldTemplateTestDescription ("Short_Column", "~/DynamicData/FieldTemplates/Int16.ascx"),
			new FieldTemplateTestDescription ("UShort_Column", "~/DynamicData/FieldTemplates/UInt16.ascx"),
			new FieldTemplateTestDescription ("DateTime_Column", "~/DynamicData/FieldTemplates/DateTime.ascx"),
			new FieldTemplateTestDescription ("Object_Column", "~/DynamicData/FieldTemplates/Object.ascx"),
			new FieldTemplateTestDescription ("ByteArray_Column", "~/DynamicData/FieldTemplates/Byte[].ascx"),
			new FieldTemplateTestDescription ("IntArray_Column"),
			new FieldTemplateTestDescription ("StringArray_Column"),
			new FieldTemplateTestDescription ("ObjectArray_Column"),
			new FieldTemplateTestDescription ("StringList_Column"),

			// Doesn't work for some reason
			//new FieldTemplateTestDescription ("StringList_Column", "~/DynamicData/FieldTemplates/List`1[System.String].ascx"),
			new FieldTemplateTestDescription ("Dictionary_Column"),
			new FieldTemplateTestDescription ("ICollection_Column", "~/DynamicData/FieldTemplates/ICollection.ascx"),
			new FieldTemplateTestDescription ("IEnumerable_Column"),
			new FieldTemplateTestDescription ("ICollectionByte_Column"),
			new FieldTemplateTestDescription ("IEnumerableByte_Column"),
			new FieldTemplateTestDescription ("ByteMultiArray_Column"),
			new FieldTemplateTestDescription ("BoolArray_Column"),
			new FieldTemplateTestDescription ("MaximumLength_Column4", "~/DynamicData/FieldTemplates/String.ascx"),
		};

		public static void SetUp_ShortTypeNameTemplates (object caller)
		{
			if (caller == null)
				throw new ArgumentNullException ("caller");
			Type type = caller.GetType ();
			foreach (string tname in NonDefaultShortTypeNameTemplates)
				WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault." + tname, TestsSetup.BuildPath ("DynamicData/FieldTemplates/" + tname));
		}

		public static void CleanUp_ShortTypeNameTemplates ()
		{
			string baseDir = WebTest.TestBaseDir;
			string filePath;

			foreach (string tname in NonDefaultShortTypeNameTemplates) {
				filePath = Path.Combine (baseDir, TestsSetup.BuildPath ("DynamicData/FieldTemplates/" + tname));
				try {
					if (File.Exists (filePath))
						File.Delete (filePath);
				} catch {
					// ignore
				}
			}
		}

		public static void SetUp_FullTypeNameTemplates (object caller)
		{
			if (caller == null)
				throw new ArgumentNullException ("caller");
			Type type = caller.GetType ();
			foreach (string tname in NonDefaultFullTypeNameTemplates)
				WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault." + tname, TestsSetup.BuildPath ("DynamicData/FieldTemplates/" + tname));
		}

		public static void CleanUp_FullTypeNameTemplates ()
		{
			string baseDir = WebTest.TestBaseDir;
			string filePath;

			foreach (string tname in NonDefaultFullTypeNameTemplates) {
				filePath = Path.Combine (baseDir, TestsSetup.BuildPath ("DynamicData/FieldTemplates/" + tname));
				try {
					if (File.Exists (filePath))
						File.Delete (filePath);
				} catch {
					// ignore
				}
			}
		}
	}
}
