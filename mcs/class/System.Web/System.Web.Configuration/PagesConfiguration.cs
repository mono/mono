// 
// System.Web.Configuration.PagesConfiguration
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System.Globalization;
using System.Text;

namespace System.Web.Configuration
{
	class PagesConfiguration
	{
		internal bool Buffer = true;
		internal string EnableSessionState = "true";
		internal bool EnableViewState = true;
		internal bool EnableViewStateMac = false;
		internal bool SmartNavigation = false;
		internal bool AutoEventWireup = true;
		internal bool ValidateRequest = true;
		internal string PageBaseType = "System.Web.UI.Page";
		internal string UserControlBaseType = "System.Web.UI.UserControl";

		internal PagesConfiguration (object p)
		{
			if (!(p is PagesConfiguration))
				return;

			PagesConfiguration parent = (PagesConfiguration) p;
			Buffer = parent.Buffer;
			EnableSessionState = parent.EnableSessionState;
			EnableViewState = parent.EnableViewState;
			EnableViewStateMac = parent.EnableViewStateMac;
			SmartNavigation = parent.SmartNavigation;
			AutoEventWireup = parent.AutoEventWireup;
			ValidateRequest = parent.ValidateRequest;
			PageBaseType = parent.PageBaseType;
			UserControlBaseType = parent.UserControlBaseType;
		}

		static public PagesConfiguration GetInstance (HttpContext context)
		{
			PagesConfiguration config;
			if (context == null)
				context = HttpContext.Context;

			try {
				config = context.GetConfig ("system.web/pages") as PagesConfiguration;
			} catch {
				return null;
			}
			return config;
		}

	}
}

