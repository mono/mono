//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using Microsoft.Win32;

    class TypeCacheManager : ITypeCacheManager
    {
        enum RegKind
        {
            Default = 0,
            Register = 1,
            None = 2
        }


        // TypeCacheManager.Provider will give access to the static instance of the TypeCache
        static Guid clrAssemblyCustomID = new Guid("90883F05-3D28-11D2-8F17-00A0C9A6186D");
        static object instanceLock = new object();


        static public ITypeCacheManager Provider
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        ITypeCacheManager localInstance = new TypeCacheManager();
                        Thread.MemoryBarrier();
                        instance = localInstance;
                    }
                }

                return instance;
            }
        }
        static internal ITypeCacheManager instance;


        // Convert to typeLibrary ID (GUID)
        private Dictionary<Guid, Assembly> assemblyTable;
        private Dictionary<Guid, Type> typeTable;
        private object typeTableLock;
        private object assemblyTableLock;

        internal TypeCacheManager()
        {
            assemblyTable = new Dictionary<Guid, Assembly>();
            typeTable = new Dictionary<Guid, Type>();
            typeTableLock = new object();
            assemblyTableLock = new object();
        }
        private Guid GettypeLibraryIDFromIID(Guid iid, bool isServer, out String version)
        {
            // In server we need to open the the User hive for the Process User.
            RegistryKey interfaceKey = null;
            try
            {
                string keyName = null;
                if (isServer)
                {
                    keyName = String.Concat("software\\classes\\interface\\{", iid.ToString(), "}\\typelib");
                    interfaceKey = Registry.LocalMachine.OpenSubKey(keyName, false);
                }
                else
                {
                    keyName = String.Concat("interface\\{", iid.ToString(), "}\\typelib");
                    interfaceKey = Registry.ClassesRoot.OpenSubKey(keyName, false);
                }
                if (interfaceKey == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InterfaceNotRegistered)));
                string typeLibID = interfaceKey.GetValue("").ToString();
                if (string.IsNullOrEmpty(typeLibID))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoTypeLibraryFoundForInterface)));
                version = interfaceKey.GetValue("Version").ToString();
                if (string.IsNullOrEmpty(version))
                    version = "1.0";

                Guid typeLibraryID;
                if (!DiagnosticUtility.Utility.TryCreateGuid(typeLibID, out typeLibraryID))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BadInterfaceRegistration)));
                }
                return typeLibraryID;

            }
            finally
            {
                if (interfaceKey != null)
                    interfaceKey.Close();
            }

        }
        private void ParseVersion(string version, bool parseVersionAsHex, out ushort major, out ushort minor)
        {
            NumberStyles numberStyle = (parseVersionAsHex) ? NumberStyles.HexNumber : NumberStyles.None;
            major = 0;
            minor = 0;
            if (String.IsNullOrEmpty(version))
                return;
            int indexOfDot = version.IndexOf(".", StringComparison.Ordinal);
            try
            {

                if (indexOfDot == -1)
                {
                    major = ushort.Parse(version, numberStyle, NumberFormatInfo.InvariantInfo);
                    minor = 0;
                }
                else
                {
                    major = ushort.Parse(version.Substring(0, indexOfDot), numberStyle, NumberFormatInfo.InvariantInfo); 
                    string minorVersion = version.Substring(indexOfDot + 1);
                    int indexOfDot2 = minorVersion.IndexOf(".", StringComparison.Ordinal);

                    if (indexOfDot2 != -1) // Ignore anything beyond the first minor version.
                        minorVersion = minorVersion.Substring(0, indexOfDot2);

                    minor = ushort.Parse(minorVersion, numberStyle, NumberFormatInfo.InvariantInfo);
                }
            }
            catch (FormatException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BadInterfaceVersion)));
            }
            catch (OverflowException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BadInterfaceVersion)));
            }

        }
        private ITypeLib2 GettypeLibrary(Guid typeLibraryID, string version, bool parseVersionAsHex)
        {
            ushort major = 0;
            ushort minor = 0;
            const int lcidLocalIndependent = 0;
            ParseVersion(version, parseVersionAsHex, out major, out minor);
            object otlb;
            int hr = SafeNativeMethods.LoadRegTypeLib(ref typeLibraryID, major, minor, lcidLocalIndependent, out otlb);
            if (hr != 0 || null == otlb)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.FailedToLoadTypeLibrary), hr));
            return otlb as ITypeLib2;


        }


        private Assembly ResolveAssemblyFromIID(Guid iid, bool noAssemblyGeneration, bool isServer)
        {

            String version;
            Guid typeLibraryID = GettypeLibraryIDFromIID(iid, isServer, out version);

            return ResolveAssemblyFromTypeLibID(iid, typeLibraryID, version, true, noAssemblyGeneration);

        }

        private Assembly ResolveAssemblyFromTypeLibID(Guid iid, Guid typeLibraryID, string version, bool parseVersionAsHex, bool noAssemblyGeneration)
        {
            ComPlusTLBImportTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationTLBImportStarting,
                                           SR.TraceCodeComIntegrationTLBImportStarting, iid, typeLibraryID);
            Assembly asm;

            bool generateNativeAssembly = false;
            ITypeLib2 typeLibrary = null;

            try
            {
                lock (assemblyTableLock)
                {
                    assemblyTable.TryGetValue(typeLibraryID, out asm);
                    if (asm == null)
                    {
                        typeLibrary = GettypeLibrary(typeLibraryID, version, parseVersionAsHex);
                        object opaqueData = null;
                        typeLibrary.GetCustData(ref clrAssemblyCustomID, out opaqueData);
                        if (opaqueData == null)
                            generateNativeAssembly = true;      // No custom data for this IID this is not a CLR typeLibrary
                        String assembly = opaqueData as String;
                        if (String.IsNullOrEmpty(assembly))
                            generateNativeAssembly = true;      // No custom data for this IID this is not a CLR typeLibrary
                        if (noAssemblyGeneration && generateNativeAssembly)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NativeTypeLibraryNotAllowed, typeLibraryID)));
                        else if (!generateNativeAssembly)
                        {
                            ComPlusTLBImportTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationTLBImportFromAssembly,
                                          SR.TraceCodeComIntegrationTLBImportFromAssembly, iid, typeLibraryID, assembly);
                            asm = Assembly.Load(assembly);            // Assembly.Load will get a full assembly name
                        }
                        else
                        {
                            ComPlusTLBImportTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationTLBImportFromTypelib,
                                               SR.TraceCodeComIntegrationTLBImportFromTypelib, iid, typeLibraryID);
                            asm = TypeLibraryHelper.GenerateAssemblyFromNativeTypeLibrary(iid, typeLibraryID, typeLibrary as ITypeLib);
                        }

                        assemblyTable[typeLibraryID] = asm;
                    }
                }
            }
            catch (Exception e)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                    (ushort)System.Runtime.Diagnostics.EventLogCategory.ComPlus,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.ComPlusTLBImportError,
                    iid.ToString(),
                    typeLibraryID.ToString(),
                    e.ToString());
                throw;
            }
            finally
            {

                // Add Try Finally to cleanup typeLibrary
                if (typeLibrary != null)
                    Marshal.ReleaseComObject((object)typeLibrary);
            }

            if (null == asm)
            {
                throw Fx.AssertAndThrow("Assembly should not be null");
            }
            ComPlusTLBImportTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationTLBImportFinished,
                               SR.TraceCodeComIntegrationTLBImportFinished, iid, typeLibraryID);
            return asm;
        }
        private bool NoCoClassAttributeOnType(ICustomAttributeProvider attrProvider)
        {
            object[] attrs = System.ServiceModel.Description.ServiceReflector.GetCustomAttributes(attrProvider, typeof(CoClassAttribute), false);
            if (attrs.Length == 0)
                return true;
            else
                return false;
        }

        Assembly ITypeCacheManager.ResolveAssembly(Guid assembly)
        {
            Assembly ret = null;

            lock (assemblyTableLock)
            {
                this.assemblyTable.TryGetValue(assembly, out ret);
            }

            return ret;
        }

        void ITypeCacheManager.FindOrCreateType(Guid typeLibId, string typeLibVersion, Guid typeDefId, out Type userDefinedType, bool noAssemblyGeneration)
        {
            lock (typeTableLock)
            {
                typeTable.TryGetValue(typeDefId, out userDefinedType);
                if (userDefinedType == null)
                {
                    Assembly asm = ResolveAssemblyFromTypeLibID(Guid.Empty, typeLibId, typeLibVersion, false, noAssemblyGeneration);
                    foreach (Type t in asm.GetTypes())
                    {
                        if (t.GUID == typeDefId)
                        {
                            if (t.IsValueType)
                            {
                                userDefinedType = t;
                                break;
                            }
                        }
                    }
                    if (userDefinedType == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UdtNotFoundInAssembly, typeDefId)));

                    typeTable[typeDefId] = userDefinedType;

                }
            }
        }


        public void FindOrCreateType(Guid iid, out Type interfaceType, bool noAssemblyGeneration, bool isServer)
        {
            lock (typeTableLock)
            {
                typeTable.TryGetValue(iid, out interfaceType);
                if (interfaceType == null)
                {
                    Type coClassInterface = null;
                    Assembly asm = ResolveAssemblyFromIID(iid, noAssemblyGeneration, isServer);
                    foreach (Type t in asm.GetTypes())
                    {
                        if (t.GUID == iid)
                        {
                            if (t.IsInterface && NoCoClassAttributeOnType(t))
                            {
                                interfaceType = t;
                                break;

                            }
                            else if (t.IsInterface && !NoCoClassAttributeOnType(t))
                            {
                                coClassInterface = t;
                            }
                        }

                    }
                    if ((interfaceType == null) && (coClassInterface != null))
                        interfaceType = coClassInterface;
                    else if (interfaceType == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InterfaceNotFoundInAssembly)));

                    typeTable[iid] = interfaceType;
                }
            }
        }
        void ITypeCacheManager.FindOrCreateType(Type serverType, Guid iid, out Type interfaceType, bool noAssemblyGeneration, bool isServer)
        {
            interfaceType = null;
            if (serverType == null)
                FindOrCreateType(iid, out interfaceType, noAssemblyGeneration, isServer);
            else
            {
                if (!serverType.IsClass)
                {
                    throw Fx.AssertAndThrow("This should be a class");
                }
                foreach (Type interfaceInType in serverType.GetInterfaces())
                {
                    if (interfaceInType.GUID == iid)
                    {
                        interfaceType = interfaceInType;
                        break;
                    }
                }
                if (interfaceType == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InterfaceNotFoundInAssembly)));
            }

        }

        public static Type ResolveClsidToType(Guid clsid)
        {
            string keyName = String.Concat("software\\classes\\clsid\\{", clsid.ToString(), "}\\InprocServer32");
            using (RegistryKey clsidKey = Registry.LocalMachine.OpenSubKey(keyName, false))
            {
                if (clsidKey != null)
                {
                    using (RegistryKey assemblyKey = clsidKey.OpenSubKey(typeof(TypeCacheManager).Assembly.ImageRuntimeVersion))
                    {
                        string assemblyName = null;
                        if (assemblyKey == null)
                        {
                            keyName = null;
                            foreach (string subKeyName in clsidKey.GetSubKeyNames())
                            {
                                keyName = subKeyName;
                                if (String.IsNullOrEmpty(keyName))
                                    continue;
                                using (RegistryKey assemblyKeyAny = clsidKey.OpenSubKey(keyName))
                                {
                                    assemblyName = (string)assemblyKeyAny.GetValue("Assembly");
                                    if (String.IsNullOrEmpty(assemblyName))
                                        continue;
                                    else
                                        break;
                                }
                            }
                        }
                        else
                        {
                            assemblyName = (string)assemblyKey.GetValue("Assembly");
                        }
                        if (String.IsNullOrEmpty(assemblyName))
                            return null;
                        Assembly asm = Assembly.Load(assemblyName);
                        foreach (Type type in asm.GetTypes())
                        {
                            if (type.IsClass && (type.GUID == clsid))
                                return type;
                        }
                        return null;
                    }
                }

            }
            // We failed to get the hive information from a native process hive lets go for the alternative bitness

            using (RegistryHandle hkcr = RegistryHandle.GetBitnessHKCR(IntPtr.Size == 8 ? false : true))
            {
                if (hkcr != null)
                {
                    using (RegistryHandle clsidKey = hkcr.OpenSubKey(String.Concat("CLSID\\{", clsid.ToString(), "}\\InprocServer32")))
                    {
                        using (RegistryHandle assemblyKey = clsidKey.OpenSubKey(typeof(TypeCacheManager).Assembly.ImageRuntimeVersion))
                        {
                            string assemblyName = null;
                            if (assemblyKey == null)
                            {
                                keyName = null;
                                foreach (string subKeyName in clsidKey.GetSubKeyNames())
                                {
                                    keyName = subKeyName;
                                    if (String.IsNullOrEmpty(keyName))
                                        continue;
                                    using (RegistryHandle assemblyKeyAny = clsidKey.OpenSubKey(keyName))
                                    {
                                        assemblyName = (string)assemblyKeyAny.GetStringValue("Assembly");
                                        if (String.IsNullOrEmpty(assemblyName))
                                            continue;
                                        else
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                assemblyName = assemblyKey.GetStringValue("Assembly");
                            }
                            if (String.IsNullOrEmpty(assemblyName))
                                return null;
                            Assembly asm = Assembly.Load(assemblyName);
                            foreach (Type type in asm.GetTypes())
                            {
                                if (type.IsClass && (type.GUID == clsid))
                                    return type;
                            }
                            return null;
                        }
                    }
                }

            }
            return null;
        }

        internal Type VerifyType(Guid iid)
        {
            Type interfaceType;
            ((ITypeCacheManager)(this)).FindOrCreateType(iid, out interfaceType, false, true);
            return interfaceType;
        }
    }
}
