using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

public partial class _Global : HttpApplication
{
	void Application_Start (object sender, EventArgs e)
	{
		RouteTable.Routes.MapPageRoute ("SearchRoute", "search/{searchterm}", "~/search.aspx");
		RouteTable.Routes.MapPageRoute ("UserRoute", "users/{username}", "~/users.aspx");
	}
}