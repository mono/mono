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
		
		internal static string GetClientValidatedEvent(/*Page page*/)
		{
			return "if (typeof(Page_ClientValidate) == 'function') Page_ClientValidate();";
		}
		
		internal static string GetClientValidatedPostBack(Control control)
		{
			return (" { if (typeof(Page_ClientValidate) != 'function' || Page_ClientValidate()) " +
			        control.Page.GetPostBackEventReference(control) +
			        " } " );
		}
	}
}
