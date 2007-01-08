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

namespace Mainsoft.Web.AspnetConfig
{
	public partial class EditUser : System.Web.UI.Page
	{
		String user_name;
		protected void Page_Load (object sender, EventArgs e)
		{
			user_name = Request.QueryString["User"];
			srch.User = user_name;
			if (!IsPostBack) {
				FillUserData (user_name);
				name_lbl.Text = user_name;
			}

			Button bt = Master.FindControl ("Back") as Button;
			if (bt != null) {
				bt.PostBackUrl = "ManageUser.aspx";
			}
		}

		protected override void OnPreRender (EventArgs e)
		{
			if (IsPostBack) {
				if (MultiView1.ActiveViewIndex == 1) {
					((Button) Master.FindControl ("Back")).Visible = false;
				}
				else {
					((Button) Master.FindControl ("Back")).Visible = true;
				}
			}
			base.OnPreRender (e);
		}

		void FillUserData (string user_name)
		{
			MembershipUser user = Membership.GetUser (user_name);
			userid_txb.Text = user.UserName;
			email_txb.Text = user.Email;
			active_chb.Checked = user.IsApproved;
			desc_txb.Text = user.Comment;
		}

		protected void roles_lst_DataBound (object sender, EventArgs e)
		{
			foreach (ListItem item in ((CheckBoxList) sender).Items) {
				item.Selected = Boolean.Parse (item.Value);
			}
		}

		protected void save_bt_Click (object sender, EventArgs e)
		{
			MembershipUser user = Membership.GetUser (user_name);
			user.IsApproved = active_chb.Checked;
			user.Email = email_txb.Text;
			user.Comment = desc_txb.Text;
			Membership.UpdateUser (user);
			MultiView1.ActiveViewIndex = 1;
		}

		protected void success_btn_Click (object sender, EventArgs e)
		{
			Server.Transfer ("ManageUser.aspx");
		}
	}
}
