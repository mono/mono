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
	public partial class PageWControl4 : GHTBaseWeb
	{
		protected void Page_Load (object sender, EventArgs e)
		{
			Control content = this.Controls [0];
			if (content is MasterPage)
				GHTTestBegin (content);
			else {
				HtmlForm form1 = (HtmlForm) (HtmlForm) FindControl ("Form1");
				GHTTestBegin (form1);
			}

			GHTSubTestBegin ("GHTSubTest1");
			GHTActiveSubTest.Controls.Add (LoadControl ("../Controls2/UserControl2.ascx"));
			GHTSubTestEnd ();

			GHTTestEnd ();
		}
	}
}
