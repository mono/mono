//------------------------------------------------------------------------------
// <copyright file="IInternalConfigHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Collections.Specialized;
using System.Configuration;
using ClassConfiguration=System.Configuration.Configuration;

//
// This file contains most of the interfaces that allow System.Web, Venus, and 
// Whitehorse to customize configuration in some way.
//
// The goal of the design of customization is to only require other MS assemblies
// to create an instance of an internal object via Activator.CreateInstance(), and then
// use these objects through *public* System.Configuration.Internal interfaces. 
// We do not want extenders to have to use reflection to call a method - it is slow,
// not typesafe, and more difficult to promote correct use of the internal object.
//
namespace System.Configuration.Internal {

    //
    // The functionality required of a configuration host.
    //
    [System.Runtime.InteropServices.ComVisible(false)]
    public interface IInternalConfigHost {
        void        Init(IInternalConfigRoot configRoot, params object[] hostInitParams);
        void        InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath, 
                        IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams);

        // To support creation of new config record - whether that path requires a configRecord.
        bool        IsConfigRecordRequired(string configPath);
        bool        IsInitDelayed(IInternalConfigRecord configRecord);
        void        RequireCompleteInit(IInternalConfigRecord configRecord);

        bool        IsSecondaryRoot(string configPath);

        // stream support
        string      GetStreamName(string configPath);
        string      GetStreamNameForConfigSource(string streamName, string configSource);
        object      GetStreamVersion(string streamName);

        // default impl treats name as a file name
        // null means stream doesn't exist for this name
        Stream      OpenStreamForRead(string streamName);                                        
        Stream      OpenStreamForRead(string streamName, bool assertPermissions);    
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId="2#")]
        Stream      OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext); 
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId="2#")]
        Stream      OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext, bool assertPermissions); 
        void        WriteCompleted(string streamName, bool success, object writeContext);        
        void        WriteCompleted(string streamName, bool success, object writeContext, bool assertPermissions);        
        void        DeleteStream(string streamName);                                             

        // ConfigurationErrorsException support
        bool        IsFile(string streamName);

        // change notification support - runtime only
        bool        SupportsChangeNotifications {get;}
        object      StartMonitoringStreamForChanges(string streamName, StreamChangeCallback callback);
        void        StopMonitoringStreamForChanges(string streamName, StreamChangeCallback callback);

        // RefreshConfig support - runtime only
        bool        SupportsRefresh {get;}

        // path support: whether we support Path attribute in location.
        bool        SupportsPath {get;}

        // location support
        bool        SupportsLocation {get;}
        bool        IsAboveApplication(string configPath);
        string      GetConfigPathFromLocationSubPath(string configPath, string locationSubPath);
        bool        IsLocationApplicable(string configPath);

        // definition support
        bool        IsDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition);
        void        VerifyDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo);

        // security support
        bool            IsTrustedConfigPath(string configPath);
        bool            IsFullTrustSectionWithoutAptcaAllowed(IInternalConfigRecord configRecord);
        void            GetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady);
        IDisposable     Impersonate();

        // prefetch support
        bool        PrefetchAll(string configPath, string streamName);  // E.g. If the config file is downloaded from HTTP, we want to prefetch everything.
        bool        PrefetchSection(string sectionGroupName, string sectionName);

        // context support
        object      CreateDeprecatedConfigContext(string configPath);
        object      CreateConfigurationContext(string configPath, string locationSubPath);

        // Encrypt/decrypt support 
        string      DecryptSection(string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection);
        string      EncryptSection(string clearTextXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection);

        // Type name support
        // E.g. to support type defined in app_code
        Type        GetConfigType(string typeName, bool throwOnError);
        string      GetConfigTypeName(Type t);

        // Remote support
        // Used by MgmtConfigurationRecord during SaveAs
        bool        IsRemote {get;}
    }
}

