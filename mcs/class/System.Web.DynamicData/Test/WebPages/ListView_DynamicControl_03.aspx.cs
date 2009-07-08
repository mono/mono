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

public partial class ListView_DynamicControl_03 : TestsBasePage <EmployeesDataContext>
{
	protected override void PopulateDataSource (DynamicDataSource ds)
	{
		var container = ds.DataContainerInstance as TestDataContainer<EmployeesDataContext>;
		if (container == null)
			return;

		List<BazDataTypeDefaultTypes> defaultDataTypes = container.ContainedTypeInstance.DefaultDataTypes;
		defaultDataTypes.Add (new BazDataTypeDefaultTypes ());
	}

	protected void Page_Init (object sender, EventArgs e)
	{
		InitializeDataSource (DynamicDataSource3, "BazDataTypeDefaultTypesTable");
		DynamicDataManager3.RegisterControl (ListView3);
	}

	protected void Page_Load (object sender, EventArgs e)
	{

	}
}
