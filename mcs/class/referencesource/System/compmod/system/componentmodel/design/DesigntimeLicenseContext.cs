//------------------------------------------------------------------------------
// <copyright file="DesigntimeLicenseContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design {
    using System.Runtime.Remoting;
    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    using System.Net;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>
    ///       Provides design-time support for licensing.
    ///    </para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public class DesigntimeLicenseContext : LicenseContext {
        internal Hashtable savedLicenseKeys = new Hashtable();

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the license usage mode.
        ///    </para>
        /// </devdoc>
        public override LicenseUsageMode UsageMode { 
            get {
                return LicenseUsageMode.Designtime;
            }
        }
        /// <devdoc>
        ///    <para>
        ///       Gets a saved license key.
        ///    </para>
        /// </devdoc>
        public override string GetSavedLicenseKey(Type type, Assembly resourceAssembly) {
            return null;
        }
        /// <devdoc>
        ///    <para>
        ///       Sets a saved license key.
        ///    </para>
        /// </devdoc>
        public override void SetSavedLicenseKey(Type type, string key) {
            savedLicenseKeys[type.AssemblyQualifiedName] = key;
        }
    }

    [HostProtection(SharedState = true)]
    internal class RuntimeLicenseContext : LicenseContext
    {
        private static TraceSwitch RuntimeLicenseContextSwitch = new TraceSwitch("RuntimeLicenseContextTrace", "RuntimeLicenseContext tracing");
        const int ReadBlock = 400;

        internal Hashtable savedLicenseKeys;

        /// <devdoc>
        ///     This method takes a file URL and converts it to a local path.  The trick here is that
        ///     if there is a '#' in the path, everything after this is treated as a fragment.  So
        ///     we need to append the fragment to the end of the path.
        /// </devdoc>
        private string GetLocalPath(string fileName) {
            System.Diagnostics.Debug.Assert(fileName != null && fileName.Length > 0, "Cannot get local path, fileName is not valid");

            Uri uri = new Uri(fileName);
            return uri.LocalPath + uri.Fragment;
        }

        public override string GetSavedLicenseKey(Type type, Assembly resourceAssembly) {

            if (savedLicenseKeys == null || savedLicenseKeys[type.AssemblyQualifiedName]==null) {
                Debug.WriteLineIf(RuntimeLicenseContextSwitch.TraceVerbose, "savedLicenseKey is null or doesnt contain our type");
                if (savedLicenseKeys == null) {
                    savedLicenseKeys = new Hashtable();
                }
                
                Uri licenseFile = null;

                if (resourceAssembly == null) {
                    Debug.WriteLineIf(RuntimeLicenseContextSwitch.TraceVerbose,"resourceAssembly is null");
                    string rawFile = (string)AppDomain.CurrentDomain.SetupInformation.LicenseFile;
                    Debug.WriteLineIf(RuntimeLicenseContextSwitch.TraceVerbose,"rawfile: " + rawFile);
                    string codeBase;
                    
                    // FileIOPermission is required for ApplicationBase in URL-hosted domains
                    FileIOPermission perm = new FileIOPermission(PermissionState.Unrestricted);
                    perm.Assert();
                    try {
                        codeBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                    }
                    finally {
                        CodeAccessPermission.RevertAssert();
                    }
                    if (rawFile != null && codeBase != null) {
                        licenseFile = new Uri(new Uri(codeBase), rawFile);
                    }
                }

                if (licenseFile == null) {
                    if(resourceAssembly == null) {
                        resourceAssembly = Assembly.GetEntryAssembly();
                    }

                    if (resourceAssembly == null) {
                        Debug.WriteLineIf(RuntimeLicenseContextSwitch.TraceVerbose,"resourceAssembly is null");
                        // If Assembly.EntryAssembly returns null, then we will 
                        // try everything!
                        // 
                        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {

                            // Though, I could not repro this, we seem to be hitting an AssemblyBuilder
                            // when walking through all the assemblies in the current app domain. This throws an 
                            // exception on Assembly.CodeBase and we bail out. Catching exceptions here is not a 
                            // bad thing.
                            if (asm.IsDynamic)
                                continue;

                            // file://fullpath/foo.exe
                            //
                            string fileName;
                            FileIOPermission perm = new FileIOPermission(PermissionState.Unrestricted);
                            perm.Assert();
                            try
                            {
                                fileName = GetLocalPath(asm.EscapedCodeBase);
                                fileName = new FileInfo(fileName).Name;
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }

                            Stream s = asm.GetManifestResourceStream(fileName + ".licenses");
                            if (s == null) {
                                //Since the casing may be different depending on how the assembly was loaded, 
                                //we'll do a case insensitive lookup for this manifest resource stream...
                                s = CaseInsensitiveManifestResourceStreamLookup(asm, fileName + ".licenses");
                            }
                            
                            if (s != null) {
                                DesigntimeLicenseContextSerializer.Deserialize(s, fileName.ToUpper(CultureInfo.InvariantCulture), this);
                                break;
                            }
                        }
                    }
                    else if(!resourceAssembly.IsDynamic) { // EscapedCodeBase won't be supported by emitted assemblies anyway
                        Debug.WriteLineIf(RuntimeLicenseContextSwitch.TraceVerbose,"resourceAssembly is not null");
                        string fileName;
                        FileIOPermission perm = new FileIOPermission(PermissionState.Unrestricted);
                        perm.Assert();
                        try
                        {
                            fileName = GetLocalPath(resourceAssembly.EscapedCodeBase);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                        fileName = Path.GetFileName(fileName); // we don't want to use FileInfo here... it requests FileIOPermission that we
                        // might now have... see VSWhidbey 527758
                        string licResourceName = fileName + ".licenses";
                        // first try the filename
                        Stream s = resourceAssembly.GetManifestResourceStream(licResourceName);
                        if (s == null) {
                            string resolvedName = null;
                            CompareInfo comparer = CultureInfo.InvariantCulture.CompareInfo;
                            string shortAssemblyName = resourceAssembly.GetName().Name;
                            //  if the assembly has been renamed, we try our best to find a good match in the available resources
                            // by looking at the assembly name (which doesn't change even after a file rename) + ".exe.licenses" or + ".dll.licenses"
                            foreach(String existingName in resourceAssembly.GetManifestResourceNames()) {
                                if (comparer.Compare(existingName, licResourceName, CompareOptions.IgnoreCase) == 0 ||
                                 comparer.Compare(existingName, shortAssemblyName + ".exe.licenses", CompareOptions.IgnoreCase) == 0 || 
                                 comparer.Compare(existingName, shortAssemblyName + ".dll.licenses", CompareOptions.IgnoreCase) == 0) {
                                    resolvedName = existingName;
                                    break;
                                }
                            }
                            if (resolvedName != null) {
                                s = resourceAssembly.GetManifestResourceStream(resolvedName);
                            }
                        }
                        if (s != null) {
                            DesigntimeLicenseContextSerializer.Deserialize(s, fileName.ToUpper(CultureInfo.InvariantCulture), this);
                        }
                    }
                }


                if (licenseFile != null) {
                    Debug.WriteLineIf(RuntimeLicenseContextSwitch.TraceVerbose,"licenseFile: " + licenseFile.ToString());
                    Debug.WriteLineIf(RuntimeLicenseContextSwitch.TraceVerbose,"opening licenses file over URI " + licenseFile.ToString());
                    Stream s = OpenRead(licenseFile);
                    if (s != null) {
                        string[] segments = licenseFile.Segments;
                        string licFileName = segments[segments.Length - 1];
                        string key = licFileName.Substring(0, licFileName.LastIndexOf("."));
                        DesigntimeLicenseContextSerializer.Deserialize(s, key.ToUpper(CultureInfo.InvariantCulture), this);
                    }
                }

            }
            Debug.WriteLineIf(RuntimeLicenseContextSwitch.TraceVerbose,"returning : " + (string)savedLicenseKeys[type.AssemblyQualifiedName]);
            return(string)savedLicenseKeys[type.AssemblyQualifiedName];
        }

        /**
        * Looks up a .licenses file in the assembly manifest using 
        * case-insensitive lookup rules.  We do this because the name
        * we are attempting to locate could have different casing 
        * depending on how the assembly was loaded.
        **/
        private Stream CaseInsensitiveManifestResourceStreamLookup(Assembly satellite, string name)
        {
            CompareInfo comparer = CultureInfo.InvariantCulture.CompareInfo;
            
            //loop through the resource names in the assembly
            // we try to handle the case where the assembly file name has been renamed
            // by trying to guess the original file name based on the assembly name...
            string assemblyShortName = satellite.GetName().Name;
            foreach(string existingName in satellite.GetManifestResourceNames()) {
                if (comparer.Compare(existingName, name, CompareOptions.IgnoreCase) == 0 ||
                    comparer.Compare(existingName, assemblyShortName + ".exe.licenses") == 0 ||
                    comparer.Compare(existingName, assemblyShortName + ".dll.licenses") == 0) {
                    name = existingName;
                    break;
                }
            }

            //finally, attempt to return our stream based on the 
            //case insensitive match we found
            //
            return satellite.GetManifestResourceStream(name);
        }

        static Stream OpenRead(Uri resourceUri) {
            Stream result = null;

            PermissionSet perms = new PermissionSet(PermissionState.Unrestricted);

            perms.Assert();
            try {
                WebClient webClient = new WebClient();
                webClient.Credentials = CredentialCache.DefaultCredentials;
                result = webClient.OpenRead(resourceUri.ToString());
            }
            catch (Exception e) {
                Debug.Fail(e.ToString());
            }
            finally {
                CodeAccessPermission.RevertAssert();
            }

            return result;
        }
    }
}

    
