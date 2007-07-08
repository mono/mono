using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace Walkthrough2Panels
{

    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void Timer1_Tick(object sender, EventArgs e)
        {
            Label1.Text = "UpdatePanel1 refreshed at: " +
              DateTime.Now.ToLongTimeString();
            Label2.Text = "UpdatePanel2 refreshed at: " +
              DateTime.Now.ToLongTimeString();
        }
    }
}