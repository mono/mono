using System;
using MonoTests.SystemWeb.Framework;

public partial class MetaColumn_RequiredField : System.Web.UI.Page
{
    protected override void OnPreInit (EventArgs e)
	{
		WebTest t = WebTest.CurrentTest;
		if (t != null)
			t.Invoke (this);
	}
}
