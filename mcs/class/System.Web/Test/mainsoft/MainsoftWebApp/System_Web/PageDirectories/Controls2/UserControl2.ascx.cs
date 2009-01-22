using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace GHTTests.System_Web_dll.PageDirectories.Controls1
{
	public partial class UserControl2 : System.Web.UI.UserControl
	{
		protected void Page_Load (object sender, EventArgs e)
		{
			Controls.Add (LoadControl ("../Controls1/UserControl1.ascx"));
		}
	}
}