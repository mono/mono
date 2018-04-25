namespace System.Workflow.ComponentModel.Compiler
{
    #region Imports

    using System;
    using System.Text;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.Reflection;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Runtime.Versioning;
    using System.Security;
    using Microsoft.Build.Utilities;
    using System.IO;
    using System.Runtime.InteropServices;

    #endregion

    [Serializable]
    class MultiTargetingInfo : ISerializable
    {
        internal static readonly Version DefaultTargetFramework = new Version("4.0");

        static readonly Version TargetFramework30 = new Version("3.0");
        internal const string TargetFramework30CompilerVersion = "v2.0";
        static readonly Version TargetFramework35 = new Version("3.5");
        internal const string TargetFramework35CompilerVersion = "v3.5";
        static readonly Version TargetFramework40 = new Version("4.0");
        internal const string TargetFramework40CompilerVersion = "v4.0";
        const string TargetFramework40CompatiblePrefix = "v4.";

        const string SerializationItem_TargetFramework = "TargetFramework";

        static IDictionary<Version, string> KnownSupportedTargetFrameworksAndRelatedCompilerVersions =
            new Dictionary<Version, string>()
            { { TargetFramework30, TargetFramework30CompilerVersion }, { TargetFramework35, TargetFramework35CompilerVersion }, 
              { TargetFramework40, TargetFramework40CompilerVersion } };

        FrameworkName targetFramework;
        string compilerVersion;

        public MultiTargetingInfo(string targetFramework)
        {
            this.targetFramework = new FrameworkName(targetFramework);
        }

        protected MultiTargetingInfo(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            this.targetFramework = new FrameworkName(info.GetString(MultiTargetingInfo.SerializationItem_TargetFramework));
        }

        public FrameworkName TargetFramework
        {
            get
            {
                return this.targetFramework;
            }
        }
        public string CompilerVersion
        {
            get
            {
                if (this.compilerVersion == null)
                {
                    this.compilerVersion = MultiTargetingInfo.GetCompilerVersion(this.targetFramework.Version);
                }
                return this.compilerVersion;
            }
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue(MultiTargetingInfo.SerializationItem_TargetFramework, this.targetFramework.FullName, typeof(string));
        }

        static string GetCompilerVersion(Version targetFrameworkVersion)
        {
            // As part of the future fx support - every 4.X framework is colided to 4.0
            Version versionKey;
            if (targetFrameworkVersion.Major == 4)
            {
                versionKey = TargetFramework40;
            }
            else
            {
                versionKey = new Version(targetFrameworkVersion.Major, targetFrameworkVersion.Minor);
            }

            string compilerVersion;
            if (!MultiTargetingInfo.KnownSupportedTargetFrameworksAndRelatedCompilerVersions.TryGetValue(versionKey, out compilerVersion))
            {
                compilerVersion = string.Empty;
            }
            return compilerVersion;
        }

        public static class MultiTargetingUtilities
        {
            const string RuntimeReferencePrefix = "<RUNTIME>";
            const string FrameworkReferencePrefix = "<FRAMEWORK>";

            static RuntimeManager runtimeManager;
            static ReferenceManager refManager;

            public static bool IsFrameworkReferenceAssembly(string path)
            {
                EnsureReferenceManager();
                return refManager.IsFrameworkReferenceAssembly(path);
            }

            public static WorkflowCompilerParameters NormalizeReferencedAssemblies(WorkflowCompilerParameters parameters)
            {
                EnsureRuntimeManager();
                EnsureReferenceManager();
                string[] normalizedAssemblies = new string[parameters.ReferencedAssemblies.Count];
                bool wasNormelized = false;
                for (int i = 0; i < parameters.ReferencedAssemblies.Count; i++)
                {
                    normalizedAssemblies[i] = NormalizePath(parameters.ReferencedAssemblies[i], ref wasNormelized);
                }
                if (wasNormelized)
                {
                    return new WorkflowCompilerParameters(parameters, normalizedAssemblies);
                }
                else
                {
                    return parameters;
                }
            }
            public static WorkflowCompilerParameters RenormalizeReferencedAssemblies(WorkflowCompilerParameters parameters)
            {
                EnsureRuntimeManager();
                EnsureReferenceManager();
                string[] renormalizedAssemblies = new string[parameters.ReferencedAssemblies.Count];
                bool wasRenormelized = false;
                for (int i = 0; i < parameters.ReferencedAssemblies.Count; i++)
                {
                    renormalizedAssemblies[i] = RenormalizePath(parameters.ReferencedAssemblies[i], ref wasRenormelized);
                }
                if (wasRenormelized)
                {
                    return new WorkflowCompilerParameters(parameters, renormalizedAssemblies);
                }
                else
                {
                    return parameters;
                }
            }

            static void EnsureRuntimeManager()
            {
                if (runtimeManager == null)
                {
                    runtimeManager = new RuntimeManager();
                }
            }
            static void EnsureReferenceManager()
            {
                if (refManager == null)
                {
                    refManager = new ReferenceManager();
                }
            }

            static string NormalizePath(string path, ref bool wasNormelized)
            {
                path = Path.GetFullPath(path);
                if (IsPathUnderDirectory(path, runtimeManager.NetFxRuntimeRoot))
                {
                    wasNormelized = true;
                    return path.Replace(runtimeManager.NetFxRuntimeRoot, RuntimeReferencePrefix);
                }
                else if (IsPathUnderDirectory(path, refManager.FrameworkReferenceAssemblyRoot))
                {
                    wasNormelized = true;
                    return path.Replace(refManager.FrameworkReferenceAssemblyRoot, FrameworkReferencePrefix);
                }
                else
                {
                    return path;
                }
            }
            static string RenormalizePath(string path, ref bool wasRenormelized)
            {
                if (path.StartsWith(RuntimeReferencePrefix, StringComparison.Ordinal))
                {
                    wasRenormelized = true;
                    return path.Replace(RuntimeReferencePrefix, runtimeManager.NetFxRuntimeRoot);
                }
                else if (path.StartsWith(FrameworkReferencePrefix, StringComparison.Ordinal))
                {
                    wasRenormelized = true;
                    return path.Replace(FrameworkReferencePrefix, refManager.FrameworkReferenceAssemblyRoot);
                }
                else
                {
                    return path;
                }
            }
            static bool IsPathUnderDirectory(string path, string parentDirectory)
            {
                if (!path.StartsWith(parentDirectory, StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
                int parentLength = parentDirectory.Length;
                if (path.Length == parentLength)
                {
                    return false;
                }
                if ((path[parentLength] != Path.DirectorySeparatorChar) && (path[parentLength] != Path.AltDirectorySeparatorChar))
                {
                    return false;
                }

                return true;
            }

            class RuntimeManager
            {
                const string NDPSetupRegistryBranch = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP";

                string netFxRuntimeRoot;

                public RuntimeManager()
                {
                    string runtimePath = XomlCompilerHelper.TrimDirectorySeparatorChar(RuntimeEnvironment.GetRuntimeDirectory());
                    this.netFxRuntimeRoot = XomlCompilerHelper.TrimDirectorySeparatorChar(Path.GetDirectoryName(runtimePath));
                }

                public string NetFxRuntimeRoot
                {
                    get
                    {
                        return this.netFxRuntimeRoot;
                    }
                }
            }
            class ReferenceManager
            {
                string frameworkReferenceAssemblyRoot;
                HashSet<string> frameworkReferenceDirectories;

                public ReferenceManager()
                {
                    this.frameworkReferenceAssemblyRoot = ToolLocationHelper.GetProgramFilesReferenceAssemblyRoot();
                    this.frameworkReferenceDirectories = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

                    IList<string> supportedTargetFrameworks = ToolLocationHelper.GetSupportedTargetFrameworks();
                    for (int i = 0; i < supportedTargetFrameworks.Count; i++)
                    {
                        FrameworkName fxName = new FrameworkName(supportedTargetFrameworks[i]);
                        IList<string> refDirectories = ToolLocationHelper.GetPathToReferenceAssemblies(fxName);
                        for (int j = 0; j < refDirectories.Count; j++)
                        {
                            string refDir = XomlCompilerHelper.TrimDirectorySeparatorChar(refDirectories[j]);
                            if (!this.frameworkReferenceDirectories.Contains(refDir))
                            {
                                this.frameworkReferenceDirectories.Add(refDir);
                            }
                        }
                    }
                }

                public string FrameworkReferenceAssemblyRoot
                {
                    get
                    {
                        return this.frameworkReferenceAssemblyRoot;
                    }
                }

                public bool IsFrameworkReferenceAssembly(string path)
                {
                    string dir = XomlCompilerHelper.TrimDirectorySeparatorChar(Path.GetDirectoryName(Path.GetFullPath(path)));
                    return this.frameworkReferenceDirectories.Contains(dir);
                }
            }
        }
    }

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowCompilerParameters : CompilerParameters
    {
        #region Private members

        internal const string NoCodeSwitch = "/nocode";
        internal const string CheckTypesSwitch = "/checktypes";

        private bool generateCCU = false;
        private string languageToUse = "CSharp";
        private IList<CodeCompileUnit> userCodeCCUs = null;
        private StringCollection libraryPaths = null;
        private Assembly localAssembly = null;
        private bool compileWithNoCode = false;
        private bool checkTypes = false;
        private string compilerOptions = null;
        [OptionalField(VersionAdded = 2)]
        MultiTargetingInfo mtInfo = null;

        #endregion

        #region Constructors

        public WorkflowCompilerParameters()
        {
        }

        public WorkflowCompilerParameters(string[] assemblyNames)
            : base(assemblyNames)
        {
        }

        public WorkflowCompilerParameters(string[] assemblyNames, string outputName)
            : base(assemblyNames, outputName)
        {
        }

        public WorkflowCompilerParameters(string[] assemblyNames, string outputName, bool includeDebugInformation)
            : base(assemblyNames, outputName, includeDebugInformation)
        {
        }

        public WorkflowCompilerParameters(WorkflowCompilerParameters parameters)
            : this(parameters, null)
        {
        }

        internal WorkflowCompilerParameters(WorkflowCompilerParameters parameters, string[] newReferencedAssemblies)
            : this()
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            this.CompilerOptions = parameters.CompilerOptions;
            foreach (string embeddedResource in parameters.EmbeddedResources)
            {
                this.EmbeddedResources.Add(embeddedResource);
            }
            this.GenerateExecutable = parameters.GenerateExecutable;
            this.GenerateInMemory = parameters.GenerateInMemory;
            this.IncludeDebugInformation = parameters.IncludeDebugInformation;
            foreach (string linkedResource in parameters.LinkedResources)
            {
                this.LinkedResources.Add(linkedResource);
            }
            this.MainClass = parameters.MainClass;
            this.OutputAssembly = parameters.OutputAssembly;
            if (newReferencedAssemblies != null)
            {
                this.ReferencedAssemblies.AddRange(newReferencedAssemblies);
            }
            else
            {
                foreach (string referenceAssembly in parameters.ReferencedAssemblies)
                {
                    this.ReferencedAssemblies.Add(referenceAssembly);
                }
            }
            this.TreatWarningsAsErrors = parameters.TreatWarningsAsErrors;
            this.UserToken = parameters.UserToken;
            this.WarningLevel = parameters.WarningLevel;
            this.Win32Resource = parameters.Win32Resource;

            this.generateCCU = parameters.generateCCU;
            this.languageToUse = parameters.languageToUse;
            if (parameters.libraryPaths != null)
            {
                this.libraryPaths = new StringCollection();
                foreach (string libraryPath in parameters.libraryPaths)
                {
                    this.libraryPaths.Add(libraryPath);
                }
            }
            if (parameters.userCodeCCUs != null)
            {
                this.userCodeCCUs = new List<CodeCompileUnit>(parameters.userCodeCCUs);
            }
            this.localAssembly = parameters.localAssembly;
        }

        #endregion

        #region Properties

        public new string CompilerOptions
        {
            get
            {
                return this.compilerOptions;
            }

            set
            {
                this.compilerOptions = value;
                base.CompilerOptions =
                    XomlCompilerHelper.ProcessCompilerOptions(value, out this.compileWithNoCode, out this.checkTypes);
            }
        }

        public bool GenerateCodeCompileUnitOnly
        {
            get
            {
                return this.generateCCU;
            }
            set
            {
                this.generateCCU = value;
            }
        }

        public string LanguageToUse
        {
            get
            {
                return this.languageToUse;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                if (String.Compare(value, SupportedLanguages.CSharp.ToString(), StringComparison.OrdinalIgnoreCase) != 0 &&
                    String.Compare(value, SupportedLanguages.VB.ToString(), StringComparison.OrdinalIgnoreCase) != 0)
                    throw new NotSupportedException(SR.GetString(SR.Error_LanguageNeedsToBeVBCSharp, value));

                this.languageToUse = value;
            }
        }

        public StringCollection LibraryPaths
        {
            get
            {
                if (this.libraryPaths == null)
                    this.libraryPaths = new StringCollection();

                return this.libraryPaths;
            }
        }

        public IList<CodeCompileUnit> UserCodeCompileUnits
        {
            get
            {
                if (this.userCodeCCUs == null)
                    this.userCodeCCUs = new List<CodeCompileUnit>();
                return this.userCodeCCUs;
            }
        }

        internal Assembly LocalAssembly
        {
            get
            {
                return this.localAssembly;
            }
            set
            {
                this.localAssembly = value;
            }
        }

        internal bool CompileWithNoCode
        {
            get
            {
                return this.compileWithNoCode;
            }
        }

        internal bool CheckTypes
        {
            get
            {
                return this.checkTypes;
            }
        }

        internal string CompilerVersion
        {
            get
            {
                if (this.mtInfo == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.mtInfo.CompilerVersion;
                }
            }
        }

        internal MultiTargetingInfo MultiTargetingInformation
        {
            get
            {
                return this.mtInfo;
            }
            set
            {
                this.mtInfo = value;
            }
        }

        #endregion

        internal static string ExtractRootNamespace(WorkflowCompilerParameters parameters)
        {
            string rootNamespace = string.Empty;

            // extract the namespace from the compiler options
            if (parameters.CompilerOptions != null && (CompilerHelpers.GetSupportedLanguage(parameters.LanguageToUse) == SupportedLanguages.VB))
            {
                Regex options = new Regex(@"\s*[/-]rootnamespace[:=]\s*(?<RootNamespace>[^\s]*)");
                Match match = options.Match(parameters.CompilerOptions);

                if (match.Success)
                    rootNamespace = match.Groups["RootNamespace"].Value;
            }
            return rootNamespace;
        }
    }
}
