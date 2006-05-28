using System;
using System.Web.UI;

public partial class MyPage : System.Web.UI.Page
{
	public MyPage ()
	{
		NunitWeb.MyHost.InitDelegates (Context, this);
	}

	public override void VerifyRenderingInServerForm (Control c)
	{

	}
}
