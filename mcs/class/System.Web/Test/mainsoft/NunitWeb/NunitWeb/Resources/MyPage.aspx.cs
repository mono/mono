using System;
using System.Web.UI;

public partial class MyPage : System.Web.UI.Page
{
	//FIXME: mono defines its own constructor here
#if BUG_78521_FIXED
	public MyPage ()
#else
	protected override void OnPreInit (EventArgs e)
#endif
	{
		NunitWeb.MyHost.InitDelegates (Context, this);
	}
		
	public override void VerifyRenderingInServerForm (Control c)
	{

	}
}
