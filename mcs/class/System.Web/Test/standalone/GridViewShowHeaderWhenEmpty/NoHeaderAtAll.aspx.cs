using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class NoHeaderAtAll : System.Web.UI.Page
{
	protected void Page_Load (object sender, EventArgs e)
	{
		var data = new List<TestData> ();

		GridView1.DataSource = data;
		GridView1.DataBind ();
	}

	protected void GridView1_RowCreated (object sender, GridViewRowEventArgs args)
	{
		log.InnerText = "OnRowCreated called";
	}
}