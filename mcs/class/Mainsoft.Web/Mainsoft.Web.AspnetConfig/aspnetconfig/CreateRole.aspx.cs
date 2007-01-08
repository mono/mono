// Mainsoft.Web.AspnetConfig - Site AspnetConfig utility
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Threading;
using System.Text;

namespace Mainsoft.Web.AspnetConfig
{
	public partial class CreateRole : System.Web.UI.Page
	{
		public string Role
		{
			get { return ViewState["Role"] == null ? String.Empty : (string) ViewState["Role"]; }
			set { ViewState["Role"] = value; }
		}


		protected override void OnPreRender (EventArgs e)
		{
			if (mv.ActiveViewIndex == 1) {
				((Button) Master.FindControl ("Back")).Visible = false;
			}
			else {
				((Button) Master.FindControl ("Back")).Visible = true;
			}

			base.OnPreRender (e);
			Roles_gv.DataBind ();
		}

		protected override void OnInit (EventArgs e)
		{
			Img.ImageUrl = this.Page.ClientScript.GetWebResourceUrl (typeof (CreateRole), "Mainsoft.Web.AspnetConfig.resources.untitled.bmp");
			base.OnInit (e);
		}

		protected void Page_Load (object sender, EventArgs e)
		{
			Button bt = Master.FindControl ("Back") as Button;
			if (bt != null) {
				bt.PostBackUrl = "Default.aspx";
			}
		}

		protected void gridbtn_click (object sender, EventArgs e)
		{
			Role = ((GridButton) sender).User;
			mv.ActiveViewIndex = 1;
		}

		protected void roleName_bt_Click (object sender, EventArgs e)
		{
			if (roleName_txb.Text != "") {
				try {
					Roles.CreateRole (roleName_txb.Text);
					error_lb.Text = "";
				}
				catch (Exception ex) {
					error_lb.Text = ex.Message;
				}
				finally {
				}
			}
			else {
				error_lb.Text = "Role name cannot be empty!";
			}
		}

		protected void Click_No (object sender, EventArgs e)
		{
			mv.ActiveViewIndex = 0;
		}

		protected void Click_Yes (object sender, EventArgs e)
		{
			RolesDS.Delete (Role);
			mv.ActiveViewIndex = 0;
		}
	}
}
