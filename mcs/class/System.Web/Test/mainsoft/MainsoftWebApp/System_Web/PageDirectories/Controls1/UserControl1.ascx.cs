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
	public partial class UserControl1 : System.Web.UI.UserControl
	{
		protected void Page_Load (object sender, EventArgs e)
		{
			Controls.Add(new LiteralControl (UrlTestUtils.FixUrlForDirectoriesTest (ResolveUrl ("UserPage.aspx"))));
			Controls.Add (new LiteralControl ("<br>"));

			Controls.Add(new LiteralControl (UrlTestUtils.FixUrlForDirectoriesTest (ResolveUrl ("/UserPage.aspx"))));
			Controls.Add (new LiteralControl ("<br>"));

			Controls.Add(new LiteralControl (UrlTestUtils.FixUrlForDirectoriesTest (ResolveUrl ("../UserPage.aspx"))));
			Controls.Add (new LiteralControl ("<br>"));

			Controls.Add(new LiteralControl (UrlTestUtils.FixUrlForDirectoriesTest (ResolveUrl ("~/UserPage.aspx"))));
			Controls.Add (new LiteralControl ("<br>"));

			Controls.Add(new LiteralControl (UrlTestUtils.FixUrlForDirectoriesTest (TemplateSourceDirectory)));
			Controls.Add (new LiteralControl ("<br>"));

			Controls.Add(new LiteralControl (UrlTestUtils.FixUrlForDirectoriesTest (AppRelativeTemplateSourceDirectory)));
			Controls.Add (new LiteralControl ("<br>"));

			Controls.Add(new LiteralControl (UrlTestUtils.FixUrlForDirectoriesTest (AppRelativeVirtualPath)));
			Controls.Add (new LiteralControl ("<br>"));
		}
	}
}