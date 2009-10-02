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

public partial class DynamicValidator_02 : TestsBasePage <TestDataContext4>
{
	protected override void PopulateDataSource (DynamicDataSource ds)
	{
		var container = ds.DataContainerInstance as TestDataContainer<TestDataContext4>;
		if (container == null)
			return;

		List<BazValidationAttributes> foo = container.ContainedTypeInstance.BazValidationAttributes;
		foo.Add (new BazValidationAttributes {
			Column1 = 0
		});
	}

	protected void Page_Init (object sender, EventArgs e)
	{
		InitializeDataSource (DynamicDataSource1, "BazValidationAttributesTable");
		DynamicDataManager1.RegisterControl (ListView1);
	}
}
