/**

 * Namespace: System.Web.Util
 * Class:     IISVersionInfo
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%

 *

 * (C) Gaurav Vaish (2001)

 */


using System;
using System.Diagnostics;
using System.Web;

namespace System.Web.Util
{
	//FIXME: This is highly Windows/IIS specific code. What about Apache related stuff?
	internal class IISVersionInfo
	{
		private static string isapiVersion;
		private static string mscoreeVersion;
		private static string systemWebVersion;

		private static readonly object lockObj = null;

		public IISVersionInfo()
		{
		}
		
		internal static string IsapiVersion
		{
			get
			{
				if(isapiVersion==null)
				{
					lock(lockObj)
					{
						// Recheck - another thread may have set the value
						// before entering lock / exiting previous lock
						if(isapiVersion==null)
						{
							//FIXME: What about Apache? What dll/shared-object to be loaded?
							isapiVersion = GetLoadedModuleVersion("aspnet_isapi.dll");
						}
					}
				}
				return isapiVersion;
			}
		}
		
		internal static string ClrVersion
		{
			get
			{
				if(mscoreeVersion==null)
				{
					lock(lockObj)
					{
						if(mscoreeVersion==null)
						{
							mscoreeVersion = GetLoadedModuleVersion("mscorlib.dll");
						}
					}
				}
				return mscoreeVersion;
			}
		}
		
		internal static string SystemWebVersion
		{
			get
			{
				if(systemWebVersion == null)
				{
					lock(lockObj)
					{
						if(systemWebVersion==null)
						{
							systemWebVersion = (FileVersionInfo.GetVersionInfo((typeof(HttpRuntime)).Module.FullyQualifiedName)).FileVersion;
						}
					}
				}
				return systemWebVersion;
			}
		}
				
		[MonoTODO]
		internal static string GetLoadedModuleVersion(string modulename)
		{
			//TODO: Load the version information from the module
			// Needs native calls - which ones, since the module will not be .Net aware
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		internal static string GetLoadedModuleFilename(string modulename)
		{
			throw new NotImplementedException();
		}
	}
}
