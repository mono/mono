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
	public class TestDataContext4 : ITestDataContext
	{
		List<BazValidationAttributes> bazValidationAttributes;

		public List<BazValidationAttributes> BazValidationAttributes
		{
			get
			{
				if (bazValidationAttributes == null)
					bazValidationAttributes = new List<BazValidationAttributes> ();

				return bazValidationAttributes;
			}
		}

		#region ITestDataContext Members
		public IList GetTableData (string tableName, DataSourceSelectArguments args, string where, ParameterCollection whereParams)
		{
			if (String.Compare (tableName, "BazValidationAttributesTable", StringComparison.OrdinalIgnoreCase) == 0)
				return BazValidationAttributes;

			return null;
		}

		public List<DynamicDataTable> GetTables ()
		{
			return new List<DynamicDataTable> {
				new TestDataTable<BazValidationAttributes>()
			};
		}

		#endregion
	}
}

