/**
 * Namespace: System.Web.UI
 * Class:     HttpRuntime
 * 
 * Author:  Gaurav Vaish
 * Contact: <gvaish@iitk.ac.in>
 * Status:  ?%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;

namespace System.Web.UI
{
	public sealed class DesignTimeParseData
	{
		private static bool inDesigner;
		
		internal static bool InDesigner
		{
			get
			{
				return inDesigner;
			}
		}
	}
}
