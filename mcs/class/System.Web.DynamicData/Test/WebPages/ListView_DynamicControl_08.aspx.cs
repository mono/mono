using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MonoTests.DataObjects;
using MonoTests.Common;
using MonoTests.SystemWeb.Framework;
using MonoTests.DataSource;

public partial class ListView_DynamicControl_08 : TestsBasePage <EmployeesDataContext>
{
	protected override void PopulateDataSource (DynamicDataSource ds)
	{
		var container = ds.DataContainerInstance as TestDataContainer<EmployeesDataContext>;
		if (container == null)
			return;

		List<Employee> employees = container.ContainedTypeInstance.Employees;
		employees.Add (new Employee {
			FirstName = "Marek",
			LastName = "Habersack"
		});
	}

	protected void Page_Init (object sender, EventArgs e)
	{
		InitializeDataSource (DynamicDataSource1, "EmployeeTable");
		DynamicDataManager1.RegisterControl (ListView1);
	}

	protected void Page_Load (object sender, EventArgs e)
	{

	}
}
