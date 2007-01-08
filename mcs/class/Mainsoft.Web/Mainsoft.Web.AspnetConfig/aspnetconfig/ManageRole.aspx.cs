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
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. -->


using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Mainsoft.Web.AspnetConfig
{
	public partial class MenageRole : System.Web.UI.Page
	{
		String role;
		protected void Page_Load (object sender, EventArgs e)
		{
			role = Request.QueryString["Role"];
			if (!Roles.RoleExists (role)) {
				Server.Transfer ("Error.aspx");
			}
			role_lbl.Text = role;
			Button bt = Master.FindControl ("Back") as Button;
			if (bt != null) {
				bt.PostBackUrl = "CreateRole.aspx";
			}
		}

		protected override void OnPreRender (EventArgs e)
		{
			Roles_gv.DataBind ();
			base.OnPreRender (e);
		}

		public void CheckBox_CheckedChanged (object sender, EventArgs e)
		{
			string user = ((GridCheckBox) sender).User;
			if (((GridCheckBox) sender).Checked) {
				try {
					Roles.AddUserToRole (user, role);
				}
				catch { 
				}
			}
			else {
				try {
					Roles.RemoveUserFromRole (user, role);
				}
				catch {
				}
			}
		}
	}
}
