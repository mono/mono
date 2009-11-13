#if !NET_2_0
#if TARGET_JVM
#define BUG_78521_FIXED
#endif

using System;
using System.Web.UI;
using MonoTests.SystemWeb.Framework;

public class MyPage : System.Web.UI.Page
{
	//FIXME: mono defines its own constructor here
#if BUG_78521_FIXED
	public MyPage ()
#else
	protected override void OnInit (EventArgs e)
#endif
	{
		WebTest t = WebTest.CurrentTest;
		if (t != null)
			t.Invoke (this);
	}
		
	public override void VerifyRenderingInServerForm (Control c)
	{

	}
}
#endif
