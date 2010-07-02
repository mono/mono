using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
	protected void Page_Load (object sender, EventArgs e)
	{

	}

	protected string GetVersion ()
	{
		var ps = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;

		
		return ps.ControlRenderingCompatibilityVersion.ToString ();
	}

	protected string SetAndGetVersion (int major, int minor)
	{
		RenderingCompatibility = new Version (major, minor);
		return RenderingCompatibility.ToString ();
	}
}