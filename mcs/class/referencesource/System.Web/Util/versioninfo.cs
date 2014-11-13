//------------------------------------------------------------------------------
// <copyright file="versioninfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Support for getting file versions
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Util {
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Runtime.Serialization.Formatters;
    using System.Configuration.Assemblies;
    using System.Security.Permissions;

    //
    // Support for getting file version of relevant files
    //

    internal class VersionInfo {
        static private string _engineVersion;
        static private string _mscoreeVersion;
        static private string _exeName;
        static private object _lock = new object();

        private VersionInfo() {
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
        internal static string GetFileVersion(String filename) {
#if !FEATURE_PAL // FEATURE_PAL does not fully support FileVersionInfo
            try {
                FileVersionInfo ver = FileVersionInfo.GetVersionInfo(filename);
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}",
                    ver.FileMajorPart, ver.FileMinorPart, ver.FileBuildPart, ver.FilePrivatePart);
            }
            catch {
                return String.Empty;
            }
#else // !FEATURE_PAL
            // ROTORTODO
            return String.Empty;
#endif // !FEATURE_PAL

        }

        internal static string GetLoadedModuleFileName(string module) {
#if !FEATURE_PAL // FEATURE_PAL does not fully support FileVersionInfo
            IntPtr h = UnsafeNativeMethods.GetModuleHandle(module);
            if (h == IntPtr.Zero)
                return null;

            StringBuilder buf = new StringBuilder(256);
            if (UnsafeNativeMethods.GetModuleFileName(h, buf, 256) == 0)
                return null;

            String fileName = buf.ToString();
            if (StringUtil.StringStartsWith(fileName, "\\\\?\\")) // on Whistler GetModuleFileName migth return this
                fileName = fileName.Substring(4);
            return fileName;
#else // !FEATURE_PAL
            // ROTORTODO
            return String.Empty;
#endif // !FEATURE_PAL
        }

        internal static string GetLoadedModuleVersion(string module) {
            String filename = GetLoadedModuleFileName(module);
            if (filename == null)
                return null;
            return GetFileVersion(filename);
        }

        internal static string SystemWebVersion {
            get {
                return ThisAssembly.InformationalVersion;
            }
        }

        internal static string EngineVersion {
#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
            get {
                if (_engineVersion == null) {
                    lock(_lock) {
                        if (_engineVersion == null)
                            _engineVersion = GetLoadedModuleVersion(ModName.ENGINE_FULL_NAME);
                    }
                }

                return _engineVersion;
#else // !FEATURE_PAL
            // ROTORTODO
            return "1.2.0.0";
#endif // !FEATURE_PAL
            }
        }

        internal static string ClrVersion {
            get {
                if (_mscoreeVersion == null) {
                    lock(_lock) {
                        if (_mscoreeVersion == null) {
                            //@





                            // Substring(1) removes the 'v' character. 
                            _mscoreeVersion =
                                System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion().Substring(1);
                        }
                    }
                }

                return _mscoreeVersion;
            }
        }

        internal static string ExeName {
            get {
                if (_exeName == null) {
                    lock(_lock) {
                        if (_exeName == null) {
                            String s = GetLoadedModuleFileName(null);
                            if (s == null)
                                s = String.Empty;

                            // strip path
                            int i = s.LastIndexOf('\\');
                            if (i >= 0)
                                s = s.Substring(i+1);

                            // strip extension
                            i = s.LastIndexOf('.');
                            if (i >= 0)
                                s = s.Substring(0, i);

                            _exeName = s.ToLower(CultureInfo.InvariantCulture);
                        }
                    }
                }

                return _exeName;
            }
        }
    }

    //
    // Support for getting OS Flavor
    //

    internal enum OsFlavor {
        Undetermined,
        Other,
        WebBlade,
        StdServer,
        AdvServer,
        DataCenter,
    }
}
