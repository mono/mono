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
using System.Security;
using System.Security.Permissions;
using System.Web.Caching;
using System.Web.UI;
using System.Web.Utils;

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
		
		private Cache cache;
		
		// Security permission helper objects
		private static IStackWalk appPathDiscoveryStackWalk;
		private static IStackWalk ctrlPrincipalStackWalk;
		private static IStackWalk sensitiveInfoStackWalk;
		private static IStackWalk unmgdCodeStackWalk;
		private static IStackWalk unrestrictedStackWalk;
		private static IStackWalk reflectionStackWalk;
				
		private string appDomainAppPath;
		private string appDomainAppVirtualPath;
		
		private FileChangesMonitor fcm;
		
		private Exception initErrorException;
		
		static HttpRuntime()
		{
			autogenKeys = new byte[88];
			initialized = false;
			isapiLoaded = false;
			
			appPathDiscoveryStackWalk = null;
			ctrlPrincipalStackWalk    = null;
			sensitiveInfoStackWalk    = null;
			unmgdCodeStackWalk        = null;
			unrestrictedStackWalk     = null;
			
			if(!DesignTimeParseData.InDesigner)
				Initialize();
			runtime     = new HttpRuntime();
			if(!DesignTimeParseData.InDesigner)
				runtime.Init();
		}
		
		//FIXME: IIS specific code. Need information on what to do for Apache?
		internal static void Initialize()
		{
			if(!initialized)
			{
				bool moduleObtained = false;
				string file = IISVersionInfo.GetLoadedModuleFilename("aspnet_isapi.dll");
				string dir = null;
				string version = null;
				if(file!=null)
				{
					dir = file.Substring(0, file.LastIndexOf('\\'));
					moduleObtained = true;
				}
				if(dir!=null && dir.Length > 0)
				{
					try
					{
						version = IISVersionInfo.SystemWebVersion;
						/* TODO: I need the code to read registry
						 * I need LOCAL_MACHINE\Software\Micorosoft\ASP.NET\<version>
						*/
					} catch(Exception e)
					{
						dir = null;
					}
				}
				if(dir==null || dir.Length == 0)
				{
					string modulefile = (typeof(HttpRuntime)).Module.FullyQualifiedName;
					dir = modulefile.Substring(0, modulefile.LastIndexOf('\\'));
				}
				if(!moduleObtained)
				{
					//TODO: Now what? Module still not obtained
					// Try loading using native calls. something like LoadLibrary(...) in *java*
					// LoadLibrary(dir+"\\aspnet_asp.dll)
				}
				if(moduleObtained)
				{
					//TODO: Initialize the library
					// InitIsapiLibrary();
				}
				installDir  = dir;
				isapiLoaded = moduleObtained;
				initialized = true;
			}
		}
		
		private void Init()
		{
			initErrorException = null;
			try
			{
				//FIXME: OS Check?
				if(false)
					throw new PlatformNotSupportedException();
				//I am here <gvaish>
			} catch(Exception e)
			{
				initErrorException = e;
			}
		}

		internal static string FormatResourceString(string key, string arg0, string type)
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
		
		public static Cache Cache
		{
			get
			{
				return runtime.cache;
			}
		}
		
		public static string AppDomainAppId
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public static string AppDomainAppPath
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public static string AppDomainAppVirtualPath
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public static string AppDomainId
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public static string AspInstallDirectory
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public static string BinDirectory
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public static string ClrInstallDirectory
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public static string CodegenDir
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public static bool IsOnUNCShare
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public static string MachineConfigurationDirectory
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public static void Close()
		{
			throw new NotImplementedException();
		}
		
		internal static IStackWalk AppPathDiscovery
		{
			get
			{
				if(appPathDiscoveryStackWalk == null)
				{
					appPathDiscoveryStackWalk = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, runtime.appDomainAppPath);
				}
				return appPathDiscoveryStackWalk;
			}
		}
		
		internal static IStackWalk ControlPrincipal
		{
			get
			{
				if(ctrlPrincipalStackWalk == null)
				{
					ctrlPrincipalStackWalk = new SecurityPermission(SecurityPermissionFlag.ControlPrincipal);
				}
				return ctrlPrincipalStackWalk;
			}
		}
		
		internal static IStackWalk Reflection
		{
			get
			{
				if(reflectionStackWalk == null)
				{
					reflectionStackWalk = new ReflectionPermission(ReflectionPermissionFlag.TypeInformation | ReflectionPermissionFlag.MemberAccess);
				}
				return reflectionStackWalk;
			}
		}
		
		internal static IStackWalk SensitiveInformation
		{
			get
			{
				if(sensitiveInfoStackWalk == null)
				{
					sensitiveInfoStackWalk = new EnvironmentPermission(PermissionState.Unrestricted);
				}
				return sensitiveInfoStackWalk;
			}
		}
		
		internal static IStackWalk UnmanagedCode
		{
			get
			{
				if(unmgdCodeStackWalk == null)
				{
					unmgdCodeStackWalk = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
				}
				return unmgdCodeStackWalk;
			}
		}
		
		internal static IStackWalk Unrestricted
		{
			get
			{
				if(unrestrictedStackWalk == null)
				{
					unrestrictedStackWalk = new PermissionSet(PermissionState.Unrestricted);
				}
				return unrestrictedStackWalk;
			}
		}
				
		internal static IStackWalk FileReadAccess(string file)
		{
			return new FileIOPermission(FileIOPermissionAccess.Read, file);
		}
		
		internal static IStackWalk PathDiscoveryAccess(string path)
		{
			return new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path);
		}
	}
}
