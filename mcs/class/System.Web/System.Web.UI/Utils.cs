/**
 * Namespace: System.Web.UI
 * Class:     Utils
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Implementation: yes
 * Contact: <gvaish@iitk.ac.in>
 * Status:  ?%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Reflection;

namespace System.Web.UI
{
	private class Utils
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
	}
}
