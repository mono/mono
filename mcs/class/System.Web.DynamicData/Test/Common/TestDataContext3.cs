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
	public class TestDataContext3 : ITestDataContext
	{
		List<AssociatedFoo> associatedFoo;
		List<AssociatedBar> associatedBar;
		List<BazWithDataTypeAttribute> bazWithDataTypeAttribute;

		public List<AssociatedFoo> AssociatedFoo
		{
			get
			{
				if (associatedFoo == null)
					associatedFoo = new List<AssociatedFoo> ();

				return associatedFoo;
			}
		}

		public List<AssociatedBar> AssociatedBar
		{
			get
			{
				if (associatedBar == null)
					associatedBar = new List<AssociatedBar> ();

				return associatedBar;
			}
		}

		public List<BazWithDataTypeAttribute> BazWithDataTypeAttribute
		{
			get
			{
				if (bazWithDataTypeAttribute == null)
					bazWithDataTypeAttribute = new List<BazWithDataTypeAttribute> ();

				return bazWithDataTypeAttribute;
			}
		}

		#region ITestDataContext Members
		public IList GetTableData (string tableName, DataSourceSelectArguments args, string where, ParameterCollection whereParams)
		{
			if (String.Compare (tableName, "AssociatedFooTable", StringComparison.OrdinalIgnoreCase) == 0)
				return AssociatedFoo;

			if (String.Compare (tableName, "AssociatedBarTable", StringComparison.OrdinalIgnoreCase) == 0)
				return AssociatedBar;

			if (String.Compare (tableName, "BazWithDataTypeAttributeTable", StringComparison.OrdinalIgnoreCase) == 0)
				return BazWithDataTypeAttribute;

			return null;
		}

		public List<DynamicDataTable> GetTables ()
		{
			return new List<DynamicDataTable> {
				new TestDataTable<AssociatedBar>(),
				new TestDataTable<AssociatedFoo>(),
				new TestDataTable<BazWithDataTypeAttribute> ()
			};
		}

		#endregion
	}
}

