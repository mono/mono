using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;

using FrameworkName=System.Runtime.Versioning.FrameworkName;

namespace System.Web.Compilation {

    internal class AssemblyResolutionResult {
        internal ICollection<string> ResolvedFiles {
            get;
            set;
        }

        internal ICollection<string> ResolvedFilesWithWarnings {
            get;
            set;
        }

        internal ICollection<Assembly> UnresolvedAssemblies {
            get;
            set;
        }

        internal ICollection<BuildErrorEventArgs> Errors {
            get;
            set;
        }

        internal ICollection<BuildWarningEventArgs> Warnings {
            get;
            set;
        }
    }

    internal enum ReferenceAssemblyType {
        FrameworkAssembly = 0,
        FrameworkAssemblyOnlyPresentInHigherVersion = 1,
        NonFrameworkAssembly = 2,
    }

    internal class AssemblyResolver {

        /// <summary>
        /// Keeps track of resolved assemblies and their locations. Value is null if the assembly was found only
        /// in a higher version framework.
        /// </summary>
        private static Dictionary<Assembly, string> s_assemblyLocations;
        private static Dictionary<Assembly, AssemblyResolutionResult> s_assemblyResults;
        private static Dictionary<Assembly, ReferenceAssemblyType> s_assemblyTypes;
        private static object s_lock = new object();

        private static IList<string> s_targetFrameworkReferenceAssemblyPaths;
        private static IList<string> s_higherFrameworkReferenceAssemblyPaths;
        private static IList<string> s_fullProfileReferenceAssemblyPaths;
        private static bool? s_needToCheckFullProfile;

        private static bool? s_warnAsError = null;
        private static object s_warnAsErrorLock = new object();

        // Maps physical paths of reference assemblies to their versions as returned by AssemblyName.GetAssemblyName
        private static readonly Lazy<ConcurrentDictionary<string, Version>> s_assemblyVersions =
            new Lazy<ConcurrentDictionary<string, Version>>(
                () => new ConcurrentDictionary<string, Version>(StringComparer.OrdinalIgnoreCase));

        private static IList<string> TargetFrameworkReferenceAssemblyPaths {
            get {
                if (s_targetFrameworkReferenceAssemblyPaths == null) {
                    IList<string> paths = GetPathToReferenceAssemblies(MultiTargetingUtil.TargetFrameworkName);
                    int count = paths.Count;

                    if (MultiTargetingUtil.IsTargetFramework20 || MultiTargetingUtil.IsTargetFramework35) {
                        // Require 3.5 to be installed to be able to target pre-4.0
                        var fxPath35 = ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version35);
                        if (string.IsNullOrEmpty(fxPath35)) {
                            throw new HttpException(SR.GetString(SR.Downlevel_requires_35));
                        }

                        // For 2.0 and 3.5, verify that the reference assemblies are actually present.
                        // For 3.5, make sure the reference assemblies path do not consist of just 2.0 and 3.0 assemblies.
                        IList<string> assemblyPaths30 = GetPathToReferenceAssemblies(MultiTargetingUtil.FrameworkNameV30);
                        IList<string> assemblyPaths20 = GetPathToReferenceAssemblies(MultiTargetingUtil.FrameworkNameV20);
                        bool missing35assemblies = MultiTargetingUtil.IsTargetFramework35 && (assemblyPaths30.Count == count || assemblyPaths20.Count == count);

                        if (count == 0 || missing35assemblies) {
                            throw new HttpException(SR.GetString(SR.Reference_assemblies_not_found));
                        }
                    }
                    else {
                        // When we are performing a build through VS, we require the reference assemblies
                        // to be present.
                        if (BuildManagerHost.SupportsMultiTargeting && count == 0) {
                            throw new HttpException(SR.GetString(SR.Reference_assemblies_not_found));
                        }
                    }

                    s_targetFrameworkReferenceAssemblyPaths = paths;
                }
                return s_targetFrameworkReferenceAssemblyPaths;
            }
        }

        /// <summary>
        /// Returns a list of assembly paths containing assemblies from higher version frameworks.
        /// </summary>
        private static IList<string> HigherFrameworkReferenceAssemblyPaths {
            get {
                if (s_higherFrameworkReferenceAssemblyPaths == null) {
                    List<string> paths = new List<string>();
                    FrameworkName targetName = MultiTargetingUtil.TargetFrameworkName;
                    // Loop through each framework name, and find those that is equal in Identifier and Profile, but
                    // higher than the target version.
                    foreach (FrameworkName name in MultiTargetingUtil.KnownFrameworkNames) {
                        if (string.Equals(name.Identifier, targetName.Identifier, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(name.Profile, targetName.Profile, StringComparison.OrdinalIgnoreCase)) {
                            Version version = name.Version;
                            Version targetVersion = targetName.Version;
                            if (targetVersion < version) {
                                paths.AddRange(GetPathToReferenceAssemblies(name));
                            }
                        }
                    }
                    s_higherFrameworkReferenceAssemblyPaths = paths;
                }
                return s_higherFrameworkReferenceAssemblyPaths;
            }
        }
             
        /// <summary>
        /// Returns a list of assembly paths containing assemblies from full profile framework.
        /// </summary>
        private static IList<string> FullProfileReferenceAssemblyPaths {
            get {
                if (s_fullProfileReferenceAssemblyPaths == null) {
                    List<string> paths = new List<string>();
                    FrameworkName targetName = MultiTargetingUtil.TargetFrameworkName;
                    // Create a copy without the profile to get the full profile.
                    FrameworkName fullName = new FrameworkName(targetName.Identifier, targetName.Version);
                    paths.AddRange(GetPathToReferenceAssemblies(fullName));
                    s_fullProfileReferenceAssemblyPaths = paths;
                }
                return s_fullProfileReferenceAssemblyPaths;
            }
        }

        /// <summary>
        /// Checks whether we need to perform a check against the full profile to determine whether a reference
        /// assembly can be used or not.
        /// </summary>
        private static bool NeedToCheckFullProfile {
            get {
                if (s_needToCheckFullProfile == null) {
                    // Find differences between the two sets of reference assembly paths.
                    var except = FullProfileReferenceAssemblyPaths.Except(TargetFrameworkReferenceAssemblyPaths,
                        StringComparer.OrdinalIgnoreCase);
                    
                    if (except.Count() == 0) {
                        // If everything is the same, then there is no need for an extra check against the
                        // full profile.
                        s_needToCheckFullProfile = false;
                    }
                    else {
                        // If something is different, we will need an additional check against the full
                        // profile.
                        s_needToCheckFullProfile = true;
                    }
                }
                return s_needToCheckFullProfile.Value;
            }
        }

        private static Dictionary<Assembly, string> AssemblyLocations {
            get {
                if (s_assemblyLocations == null) {
                    s_assemblyLocations = new Dictionary<Assembly, string>();
                }
                return s_assemblyLocations;
            }
        }

        private static Dictionary<Assembly, AssemblyResolutionResult> AssemblyResolutionResults {
            get {
                if (s_assemblyResults == null) {
                    s_assemblyResults = new Dictionary<Assembly, AssemblyResolutionResult>();
                }
                return s_assemblyResults;
            }
        }

        private static Dictionary<Assembly, ReferenceAssemblyType> ReferenceAssemblyTypes {
            get {
                if (s_assemblyTypes == null) {
                    s_assemblyTypes = new Dictionary<Assembly, ReferenceAssemblyType>();
                }
                return s_assemblyTypes;
            }
        }

        private static ConcurrentDictionary<string, Version> AssemblyVersions {
            get {
                return s_assemblyVersions.Value;
            }
        }

        /// <summary>
        /// Returns the assembly version of the assembly found at the specified path using AssemblyName.GetAssemblyName.
        /// Returns and stores null if GetAssemblyName throws.
        /// </summary>
        private static Version GetAssemblyVersion(string path) {
            Version version = null;
            var assemblyVersions = AssemblyVersions;
            if (!assemblyVersions.TryGetValue(path, out version)) {
                try {
                    AssemblyName resolvedAssemblyName = AssemblyName.GetAssemblyName(path);
                    version = resolvedAssemblyName.Version;
                } catch {
                    // Ignore any exceptions thrown
                }
                assemblyVersions.TryAdd(path, version);
            }
            return version;
        }

        /// <summary>
        /// Resolve a single assembly using the provided search paths and setting the targetframework directories.
        /// </summary>
        private static AssemblyResolutionResult ResolveAssembly(string assemblyName, IList<string> searchPaths, IList<string> targetFrameworkDirectories, bool checkDependencies) {
            ResolveAssemblyReference rar = new ResolveAssemblyReference();
            MockEngine engine = new MockEngine();
            rar.BuildEngine = engine;
            if (searchPaths != null) {
                rar.SearchPaths = searchPaths.ToArray();
            }
            if (targetFrameworkDirectories != null) {
                rar.TargetFrameworkDirectories = targetFrameworkDirectories.ToArray();
            }
            rar.Assemblies = new ITaskItem[] {
                new TaskItem(assemblyName),
            };
            rar.Silent = true;
            rar.Execute();

            AssemblyResolutionResult result = new AssemblyResolutionResult();

            List<string> resolvedFiles = new List<string>();
            foreach (ITaskItem item in rar.ResolvedFiles) {
                resolvedFiles.Add(item.ItemSpec);
            }
            if (checkDependencies) {
                CheckOutOfRangeDependencies(assemblyName);
            }
            result.ResolvedFiles = resolvedFiles.ToArray();
            result.Warnings = engine.Warnings;
            result.Errors = engine.Errors;
            return result;
        }

        /// <summary>
        /// Check whether an assembly has dependencies to a framework assembly of a higher version,
        /// report the issue as a warning or error.
        /// </summary>
        private static void CheckOutOfRangeDependencies(string assemblyName) {

            string dependencies = null;
            Assembly assembly = Assembly.Load(assemblyName);
            AssemblyName aName = new AssemblyName(assemblyName);

            // If the loaded assembly has a different version than the specified assembly,
            // then it is likely that there was unification or binding redirect in place.
            // If that is the case, then GetReferenceAssemblies won't be accurate for
            // finding the references of the actual assembly, so we skip checking its references.
            if (assembly.GetName().Version != aName.Version) {
                return;
            }

            foreach (AssemblyName name in assembly.GetReferencedAssemblies()) {
                try {
                    Assembly referenceAssembly = CompilationSection.LoadAndRecordAssembly(name);
                    string path;
                    ReferenceAssemblyType referenceAssemblyType =
                        GetPathToReferenceAssembly(referenceAssembly, out path, null, null, false /*checkDependencies*/);

                    // We need to check the following 2 conditions:
                    // 1. If the assembly is available in the target framework, we also need to 
                    // verify that the version being referenced is no higher than what we have
                    // in the target framework.
                    // 2. If the assembly is only available in a higher version framework.
                    Version resolvedAssemblyVersion = GetAssemblyVersion(path);
                    if (resolvedAssemblyVersion == null) {
                        continue;
                    }

                    if ((referenceAssemblyType == ReferenceAssemblyType.FrameworkAssembly && resolvedAssemblyVersion < name.Version)
                        || referenceAssemblyType == ReferenceAssemblyType.FrameworkAssemblyOnlyPresentInHigherVersion) {
                        if (dependencies == null) {
                            dependencies = name.FullName;
                        }
                        else {
                            dependencies += "; " + name.FullName;
                        }
                    }
                }
                catch {
                    // Ignore dependencies that are not found, as we are primarily concerned
                    // with framework assemblies that are on the machine.
                }
            }

            if (dependencies != null) {
                string message = SR.GetString(SR.Higher_dependencies, assemblyName, dependencies);
                ReportWarningOrError(message);
            }
        }

        private static void ReportWarningOrError(string message) {
            if (WarnAsError) {
                // Report the issue as an error.
                throw new HttpCompileException(message);
            }
            else {
                // Report the issue as a compiler warning.
                CompilerError error = new CompilerError();
                error.ErrorText = message;
                error.IsWarning = true;
                if (BuildManager.CBMCallback != null) {
                    BuildManager.CBMCallback.ReportCompilerError(error);
                }
            }
        }


        internal static ReferenceAssemblyType GetPathToReferenceAssembly(Assembly a, out string path) {
            return GetPathToReferenceAssembly(a, out path, null, null);
        }

        private static void StoreResults(Assembly a, string path, AssemblyResolutionResult result, ReferenceAssemblyType assemblyType) {
            lock (s_lock) {
                if (!AssemblyLocations.ContainsKey(a)) {
                    AssemblyLocations.Add(a, path);
                    AssemblyResolutionResults.Add(a, result);
                    ReferenceAssemblyTypes.Add(a, assemblyType);
                }
            }
        }

        internal static ReferenceAssemblyType GetPathToReferenceAssembly(Assembly a, out string path,
            ICollection<BuildErrorEventArgs> errors, ICollection<BuildWarningEventArgs> warnings) {
            return GetPathToReferenceAssembly(a, out path, errors, warnings, true /*checkDependencies*/);
        }

        internal static ReferenceAssemblyType GetPathToReferenceAssembly(Assembly a, out string path,
            ICollection<BuildErrorEventArgs> errors, ICollection<BuildWarningEventArgs> warnings,
            bool checkDependencies) {

            lock (s_lock) {
                if (AssemblyLocations.TryGetValue(a, out path)) {
                    return ReferenceAssemblyTypes[a];
                }
            }

            // If there are no reference assemblies available, just use the path to the loaded assembly.
            if (TargetFrameworkReferenceAssemblyPaths == null || TargetFrameworkReferenceAssemblyPaths.Count == 0) {
                path = System.Web.UI.Util.GetAssemblyCodeBase(a);
                return ReferenceAssemblyType.FrameworkAssembly;
            }

            AssemblyResolutionResult result = null;
            ReferenceAssemblyType referenceAssemblyType = ReferenceAssemblyType.NonFrameworkAssembly;

            // If the assembly is generated by us, it is a non framework assembly and does not need to be resolved.
            if (BuildResultCompiledAssemblyBase.AssemblyIsInCodegenDir(a)) {
                path = System.Web.UI.Util.GetAssemblyCodeBase(a);
            }
            else {
                // Try using the assembly full name.
                referenceAssemblyType = GetPathToReferenceAssembly(a, out path, errors, warnings,
                    checkDependencies, true /*useFullName*/, out result);
            }

            StoreResults(a, path, result, referenceAssemblyType);
            return referenceAssemblyType;
        }

        private static ReferenceAssemblyType GetPathToReferenceAssembly(Assembly a, out string path,
            ICollection<BuildErrorEventArgs> errors, ICollection<BuildWarningEventArgs> warnings,
            bool checkDependencies, bool useFullName, out AssemblyResolutionResult result) {
            // 1. Find the assembly using RAR in the target framework.
            //    - If found, assembly is a framework assembly. Done
            // 2. Find the assembly using RAR in higher frameworks.
            //    - If found, assembly is a framework assembly only present in a higher version. Done.
            // 3. Find the assembly using RAR in the full profile framework.
            //    - If found, assembly is a framework assembly, but is only present in the full profile framework and not the current target profile. Done.
            // 4. Is useFullName true?
            //    - Yes: Use GAC and directory of loaded assembly as search paths.
            //    - No: Use directory of loaded assembly as search path.
            //    - Use RAR to find assembly in search paths. 
            //    - Check for out of range dependencies.
            // 5. If useFullName
            //    - Check if the short name exists in a higher framework, if so, it is a framework assembly.

            // Find the assembly in the target framework.
            string assemblyName;
            string partialName = a.GetName().Name;
            if (useFullName) {
                // Use the actual assembly name as specified in the config.
                assemblyName = CompilationSection.GetOriginalAssemblyName(a);
            }
            else {
                assemblyName = partialName;
            }
            result = ResolveAssembly(assemblyName, TargetFrameworkReferenceAssemblyPaths, 
                TargetFrameworkReferenceAssemblyPaths, false /*checkDependencies*/);
            if (result.ResolvedFiles != null && result.ResolvedFiles.Count > 0) {
                path = result.ResolvedFiles.FirstOrDefault();
                return ReferenceAssemblyType.FrameworkAssembly;
            }

            // At this point, the assembly was not found in the target framework.
            // Try finding it in the latest framework.
            result = ResolveAssembly(assemblyName, HigherFrameworkReferenceAssemblyPaths, HigherFrameworkReferenceAssemblyPaths, 
                false /*checkDependencies*/);
            if (result.ResolvedFiles != null && result.ResolvedFiles.Count > 0) {
                path = result.ResolvedFiles.FirstOrDefault();
                // Assembly was found in a target framework of a later version.
                return ReferenceAssemblyType.FrameworkAssemblyOnlyPresentInHigherVersion;
            }

            // Try to find the assembly in the full profile, in case the user 
            // is using an assembly that is not in the target profile framework. 
            // For example, System.Web is not present in the Client profile, but is present in the full profile.
            if (NeedToCheckFullProfile) {
                result = ResolveAssembly(assemblyName, FullProfileReferenceAssemblyPaths, FullProfileReferenceAssemblyPaths,
                    false /*checkDependencies*/);
                if (result.ResolvedFiles != null && result.ResolvedFiles.Count > 0) {
                    // Assembly was found in the full profile, but not in the target profile.
                    path = result.ResolvedFiles.FirstOrDefault();
                    // Report warning/error message.
                    string profile = "";
                    if (!string.IsNullOrEmpty(MultiTargetingUtil.TargetFrameworkName.Profile)) {
                        profile = " '" + MultiTargetingUtil.TargetFrameworkName.Profile + "'";
                    }
                    ReportWarningOrError(SR.GetString(SR.Assembly_not_found_in_profile, assemblyName, profile));
                    // Return as OnlyPresentInHigherVersion so that it will not be used as a reference assembly.
                    return ReferenceAssemblyType.FrameworkAssemblyOnlyPresentInHigherVersion;
                }
            }

            // Assembly is not found in the framework.
            // Check whether it has any references to assemblies of a higher version.
            List<string> searchPaths = new List<string>();
            searchPaths.AddRange(TargetFrameworkReferenceAssemblyPaths);
            searchPaths.Add(Path.GetDirectoryName(a.Location));
            // If we are using full names, include the GAC so that we can retrieve the actual
            // specified version of an OOB assembly even if it is unified/redirected to a later version.
            // For example, System.Web.Extensions 1.0.61025 is available from the GAC, but the actual
            // loaded assembly is 4.0 due to unification.
            if (useFullName) {
                searchPaths.Add("{GAC}");
            }

            // When checking dependencies of a custom assembly, use the full
            // name of the assembly as it might have a strong name or
            // be in the GAC.
            if (!useFullName) {
                assemblyName = a.GetName().FullName;
            }
            result = ResolveAssembly(assemblyName, searchPaths, TargetFrameworkReferenceAssemblyPaths, checkDependencies);
            // Use the actual resolved path, in case the loaded assembly is different from the specified assembly 
            // due to unification or binding redirect.
            path = result.ResolvedFiles.FirstOrDefault();

            if (string.IsNullOrEmpty(path)) {
                // In some cases, we might not be able to resolve the path to the assembly successfully, for example when
                // the config specifies the full name as System.Web 4.0.10101.0. Assembly.Load returns the 4.0.0.0 version,
                // but we can't find any actual assembly with such a full name.
                path = System.Web.UI.Util.GetAssemblyCodeBase(a);
            }

            // If we are using full names, do another check using the partial name to see if the assembly is part of 
            // a higher framework.
            // If so, then this is an OOB assembly that later got rolled into the framework, so we consider the assembly
            // as a framework assembly.
            if (useFullName) {
                AssemblyResolutionResult r = ResolveAssembly(partialName, HigherFrameworkReferenceAssemblyPaths, HigherFrameworkReferenceAssemblyPaths,
                    false /*checkDependencies*/);
                if (r.ResolvedFiles != null && r.ResolvedFiles.Count > 0) {
                    return ReferenceAssemblyType.FrameworkAssembly;
                }
            }

            return ReferenceAssemblyType.NonFrameworkAssembly;
        }

        private static IList<string> GetPathToReferenceAssemblies(FrameworkName frameworkName){
            return ToolLocationHelper.GetPathToReferenceAssemblies(frameworkName);
        }

        /// <summary>
        /// Returns true if any of the codedom providers has warnAsError set to true.
        /// </summary>
        private static bool WarnAsError {
            get {
                if (s_warnAsError == null) {
                    lock (s_warnAsErrorLock) {
                        // Check again, in case it was already set by another thread while the current thread
                        // was waiting to acquire the lock
                        if (s_warnAsError == null) {

                            // Set default value to false
                            s_warnAsError = false;
                            CompilerInfo[] compilerInfoArray = CodeDomProvider.GetAllCompilerInfo();
                            foreach (CompilerInfo info in compilerInfoArray) {
                                if (info == null || !info.IsCodeDomProviderTypeValid) {
                                    continue;
                                }

                                if (CompilationUtil.WarnAsError(info.CodeDomProviderType)) {
                                    s_warnAsError = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                return s_warnAsError.Value;
            }
        }
    }

    /// Adapted the following code from \\ddindex2\sources2\OrcasSP\vsproject\xmake\Shared\UnitTests

    internal class MockEngine : IBuildEngine {
        private List<BuildMessageEventArgs> messages = new List<BuildMessageEventArgs>();
        private List<BuildWarningEventArgs> warnings = new List<BuildWarningEventArgs>();
        private List<BuildErrorEventArgs> errors = new List<BuildErrorEventArgs>();
        private List<CustomBuildEventArgs> customEvents = new List<CustomBuildEventArgs>();

        internal MockEngine() {
        }

        internal ICollection<BuildMessageEventArgs> Messages {
            get { return messages; }
        }

        internal ICollection<BuildWarningEventArgs> Warnings {
            get { return warnings; }
        }

        internal ICollection<BuildErrorEventArgs> Errors {
            get { return errors; }
        }

        internal ICollection<CustomBuildEventArgs> CustomEvents {
            get { return customEvents; }
        }

        public virtual void LogErrorEvent(BuildErrorEventArgs eventArgs) {
            errors.Add(eventArgs);
        }

        public virtual void LogWarningEvent(BuildWarningEventArgs eventArgs) {
            warnings.Add(eventArgs);
        }

        public virtual void LogCustomEvent(CustomBuildEventArgs eventArgs) {
            customEvents.Add(eventArgs);
        }

        public virtual void LogMessageEvent(BuildMessageEventArgs eventArgs) {
            messages.Add(eventArgs);
        }

        public bool ContinueOnError {
            get {
                return false;
            }
        }

        public string ProjectFileOfTaskNode {
            get {
                return String.Empty;
            }
        }

        public int LineNumberOfTaskNode {
            get {
                return 0;
            }
        }

        public int ColumnNumberOfTaskNode {
            get {
                return 0;
            }
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs) {
            throw new NotImplementedException();
        }
    }

}
