using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using MonoTests.System.Web.DynamicData;
using MonoTests.ModelProviders;
using MonoTests.DataSource;

namespace MonoTests.Common
{
	public interface ITestDataContext
	{
		IList GetTableData (string tableName, DataSourceSelectArguments args, string where, ParameterCollection whereParams);
		List <DynamicDataTable> GetTables ();
	}
}
