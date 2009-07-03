using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;
using System.Web.UI;
using System.Web.UI.WebControls;

using MonoTests.System.Web.DynamicData;
using MonoTests.ModelProviders;
using MonoTests.DataSource;

namespace MonoTests.Common
{
	class TestDataContext : ITestDataContext
	{
		public const int TableFooWithDefaults = 0;
		public const int TableFooNoPrimaryColumns = 1;
		public const int TableFooNoDefaultsWithPrimaryKey = 2;
		public const int TableFooDisplayColumnAttribute = 3;
		public const int TableFooEmpty = 4;
		public const int TableBaz = 5;
		public const int TableBazNoStrings = 6;
		public const int TableBazNoStringsNoPrimary = 7;
		public const int TableFooWithToString = 8;
		public const int TableFooInvalidDisplayColumnAttribute = 9;
		public const int TableFooEmptyDisplayColumnAttribute = 10;
		public const int TableFooSettableDefaults = 11;
		public const int TableFooDisplayName = 12;
		public const int TableFooDisplayNameEmptyName = 13;
		public const int TableBar = 14;
		public const int TableFooReadOnly = 15;
		public const int TableAssociatedFoo = 16;
		public const int TableAssociatedBar = 17;
		public const int TableFooMisnamedSortColumn = 18;
		public const int TableFooEmptySortColumn = 19;
		public const int TableFooNoScaffold = 20;
		public const int TableBazColumnAttributes = 21;
		public const int TableFooWithMetadataType = 22;
		public const int TableBazDataTypeDefaultTypes = 23;

		public FooWithDefaults FooWithDefaults { get; set; }
		public FooNoPrimaryColumns FooNoPrimaryColumns { get; set; }
		public FooNoDefaultsWithPrimaryKey FooNoDefaultsWithPrimaryKey { get; set; }
		public FooDisplayColumnAttribute FooDisplayColumnAttribute { get; set; }
		public FooEmpty FooEmpty { get; set; }
		public Baz Baz { get; set; }
		public BazNoStrings BazNoStrings { get; set; }
		public BazNoStringsNoPrimary BazNoStringsNoPrimary { get; set; }
		public FooWithToString FooWithToString { get; set; }
		public FooInvalidDisplayColumnAttribute FooInvalidDisplayColumnAttribute { get; set; }
		public FooEmptyDisplayColumnAttribute FooEmptyDisplayColumnAttribute { get; set; }
		public FooSettableDefaults FooSettableDefaults { get; set; }
		public FooDisplayName FooDisplayName { get; set; }
		public FooDisplayNameEmptyName FooDisplayNameEmptyName { get; set; }
		public Bar Bar { get; set; }
		public FooReadOnly FooReadOnly { get; set; }
		public AssociatedFoo AssociatedFoo { get; set; }
		public AssociatedBar AssociatedBar { get; set; }
		public FooMisnamedSortColumn FooMissingSortColumn { get; set; }
		public FooEmptySortColumn FooEmptySortColumn { get; set; }
		public FooNoScaffold FooNoScaffold { get; set; }
		public BazColumnAttributes BazColumnAttributes { get; set; }
		public FooWithMetadataType FooWithMetadataType { get; set; }
		public BazDataTypeDefaultTypes BazDataTypeDefaultTypes { get; set; }

		#region ITestDataContext Members

		public IList GetTableData (string tableName, DataSourceSelectArguments args, string where, ParameterCollection whereParams)
		{
			return null;
		}

		public List<DynamicDataTable> GetTables ()
		{
			var ret = new List<DynamicDataTable> ();

			ret.Add (new TestDataTable<FooWithDefaults> ());
			ret.Add (new TestDataTable<FooNoPrimaryColumns> ());
			ret.Add (new TestDataTable<FooNoDefaultsWithPrimaryKey> ());
			ret.Add (new TestDataTable<FooDisplayColumnAttribute> ());
			ret.Add (new TestDataTable<FooEmpty> ());
			ret.Add (new TestDataTable<Baz> ());
			ret.Add (new TestDataTable<BazNoStrings> ());
			ret.Add (new TestDataTable<BazNoStringsNoPrimary> ());
			ret.Add (new TestDataTable<FooWithToString> ());
			ret.Add (new TestDataTable<FooInvalidDisplayColumnAttribute> ());
			ret.Add (new TestDataTable<FooEmptyDisplayColumnAttribute> ());
			ret.Add (new TestDataTable<FooSettableDefaults> ());
			ret.Add (new TestDataTable<FooDisplayName> ());
			ret.Add (new TestDataTable<FooDisplayNameEmptyName> ());
			ret.Add (new TestDataTable<Bar> ());
			ret.Add (new TestDataTable<FooReadOnly> ());
			ret.Add (new TestDataTable<AssociatedFoo> ());
			ret.Add (new TestDataTable<AssociatedBar> ());
			ret.Add (new TestDataTable<FooMisnamedSortColumn> ());
			ret.Add (new TestDataTable<FooEmptySortColumn> ());
			ret.Add (new TestDataTable<FooNoScaffold> ());
			ret.Add (new TestDataTable<BazColumnAttributes> ());
			ret.Add (new TestDataTable<FooWithMetadataType> ());
			ret.Add (new TestDataTable<BazDataTypeDefaultTypes> ());

			return ret;
		}

		#endregion
	}
}
