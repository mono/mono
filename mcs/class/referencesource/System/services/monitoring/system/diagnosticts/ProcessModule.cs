//------------------------------------------------------------------------------
// <copyright file="ProcessModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.IO;
    using Microsoft.Win32;    
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;
//    using System.Windows.Forms;
    using System.Runtime.Versioning;

    /// <devdoc>
    ///     A process module component represents a DLL or EXE loaded into
    ///     a particular process.  Using this component, you can determine
    ///     information about the module.
    /// </devdoc>
    [Designer("System.Diagnostics.Design.ProcessModuleDesigner, " + AssemblyRef.SystemDesign)]
    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class ProcessModule : Component {
        internal ModuleInfo moduleInfo;
        FileVersionInfo fileVersionInfo;

        /// <devdoc>
        ///     Initialize the module.
        /// </devdoc>
        /// <internalonly/>
        internal ProcessModule(ModuleInfo moduleInfo) {
            this.moduleInfo = moduleInfo;
            GC.SuppressFinalize(this);
        }

        /// <devdoc>
        ///     Make sure we are running on NT.
        /// </devdoc>
        /// <internalonly/>
        internal void EnsureNtProcessInfo() {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new PlatformNotSupportedException(SR.GetString(SR.WinNTRequired));
        }

        /// <devdoc>
        ///     Returns the name of the Module.
        /// </devdoc>
        [MonitoringDescription(SR.ProcModModuleName)]
        public string ModuleName {
            get {
                return moduleInfo.baseName;
            }
        }

        /// <devdoc>
        ///     Returns the full file path for the location of the module.
        /// </devdoc>
        [MonitoringDescription(SR.ProcModFileName)]
        public string FileName {
            [ResourceExposure(ResourceScope.Machine)]
            get {
                return moduleInfo.fileName;
            }
        }

        /// <devdoc>
        ///     Returns the memory address that the module was loaded at.
        /// </devdoc>
        [MonitoringDescription(SR.ProcModBaseAddress)]
        public IntPtr BaseAddress {
            [ResourceExposure(ResourceScope.Process)]
            get {
                return moduleInfo.baseOfDll;
            }
        }

        /// <devdoc>
        ///     Returns the amount of memory required to load the module.  This does
        ///     not include any additional memory allocations made by the module once
        ///     it is running; it only includes the size of the static code and data
        ///     in the module file.
        /// </devdoc>
        [MonitoringDescription(SR.ProcModModuleMemorySize)]
        public int ModuleMemorySize {
            get {
                return moduleInfo.sizeOfImage;
            }
        }

        /// <devdoc>
        ///     Returns the memory address for function that runs when the module is
        ///     loaded and run.
        /// </devdoc>
        [MonitoringDescription(SR.ProcModEntryPointAddress)]
        public IntPtr EntryPointAddress {
            get {
                EnsureNtProcessInfo();
                return moduleInfo.entryPoint;
            }
        }

        /// <devdoc>
        ///     Returns version information about the module.
        /// </devdoc>
        [Browsable(false)]
        public FileVersionInfo FileVersionInfo {
            [ResourceExposure(ResourceScope.Machine)]  // Let's review callers - why do they want this?
            [ResourceConsumption(ResourceScope.Machine)]
            get {
                if (fileVersionInfo == null)
                    fileVersionInfo = FileVersionInfo.GetVersionInfo(FileName);
                return fileVersionInfo;
            }
        }

        public override string ToString() {
            return String.Format(CultureInfo.CurrentCulture, "{0} ({1})", base.ToString(), this.ModuleName);
        }
    }
}
