using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class search : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
	    string searchterm = Page.RouteData.Values ["searchterm"] as string;
	    label1.Text = searchterm;
    }
}