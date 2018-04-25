//------------------------------------------------------------------------------
// <copyright file="EventlogProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System;
    using System.Configuration;
    using System.Runtime.InteropServices;
    using System.Collections.Specialized;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Text;
    using System.Reflection;
    using System.Security.Permissions;
    using System.IO;
    using System.Runtime.InteropServices.ComTypes;

    [ComImport, Guid("c84f668a-cc3f-11d7-b79e-505054503030"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    public interface IRegiisUtility {

        void ProtectedConfigAction(long actionToPerform,
            [In, MarshalAs(UnmanagedType.LPWStr)] string firstArgument,
            [In, MarshalAs(UnmanagedType.LPWStr)] string secondArgument,
            [In, MarshalAs(UnmanagedType.LPWStr)] string providerName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string appPath,
            [In, MarshalAs(UnmanagedType.LPWStr)] string site,
            [In, MarshalAs(UnmanagedType.LPWStr)] string cspOrLocation,
            int keySize,
            out IntPtr exception);

        void RegisterSystemWebAssembly(int doReg, out IntPtr exception);

        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        void RegisterAsnetMmcAssembly(int doReg, [In, MarshalAs(UnmanagedType.LPWStr)] string assemblyName, [In, MarshalAs(UnmanagedType.LPWStr)] string binaryDirectory, out IntPtr exception);

        void RemoveBrowserCaps(out IntPtr exception);
    }

    public sealed class RegiisUtility : IRegiisUtility {
        // The following two sets of constants are copied from register.cxx

        const int WATSettingLocalOnly = 0;
        const int WATSettingRequireSSL = 1;
        const int WATSettingAuthSettings = 2;
        const int WATSettingAuthMode = 3;
        const int WATSettingMax = 4;

        const int WATValueDoNothing = 0;
        const int WATValueTrue = 1;
        const int WATValueFalse = 2;
        const int WATValueHosted = 3;
        const int WATValueLocal = 4;
        const int WATValueForms = 5;
        const int WATValueWindows = 6;

        // Note: this name has to match the name used in System.Configuration.RsaProtectedConfigurationProvider 
        const string DefaultRsaKeyContainerName = "NetFrameworkConfigurationKey";

        const string NewLine = "\n\r";

        ///////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////
        public void RegisterSystemWebAssembly(int doReg, out IntPtr exception)
        {
            exception = IntPtr.Zero;

            try {
                Assembly               webAssembly = Assembly.GetExecutingAssembly();
                RegistrationServices   rs          = new RegistrationServices();

                if (doReg != 0)
                {
                    if (!rs.RegisterAssembly(webAssembly, AssemblyRegistrationFlags.None))
                        exception = Marshal.StringToBSTR((new Exception(SR.GetString(SR.Unable_To_Register_Assembly, webAssembly.FullName))).ToString());
                }
                else
                {
                    if (!rs.UnregisterAssembly(webAssembly))
                        exception = Marshal.StringToBSTR((new Exception(SR.GetString(SR.Unable_To_UnRegister_Assembly, webAssembly.FullName))).ToString());
                }
            }
            catch (Exception e) {
                exception = Marshal.StringToBSTR(e.ToString());
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void RegisterAsnetMmcAssembly(int doReg, string typeName, string binaryDirectory, out IntPtr exception)
        {
            exception = IntPtr.Zero;

            try
            {
                Assembly webAssembly = Assembly.GetAssembly(Type.GetType(typeName, true));
                RegistrationServices rs = new RegistrationServices();

                if (doReg != 0)
                {
                    if (!rs.RegisterAssembly(webAssembly, AssemblyRegistrationFlags.None))
                        exception = Marshal.StringToBSTR((new Exception(SR.GetString(SR.Unable_To_Register_Assembly, webAssembly.FullName))).ToString());
                    TypeLibConverter converter = new TypeLibConverter();
                    ConversionEventSink eventHandler = new ConversionEventSink();

                    IRegisterCreateITypeLib typeLib = (IRegisterCreateITypeLib)converter.ConvertAssemblyToTypeLib(webAssembly, System.IO.Path.Combine(binaryDirectory, "AspNetMMCExt.tlb"), 0, eventHandler);
                    typeLib.SaveAllChanges();
                }
                else
                {
                    // Consider deleting tlb file
                    if (!rs.UnregisterAssembly(webAssembly))
                        exception = Marshal.StringToBSTR((new Exception(SR.GetString(SR.Unable_To_UnRegister_Assembly, webAssembly.FullName))).ToString());

                    try {
                        File.Delete(System.IO.Path.Combine(binaryDirectory, "AspNetMMCExt.tlb"));
                    }
                    catch {
                    }
                }
            }
            catch (Exception e)
            {
                exception = Marshal.StringToBSTR(e.ToString());
            }
        }

        [ComImport, GuidAttribute("00020406-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false)]
        interface IRegisterCreateITypeLib
        {
            void CreateTypeInfo();
            void SetName();
            void SetVersion();
            void SetGuid();
            void SetDocString();
            void SetHelpFileName();
            void SetHelpContext();
            void SetLcid();
            void SetLibFlags();
            void SaveAllChanges();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////
        private const long DO_RSA_ENCRYPT        =      0x0000000100000000;
        private const long DO_RSA_DECRYPT        =      0x0000000200000000;
        private const long DO_RSA_ADD_KEY        =      0x0000000400000000;
        private const long DO_RSA_DEL_KEY        =      0x0000000800000000;
        private const long DO_RSA_ACL_KEY_ADD    =      0x0000001000000000;
        private const long DO_RSA_ACL_KEY_DEL    =      0x0000002000000000;
        private const long DO_RSA_EXPORT_KEY     =      0x0000004000000000;
        private const long DO_RSA_IMPORT_KEY     =      0x0000008000000000;
        private const long DO_RSA_PKM            =      0x0000080000000000;
        private const long DO_RSA_PKU            =      0x0000100000000000;
        private const long DO_RSA_EXPORTABLE     =      0x0000400000000000;
        private const long DO_RSA_FULL_ACCESS    =      0x0000800000000000;
        private const long DO_RSA_PRIVATE        =      0x0001000000000000;
        private const long DO_RSA_ENCRYPT_FILE   =      0x0004000000000000;
        private const long DO_RSA_DECRYPT_FILE   =      0x0008000000000000;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////
        public void ProtectedConfigAction(long options, string firstArgument, string secondArgument, string providerName, string appPath, string site, string cspOrLocation, int keySize, out IntPtr exception)
        {
            exception = IntPtr.Zero;

            try {
                if ((options & DO_RSA_ENCRYPT) != 0) {
                    DoProtectSection(firstArgument, providerName, appPath, site, cspOrLocation, (options & DO_RSA_PKM) != 0);
                } else if ((options & DO_RSA_DECRYPT) != 0) {
                    DoUnprotectSection(firstArgument, appPath, site, cspOrLocation, (options & DO_RSA_PKM) != 0);
                } else if ((options & DO_RSA_ENCRYPT_FILE) != 0) {
                    DoProtectSectionFile(firstArgument, secondArgument, providerName);
                } else if ((options & DO_RSA_DECRYPT_FILE) != 0) {
                    DoUnprotectSectionFile(firstArgument, secondArgument);
                } else if ((options & DO_RSA_ADD_KEY) != 0) {
                    DoKeyCreate(firstArgument, cspOrLocation, options, keySize);
                } else if ((options & DO_RSA_DEL_KEY) != 0) {
                    DoKeyDelete(firstArgument, cspOrLocation, options);
                } else if ((options & DO_RSA_EXPORT_KEY) != 0) {
                    DoKeyExport(firstArgument, secondArgument, cspOrLocation, options);
                } else if ((options & DO_RSA_IMPORT_KEY) != 0) {
                    DoKeyImport(firstArgument, secondArgument, cspOrLocation, options);
                } else if ((options & DO_RSA_ACL_KEY_ADD) != 0 || (options &  DO_RSA_ACL_KEY_DEL) != 0)  {
                    DoKeyAclChange(firstArgument, secondArgument, cspOrLocation, options);
                } else {
                    exception = Marshal.StringToBSTR(SR.GetString(SR.Command_not_recognized));
                }
            } catch (Exception e) {
                StringBuilder   sb = new StringBuilder();
                GetExceptionMessage(e, sb);
                exception = Marshal.StringToBSTR(sb.ToString());
            }
        }

        private void GetExceptionMessage(Exception exception, StringBuilder sb) {
            if (sb.Length != 0) {
                sb.Append(NewLine);
            }
            
            if (exception is ConfigurationErrorsException) {
                foreach(ConfigurationErrorsException e in ((ConfigurationErrorsException)exception).Errors) {
                    sb.Append(e.Message);
                    sb.Append(NewLine);
                    
                    if (e.InnerException != null) {
                        sb.Append(NewLine);
                        sb.Append(e.InnerException.Message);
                        sb.Append(NewLine);
                    }
                }
            }
            else {
                sb.Append(exception.Message  );
                sb.Append(NewLine);
                
                if (exception.InnerException != null) {
                    GetExceptionMessage(exception.InnerException, sb);
                }
            }
        }

        private void DoProtectSection(string configSection, string providerName, string appPath, string site, string location, bool useMachineConfig)
        {
            Configuration           config;
            ConfigurationSection    section = GetConfigSection(configSection, appPath, site, location, useMachineConfig, out config);
            if (section == null) // Throw an error that the section was not found.
                throw new Exception(SR.GetString(SR.Configuration_Section_not_found, configSection));
            section.SectionInformation.ProtectSection(providerName);
            config.Save();
        }
        private void DoUnprotectSection(string configSection, string appPath, string site, string location, bool useMachineConfig)
        {
            Configuration config;
            ConfigurationSection section = GetConfigSection(configSection, appPath, site, location, useMachineConfig, out config);
            if (section == null) // Throw an error that the section was not found.
                throw new Exception(SR.GetString(SR.Configuration_Section_not_found, configSection));
            section.SectionInformation.UnprotectSection();
            config.Save();
        }
        private void DoProtectSectionFile(string configSection, string dirName, string providerName)
        {
            Configuration           config;
            ConfigurationSection    section = GetConfigSectionFile(configSection, dirName, out config);
            if (section == null) // Throw an error that the section was not found.
                throw new Exception(SR.GetString(SR.Configuration_Section_not_found, configSection));
            section.SectionInformation.ProtectSection(providerName);
            config.Save();
        }
        private void DoUnprotectSectionFile(string configSection, string dirName)
        {
            Configuration config;
            ConfigurationSection section = GetConfigSectionFile(configSection, dirName, out config);
            if (section == null) // Throw an error that the section was not found.
                throw new Exception(SR.GetString(SR.Configuration_Section_not_found, configSection));
            section.SectionInformation.UnprotectSection();
            config.Save();
        }
        private ConfigurationSection GetConfigSectionFile(string configSection, string dirName, out Configuration config)
        {
            if (dirName == ".") {
                dirName = Environment.CurrentDirectory;
            } else {
                if (!Path.IsPathRooted(dirName))
                    dirName = Path.Combine(Environment.CurrentDirectory, dirName);
                if (!Directory.Exists(dirName))
                    throw new Exception(SR.GetString(SR.Configuration_for_physical_path_not_found, dirName));
            }
            WebConfigurationFileMap fileMap = new WebConfigurationFileMap();
            string appVPath = dirName.Replace('\\', '/');
            if (appVPath.Length > 2 && appVPath[1] == ':')
                appVPath = appVPath.Substring(2);
            else if (appVPath.StartsWith("//", StringComparison.Ordinal)) // UNC share?
                appVPath = "/";
            fileMap.VirtualDirectories.Add(appVPath, new VirtualDirectoryMapping(dirName, true));
            try {
                config = WebConfigurationManager.OpenMappedWebConfiguration(fileMap, appVPath);
            }
            catch (Exception e) {
                throw new Exception(SR.GetString(SR.Configuration_for_physical_path_not_found, dirName), e);
            }

            return config.GetSection(configSection);
        }

        private ConfigurationSection GetConfigSection(string configSection, string appPath, string site, string location, bool useMachineConfig, out Configuration config)
        {
            if (string.IsNullOrEmpty(appPath)) {
                appPath = null;
            } else {
                Debug.Assert(appPath.StartsWith("/", StringComparison.Ordinal), "This check is done in main.cxx in regiis");
            }

            if (string.IsNullOrEmpty(location))
                location = null;

            try {
                if (useMachineConfig)
                    config = WebConfigurationManager.OpenMachineConfiguration(location);
                else
                    config = WebConfigurationManager.OpenWebConfiguration(appPath, site, location);
            }
            catch (Exception e) {
                if (useMachineConfig) {
                    throw new Exception(SR.GetString(SR.Configuration_for_machine_config_not_found), e);
                }
                else {
                    throw new Exception(SR.GetString(SR.Configuration_for_path_not_found, appPath, 
                                String.IsNullOrEmpty(site) ? SR.GetString(SR.DefaultSiteName) : site), e);
                }
            }

            return config.GetSection(configSection); 
        }

        private void DoKeyCreate(string containerName, string csp, long options, int keySize)
        {
            if (containerName == null || containerName.Length < 1) {
                containerName = DefaultRsaKeyContainerName;
            }
            
            uint returnHR = (uint)UnsafeNativeMethods.DoesKeyContainerExist(containerName, csp, ((options & DO_RSA_PKU) == 0) ? 1 : 0);
            switch (returnHR) {
                case 0:
                    throw new Exception(SR.GetString(SR.RSA_Key_Container_already_exists));
                case 0x80090016: // Not found -- create it
                    RsaProtectedConfigurationProvider rsaProv = CreateRSAProvider(containerName, csp, options);
                    try {
                        rsaProv.AddKey(keySize, (options & DO_RSA_EXPORTABLE) != 0);
                    } catch {
                        rsaProv.DeleteKey();
                        throw;
                    }
                    return;
                case 0x80070005:
                    throw new Exception(SR.GetString(SR.RSA_Key_Container_access_denied));
                default:
                    Marshal.ThrowExceptionForHR((int)returnHR);
                    return;
            }
        }

        private void DoKeyDelete(string containerName, string csp, long options)
        {
            if (containerName == null || containerName.Length < 1) {
                containerName = DefaultRsaKeyContainerName;
            }
            
            MakeSureContainerExists(containerName, csp, (options & DO_RSA_PKU) == 0);
            RsaProtectedConfigurationProvider rsaProv = CreateRSAProvider(containerName, csp, options);
            rsaProv.DeleteKey();
        }

        private void DoKeyExport(string containerName, string fileName, string csp, long options)
        {
            if (!Path.IsPathRooted(fileName))
                fileName = Path.Combine(Environment.CurrentDirectory, fileName);
            
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fileName)))
                throw new System.IO.DirectoryNotFoundException();

            if (containerName == null || containerName.Length < 1) {
                containerName = DefaultRsaKeyContainerName;
            }
            
            MakeSureContainerExists(containerName, csp, (options & DO_RSA_PKU) == 0);
            RsaProtectedConfigurationProvider rsaProv = CreateRSAProvider(containerName, csp, options);
            rsaProv.ExportKey(fileName, (options & DO_RSA_PRIVATE) != 0);
        }

        private void DoKeyImport(string containerName, string fileName, string csp, long options)
        {
            if (!System.IO.File.Exists(fileName))
                throw new System.IO.FileNotFoundException();

            if (containerName == null || containerName.Length < 1) {
                containerName = DefaultRsaKeyContainerName;
            }
            
            RsaProtectedConfigurationProvider rsaProv = CreateRSAProvider(containerName, csp, options);
            rsaProv.ImportKey(fileName, (options & DO_RSA_EXPORTABLE) != 0);
        }

        private void DoKeyAclChange(string containerName, string account, string csp, long options)
        {
            if (containerName == null || containerName.Length < 1) {
                containerName = DefaultRsaKeyContainerName;
            }
            
            MakeSureContainerExists(containerName, csp, (options & DO_RSA_PKU) == 0);
            int flags = 0;
            if ((options & DO_RSA_ACL_KEY_ADD) != 0)
                flags |= 0x1; // Add access
            if ((options & DO_RSA_PKU) == 0)
                flags |= 0x2;
            if ((options & DO_RSA_FULL_ACCESS) != 0)
                flags |= 0x4;
            int returnHR = UnsafeNativeMethods.ChangeAccessToKeyContainer(containerName, account, csp, flags);
            if (returnHR != 0)
                Marshal.ThrowExceptionForHR(returnHR);
        }

        private RsaProtectedConfigurationProvider CreateRSAProvider(string containerName, string csp, long options)
        {
            RsaProtectedConfigurationProvider prov = new RsaProtectedConfigurationProvider();
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("keyContainerName", containerName);
            nvc.Add("cspProviderName", csp);
            nvc.Add("useMachineContainer", ((options & DO_RSA_PKU) != 0) ? "false" : "true");
            prov.Initialize("foo", nvc);
            return prov;
        }
        private static void MakeSureContainerExists(string containerName, string csp, bool machineContainer) {
            uint returnHR = (uint) UnsafeNativeMethods.DoesKeyContainerExist(containerName, csp, machineContainer ? 1 : 0);
            switch (returnHR) {
                case 0:
                    return; // success!
                case 0x80090016:
                    throw new Exception(SR.GetString(SR.RSA_Key_Container_not_found));
                case 0x80070005:
                    throw new Exception(SR.GetString(SR.RSA_Key_Container_access_denied));
                default:
                    Marshal.ThrowExceptionForHR((int)returnHR);
                    return;
            }
        }

        public void RemoveBrowserCaps(out IntPtr exception) {
            try {
                BrowserCapabilitiesCodeGenerator generator = new BrowserCapabilitiesCodeGenerator();
                generator.UninstallInternal();
                exception = IntPtr.Zero;
            }
            catch (Exception e) {
                exception = Marshal.StringToBSTR(e.Message);
            }
        }
    }

    class ConversionEventSink : ITypeLibExporterNotifySink
    {
        public void ReportEvent(ExporterEventKind eventKind, int eventCode, string eventMsg)
        {
            // Handle the warning event here.
        }

        public Object ResolveRef(Assembly assemblyReference)
        {
            // Resolve the reference here and return a correct type library.
            return null;
        }
    }
}

