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
    public partial class Searcher : System.Web.UI.UserControl
    {
        public string User
        {
            get { return ViewState["User"] == null ? String.Empty : (string)ViewState["User"];}
            set { ViewState["User"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void OnPreRender(EventArgs e)
        {
            if (User == String.Empty || (Membership.GetUser(User)==null))
            {
                GridView1.DataSource = null;
            }
            else
            {
                GridView1.DataSource = RolesDS.SelectUsersRole(User);
            }
            GridView1.DataBind();
            base.OnPreRender(e);
        }

        protected void Roles_Changed(object sender, EventArgs e)
        {
            String user_name = (string)ViewState["User"];
            if (((CheckBox)sender).Checked)
            {
		    try {
			    Roles.AddUserToRole (user_name, ((CheckBox) sender).Text);
		    }
		    catch {
		    }
            }
            else
            {
		    try {
			    Roles.RemoveUserFromRole (user_name, ((CheckBox) sender).Text);
		    }
		    catch {
		    }
            }
        }
    }
}