//
// System.Configuration.InternalConfigurationHost.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.IO;
using System.Security;
using System.Configuration.Internal;

namespace System.Configuration
{
	abstract class InternalConfigurationHost: IInternalConfigHost
	{
		public virtual object CreateConfigurationContext (string configPath, string locationSubPath)
		{
			return null;
		}
		
		public virtual object CreateDeprecatedConfigContext (string configPath)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void DeleteStream (string streamName)
		{
			File.Delete (streamName);
		}
		
		string IInternalConfigHost.DecryptSection (string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedSection)
		{
			return protectedSection.DecryptSection (encryptedXml, protectionProvider);
		}
		
		string IInternalConfigHost.EncryptSection (string clearXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedSection)
		{
			return protectedSection.EncryptSection (clearXml, protectionProvider);
		}
		
		public virtual string GetConfigPathFromLocationSubPath (string configPath, string locationSubPath)
		{
			return configPath;
		}
		
		public virtual Type GetConfigType (string typeName, bool throwOnError)
		{
			Type type = Type.GetType (typeName);
			if (type == null && throwOnError)
				throw new ConfigurationErrorsException ("Type '" + typeName + "' not found.");
			return type;
		}
		
		public virtual string GetConfigTypeName (Type t)
		{
			return t.AssemblyQualifiedName;
		}
		
		public virtual void GetRestrictedPermissions (IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady)
		{
			throw new NotImplementedException ();
		}
		
		public abstract string GetStreamName (string configPath);
		public abstract void Init (IInternalConfigRoot root, params object[] hostInitParams);
		public abstract void InitForConfiguration (ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot root, params object[] hostInitConfigurationParams);
		
		[MonoTODO ("remote config")]
		public virtual string GetStreamNameForConfigSource (string streamName, string configSource)
		{
			throw new NotSupportedException ("mono does not support remote configuration");
		}
		
		public virtual object GetStreamVersion (string streamName)
		{
			throw new NotImplementedException ();
		}
		
		public virtual IDisposable Impersonate ()
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool IsAboveApplication (string configPath)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool IsConfigRecordRequired (string configPath)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool IsDefinitionAllowed (string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition)
		{
			switch (allowDefinition) {
				case ConfigurationAllowDefinition.MachineOnly:
					return configPath == "machine";
				case ConfigurationAllowDefinition.MachineToApplication:
					return configPath == "machine" || configPath == "exe";
				default:
					return true;
			}
		}
		
		public virtual bool IsFile (string streamName)
		{
			throw new NotImplementedException ();
		}

		public virtual bool IsFullTrustSectionWithoutAptcaAllowed (IInternalConfigRecord configRecord)
		{
			throw new NotImplementedException ();
		}

		public virtual bool IsInitDelayed (IInternalConfigRecord configRecord)
		{
			throw new NotImplementedException ();
		}

		public virtual bool IsLocationApplicable (string configPath)
		{
			throw new NotImplementedException ();
		}

		public virtual bool IsRemote {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual bool IsSecondaryRoot (string configPath)
		{
			throw new NotImplementedException ();
		}

		public virtual bool IsTrustedConfigPath (string configPath)
		{
			throw new NotImplementedException ();
		}
		
		public virtual Stream OpenStreamForRead (string streamName)
		{
#if TARGET_JVM
			if (String.CompareOrdinal (streamName, "/META-INF/machine.config") == 0)
				return (Stream) vmw.common.IOUtils.getStreamForGHConfigs (streamName);
#endif
			if (!File.Exists (streamName))
				throw new ConfigurationException ("File '" + streamName + "' not found");
				
			return new FileStream (streamName, FileMode.Open, FileAccess.Read);
		}

		public virtual Stream OpenStreamForRead (string streamName, bool assertPermissions)
		{
			throw new NotImplementedException ();
		}
		
		public virtual Stream OpenStreamForWrite (string streamName, string templateStreamName, ref object writeContext)
		{
			return new FileStream (streamName, FileMode.Create, FileAccess.Write);
		}

		public virtual Stream OpenStreamForWrite (string streamName, string templateStreamName, ref object writeContext, bool assertPermissions)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool PrefetchAll (string configPath, string streamName)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool PrefetchSection (string sectionGroupName, string sectionName)
		{
			throw new NotImplementedException ();
		}

		public virtual void RequireCompleteInit (IInternalConfigRecord configRecord)
		{
			throw new NotImplementedException ();
		}

		public virtual object StartMonitoringStreamForChanges (string streamName, StreamChangeCallback callback)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void StopMonitoringStreamForChanges (string streamName, StreamChangeCallback callback)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void VerifyDefinitionAllowed (string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo)
		{
			if (!IsDefinitionAllowed (configPath, allowDefinition, allowExeDefinition))
				throw new ConfigurationErrorsException ("The section can't be defined in this file (the allowed definition context is '" + allowDefinition + "').", errorInfo.Filename, errorInfo.LineNumber);
		}
		
		public virtual void WriteCompleted (string streamName, bool success, object writeContext)
		{
		}

		public virtual void WriteCompleted (string streamName, bool success, object writeContext, bool assertPermissions)
		{
		}
		
		public virtual bool SupportsChangeNotifications {
			get { return false; }
		}
		
		public virtual bool SupportsLocation {
			get { return false; }
		}
		
		public virtual bool SupportsPath {
			get { return false; }
		}
		
		public virtual bool SupportsRefresh {
			get { return false; }
		}
	}
	
	class ExeConfigurationHost: InternalConfigurationHost
	{
		ExeConfigurationFileMap map;
		
		public override void Init (IInternalConfigRoot root, params object[] hostInitParams)
		{
			map = (ExeConfigurationFileMap) hostInitParams [0];
		}
		
		public override string GetStreamName (string configPath)
		{
			switch (configPath) {
				case "exe": return map.ExeConfigFilename; 
				case "local": return map.LocalUserConfigFilename;
				case "roaming": return map.LocalUserConfigFilename;
				case "machine": return map.MachineConfigFilename;
				default: return map.ExeConfigFilename;
			}
		}
		
		public override void InitForConfiguration (ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot root, params object[] hostInitConfigurationParams)
		{
			map = (ExeConfigurationFileMap) hostInitConfigurationParams [0];
			configPath = null;
			string next = null;

			locationConfigPath = null;

			if ((locationSubPath == "exe" || locationSubPath == null) && map.ExeConfigFilename != null) {
				configPath = "exe";
				next = "local";
				locationConfigPath = map.ExeConfigFilename;
			}
			
			if ((locationSubPath == "local" || configPath == null) && map.LocalUserConfigFilename != null) {
				configPath = "local";
				next = "roaming";
				locationConfigPath = map.LocalUserConfigFilename;
			}
			
			if ((locationSubPath == "roaming" || configPath == null) && map.RoamingUserConfigFilename != null) {
				configPath = "roaming";
				next = "machine";
				locationConfigPath = map.RoamingUserConfigFilename;
			}
			
			if ((locationSubPath == "machine" || configPath == null) && map.MachineConfigFilename != null) {
				configPath = "machine";
				next = null;
			}

			locationSubPath = next;
		}
	}
	
	class MachineConfigurationHost: InternalConfigurationHost
	{
		ConfigurationFileMap map;
		
		public override void Init (IInternalConfigRoot root, params object[] hostInitParams)
		{
			map = (ConfigurationFileMap) hostInitParams [0];
		}
		
		public override string GetStreamName (string configPath)
		{
			return map.MachineConfigFilename;
		}
		
		public override void InitForConfiguration (ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot root, params object[] hostInitConfigurationParams)
		{
			map = (ConfigurationFileMap) hostInitConfigurationParams [0];
			locationSubPath = null;
			configPath = null;
			locationConfigPath = null;
		}

		public override bool IsDefinitionAllowed (string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition)
		{
			return true;
		}
	}
}

#endif
