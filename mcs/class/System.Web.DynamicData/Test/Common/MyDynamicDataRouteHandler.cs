using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.DynamicData;

namespace MonoTests.Common
{
	class MyDynamicDataRouteHandler : DynamicDataRouteHandler
	{
		public string DoGetCustomPageVirtualPath (MetaTable table, string viewName)
		{
			return GetCustomPageVirtualPath (table, viewName);
		}
		
		public string DoGetScaffoldPageVirtualPath (MetaTable table, string viewName)
		{
			return GetScaffoldPageVirtualPath (table, viewName);
		}

		public override IHttpHandler CreateHandler (DynamicDataRoute route, MetaTable table, string action)
		{
			return new Page () as IHttpHandler;
		}
	}
}
