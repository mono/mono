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
using MonoTests.DataObjects;

namespace MonoTests.Common
{
	public class EmployeesDataContext : ITestDataContext
	{
		List<Employee> employees;
		List<SeasonalEmployee> seasonalEmployees;
		List<BazDataTypeDefaultTypes> defaultDataTypes;

		public List<Employee> Employees {
			get {
				if (employees == null)
					employees = new List<Employee> ();

				return employees;
			}
		}

		public List<SeasonalEmployee> SeasonalEmployees {
			get
			{
				if (seasonalEmployees == null)
					seasonalEmployees = new List<SeasonalEmployee> ();
				return seasonalEmployees;
			}
		}

		public List<BazDataTypeDefaultTypes> DefaultDataTypes {
			get {
				if (defaultDataTypes == null)
					defaultDataTypes = new List<BazDataTypeDefaultTypes> ();
				return defaultDataTypes;
			}
		}

		public IList GetTableData (string tableName, DataSourceSelectArguments args, string where, ParameterCollection whereParams)
		{
			if (String.Compare (tableName, "EmployeeTable", StringComparison.OrdinalIgnoreCase) == 0)
				return Employees;

			if (String.Compare (tableName, "SeasonalEmployeeTable", StringComparison.OrdinalIgnoreCase) == 0)
				return SeasonalEmployees;

			if (String.Compare (tableName, "BazDataTypeDefaultTypesTable", StringComparison.OrdinalIgnoreCase) == 0)
				return DefaultDataTypes;

			return null;
		}

		public List<DynamicDataTable> GetTables ()
		{
			var ret = new List<DynamicDataTable> ();

			ret.Add (new TestDataTable<Employee> ());
			ret.Add (new TestDataTable<SeasonalEmployee> ());
			ret.Add (new TestDataTable<BazDataTypeDefaultTypes> ());

			return ret;
		}
	}
}
