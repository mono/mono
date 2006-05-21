using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Diagnostics;

	public partial class MyParent : System.Web.UI.UserControl
	{
		protected override void OnInit (EventArgs e)
		{
			Debug.WriteLine ("parent.OnInit");
			base.OnInit (e);
		}
	}
