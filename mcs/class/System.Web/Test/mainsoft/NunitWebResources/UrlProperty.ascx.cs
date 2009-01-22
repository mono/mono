using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

[Themeable(true)]
public partial class UrlPropertyControl : System.Web.UI.UserControl
{
	string property1;
	string urlProperty2;

	[Themeable(true)]
	public string Property1
	{
		get { return property1; }
		set { property1 = value; }
	}

	[Themeable(true)]
	[UrlProperty]
	public string UrlProperty2
	{
		get { return urlProperty2; }
		set { urlProperty2 = value; }
	}

	protected override void OnLoad (EventArgs e)
	{
		Property1Literal.Text = Property1;
		UrlProperty2Literal.Text = UrlProperty2;
	}
}
