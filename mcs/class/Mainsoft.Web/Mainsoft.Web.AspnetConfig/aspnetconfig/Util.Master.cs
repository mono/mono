using System;
using System.Resources;
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
	public partial class Util : System.Web.UI.MasterPage
	{
		bool allowRemoteConfiguration  // Default is false
		{
			get
			{
				if (System.Configuration.ConfigurationSettings.AppSettings["allowRemoteConfiguration"] != null) {
					return bool.Parse (System.Configuration.ConfigurationSettings.AppSettings["allowRemoteConfiguration"]);
				}
				else {
					return false;
				}
			}
		}

		public String Backurl
		{
			get { return ViewState["BackUrl"] == null ? String.Empty : (string) ViewState["BackUrl"]; }
			set { ViewState["BackUrl"] = value; }
		}

		protected override void OnInit (EventArgs e)
		{
			if (!(allowRemoteConfiguration || Request.IsLocal)) {
				Server.Transfer ("~/aspnetconfig/SecurError.aspx");
			}


			base.OnInit (e);
		}

		protected void Page_Load (object sender, EventArgs e)
		{
		}
	}
}
