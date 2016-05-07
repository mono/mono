//------------------------------------------------------------------------------
// <copyright file="RemoteWebConfigurationHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Collections;
    using System.Configuration;
    using System.Configuration.Internal;
    using System.Web;
    using System.Web.Util;
    using System.Security;
    using System.IO;
    using System.Web.Hosting;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Security.Principal;
    using System.Threading;
    using System.Globalization;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
    internal sealed class RemoteWebConfigurationHost : DelegatingConfigHost
    {
        private const string        KEY_MACHINE = "MACHINE";

        private static object       s_version = new object();

        private string              _Server;
        private string              _Username;
        private string              _Domain;
        private string              _Password;
        private WindowsIdentity     _Identity;

        private Hashtable           _PathMap;   // configPath -> configFile
        private string              _ConfigPath;

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal RemoteWebConfigurationHost() {}

        public override void Init(IInternalConfigRoot configRoot, params object[] hostInitParams) {
            throw ExceptionUtil.UnexpectedError("RemoteWebConfigurationHost::Init");
        }

        public override void InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath,
                        IInternalConfigRoot root, params object[] hostInitConfigurationParams) {


            WebLevel                webLevel = (WebLevel)               hostInitConfigurationParams[0];
            // ConfigurationFileMap    fileMap = (ConfigurationFileMap)    hostInitConfigurationParams[1];
            string                  path = (string)                     hostInitConfigurationParams[2];
            string                  site = (string)                     hostInitConfigurationParams[3];

            if (locationSubPath == null) {
                locationSubPath = (string)                              hostInitConfigurationParams[4];
            }

            string                  server = (string)                   hostInitConfigurationParams[5];
            string                  userName = (string)                 hostInitConfigurationParams[6];
            string                  password = (string)                 hostInitConfigurationParams[7];
            IntPtr                  tokenHandle = (IntPtr)              hostInitConfigurationParams[8];

            configPath = null;
            locationConfigPath = null;

            _Server = server;
            _Username = GetUserNameFromFullName(userName);
            _Domain = GetDomainFromFullName(userName);
            _Password = password;
            _Identity = (tokenHandle == IntPtr.Zero) ? null : new WindowsIdentity(tokenHandle); //CreateWindowsIdentity(username, domain, password, tokenHandle);

            _PathMap = new Hashtable(StringComparer.OrdinalIgnoreCase);

#if !FEATURE_PAL // FEATURE_PAL does not have WindowsImpersonationContext, COM objects
            //
            // Send the path arguments to the server for parsing,
            // and retreive the normalized paths and path mapping
            // from config paths to file paths.
            //
            string filePaths;
            try {
                WindowsImpersonationContext wiContext = (_Identity != null) ? _Identity.Impersonate() : null;
                try {
                    IRemoteWebConfigurationHostServer remoteSrv = RemoteWebConfigurationHost.CreateRemoteObject(server, _Username, _Domain, password); //(IRemoteWebConfigurationHostServer) Activator.CreateInstance(type);
                    try {
                        filePaths = remoteSrv.GetFilePaths((int) webLevel, path, site, locationSubPath);
                    } finally {
                        // Release COM objects
                        while (Marshal.ReleaseComObject(remoteSrv) > 0) {
                        }
                    }
                } finally {
                    if (wiContext != null)
                        wiContext.Undo();
                }
            }
            catch {
                // Wrap finally clause with a try to avoid exception clauses being run
                // while the thread is impersonated.
                throw;
            }

            if (filePaths == null) {
                throw ExceptionUtil.UnexpectedError("RemoteWebConfigurationHost::InitForConfiguration");
            }

            //
            // Format of filePaths:
            //      appPath < appSiteName < appSiteID < configPath < locationConfigPath [< configPath < fileName]+
            //
            string[] parts = filePaths.Split(RemoteWebConfigurationHostServer.FilePathsSeparatorParams);

            if (parts.Length < 7 || (parts.Length - 5) % 2 != 0) {
                throw ExceptionUtil.UnexpectedError("RemoteWebConfigurationHost::InitForConfiguration");
            }

            // convert empty strings to nulls
            for (int i = 0; i < parts.Length; i++) {
                if (parts[i].Length == 0) {
                    parts[i] = null;
                }
            }

            // get config paths
            string appPath = parts[0];
            string appSiteName = parts[1];
            string appSiteID = parts[2];
            configPath = parts[3];
            locationConfigPath = parts[4];
            _ConfigPath = configPath;

            // Create a WebConfigurationFileMap to be used when we later initialize our delegating WebConfigurationHost
            WebConfigurationFileMap configFileMap = new WebConfigurationFileMap();
            VirtualPath             appPathVirtualPath = VirtualPath.CreateAbsoluteAllowNull(appPath);

            configFileMap.Site = appSiteID;
            
            // populate the configpath->physical path mapping
            for (int i = 5; i < parts.Length; i += 2) {
                string      configPathTemp = parts[i];
                string      physicalFilePath = parts[i+1];
                
                _PathMap.Add(configPathTemp, physicalFilePath);

                // Update the WebConfigurationFileMap
                if (WebConfigurationHost.IsMachineConfigPath(configPathTemp)) {
                    configFileMap.MachineConfigFilename = physicalFilePath;
                }
                else {
                    string      vPathString;
                    bool        isRootApp;

                    if (WebConfigurationHost.IsRootWebConfigPath(configPathTemp)) {
                        vPathString = null;
                        isRootApp = false;
                    }
                    else {
                        VirtualPath vPath;
                        string      dummy;
                        
                        WebConfigurationHost.GetSiteIDAndVPathFromConfigPath(configPathTemp, out dummy, out vPath);
                        vPathString = VirtualPath.GetVirtualPathString(vPath);
                        isRootApp = (vPath == appPathVirtualPath);
                    }

                    configFileMap.VirtualDirectories.Add(vPathString, 
                        new VirtualDirectoryMapping(Path.GetDirectoryName(physicalFilePath), isRootApp));
                }
            }

#else // !FEATURE_PAL: set dummy config path
            string appPath = null;
            _ConfigPath = configPath;
#endif // !FEATURE_PAL

            // Delegate to a WebConfigurationHost for unhandled methods.
            WebConfigurationHost webConfigurationHost = new WebConfigurationHost();
            webConfigurationHost.Init(root, true, new UserMapPath(configFileMap, /*pathsAreLocal*/ false), null, appPath, appSiteName, appSiteID);
            Host = webConfigurationHost;
        }

        // config path support
        public override bool IsConfigRecordRequired(string configPath) {
            // a record is required for every part of the config path
            return configPath.Length <= _ConfigPath.Length;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override string GetStreamName(string configPath)
        {
            return (string) _PathMap[configPath];
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override object GetStreamVersion(string streamName) {
#if FEATURE_PAL // FEATURE_PAL: singelton version
	return s_version;
#else 
            // for now, assume it is the same
            // return s_version;
            bool exists;
            long size, createDate, lastWriteDate;
            WindowsImpersonationContext wiContext = null;

            try {
                if (_Identity != null) {
                    wiContext = _Identity.Impersonate();
                }

                try {
                    //////////////////////////////////////////////////////////////////
                    // Step 3: Get the type and create the object on the remote server
                    IRemoteWebConfigurationHostServer remoteSrv = CreateRemoteObject(_Server, _Username, _Domain, _Password);
                    try {
                        //////////////////////////////////////////////////////////////////
                        // Step 4: Call the API
                        remoteSrv.GetFileDetails(streamName, out exists, out size, out createDate, out lastWriteDate);
                    }
                    finally {
                        while (Marshal.ReleaseComObject(remoteSrv) > 0) { } // release the COM object
                    }
                }
                finally {
                    if (wiContext != null) {
                        wiContext.Undo(); // revert impersonation
                    }
                }
            }
            catch {
                // Wrap finally clause with a try to avoid exception clauses being run
                // while the thread is impersonated.
                throw;
            }

            return new FileDetails(exists, size, DateTime.FromFileTimeUtc(createDate), DateTime.FromFileTimeUtc(lastWriteDate));
#endif // FEATURE_PAL
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override Stream OpenStreamForRead(string streamName) {
            RemoteWebConfigurationHostStream rcs = new RemoteWebConfigurationHostStream(false, _Server, streamName, null, _Username, _Domain, _Password, _Identity);
            if (rcs == null || rcs.Length < 1)
                return null;
            return rcs;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext) {
            RemoteWebConfigurationHostStream rcs = new RemoteWebConfigurationHostStream(true, _Server, streamName, templateStreamName, _Username, _Domain, _Password, _Identity);
            writeContext = rcs;
            return rcs;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override void DeleteStream(string StreamName)
        {
            // 
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override void WriteCompleted(string streamName, bool success, object writeContext)
        {
            if (success) {
                RemoteWebConfigurationHostStream rcs = (RemoteWebConfigurationHostStream)writeContext;
                rcs.FlushForWriteCompleted();
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override bool IsFile(string StreamName)
        {
            return false;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override bool PrefetchAll(string configPath, string StreamName)
        {
            return true;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override bool PrefetchSection(string sectionGroupName, string sectionName)
        {
            return true;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override void GetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady) {
            WebConfigurationHost.StaticGetRestrictedPermissions(configRecord, out permissionSet, out isHostReady);
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public override bool IsRemote {
            get { return true; }
        }
        
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        // Encrypt/decrypt support
        public override string DecryptSection(string encryptedXmlString, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection) {
            return CallEncryptOrDecrypt(false, encryptedXmlString, protectionProvider, protectedConfigSection);
        }
        public override string EncryptSection(string clearTextXmlString, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection) {
            return CallEncryptOrDecrypt(true, clearTextXmlString, protectionProvider, protectedConfigSection);
        }

        private string CallEncryptOrDecrypt(bool doEncrypt, string xmlString, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection)
        {
#if !FEATURE_PAL // FEATURE_PAL has no COM objects => no encryption
            // ROTORTODO: COM Objects are not implemented.
            // CORIOLISTODO: COM Objects are not implemented.
            ProviderSettings                   ps;
            NameValueCollection                nvc;
            string  []                         paramKeys;
            string  []                         paramValues;
            string                             returnString = null;
            string                             typeName;
            WindowsImpersonationContext        wiContext = null;

            ////////////////////////////////////////////////////////////
            // Step 1: Create list of parameters for the protection provider
            typeName = protectionProvider.GetType().AssemblyQualifiedName;
            ps = protectedConfigSection.Providers[protectionProvider.Name];
            if (ps == null)
                throw ExceptionUtil.ParameterInvalid("protectionProvider");

            nvc = ps.Parameters;
            if (nvc == null)
                nvc = new NameValueCollection();

            paramKeys = nvc.AllKeys;
            paramValues = new string[paramKeys.Length];
            for(int iter = 0; iter<paramKeys.Length; iter++)
                paramValues[iter] = nvc[paramKeys[iter]];

            ////////////////////////////////////////////////////////////
            // Step 2: Set the impersonation if required
            if (_Identity != null)
                wiContext = _Identity.Impersonate();

            try {
                try {
                    //////////////////////////////////////////////////////////////////
                    // Step 3: Get the type and create the object on the remote server
                    IRemoteWebConfigurationHostServer remoteSrv = CreateRemoteObject(_Server, _Username, _Domain, _Password);
                    try {
                        //////////////////////////////////////////////////////////////////
                        // Step 4: Call the API
                        returnString = remoteSrv.DoEncryptOrDecrypt(doEncrypt, xmlString, protectionProvider.Name, typeName, paramKeys, paramValues);
                    } finally {
                        while (Marshal.ReleaseComObject(remoteSrv) > 0) { } // release the COM object
                    }
                } finally {
                    if (wiContext != null)
                        wiContext.Undo(); // revert impersonation
                }
            }
            catch {
            }

            return returnString;
#else       // !FEATURE_PAL
            throw new NotImplementedException("ROTORTODO");
#endif      // !FEATURE_PAL
        }

        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        private static string GetUserNameFromFullName(string fullUserName) {
            if (string.IsNullOrEmpty(fullUserName))
                return null;
            if (fullUserName.Contains("@")) {
                return fullUserName;
            }
            string[] splitted = fullUserName.Split(new char[] { '\\' });
            if (splitted.Length == 1)
                return fullUserName;
            else
                return splitted[1];
        }
        private static string GetDomainFromFullName(string fullUserName) {
            if (string.IsNullOrEmpty(fullUserName))
                return null;
            if (fullUserName.Contains("@"))
                return null;
            string[] splitted = fullUserName.Split(new char[] { '\\' });
            if (splitted.Length == 1)
                return ".";
            return splitted[0];
        }

        // impersonation support: create an identity from credentials
#if OLD_WAY
        private static WindowsIdentity CreateWindowsIdentity(string userName, string password, IntPtr tokenHandle) {

            //////////////////////////////////////////////////////////////////
            // Step 0: Most common case: check if no credentials are supplied
            if (string.IsNullOrEmpty(userName) && tokenHandle == IntPtr.Zero && string.IsNullOrEmpty(password))
                return null;

            //////////////////////////////////////////////////////////////////
            // Step 1: Make sure that either username & password OR token is supplied
            if ((string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password)) || (!string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password)))
                throw ExceptionUtil.ParameterNullOrEmpty("password");

            if (!string.IsNullOrEmpty(userName) && tokenHandle != IntPtr.Zero)
                throw ExceptionUtil.ParameterInvalid("tokenHandle");

            //////////////////////////////////////////////////////////////////
            // Step 2: Create token if not supplied
            if (!string.IsNullOrEmpty(userName))
                tokenHandle = CreateUserToken(userName, password);

            //////////////////////////////////////////////////////////////////
            // Step 3: Create a windows identity from the token
            WindowsIdentity wi = new WindowsIdentity(tokenHandle);

            if (!string.IsNullOrEmpty(userName) && tokenHandle != IntPtr.Zero) // Close the handle if we created it.
                UnsafeNativeMethods.CloseHandle(tokenHandle);
            return wi;
        }

        private static IntPtr CreateUserToken(string fullUserName, string password) {
            string userName, domain;
            if (fullUserName.Contains("@")) {
                userName = fullUserName;
                domain = null;
            } else {
                string[] splitted = fullUserName.Split(new char[] { '\\' });
                if (splitted.Length == 1) {
                    userName = fullUserName;
                    domain = ".";
                } else {
                    userName = splitted[1];
                    domain = splitted[0];
                }
            }
            //This parameter causes LogonUser to create a primary token.
            const int LOGON32_PROVIDER_DEFAULT        = 0;
            const int LOGON32_LOGON_INTERACTIVE       = 2;
            const int LOGON32_LOGON_NETWORK           = 3;
            const int LOGON32_LOGON_BATCH             = 4;
            const int LOGON32_LOGON_SERVICE           = 5;
            const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;

            int[] Logon32Values = new int[] { LOGON32_LOGON_INTERACTIVE, LOGON32_LOGON_BATCH, LOGON32_LOGON_SERVICE, LOGON32_LOGON_NETWORK_CLEARTEXT, LOGON32_LOGON_NETWORK};

            IntPtr tokenHandle = IntPtr.Zero;
            int lastError = 0;
            if (UnsafeNativeMethods.LogonUser(userName, domain, password, Logon32Values[0], LOGON32_PROVIDER_DEFAULT, ref tokenHandle) != 0)
                return tokenHandle;
            lastError = Marshal.GetHRForLastWin32Error();
            for (int iter = 1; iter < Logon32Values.Length; iter++)
                if (UnsafeNativeMethods.LogonUser(userName, domain, password, Logon32Values[iter], LOGON32_PROVIDER_DEFAULT, ref tokenHandle) != 0)
                    return tokenHandle;
            Marshal.ThrowExceptionForHR(lastError);
            return IntPtr.Zero;
        }
#endif
        internal static IRemoteWebConfigurationHostServer CreateRemoteObject(string server, string username, string domain, string password) {

#if !FEATURE_PAL // FEATURE_PAL has no COM objects
            try {
                if (string.IsNullOrEmpty(username))
                    return CreateRemoteObjectUsingGetTypeFromCLSID(server);
                if (IntPtr.Size == 8)
                    return CreateRemoteObjectOn64BitPlatform(server, username, domain, password);
                return CreateRemoteObjectOn32BitPlatform(server, username, domain, password);
            } catch (COMException ex) {
                if ((uint)ex.ErrorCode == 0x80040154)
                    throw new Exception(SR.GetString(SR.Make_sure_remote_server_is_enabled_for_config_access));
                throw;
            }
        }
        private static IRemoteWebConfigurationHostServer CreateRemoteObjectUsingGetTypeFromCLSID(string server) {
            Type type = Type.GetTypeFromCLSID(typeof(RemoteWebConfigurationHostServer).GUID, server, true);
            return (IRemoteWebConfigurationHostServer)Activator.CreateInstance(type);
        }

        private static IRemoteWebConfigurationHostServer CreateRemoteObjectOn32BitPlatform(string server, string username, string domain, string password)
        {
            MULTI_QI []     amqi            = new MULTI_QI[1];
            IntPtr          guidbuf         = IntPtr.Zero;
            COAUTHINFO      ca              = null;
            IntPtr          captr           = IntPtr.Zero;
            COSERVERINFO    cs              = null;
            Guid            clsid           = typeof(RemoteWebConfigurationHostServer).GUID;
            int             hr              = 0;
            COAUTHIDENTITY  ci              = null;
            IntPtr          ciptr           = IntPtr.Zero;

            try {
                guidbuf = Marshal.AllocCoTaskMem(16);
                Marshal.StructureToPtr(typeof(IRemoteWebConfigurationHostServer).GUID, guidbuf, false);
                amqi[0] = new MULTI_QI(guidbuf);

                ci = new COAUTHIDENTITY(username, domain, password);
                ciptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(ci));
                Marshal.StructureToPtr(ci, ciptr, false);

                ca = new COAUTHINFO(RpcAuthent.WinNT, RpcAuthor.None, null, /*RpcLevel.Connect*/ RpcLevel.Default, RpcImpers.Impersonate, ciptr);
                captr = Marshal.AllocCoTaskMem(Marshal.SizeOf(ca));
                Marshal.StructureToPtr(ca, captr, false);

                cs = new COSERVERINFO(server, captr);
                hr = UnsafeNativeMethods.CoCreateInstanceEx(ref clsid, IntPtr.Zero, (int)ClsCtx.RemoteServer, cs, 1, amqi);
                if ((uint)hr == 0x80040154)
                        throw new Exception(SR.GetString(SR.Make_sure_remote_server_is_enabled_for_config_access));
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);
                if (amqi[0].hr < 0)
                    Marshal.ThrowExceptionForHR(amqi[0].hr);
                hr = UnsafeNativeMethods.CoSetProxyBlanket(amqi[0].pItf, RpcAuthent.WinNT, RpcAuthor.None, null, /*RpcLevel.Connect*/ RpcLevel.Default, RpcImpers.Impersonate, ciptr, 0);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);
                return (IRemoteWebConfigurationHostServer)Marshal.GetObjectForIUnknown(amqi[0].pItf);
            } finally {
                if (amqi[0].pItf != IntPtr.Zero)
                {
                    Marshal.Release(amqi[0].pItf);
                    amqi[0].pItf = IntPtr.Zero;
                }
                amqi[0].piid = IntPtr.Zero;
                if (captr != IntPtr.Zero) {
                    Marshal.DestroyStructure(captr, typeof(COAUTHINFO));
                    Marshal.FreeCoTaskMem(captr);
                }
                if (ciptr != IntPtr.Zero) {
                    Marshal.DestroyStructure(ciptr, typeof(COAUTHIDENTITY));
                    Marshal.FreeCoTaskMem(ciptr);
                }
                if (guidbuf != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(guidbuf);
            }
#else // !FEATURE_PAL
            throw new NotSupportedException();
#endif // !FEATURE_PAL
        }

        private static IRemoteWebConfigurationHostServer CreateRemoteObjectOn64BitPlatform(string server, string username, string domain, string password)
        {
            MULTI_QI_X64[]      amqi            = new MULTI_QI_X64[1];
            IntPtr              guidbuf         = IntPtr.Zero;
            COAUTHINFO_X64      ca              = null;
            IntPtr              captr           = IntPtr.Zero;
            COSERVERINFO_X64    cs              = null;
            Guid                clsid           = typeof(RemoteWebConfigurationHostServer).GUID;
            int                 hr              = 0;
            COAUTHIDENTITY_X64  ci              = null;
            IntPtr              ciptr           = IntPtr.Zero;

            try {
                guidbuf = Marshal.AllocCoTaskMem(16);
                Marshal.StructureToPtr(typeof(IRemoteWebConfigurationHostServer).GUID, guidbuf, false);
                amqi[0] = new MULTI_QI_X64(guidbuf);

                ci = new COAUTHIDENTITY_X64(username, domain, password);
                ciptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(ci));
                Marshal.StructureToPtr(ci, ciptr, false);

                ca = new COAUTHINFO_X64(RpcAuthent.WinNT, RpcAuthor.None, null, /*RpcLevel.Connect*/ RpcLevel.Default, RpcImpers.Impersonate, ciptr);
                captr = Marshal.AllocCoTaskMem(Marshal.SizeOf(ca));
                Marshal.StructureToPtr(ca, captr, false);

                cs = new COSERVERINFO_X64(server, captr);
                hr = UnsafeNativeMethods.CoCreateInstanceEx(ref clsid, IntPtr.Zero, (int)ClsCtx.RemoteServer, cs, 1, amqi);
                if ((uint)hr == 0x80040154)
                    throw new Exception(SR.GetString(SR.Make_sure_remote_server_is_enabled_for_config_access));
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);
                if (amqi[0].hr < 0)
                    Marshal.ThrowExceptionForHR(amqi[0].hr);
                hr = UnsafeNativeMethods.CoSetProxyBlanket(amqi[0].pItf, RpcAuthent.WinNT, RpcAuthor.None, null, /*RpcLevel.Connect*/ RpcLevel.Default, RpcImpers.Impersonate, ciptr, 0);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);
                return (IRemoteWebConfigurationHostServer)Marshal.GetObjectForIUnknown(amqi[0].pItf);
            } finally {
                if (amqi[0].pItf != IntPtr.Zero) {
                    Marshal.Release(amqi[0].pItf);
                    amqi[0].pItf = IntPtr.Zero;
                }
                amqi[0].piid = IntPtr.Zero;
                if (captr != IntPtr.Zero) {
                    Marshal.DestroyStructure(captr, typeof(COAUTHINFO_X64));
                    Marshal.FreeCoTaskMem(captr);
                }
                if (ciptr != IntPtr.Zero) {
                    Marshal.DestroyStructure(ciptr, typeof(COAUTHIDENTITY_X64));
                    Marshal.FreeCoTaskMem(ciptr);
                }
                if (guidbuf != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(guidbuf);
            }
        }
    }
}
