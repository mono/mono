using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using MonoTests.DataSource;
using MonoTests.ModelProviders;

namespace MonoTests.Common
{
	public class TestDataContainer <TContext>: DynamicDataContainer<TContext> where TContext: ITestDataContext
	{
		public TestDataContainer ()
		{ }

		public TestDataContainer (string tableName)
		: base (tableName)
		{ }

		public override int Update (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			throw new NotImplementedException ();
		}

		public override int Insert (IDictionary values)
		{
			throw new NotImplementedException ();
		}

		public override int Delete (IDictionary keys, IDictionary oldValues)
		{
			throw new NotImplementedException ();
		}

		public override IEnumerable Select (DataSourceSelectArguments args, string where, ParameterCollection whereParams)
		{
			TContext contextInstance = ContainedTypeInstance;
			return ContainedTypeInstance.GetTableData (TableName, args, where, whereParams);
		}

		public override List<DynamicDataTable> GetTables ()
		{
			var data = Activator.CreateInstance<TContext> ();
			return data.GetTables ();
		}
	}
}
