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

namespace GHTTests.System_Web_dll.PageDirectories.Pages
{
	public partial class PageWControl3 : GHTBaseWeb
	{
		protected void Page_Load (object sender, EventArgs e)
		{
			HtmlForm form1 = (HtmlForm) FindControl ("Form1");
			GHTTestBegin (form1);

			GHTSubTestBegin ("GHTSubTest1");
			GHTActiveSubTest.Controls.Add (LoadControl ("../Controls2/UserControl2.ascx"));
			GHTSubTestEnd ();

			GHTTestEnd ();
		}
	}
}
