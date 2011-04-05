using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
	    SiteMapNode node = SiteMap.Provider.FindSiteMapNode ("~/");
	    log.InnerHtml = String.Format ("node is {0}null and it is {1}the root node", node == null ? String.Empty : "<strong>not</strong> ",
					    node == null || node != SiteMap.Provider.RootNode ? "<strong>not</strong> " : String.Empty);
    }
}