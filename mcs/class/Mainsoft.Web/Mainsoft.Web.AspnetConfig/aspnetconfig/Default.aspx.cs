// Mainsoft.Web.AspnetConfig - Site AspnetConfig utility
// Authors:
//  Klain Yoni <yonik@mainsoft.com>
//
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
	public partial class Default : System.Web.UI.Page
	{
		private bool IsPortalRoleProvider;
		private bool IsPortalMembershipProvider;
		protected override void OnInit (EventArgs e)
		{
			try {
				IsPortalRoleProvider = (Roles.Provider).GetType ().ToString () == "Mainsoft.Web.Security.WPGroupsRoleProvider";
			}
			catch{
			}
			IsPortalMembershipProvider = (Membership.Provider).GetType().ToString () == "Mainsoft.Web.Security.WPMembershipProvider";
		}

		protected void Page_Load (object sender, EventArgs e)
		{
			Button bt = Master.FindControl ("Back") as Button;
			if (bt != null) {
				bt.Enabled = false;
			}
		}

		public string User_count
		{
			get
			{
				if (IsPortalMembershipProvider) {
					return "You cannot create or manage users when WPMembershipProvider is configured as the default provider.";
				}
				else {
					MembershipUserCollection user_collection = Membership.GetAllUsers ();
					return "Created users :" + user_collection.Count.ToString ();
				}
			}
		}

		public string Roles_count
		{
			get
			{
				if (IsPortalRoleProvider) {
					return @"You cannot create or manage roles when WPGroupsRoleProvider is configured as the default provider.";
				}
				else if (Roles.Enabled) {
					string[] list = Roles.GetAllRoles ();
					return "Existing roles :" + list.Length.ToString ();
				}
				else
					return @"In order to create or manage roles, the roleManger key must be enabled. To enable roleManager, please modify your Web.config file as follows: <br />
							&nbsp; &lt;configuration xmlns=""http://schemas.microsoft.com/.NetConfiguration/v2.0""&gt; <br />
							&nbsp;&nbsp;	&lt;system.web&gt; <br />
							&nbsp;&nbsp;&nbsp;	&lt;roleManager enabled=""true"" /&gt; <br />
                                                        &nbsp;&nbsp;    &lt/system.web&gt; <br />
							&nbsp; &lt;/configuration&gt;";
			}
		}

		protected void HyperLink1_Load (object sender, EventArgs e)
		{
			if (!Roles.Enabled || IsPortalRoleProvider)
				((HyperLink) sender).Visible = false;
		}

		protected void UsersLinks_Load (object sender, EventArgs e)
		{
			if (IsPortalMembershipProvider) {
				((HyperLink) sender).Visible = false;
			}
		}
	}
}
