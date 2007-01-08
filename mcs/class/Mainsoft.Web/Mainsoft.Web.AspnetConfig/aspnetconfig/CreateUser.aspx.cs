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
	public partial class CreateUser : System.Web.UI.Page
	{
		protected override void OnPreRender (EventArgs e)
		{
			roles_lst.DataBind ();
			base.OnPreRender (e);
			if (CreateUserWizard1.ActiveStepIndex != 0) {
				text_lbl.Visible = false;
				((Button) Master.FindControl ("Back")).Visible = false;
				active_chb.Enabled = false;
			}
		}

		protected void Page_Load (object sender, EventArgs e)
		{
			if (!IsPostBack) {
				roles_lst.DataValueField = "Role";
				roles_lst.DataSource = RolesDS.Select ();
			}
			CreateUserWizard1.CreatingUser += new LoginCancelEventHandler (CreateUserWizard1_CreatingUser);
			CreateUserWizard1.CreatedUser += new EventHandler (CreateUserWizard1_CreatedUser);
			Button bt = Master.FindControl ("Back") as Button;
			if (bt != null) {
				bt.PostBackUrl = "Default.aspx";
			}
		}

		public void CreateUserWizard1_CreatedUser (object sender, EventArgs e)
		{
			MembershipUser user = Membership.GetUser (((CreateUserWizard) sender).UserName);
			roles_lst.Enabled = false;
			
			int i = 0;
			while (i < roles_lst.Items.Count) {
				if (roles_lst.Items[i].Selected) {
					try {
						Roles.AddUserToRole (((CreateUserWizard) sender).UserName, roles_lst.Items[i].Text);
					}
					catch (Exception ex) {
						((CreateUserWizard) sender).UnknownErrorMessage = ex.Message;
					}
				}
				i++;
			}
		}

		public void CreateUserWizard1_CreatingUser (object sender, LoginCancelEventArgs e)
		{
			((CreateUserWizard) sender).DisableCreatedUser = !active_chb.Checked;
		}
	}
}
