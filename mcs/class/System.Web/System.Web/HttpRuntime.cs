/**
 * Namespace: System.Web.UI.Utils
 * Class:     HttpRuntime
 * 
 * Author:  Gaurav Vaish
 * Contact: <gvaish@iitk.ac.in>
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web.UI;

namespace System.Web
{
	public sealed class HttpRuntime
	{
		internal static byte[]  autogenKeys;

		private static bool        initialized;
		private static string      installDir;
		private static HttpRuntime runtime;
		
		/// <summary>
		/// Loading of ISAPI
		/// </summary>
		private static bool        isapiLoaded;
		
		static HttpRuntime()
		{
			autogenKeys = new byte[88];
			initialized = false;
			isapiLoaded = false;
			if(!DesignTimeParseData.InDesigner)
				Initialize();
			runtime     = new HttpRuntime();
			if(!DesignerTimeParseData.InDesigner)
				runtime.Init();
		}
		
		internal static void Initialize()
		{
			throw new NotImplementedException();
		}
		
		private void Init()
		{
			throw new NotImplementedException();
		}
	}
}
