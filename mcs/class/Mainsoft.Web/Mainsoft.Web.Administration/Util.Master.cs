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

namespace Mainsoft.Web.Administration
{
	public partial class Util : System.Web.UI.MasterPage
	{
		public String Backurl
		{
			get { return ViewState["BackUrl"] == null ? String.Empty : (string) ViewState["BackUrl"]; }
			set { ViewState["BackUrl"] = value; }
		}

		protected void Page_Load (object sender, EventArgs e)
		{
			if (!IsPostBack) {
				if (HttpContext.Current.Request.UrlReferrer != null) {
					Backurl = HttpContext.Current.Request.UrlReferrer.ToString ();
				}
			}

			if (HttpContext.Current.Request.Url.ToString ().IndexOf ("Default.aspx") == -1) {
				Back.Enabled = true;
			}
		}

		protected void Button1_Click (object sender, EventArgs e)
		{
			Response.Redirect (Backurl);
		}
	}
}
