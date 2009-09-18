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

public partial class ListView_DynamicControl_10 : TestsBasePage <TestDataContext3>
{
	protected override void PopulateDataSource (DynamicDataSource ds)
	{
		var container = ds.DataContainerInstance as TestDataContainer<TestDataContext3>;
		if (container == null)
			return;

		List<AssociatedFoo> foo = container.ContainedTypeInstance.AssociatedFoo;
		foo.Add (new AssociatedFoo {
			PrimaryKeyColumn1 = "Marek",
			PrimaryKeyColumn2 = 2,
			Column1 = 1
		});
	}

	protected void Page_Init (object sender, EventArgs e)
	{
		InitializeDataSource (DynamicDataSource1, "AssociatedFooTable");
		DynamicDataManager1.RegisterControl (ListView1);
	}
}
