using System;
using System.Web.UI;
using MonoTests.SystemWeb.Framework;

public partial class MyPage : System.Web.UI.Page
{
	//FIXME: mono defines its own constructor here
	protected override void OnPreInit (EventArgs e)
	{
		WebTest t = WebTest.CurrentTest;
		if (t != null)
			t.Invoke (this);
	}
		
	public override void VerifyRenderingInServerForm (Control c)
	{

	}
}
