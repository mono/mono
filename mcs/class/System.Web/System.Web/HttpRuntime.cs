/**
 * Namespace: System.Web
 * Class:     HttpRuntime
 * 
 * Author:  Gaurav Vaish
 * Contact: <gvaish@iitk.ac.in>
 * Status:  ?%
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
		
		private string appDomainAppVirtualPath;
		
		static HttpRuntime()
		{
			autogenKeys = new byte[88];
			initialized = false;
			isapiLoaded = false;
			if(!DesignTimeParseData.InDesigner)
				Initialize();
			runtime     = new HttpRuntime();
			if(!DesignTimeParseData.InDesigner)
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
		
		internal static string FormatResourceString(string key, string arg0)
		{
			string format = GetResourceString(key);
			if(format==null)
				return null;
			return String.Format(format, arg0);
		}
		
		internal static string FormatResourceString(string key)
		{
			return GetResourceString(key);
		}
		
		private static string GetResourceString(string key)
		{
			return runtime.GetResourceStringFromResourceManager(key);
		}
		
		private string GetResourceStringFromResourceManager(string key)
		{
			throw new NotImplementedException();
		}
		
		public static string AppDomainAppVirtualPath
		{
			get
			{
				return runtime.appDomainAppVirtualPath;
			}
		}
	}
}
