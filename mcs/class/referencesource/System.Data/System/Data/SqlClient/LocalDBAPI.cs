//------------------------------------------------------------------------------
// <copyright file="LocalDB.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">antonam</owner>
//------------------------------------------------------------------------------


namespace System.Data
{
    using System.Configuration;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Data.Common;
    using System.Globalization;
    using System.Data.SqlClient;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Diagnostics;
    using System.Security;
    using System.Security.Permissions;

    internal static class LocalDBAPI
    {
        const string const_localDbPrefix = @"(localdb)\";
        const string const_partialTrustFlagKey = "ALLOW_LOCALDB_IN_PARTIAL_TRUST";

        static PermissionSet _fullTrust = null;
        static bool _partialTrustFlagChecked = false;
        static bool _partialTrustAllowed = false;


        // check if name is in format (localdb)\<InstanceName - not empty> and return instance name if it is
        internal static string GetLocalDbInstanceNameFromServerName(string serverName)
        {
            if (serverName == null)
                return null;
            serverName = serverName.TrimStart(); // it can start with spaces if specified in quotes
            if (!serverName.StartsWith(const_localDbPrefix, StringComparison.OrdinalIgnoreCase))
                return null;
            string instanceName = serverName.Substring(const_localDbPrefix.Length).Trim();
            if (instanceName.Length == 0)
                return null;
            else
                return instanceName;
        }


        internal static void ReleaseDLLHandles()
        {
            s_userInstanceDLLHandle = IntPtr.Zero;
            s_localDBFormatMessage = null;
            s_localDBCreateInstance = null;
        }

             

        //This is copy of handle that SNI maintains, so we are responsible for freeing it - therefore there we are not using SafeHandle
        static IntPtr s_userInstanceDLLHandle = IntPtr.Zero;        

        static object s_dllLock = new object();
        
        static IntPtr UserInstanceDLLHandle
        {
            get
            {
                if (s_userInstanceDLLHandle==IntPtr.Zero)
                {
                    bool lockTaken = false;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        Monitor.Enter(s_dllLock, ref lockTaken);
                        if (s_userInstanceDLLHandle == IntPtr.Zero)
                        {
                            SNINativeMethodWrapper.SNIQueryInfo(SNINativeMethodWrapper.QTypes.SNI_QUERY_LOCALDB_HMODULE, ref s_userInstanceDLLHandle);
                            if (s_userInstanceDLLHandle != IntPtr.Zero)
                            {
                                Bid.Trace("<sc.LocalDBAPI.UserInstanceDLLHandle> LocalDB - handle obtained");                                
                            }
                            else
                            {
                                SNINativeMethodWrapper.SNI_Error sniError = new SNINativeMethodWrapper.SNI_Error();
                                SNINativeMethodWrapper.SNIGetLastError(sniError);
                                throw CreateLocalDBException(errorMessage: Res.GetString("LocalDB_FailedGetDLLHandle"), sniError: (int)sniError.sniError);
                            }
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                            Monitor.Exit(s_dllLock);
                    }
                }               
                return s_userInstanceDLLHandle;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int LocalDBCreateInstanceDelegate([MarshalAs(UnmanagedType.LPWStr)] string version, [MarshalAs(UnmanagedType.LPWStr)] string instance, UInt32 flags);

        static LocalDBCreateInstanceDelegate s_localDBCreateInstance = null;

        static LocalDBCreateInstanceDelegate LocalDBCreateInstance
        {
            get
            {
                if (s_localDBCreateInstance==null)
                {
                    bool lockTaken = false;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        Monitor.Enter(s_dllLock, ref lockTaken);
                        if (s_localDBCreateInstance == null)
                        {
                            IntPtr functionAddr = SafeNativeMethods.GetProcAddress(UserInstanceDLLHandle, "LocalDBCreateInstance");

                            if (functionAddr == IntPtr.Zero)
                            {
                                int hResult=Marshal.GetLastWin32Error();
                                Bid.Trace("<sc.LocalDBAPI.LocalDBCreateInstance> GetProcAddress for LocalDBCreateInstance error 0x{%X}",hResult);
                                throw CreateLocalDBException(errorMessage: Res.GetString("LocalDB_MethodNotFound"));
                            }
                            s_localDBCreateInstance = (LocalDBCreateInstanceDelegate)Marshal.GetDelegateForFunctionPointer(functionAddr, typeof(LocalDBCreateInstanceDelegate));
                        }
                    }                    
                    finally
                    {
                        if (lockTaken)
                            Monitor.Exit(s_dllLock);
                    }
                }
                return s_localDBCreateInstance;
            }
        }


        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl,CharSet=CharSet.Unicode)]
        private delegate int LocalDBFormatMessageDelegate(int hrLocalDB, UInt32 dwFlags, UInt32 dwLanguageId, StringBuilder buffer, ref UInt32 buflen);

        static LocalDBFormatMessageDelegate s_localDBFormatMessage = null;

        static LocalDBFormatMessageDelegate LocalDBFormatMessage
        {
            get
            {                
                if (s_localDBFormatMessage==null)
                {
                    bool lockTaken = false;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        Monitor.Enter(s_dllLock, ref lockTaken);
                        if (s_localDBFormatMessage == null)
                        {
                            IntPtr functionAddr = SafeNativeMethods.GetProcAddress(UserInstanceDLLHandle, "LocalDBFormatMessage");

                            if (functionAddr == IntPtr.Zero)
                            {
                                // SNI checks for LocalDBFormatMessage during DLL loading, so it is practically impossibe to get this error.
                                int hResult=Marshal.GetLastWin32Error();
                                Bid.Trace("<sc.LocalDBAPI.LocalDBFormatMessage> GetProcAddress for LocalDBFormatMessage error 0x{%X}", hResult);
                                throw CreateLocalDBException(errorMessage: Res.GetString("LocalDB_MethodNotFound"));
                            }
                            s_localDBFormatMessage = (LocalDBFormatMessageDelegate)Marshal.GetDelegateForFunctionPointer(functionAddr, typeof(LocalDBFormatMessageDelegate));                            
                        }
                    }                                         
                    finally
                    {
                        if (lockTaken)
                            Monitor.Exit(s_dllLock);
                    }
                }
                return s_localDBFormatMessage;
            }
        }

        const UInt32 const_LOCALDB_TRUNCATE_ERR_MESSAGE = 1;// flag for LocalDBFormatMessage that indicates that message can be truncated if it does not fit in the buffer
        const int const_ErrorMessageBufferSize = 1024;      // Buffer size for Local DB error message, according to Serverless team, 1K will be enough for all messages

        
        internal static string GetLocalDBMessage(int hrCode)
        {
            Debug.Assert(hrCode < 0, "HRCode does not indicate error");
            try
            {
                StringBuilder buffer = new StringBuilder((int)const_ErrorMessageBufferSize);
                UInt32 len = (UInt32)buffer.Capacity;

                
                // First try for current culture                
                int hResult=LocalDBFormatMessage(hrLocalDB: hrCode, dwFlags: const_LOCALDB_TRUNCATE_ERR_MESSAGE, dwLanguageId: (UInt32)CultureInfo.CurrentCulture.LCID, 
                                                 buffer: buffer, buflen: ref len);
                if (hResult>=0)
                    return buffer.ToString();
                else
                {
                    // Message is not available for current culture, try default 
                    buffer = new StringBuilder((int)const_ErrorMessageBufferSize);
                    len = (UInt32) buffer.Capacity;
                    hResult=LocalDBFormatMessage(hrLocalDB: hrCode, dwFlags: const_LOCALDB_TRUNCATE_ERR_MESSAGE, dwLanguageId: 0 /* thread locale with fallback to English */,
                                                 buffer: buffer, buflen: ref len);
                    if (hResult >= 0)
                        return buffer.ToString();
                    else
                        return string.Format(CultureInfo.CurrentCulture, "{0} (0x{1:X}).", Res.GetString("LocalDB_UnobtainableMessage"), hResult);
                }
            }
            catch (SqlException exc)
            {
                return string.Format(CultureInfo.CurrentCulture, "{0} ({1}).", Res.GetString("LocalDB_UnobtainableMessage"), exc.Message);
            }
        }

        
        static SqlException CreateLocalDBException(string errorMessage, string instance = null, int localDbError = 0, int sniError = 0)
        {
            Debug.Assert((localDbError == 0) || (sniError == 0), "LocalDB error and SNI error cannot be specified simultaneously");
            Debug.Assert(!string.IsNullOrEmpty(errorMessage), "Error message should not be null or empty");
            SqlErrorCollection collection = new SqlErrorCollection();

            int errorCode = (localDbError == 0) ? sniError : localDbError;

            if (sniError!=0)
            {
                string sniErrorMessage = SQL.GetSNIErrorMessage(sniError);
                errorMessage = String.Format((IFormatProvider)null, "{0} (error: {1} - {2})",
                         errorMessage, sniError, sniErrorMessage);
            }
            
            collection.Add(new SqlError(errorCode, 0, TdsEnums.FATAL_ERROR_CLASS, instance, errorMessage, null, 0));

            if (localDbError != 0)
                collection.Add(new SqlError(errorCode, 0, TdsEnums.FATAL_ERROR_CLASS, instance, GetLocalDBMessage(localDbError),  null, 0));

            SqlException exc = SqlException.CreateException(collection, null);

            exc._doNotReconnect = true;

            return exc;
        }

        private class InstanceInfo
        {
            internal InstanceInfo(string version)
            {
                this.version = version;
                this.created = false;
            }

            internal readonly string version;
            internal bool created;
        }

        static object s_configLock=new object();
        static Dictionary<string, InstanceInfo> s_configurableInstances = null;

        internal static void DemandLocalDBPermissions()
        {
            if (!_partialTrustAllowed) 
            { 
                if (!_partialTrustFlagChecked) 
                {
                    object partialTrustFlagValue = AppDomain.CurrentDomain.GetData(const_partialTrustFlagKey);
                    if (partialTrustFlagValue != null && partialTrustFlagValue is bool) 
                    {
                        _partialTrustAllowed = (bool)partialTrustFlagValue;                        
                    }
                    _partialTrustFlagChecked = true;        
                    if (_partialTrustAllowed) 
                    {
                        return;
                    }
                }
                if (_fullTrust == null) 
                {
                    _fullTrust = new NamedPermissionSet("FullTrust");
                }
                _fullTrust.Demand();
            }
        }        

        internal static void AssertLocalDBPermissions()
        {
            _partialTrustAllowed = true;
        }
            

        internal static void CreateLocalDBInstance(string instance)
        {
            DemandLocalDBPermissions();
            if (s_configurableInstances == null)
            {
                // load list of instances from configuration, mark them as not created
                bool lockTaken = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(s_configLock, ref lockTaken);
                    if (s_configurableInstances == null)
                    {
                        Dictionary<string, InstanceInfo> tempConfigurableInstances = new Dictionary<string, InstanceInfo>(StringComparer.OrdinalIgnoreCase);
                        object section = PrivilegedConfigurationManager.GetSection("system.data.localdb");
                        if (section != null) // if no section just skip creation
                        {
                            // validate section type
                            LocalDBConfigurationSection configSection = section as LocalDBConfigurationSection;
                            if (configSection == null) 
                                throw CreateLocalDBException(errorMessage: Res.GetString("LocalDB_BadConfigSectionType"));
                            foreach (LocalDBInstanceElement confElement in configSection.LocalDbInstances)
                            {
                                Debug.Assert(confElement.Name != null && confElement.Version != null, "Both name and version should not be null");
                                tempConfigurableInstances.Add(confElement.Name.Trim(), new InstanceInfo(confElement.Version.Trim()));
                            }
                        }
                        else
                            Bid.Trace( "<sc.LocalDBAPI.CreateLocalDBInstance> No system.data.localdb section found in configuration");
                        s_configurableInstances = tempConfigurableInstances;
                    }
                }
                finally
                {
                    if (lockTaken)
                        Monitor.Exit(s_configLock);
                }
            }
            
            InstanceInfo instanceInfo = null;

            if (!s_configurableInstances.TryGetValue(instance,out instanceInfo))           
                return; // instance name was not in the config

            if (instanceInfo.created)
                return; // instance has already been created

            Debug.Assert(!instance.Contains("\0"), "Instance name should contain embedded nulls");

            if (instanceInfo.version.Contains("\0"))
                throw CreateLocalDBException(errorMessage: Res.GetString("LocalDB_InvalidVersion"), instance: instance);
         
            // LocalDBCreateInstance is thread- and cross-process safe method, it is OK to call from two threads simultaneously
            int hr = LocalDBCreateInstance(instanceInfo.version, instance, flags: 0);
            Bid.Trace("<sc.LocalDBAPI.CreateLocalDBInstance> Starting creation of instance %ls version %ls", instance, instanceInfo.version);
            if (hr < 0)
                throw CreateLocalDBException(errorMessage: Res.GetString("LocalDB_CreateFailed"), instance: instance, localDbError: hr);
            Bid.Trace("<sc.LocalDBAPI.CreateLocalDBInstance> Finished creation of instance %ls", instance);
            instanceInfo.created=true; // mark instance as created

        } // CreateLocalDbInstance

    }

}
