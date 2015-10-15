using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Permissions;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Util;
using Microsoft.Build.Utilities;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Microsoft.Win32;

using FrameworkName=System.Runtime.Versioning.FrameworkName;

namespace System.Web.Compilation {
    internal class MultiTargetingUtil {

        // Well-known previous versions
        static internal readonly FrameworkName FrameworkNameV20 = CreateFrameworkName(".NETFramework,Version=v2.0");
        static internal readonly FrameworkName FrameworkNameV30 = CreateFrameworkName(".NETFramework,Version=v3.0");
        static internal readonly FrameworkName FrameworkNameV35 = CreateFrameworkName(".NETFramework,Version=v3.5");
        static internal readonly FrameworkName FrameworkNameV40 = CreateFrameworkName(".NETFramework,Version=v4.0");
        static internal readonly FrameworkName FrameworkNameV45 = CreateFrameworkName(".NETFramework,Version=v4.5");
        
        internal static Version Version40 = new Version(4, 0);
        internal static Version Version35 = new Version(3, 5);
        private static FrameworkName s_targetFrameworkName = null;
        private static string s_configTargetFrameworkMoniker = null;
        private static object s_configTargetFrameworkMonikerLock = new object();
        private static bool s_initializedConfigTargetFrameworkMoniker = false;
        private static object s_targetFrameworkNameLock = new object();
        private static string s_configTargetFrameworkAttributeName = "targetFramework";

        /// <summary>
        /// Latest framework version. 
        /// </summary>
        private static FrameworkName s_latestFrameworkName = null;

        private static List<FrameworkName> s_knownFrameworkNames = null;
        
        /// <summary>
        /// Returns the target framework moniker, eg ".NETFramework,Version=3.5"
        /// </summary>
        internal static FrameworkName TargetFrameworkName {
            get {
                EnsureFrameworkNamesInitialized();
                return s_targetFrameworkName;
            }
            set {
                s_targetFrameworkName = value;
            }
        }

        /// <summary>
        /// Returns the current latest known framework moniker, eg ".NETFramework,Version=4.0"
        /// </summary>
        internal static FrameworkName LatestFrameworkName {
            get {
                EnsureFrameworkNamesInitialized();
                return s_latestFrameworkName;
            }
        }

        internal static List<FrameworkName> KnownFrameworkNames {
            get {
                EnsureFrameworkNamesInitialized();
                return s_knownFrameworkNames;
            }
        }

        internal static void EnsureFrameworkNamesInitialized() {
            if (s_targetFrameworkName == null) {
                lock (s_targetFrameworkNameLock) {
                    if (s_targetFrameworkName == null) {
                        InitializeKnownAndLatestFrameworkNames();
                        InitializeTargetFrameworkName();
                        Debug.Assert(s_targetFrameworkName != null, "s_targetFrameworkName should not be null");
                    }
                }
            }
        }

        /// <summary>
        /// Finds out what the known framework names and also the latest one
        /// </summary>
        private static void InitializeKnownAndLatestFrameworkNames() {
            IList<string> names = ToolLocationHelper.GetSupportedTargetFrameworks();
            Version latestVersion = null;
            s_knownFrameworkNames = new List<FrameworkName>();
            foreach (string name in names) {
                FrameworkName frameworkName = new FrameworkName(name);
                s_knownFrameworkNames.Add(frameworkName);
                Version version = GetFrameworkNameVersion(frameworkName);
                if (s_latestFrameworkName == null || latestVersion < version) {
                    s_latestFrameworkName = frameworkName;
                    latestVersion = version;
                }
            }
        }

        /// <summary>
        /// Returns the string for the target framework as specified in the
        /// config.
        /// </summary>
        internal static string ConfigTargetFrameworkMoniker {
            get {
                if (!s_initializedConfigTargetFrameworkMoniker) {
                    lock (s_configTargetFrameworkMonikerLock) {
                        if (!s_initializedConfigTargetFrameworkMoniker) {
                            RuntimeConfig appConfig = RuntimeConfig.GetAppConfig();
                            CompilationSection compConfig = appConfig.Compilation;

                            string targetFramework = compConfig.TargetFramework;
                            if (targetFramework != null) {
                                targetFramework = targetFramework.Trim();
                            }

                            s_configTargetFrameworkMoniker = targetFramework;
                            s_initializedConfigTargetFrameworkMoniker = true;
                        }
                    }
                }
                return s_configTargetFrameworkMoniker;
            }
        }

        /// <summary>
        /// Checks what is the target framework version and initializes the targetFrameworkName
        /// </summary>
        private static void InitializeTargetFrameworkName() {
            string targetFrameworkMoniker = ConfigTargetFrameworkMoniker;

            // Check if web.config exists, and if not, assume 4.0
            if (!WebConfigExists) {
                s_targetFrameworkName = FrameworkNameV40;
                ValidateCompilerVersionFor40AndAbove();
            }
            else if (targetFrameworkMoniker == null) {
                if (BuildManagerHost.SupportsMultiTargeting) {
                    // We check for null because the user could have specified 
                    // an empty string.
                    // TargetFrameworkMoniker was not specified in config, 
                    // so we need to check codedom settings.
                    InitializeTargetFrameworkNameFor20Or35();
                } else {
                    // We are running in a 4.0 application pool or in the aspnet_compiler,
                    // but the target framework moniker is not specified.
                    // Assume it is 4.0 so that the application can run.
                    s_targetFrameworkName = FrameworkNameV40;
                }
            } else {
                // The targetFrameworkMonike is specified, so we need to validate it.
                InitializeTargetFrameworkNameFor40AndAbove(targetFrameworkMoniker);
            }
        }

        /// <summary>
        /// Verifies that the moniker is valid, and that the version is 4.0 and above.
        /// </summary>
        private static void ValidateTargetFrameworkMoniker(string targetFrameworkMoniker) {
            CompilationSection compConfig = RuntimeConfig.GetAppConfig().Compilation;
            int lineNumber = compConfig.ElementInformation.LineNumber;
            string source = compConfig.ElementInformation.Source;
            try {
                string moniker = targetFrameworkMoniker;
                // Try treating it as a version, eg "4.0" first.
                Version v = GetVersion(targetFrameworkMoniker);
                if (v != null) {
                    // If it is of the form "4.0", construct the full moniker string,
                    // eg ".NETFramework,Version=v4.0"
                    moniker = ".NETFramework,Version=v" + moniker;
                }
                s_targetFrameworkName = CreateFrameworkName(moniker);
            }
            catch (ArgumentException e) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_target_framework_version, 
                    s_configTargetFrameworkAttributeName, targetFrameworkMoniker, e.Message), source, lineNumber);
            }
            Version ver = GetFrameworkNameVersion(s_targetFrameworkName);
            if (ver < Version40) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_lower_target_version, s_configTargetFrameworkAttributeName),
                    source, lineNumber);
            }

            // Check the specified version is no higher than the latest known framework for which we have
            // reference assemblies installed.
            Version latestVersion = GetFrameworkNameVersion(LatestFrameworkName);
            if (latestVersion != null && latestVersion >= ver) {
                // If the specified version is lower than the latest version installed,
                // we are fine.
                return;
            }

            // NOTE: This check is not entirely correct. See comments in GetInstalledTargetVersion().
            // It might be possible that the actual installed (runtime) version is of a higher version,
            // but the reference assemblies are not installed, so latestFrameworkName might be lower. 
            // In that case we also need to check the registry key.
            int majorVersion = ver.Major;
            Version installedTargetVersion = GetInstalledTargetVersion(majorVersion);
            if (installedTargetVersion != null && installedTargetVersion >= ver) {
                return;
            }

            if (IsSupportedVersion(s_targetFrameworkName)) {
                return;
            }

            // If the above checks failed, report that the version is invalid, higher than expected
            throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_higher_target_version, s_configTargetFrameworkAttributeName), source, lineNumber);
        }

        [RegistryPermission(SecurityAction.Assert, Unrestricted = true)]
        private static Version GetInstalledTargetVersion(int majorVersion) {
            // NOTE: This code is wrong to assume "Full", but it is left as is to avoid
            // introducing any breaking change. The mitigation is handled by IsSupportedVersion which
            // is more flexible with regards to framework profile.

            // registry key is of the form:
            // [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full]
            // "TargetVersion"="4.0.0"
            // The path includes the major version, eg "v4" or "v5", so we need to use a parameter.
            string path = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v" + majorVersion + @"\Full";
            try {
                object o = Registry.GetValue(path, "TargetVersion", null);
                string targetVersion = o as string;
                if (!string.IsNullOrEmpty(targetVersion)) {
                    Version ver = new Version(targetVersion);
                    return ver;
                }
            }
            catch { // ignore exceptions
            }
            return null;
        }

        [RegistryPermission(SecurityAction.Assert, Unrestricted = true)]
        private static bool IsSupportedVersion(FrameworkName frameworkName) {
            // Look under the following registry to get the list of supported keys, and check for matching
            // identifier and version.
            // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\v4.0.30319\SKUs\[TFM]
            try {
                var name = new FrameworkName(frameworkName.Identifier, frameworkName.Version);
                var runtime = Environment.Version;
                var runtimeVersion = runtime.Major + "." + runtime.Minor + "." + runtime.Build;
                string path = @"SOFTWARE\Microsoft\.NETFramework\v" + runtimeVersion + @"\SKUs";
                var baseKey = Registry.LocalMachine.OpenSubKey(path);
                foreach (string subKey in baseKey.GetSubKeyNames()) {
                    try {
                        var subKeyName = CreateFrameworkName(subKey);
                        var supportedName = new FrameworkName(subKeyName.Identifier, subKeyName.Version);
                        if (String.Equals(name.FullName, supportedName.FullName, StringComparison.OrdinalIgnoreCase)) {
                            return true;
                        }
                    }
                    catch {
                        continue;
                    }
                }
            }
            catch {
            }
            return false;
        }

        /// <summary>
        /// Checks whether the application web.config exists or not
        /// </summary>
        private static bool WebConfigExists {
            get {
                VirtualPath vpath = HttpRuntime.AppDomainAppVirtualPathObject;
                if (vpath != null) {
                    string path = vpath.SimpleCombine(HttpConfigurationSystem.WebConfigFileName).MapPath();
                    return System.IO.File.Exists(path);
                }
                return false;
            }
        }

        /// <summary>
        /// Returns the higher compilerVersion specified in codedom for the case when targeting 2.0/3.5.
        /// Either "v3.5" is returned, or "v2.0" is returned if the compilerVersion
        /// is anything other that "v3.5". This is because the root web.config has compilerVersion=v4.0. If we
        /// know that we are compiling for 2.0 or 3.5, then we override the value to 2.0 if it is not 3.5.
        /// </summary>
        private static string GetCompilerVersionFor20Or35() {
            string vbCompilerVersion = GetCSharpCompilerVersion();
            string csharpCompilerVersion = GetVisualBasicCompilerVersion();

            // The root web.config will have compilerVersion=4.0, so if we are targeting 2.0 or 3.5, we need to 
            // use compilerVersion=2.0 if the compilerVersion is NOT 3.5.
            vbCompilerVersion = ReplaceCompilerVersionFor20Or35(vbCompilerVersion);
            csharpCompilerVersion = ReplaceCompilerVersionFor20Or35(csharpCompilerVersion);

            Version vbVersion = CompilationUtil.GetVersionFromVString(vbCompilerVersion);
            Version csVersion = CompilationUtil.GetVersionFromVString(csharpCompilerVersion);

            // Return the larger value as the intended version
            if (vbVersion > csVersion) {
                return vbCompilerVersion;
            }
            return csharpCompilerVersion;
        }

        /// <summary>
        /// Checks codedom settings to determine whether we are targeting 2.0 or 3.5.
        /// </summary>
        private static void InitializeTargetFrameworkNameFor20Or35() {
            string compilerVersion = GetCompilerVersionFor20Or35();

            // Make sure the compiler version is either 2.0 or 3.5
            if (CompilationUtil.IsCompilerVersion35(compilerVersion)) {
                s_targetFrameworkName = FrameworkNameV35;
            }
            else if (compilerVersion == "v2.0" || compilerVersion == null) {
                // If the compiler version is null, it means the user did not set it
                // in the codedom section.
                // We use 3.0 because it is not possible to distinguish between 2.0 and 3.0
                // by just looking at web.config.
                s_targetFrameworkName = FrameworkNameV30;
            }
            else {
                throw new ConfigurationErrorsException(SR.GetString(SR.Compiler_version_20_35_required, s_configTargetFrameworkAttributeName));
            }
        }

        /// <summary>
        /// If the compilerVersion is anything other than "v3.5", return "v2.0".
        /// </summary>
        private static string ReplaceCompilerVersionFor20Or35(string compilerVersion) {
            if (CompilationUtil.IsCompilerVersion35(compilerVersion)) {
                return compilerVersion;
            }
            return "v2.0";
        }

        private static string GetCSharpCompilerVersion() {
            return CompilationUtil.GetCompilerVersion(typeof(CSharpCodeProvider));
        }

        private static string GetVisualBasicCompilerVersion() {
            return CompilationUtil.GetCompilerVersion(typeof(VBCodeProvider));
        }

        private static void ReportInvalidCompilerVersion(string compilerVersion) {
            throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_attribute_value, compilerVersion, CompilationUtil.CodeDomProviderOptionPath + "CompilerVersion"));
        }

        private static void InitializeTargetFrameworkNameFor40AndAbove(string targetFrameworkMoniker) {
            ValidateTargetFrameworkMoniker(targetFrameworkMoniker);
            ValidateCompilerVersionFor40AndAbove();
        }

        /// <summary>
        /// Ensures that the compiler version is 4.0 and above.
        /// </summary>
        private static void ValidateCompilerVersionFor40AndAbove() {
            // Since the root web.config already specifies 4.0, we need to make sure both compilerVersions
            // are actually greater than or equal to 4.0, in case the user only sets compilerVersion=3.5
            // for one language. (Dev10 
            ValidateCompilerVersionFor40AndAbove(GetCSharpCompilerVersion());
            ValidateCompilerVersionFor40AndAbove(GetVisualBasicCompilerVersion());
        }

        private static void ValidateCompilerVersionFor40AndAbove(string compilerVersion) {
            if (compilerVersion != null) {
                Exception exception = null;
                if (compilerVersion.Length < 4 || compilerVersion[0] != 'v') {
                    ReportInvalidCompilerVersion(compilerVersion);
                }
                try {
                    Version version = CompilationUtil.GetVersionFromVString(compilerVersion);
                    if (version < Version40) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Compiler_version_40_required, s_configTargetFrameworkAttributeName));
                    }
                }
                catch (ArgumentNullException e) {
                    exception = e;
                }
                catch (ArgumentOutOfRangeException e) {
                    exception = e;
                }
                catch (ArgumentException e) {
                    exception = e;
                }
                catch (FormatException e) {
                    exception = e;
                }
                catch (OverflowException e) {
                    exception = e;
                }
                if (exception != null) {
                    ReportInvalidCompilerVersion(compilerVersion);
                }
            }
        }

        /// <summary>
        /// Returns true if the target framework version is 3.5.
        /// </summary>
        internal static bool IsTargetFramework35 {
            get {
                return Object.Equals(TargetFrameworkName, FrameworkNameV35);
            }
        }

        /// <summary>
        /// Returns true if the target framework version is 2.0 or 3.0.
        /// </summary>
        internal static bool IsTargetFramework20 {
            get {
                return Object.Equals(TargetFrameworkName, FrameworkNameV20) ||
                    Object.Equals(TargetFrameworkName, FrameworkNameV30);
            }
        }

        // Gets the target framework version as a Version instance.
        internal static Version TargetFrameworkVersion {
            get {
                return GetFrameworkNameVersion(TargetFrameworkName);
            }
        }

        internal static bool IsTargetFramework40OrAbove {
            get {
                return MultiTargetingUtil.TargetFrameworkVersion.Major >= 4;
            }
        }

        internal static bool IsTargetFramework45OrAbove {
            get {
                return IsTargetFramework40OrAbove && TargetFrameworkVersion.Minor >= 5;
            }
        }

        /// <summary>
        /// Enable use of RAR only in CBM scenarios
        /// </summary>
        internal static bool EnableReferenceAssemblyResolution {
            get {
                return BuildManagerHost.InClientBuildManager; // Enable only in CBM scenarios.
            }
        }

        internal static FrameworkName CreateFrameworkName(string name) {
            return new FrameworkName(name);
        }

        private static Version GetFrameworkNameVersion(FrameworkName name) {
            if (name == null) {
                return null;
            }
            return name.Version;
        }

        /// <summary>
        /// Returns a Version instance if possible from the version string. 
        /// Otherwise returns null.
        /// </summary>
        private static Version GetVersion(string version) {
            if (string.IsNullOrEmpty(version) || !char.IsDigit(version[0])) {
                return null;
            }

            try {
                Version ver = new Version(version);
                return ver;
            }
            catch { }
            return null;
        }
    }
}
