/**
 * Namespace: System.Web.UI
 * Class:     Utils
 *
 * Author:  Gaurav Vaish
 * Maintainer-> gvaish@iitk.ac.in
 * Implementation: yes
 * Contact: <gvaish@iitk.ac.in>
 * Status:  ??%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Web;
using System.Reflection;

namespace System.Web.UI
{
	internal class Utils
	{
		internal static object InvokeMethod(MethodInfo info, object obj, object[] parameters)
		{
			object retVal = null;
			try
			{
				retVal = info.Invoke(obj, parameters);
			} catch(TargetInvocationException tie)
			{
				throw tie.InnerException;
			}
			return retVal;
		}
		
		internal static string GetClientValidatedEvent(Page page)
		{
			return "if (typeof(Page_ClientValidate) == 'function') Page_ClientValidate();";
		}
		
		internal static string GetClientValidatedPostBack(Control control)
		{
			return (" { if (typeof(Page_ClientValidate) != 'function' || Page_ClientValidate()) " +
			        control.Page.GetPostBackEventReference(control) +
			        " } " );
		}
		
		[MonoTODO]
		internal static string GetScriptLocation(HttpContext context)
		{
			IDictionary dict = context.GetConfig("system.web/webControls")
			                    as IDictionary;
			string loc = null;
			if(dict != null)
			{
				loc = dict["clientScriptsLocation"] as string;
			}
			if(loc == null)
			{
				throw new HttpException("Missing_clientScriptsLocation");
			}
			if(loc.IndexOf("{0}") > 0)
			{
				//FIXME: Version Number of the ASP.Net should come into play.
				//Like if ASP 1.0 and 1.1 both are installed, the script
				// locations are in /aspnet_client/system_web/1_0_3705_0/
				// and /aspnet_client/system_web/1_1_4322/
				// (these entries are from my machine
				// So, first I should get this Version Info from somewhere
				loc = String.Format(loc, "system_web");
			}
			return loc;
		}
	}
}
