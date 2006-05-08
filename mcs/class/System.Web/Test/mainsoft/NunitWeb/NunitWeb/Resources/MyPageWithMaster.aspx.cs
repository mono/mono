using System;
using NunitWeb;
using System.Runtime.Remoting.Messaging;

public partial class MyPageWithMaster : System.Web.UI.Page
{
	protected void Page_Load (object sender, EventArgs e)
	{
		NunitWeb.MyHost.RunDelegate (Context, this);
	}
}
