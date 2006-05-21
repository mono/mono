using System;
using System.Web.UI;

public partial class MyPage : System.Web.UI.Page
{
	protected void Page_Load (object sender, EventArgs e)
	{
		NunitWeb.MyHost.RunDelegate (Context, this);
	}
	public override void VerifyRenderingInServerForm (Control c)
	{

	}
}
