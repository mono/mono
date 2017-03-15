//
// System.Configuration.Internal.IInternalConfigHost.cs
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


using System;
using System.IO;
using System.Security;

namespace System.Configuration.Internal
{
	[System.Runtime.InteropServices.ComVisible (false)]
	public interface IInternalConfigHost
	{
		object CreateConfigurationContext (string configPath, string locationSubPath);
		object CreateDeprecatedConfigContext (string configPath);
		string DecryptSection (string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection);
		void DeleteStream (string streamName);
		string EncryptSection (string clearTextXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection);
		string GetConfigPathFromLocationSubPath (string configPath, string locationSubPath);
		Type GetConfigType (string typeName, bool throwOnError);
		string GetConfigTypeName (Type t);
		void GetRestrictedPermissions (IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady);
		string GetStreamName (string configPath);
		string GetStreamNameForConfigSource (string streamName, string configSource);
		object GetStreamVersion (string streamName);
		IDisposable Impersonate ();
		void Init (IInternalConfigRoot configRoot, params object[] hostInitParams);
		void InitForConfiguration (ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams);
		bool IsAboveApplication (string configPath);
		bool IsConfigRecordRequired (string configPath);
		bool IsDefinitionAllowed (string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition);
		bool IsFile (string streamName);
		bool IsFullTrustSectionWithoutAptcaAllowed (IInternalConfigRecord configRecord);
		bool IsInitDelayed (IInternalConfigRecord configRecord);
		bool IsLocationApplicable (string configPath);
		bool IsRemote { get; }
		bool IsSecondaryRoot (string configPath);
		bool IsTrustedConfigPath (string configPath);
		Stream OpenStreamForRead (string streamName);
		Stream OpenStreamForRead (string streamName, bool assertPermissions);
		Stream OpenStreamForWrite (string streamName, string templateStreamName, ref object writeContext);
		Stream OpenStreamForWrite (string streamName, string templateStreamName, ref object writeContext, bool assertPermissions);
		bool PrefetchAll (string configPath, string streamName);
		bool PrefetchSection (string sectionGroupName, string sectionName);
		void RequireCompleteInit (IInternalConfigRecord configRecord);
		object StartMonitoringStreamForChanges (string streamName, StreamChangeCallback callback);
		void StopMonitoringStreamForChanges (string streamName, StreamChangeCallback callback);
		void VerifyDefinitionAllowed (string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo);
		void WriteCompleted (string streamName, bool success, object writeContext);
		void WriteCompleted (string streamName, bool success, object writeContext, bool assertPermissions);
		
		bool SupportsChangeNotifications { get; }
		bool SupportsLocation { get; }
		bool SupportsPath { get; }
		bool SupportsRefresh { get; }
	}
}

