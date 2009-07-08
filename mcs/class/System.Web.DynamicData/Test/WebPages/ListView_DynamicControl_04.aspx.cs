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

public partial class ListView_DynamicControl_04 : TestsBasePage <EmployeesDataContext>
{
	protected override void PopulateDataSource (DynamicDataSource ds)
	{
		var container = ds.DataContainerInstance as TestDataContainer<EmployeesDataContext>;
		if (container == null)
			return;

		List<BazDataTypeDefaultTypes> defaultDataTypes = container.ContainedTypeInstance.DefaultDataTypes;
		defaultDataTypes.Add (new BazDataTypeDefaultTypes (true));
	}

	protected void Page_Init (object sender, EventArgs e)
	{
		InitializeDataSource (DynamicDataSource4, "BazDataTypeDefaultTypesTable");
		DynamicDataManager4.RegisterControl (ListView4);
	}

	protected void Page_Load (object sender, EventArgs e)
	{

	}

	protected void ListView4_ItemCommand (object sender, ListViewCommandEventArgs e)
	{
	}
}
