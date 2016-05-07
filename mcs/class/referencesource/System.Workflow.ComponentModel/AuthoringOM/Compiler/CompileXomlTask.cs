namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Resources;
    using System.Reflection;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using Microsoft.Win32;
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System.Runtime.InteropServices;
    using Microsoft.Build.Tasks;
    using System.Collections.Generic;
    using Microsoft.Workflow.Compiler;
    using System.Runtime.Versioning;
    using System.Security;

    [Guid("59B2D1D0-5DB0-4F9F-9609-13F0168516D6")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IVsHierarchy
    {
    }

    [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOleServiceProvider
    {
        [PreserveSig]
        int QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject);
    }

    [ComImport(), Guid("8AA9644E-1F6A-4F4C-83E3-D0BAD4B2BB21"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IWorkflowBuildHostProperties
    {
        bool SkipWorkflowCompilation { get; set; }
    }

    internal class ServiceProvider : IServiceProvider
    {
        private static readonly Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");
        private IOleServiceProvider serviceProvider;
        public ServiceProvider(IOleServiceProvider sp)
        {
            this.serviceProvider = sp;
        }
        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            IntPtr pUnk = IntPtr.Zero;
            Guid guidService = serviceType.GUID;
            Guid guidUnk = IID_IUnknown;
            int hr = this.serviceProvider.QueryService(ref guidService, ref guidUnk, out pUnk);

            object service = null;
            if (hr >= 0)
            {
                try
                {
                    service = Marshal.GetObjectForIUnknown(pUnk);
                }
                finally
                {
                    Marshal.Release(pUnk);
                }
            }
            return service;
        }
    }

    #region CompileWorkflowTask
    /// <summary>
    /// This class extends the Task class of MSBuild framework.
    /// Methods of this class are invoked by the MSBuild framework to customize
    /// the build process when compiling WinOE flavors of CSharp and VB.net projects.
    /// It provides support for compiling .xoml files into intermediate
    /// code files (either CSharp or VB). It calls into the WorkflowCompiler to do the
    /// validations and code compile unit generation.
    /// This component is used during the build process of WinOE flavor projects
    /// both from within the Visual Studio IDE and the standalone MSBuild executable.
    /// As such this component's assembly should not have direct or indirect dependencies
    /// on the Visual Studio assemblies to work in the standalone scenario.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CompileWorkflowTask : Microsoft.Build.Utilities.Task, ITask
    {

        #region Members and Constructors

        private string projectExt = null;
        private string projectDirectory = null;
        private object hostObject = null;
        private string rootNamespace = null;
        private string imports = null;
        private string assemblyName = null;
        private ITaskItem[] xomlFiles = null;
        private ITaskItem[] referenceFiles = null;
        private ITaskItem[] sourceCodeFiles = null;
        private ITaskItem[] resourceFiles = null;
        private ITaskItem[] outputFiles = null; //new TaskItem[0]; // The outputs should be non-null if we bail out successfully or otherwise from the Execute method.
        private ITaskItem[] compilationOptions = null;
        private SupportedLanguages projectType;
        private StringCollection temporaryFiles = new StringCollection();
        private bool delaySign = false;
        private string targetFramework = null;
        private string keyContainer = null;
        private string keyFile = null;

        public CompileWorkflowTask()
            : base(new ResourceManager("System.Workflow.ComponentModel.BuildTasksStrings", Assembly.GetExecutingAssembly()))
        {
            this.BuildingProject = true;
        }

        #endregion

        #region Input parameters and property overrides
        public string ProjectDirectory
        {
            get
            {
                return this.projectDirectory;
            }
            set
            {
                this.projectDirectory = value;
            }
        }

        public string ProjectExtension
        {
            get
            {
                return this.projectExt;
            }
            set
            {
                this.projectExt = value;
                if (String.Compare(this.projectExt, ".csproj", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ProjectType = SupportedLanguages.CSharp;
                }
                else if (String.Compare(this.projectExt, ".vbproj", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ProjectType = SupportedLanguages.VB;
                }
            }
        }

        public string RootNamespace
        {
            get
            {
                return this.rootNamespace;
            }
            set
            {
                this.rootNamespace = value;
            }
        }

        public string AssemblyName
        {
            get
            {
                return this.assemblyName;
            }
            set
            {
                this.assemblyName = value;
            }
        }

        public string Imports
        {
            get
            {
                return this.imports;
            }
            set
            {
                this.imports = value;
            }
        }

        public ITaskItem[] WorkflowMarkupFiles
        {
            get
            {
                return xomlFiles;
            }
            set
            {
                if (value != null)
                {
                    ArrayList xomlFilesOnly = new ArrayList();
                    foreach (ITaskItem inputFile in value)
                    {
                        if (inputFile != null)
                        {
                            string fileSpec = inputFile.ItemSpec;
                            if (fileSpec != null && fileSpec.EndsWith(".xoml", StringComparison.OrdinalIgnoreCase))
                            {
                                xomlFilesOnly.Add(inputFile);
                            }
                        }
                    }

                    if (xomlFilesOnly.Count > 0)
                    {
                        this.xomlFiles = xomlFilesOnly.ToArray(typeof(ITaskItem)) as ITaskItem[];
                    }
                }
                else
                {
                    this.xomlFiles = value;
                }
            }
        }

        public ITaskItem[] ReferenceFiles
        {
            get
            {
                return this.referenceFiles;
            }
            set
            {
                this.referenceFiles = value;
            }
        }

        public ITaskItem[] ResourceFiles
        {
            get
            {
                return this.resourceFiles;
            }
            set
            {
                this.resourceFiles = value;
            }
        }

        public ITaskItem[] SourceCodeFiles
        {
            get
            {
                return this.sourceCodeFiles;
            }
            set
            {
                this.sourceCodeFiles = value;
            }
        }

        public ITaskItem[] CompilationOptions
        {
            get
            {
                return this.compilationOptions;
            }
            set
            {
                this.compilationOptions = value;
            }
        }

        public bool DelaySign
        {
            get
            {
                return this.delaySign;
            }
            set
            {
                this.delaySign = value;
            }
        }

        public string TargetFramework
        {
            get
            {
                return this.targetFramework;
            }
            set
            {
                this.targetFramework = value;
            }
        }

        public string KeyContainer
        {
            get
            {
                return this.keyContainer;
            }
            set
            {
                this.keyContainer = value;
            }
        }

        public string KeyFile
        {
            get
            {
                return this.keyFile;
            }
            set
            {
                this.keyFile = value;
            }
        }

        public new object HostObject
        {
            get
            {
                return this.hostObject;
            }
        }

        ITaskHost ITask.HostObject
        {
            get
            {
                return (ITaskHost)this.hostObject;
            }
            set
            {
                this.hostObject = value;
            }
        }

        public bool BuildingProject { get; set; }
        #endregion

        #region Output parameter properties
        [OutputAttribute]
        public ITaskItem[] OutputFiles
        {
            get
            {
                if (this.outputFiles == null)
                {
                    if (this.ProjectType == SupportedLanguages.VB)
                        this.outputFiles = new ITaskItem[0];
                    else
                    {
                        ArrayList oFiles = new ArrayList();
                        if (this.WorkflowMarkupFiles != null)
                            oFiles.AddRange(this.WorkflowMarkupFiles);
                        this.outputFiles = oFiles.ToArray(typeof(ITaskItem)) as ITaskItem[];
                    }
                }
                return this.outputFiles;
            }
        }

        [OutputAttribute]
        public string KeepTemporaryFiles
        {
            get
            {
                return ShouldKeepTempFiles().ToString();
            }
        }

        [OutputAttribute]
        public string[] TemporaryFiles
        {
            get
            {
                string[] tempFiles = new string[this.temporaryFiles.Count];

                this.temporaryFiles.CopyTo(tempFiles, 0);
                return tempFiles;
            }
        }

        #endregion

        #region Public method overrides

        public override bool Execute()
        {
#if DEBUG
            DumpInputParameters();
#endif

            // Validate the input parameters for the task.
            if (!this.ValidateParameters())
                return false;

            // If no .xoml files were specified, return success.
            if (this.WorkflowMarkupFiles == null)
                this.Log.LogMessageFromResources(MessageImportance.Normal, "NoXomlFiles");

            // Check if there are any referenced assemblies.
            if (this.ReferenceFiles == null || this.ReferenceFiles.Length == 0)
                this.Log.LogMessageFromResources(MessageImportance.Normal, "NoReferenceFiles");

            // Check if there are any souce code files (cs/vb).
            if (this.SourceCodeFiles == null || this.SourceCodeFiles.Length == 0)
                this.Log.LogMessageFromResources(MessageImportance.Normal, "NoSourceCodeFiles");

            // we return early if this is not invoked during the build phase of the project (eg project load)
            IWorkflowBuildHostProperties workflowBuildHostProperty = this.HostObject as IWorkflowBuildHostProperties;
            if (!this.BuildingProject || (workflowBuildHostProperty != null && workflowBuildHostProperty.SkipWorkflowCompilation))
            {
                return true;
            }

            // Create an instance of WorkflowCompilerParameters.
            int errorCount = 0, warningCount = 0;
            WorkflowCompilerParameters compilerParameters = new WorkflowCompilerParameters();

            // set the service provider
            IWorkflowCompilerErrorLogger workflowErrorLogger = null;
            IServiceProvider externalServiceProvider = null;
            if (this.HostObject is IOleServiceProvider)
            {
                externalServiceProvider = new ServiceProvider(this.HostObject as IOleServiceProvider);
                workflowErrorLogger = externalServiceProvider.GetService(typeof(IWorkflowCompilerErrorLogger)) as IWorkflowCompilerErrorLogger;
            }

            string[] userCodeFiles = GetFiles(this.SourceCodeFiles, this.ProjectDirectory);
            foreach (ITaskItem referenceFile in this.ReferenceFiles)
                compilerParameters.ReferencedAssemblies.Add(referenceFile.ItemSpec);

            if (string.IsNullOrEmpty(this.targetFramework))
            {
                string defaultFrameworkName = null;

                const string NDPSetupRegistryBranch = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP";
                const string NetFrameworkIdentifier = ".NETFramework";

                RegistryKey ndpSetupKey = null;
                try
                {
                    ndpSetupKey = Registry.LocalMachine.OpenSubKey(NDPSetupRegistryBranch);

                    if (ndpSetupKey != null)
                    {
                        string[] installedNetFxs = ndpSetupKey.GetSubKeyNames();

                        if (installedNetFxs != null)
                        {
                            char[] splitChars = new char[] { '.' };
                            for (int i = 0; i < installedNetFxs.Length; i++)
                            {
                                string framework = installedNetFxs[i];
                                if (framework.Length > 0)
                                {
                                    string frameworkVersion = framework.TrimStart('v', 'V');
                                    if (!string.IsNullOrEmpty(frameworkVersion))
                                    {
                                        string[] parts = frameworkVersion.Split(splitChars);

                                        string normalizedVersion = null;
                                        if (parts.Length > 1)
                                        {
                                            normalizedVersion = string.Format(CultureInfo.InvariantCulture, "v{0}.{1}", parts[0], parts[1]);
                                        }
                                        else
                                        {
                                            normalizedVersion = string.Format(CultureInfo.InvariantCulture, "v{0}.0", parts[0]);
                                        }

                                        if (string.Compare(normalizedVersion, "v3.5", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            defaultFrameworkName = new FrameworkName(NetFrameworkIdentifier, new Version(3, 5)).ToString();
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (SecurityException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (IOException)
                {
                }
                finally
                {
                    if (ndpSetupKey != null)
                    {
                        ndpSetupKey.Close();
                    }
                }

                if (defaultFrameworkName == null)
                {
                    defaultFrameworkName = new FrameworkName(NetFrameworkIdentifier, new Version(2, 0)).ToString();
                }

                compilerParameters.MultiTargetingInformation = new MultiTargetingInfo(defaultFrameworkName);
            }
            else
            {
                compilerParameters.MultiTargetingInformation = new MultiTargetingInfo(this.targetFramework);
            }

            CompilerOptionsBuilder optionsBuilder;
            switch (this.ProjectType)
            {
                case SupportedLanguages.VB:
                    switch (compilerParameters.CompilerVersion)
                    {
                        case MultiTargetingInfo.TargetFramework30CompilerVersion:
                            optionsBuilder = new WhidbeyVBCompilerOptionsBuilder();
                            break;
                        case MultiTargetingInfo.TargetFramework35CompilerVersion:
                            optionsBuilder = new OrcasVBCompilerOptionsBuilder();
                            break;
                        default:
                            optionsBuilder = new CompilerOptionsBuilder();
                            break;
                    }
                    break;
                default:
                    optionsBuilder = new CompilerOptionsBuilder();
                    break;
            }
            compilerParameters.CompilerOptions = this.PrepareCompilerOptions(optionsBuilder);
            compilerParameters.GenerateCodeCompileUnitOnly = true;
            compilerParameters.LanguageToUse = this.ProjectType.ToString();
            compilerParameters.TempFiles.KeepFiles = ShouldKeepTempFiles();

            compilerParameters.OutputAssembly = AssemblyName;
            if (!string.IsNullOrEmpty(assemblyName))
            {
                // Normalizing the assembly name. 
                // The codeDomProvider expects the proper extension to be set.
                string extension = (compilerParameters.GenerateExecutable) ? ".exe" : ".dll";
                compilerParameters.OutputAssembly += extension;
            }

            CodeDomProvider codeProvider = null;
            if (this.ProjectType == SupportedLanguages.VB)
                codeProvider = CompilerHelpers.CreateCodeProviderInstance(typeof(VBCodeProvider), compilerParameters.CompilerVersion);
            else
                codeProvider = CompilerHelpers.CreateCodeProviderInstance(typeof(CSharpCodeProvider), compilerParameters.CompilerVersion);

            using (TempFileCollection tempFileCollection = new TempFileCollection(Environment.GetEnvironmentVariable("temp", EnvironmentVariableTarget.User), true))
            {
                this.outputFiles = new TaskItem[1];

                // Compile and generate a temporary code file for each xoml file.
                string[] xomlFilesPaths;
                if (this.WorkflowMarkupFiles != null)
                {
                    xomlFilesPaths = new string[WorkflowMarkupFiles.GetLength(0) + userCodeFiles.Length];
                    int index = 0;
                    for (; index < this.WorkflowMarkupFiles.GetLength(0); index++)
                        xomlFilesPaths[index] = Path.Combine(ProjectDirectory, this.WorkflowMarkupFiles[index].ItemSpec);

                    userCodeFiles.CopyTo(xomlFilesPaths, index);
                }
                else
                {
                    xomlFilesPaths = new string[userCodeFiles.Length];
                    userCodeFiles.CopyTo(xomlFilesPaths, 0);
                }

                WorkflowCompilerResults compilerResults = new CompilerWrapper().Compile(compilerParameters, xomlFilesPaths);

                foreach (WorkflowCompilerError error in compilerResults.Errors)
                {
                    if (error.IsWarning)
                    {
                        warningCount++;
                        if (workflowErrorLogger != null)
                        {
                            error.FileName = Path.Combine(this.ProjectDirectory, error.FileName);
                            workflowErrorLogger.LogError(error);
                            workflowErrorLogger.LogMessage(error.ToString() + "\n");
                        }
                        else
                            this.Log.LogWarning(error.ErrorText, error.ErrorNumber, error.FileName, error.Line, error.Column);
                    }
                    else
                    {
                        errorCount++;
                        if (workflowErrorLogger != null)
                        {
                            error.FileName = Path.Combine(this.ProjectDirectory, error.FileName);
                            workflowErrorLogger.LogError(error);
                            workflowErrorLogger.LogMessage(error.ToString() + "\n");
                        }
                        else
                            this.Log.LogError(error.ErrorText, error.ErrorNumber, error.FileName, error.Line, error.Column);
                    }
                }

                if (!compilerResults.Errors.HasErrors)
                {
                    CodeCompileUnit ccu = compilerResults.CompiledUnit;
                    if (ccu != null)
                    {
                        // Fix standard namespaces and root namespace.
                        WorkflowMarkupSerializationHelpers.FixStandardNamespacesAndRootNamespace(ccu.Namespaces, this.RootNamespace, CompilerHelpers.GetSupportedLanguage(this.ProjectType.ToString())); //just add the standard namespaces

                        string tempFile = tempFileCollection.AddExtension(codeProvider.FileExtension);
                        using (StreamWriter fileStream = new StreamWriter(new FileStream(tempFile, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                        {
                            CodeGeneratorOptions options = new CodeGeneratorOptions();
                            options.BracingStyle = "C";
                            codeProvider.GenerateCodeFromCompileUnit(ccu, fileStream, options);
                        }

                        this.outputFiles[0] = new TaskItem(tempFile);
                        this.temporaryFiles.Add(tempFile);
                        this.Log.LogMessageFromResources(MessageImportance.Normal, "TempCodeFile", tempFile);
                    }
                }
            }
            if ((errorCount > 0 || warningCount > 0) && workflowErrorLogger != null)
                workflowErrorLogger.LogMessage(string.Format(CultureInfo.CurrentCulture, "\nCompile complete -- {0} errors, {1} warnings \n", new object[] { errorCount, warningCount }));

#if DEBUG
            DumpOutputParameters();
#endif
            this.Log.LogMessageFromResources(MessageImportance.Normal, "XomlValidationCompleted", errorCount, warningCount);
            return (errorCount == 0);
        }

        #endregion

        #region Private properties and methods
        private SupportedLanguages ProjectType
        {
            get
            {
                return this.projectType;
            }
            set
            {
                this.projectType = value;
            }
        }

        /// <summary>
        /// This method validates all the input parameters for the custom task.
        /// </summary>
        /// <returns>True if all parameters are valid, false otherwise</returns>
        private bool ValidateParameters()
        {
            // If the project directory is not supplied then bail out with an error.
            if (ProjectDirectory == null || ProjectDirectory.Trim().Length == 0)
            {
                this.Log.LogErrorFromResources("NoProjectType");
                return false;
            }

            // If the project extension is not supplied then bail out with an error.
            if (ProjectExtension == null || ProjectExtension.Trim().Length == 0)
            {
                this.Log.LogErrorFromResources("NoProjectType");
                return false;
            }

            // If the project extension is not .csproj or .vbproj bail out with an error.
            if (String.Compare(ProjectExtension, ".csproj", StringComparison.OrdinalIgnoreCase) != 0 && String.Compare(ProjectExtension, ".vbproj", StringComparison.OrdinalIgnoreCase) != 0)
            {
                this.Log.LogErrorFromResources("UnsupportedProjectType");
                return false;
            }

            // All parameters are valid so return true.
            return true;
        }

#if DEBUG
        void DumpInputParameters()
        {
            DumpParametersLine("CompileWorkflowTask - Input Parameters:");
            DumpParametersLine("  projectExt={0}", this.projectExt);
            DumpParametersLine("  projectDirectory='{0}'", this.projectDirectory);
            DumpParametersLine("  rootNamespace={0}", this.rootNamespace);
            DumpParametersLine("  imports='{0}'", this.imports);
            DumpParametersLine("  assemblyName='{0}", this.assemblyName);
            DumpParametersTaskItems("xomlFiles", this.xomlFiles);
            DumpParametersTaskItems("sourceCodeFiles", this.sourceCodeFiles);
            DumpParametersTaskItems("resourceFiles", this.resourceFiles);
            DumpParametersTaskItems("referenceFiles", this.referenceFiles);
            DumpParametersTaskItems("compilationOptions", this.compilationOptions);
            DumpParametersLine("  delaySign={0},keyContainer='{1}',keyFile='{2}'", this.delaySign, this.keyContainer, this.keyFile);
            DumpParametersLine("  targetFramework='{0}'", this.targetFramework);
        }
        void DumpOutputParameters()
        {
            DumpParametersLine("CompileWorkflowTask - Output Parameters:");
            DumpParametersTaskItems("outputFiles", this.outputFiles);
            DumpParametersLine("  KeepTemporaryFiles={0},temporaryFiles=[{1} items]", this.KeepTemporaryFiles, this.temporaryFiles.Count);
            for (int i = 0; i < this.temporaryFiles.Count; i++)
            {
                DumpParametersLine("    '{0}' [{1}]", this.temporaryFiles[i], i);
            }
        }
        void DumpParametersTaskItems(string name, ITaskItem[] items)
        {
            if (items == null)
            {
                DumpParametersLine("  {0}=<null>", name);
            }
            else
            {
                DumpParametersLine("  {0}=[{1} items]", name, items.Length);
                for (int i = 0; i < items.Length; i++)
                {
                    ITaskItem item = items[i];
                    if (item == null)
                    {
                        DumpParametersLine("    <null> [{0}]", i);
                    }
                    else
                    {
                        DumpParametersLine("    {0} [{1}]", item.ItemSpec, i);
                        foreach (string metadataName in item.MetadataNames)
                        {
                            DumpParametersLine("      {0}='{1}'", metadataName, item.GetMetadata(metadataName));
                        }
                    }
                }
            }
        }
        void DumpParametersLine(string lineFormat, params object[] lineArguments)
        {
            if ((lineArguments != null) && (lineArguments.Length > 0))
            {
                for (int i = 0; i < lineArguments.Length; i++)
                {
                    if (lineArguments[i] == null)
                    {
                        lineArguments[i] = "<null>";
                    }
                }
            }
            this.Log.LogMessage(MessageImportance.Low, lineFormat, lineArguments);
        }
#endif

        /// <summary>
        /// This method is used to get the absolute paths of the files 
        /// in a project.
        /// </summary>
        /// <param name="taskItems"></param>
        /// <param name="projDir"></param>
        /// <returns></returns>
        private static string[] GetFiles(ITaskItem[] taskItems, string projDir)
        {
            if (taskItems == null)
                return new string[0];
            string[] itemSpecs = new string[taskItems.Length];

            for (int i = 0; i < taskItems.Length; i++)
            {
                if (projDir != null)
                {
                    itemSpecs[i] = Path.Combine(projDir, taskItems[i].ItemSpec);
                }
                else
                {
                    itemSpecs[i] = taskItems[i].ItemSpec;
                }
            }

            return itemSpecs;
        }

        private static bool HasManifestResourceName(ITaskItem resourceFile, out string manifestResourceName)
        {
            IEnumerator metadataNames = resourceFile.MetadataNames.GetEnumerator();

            manifestResourceName = null;
            bool hasName = false;
            while (!hasName && metadataNames.MoveNext())
            {
                string metadataName = (string)metadataNames.Current;
                if (metadataName == "ManifestResourceName")
                {
                    hasName = true;
                    manifestResourceName = resourceFile.GetMetadata(metadataName);
                }
            }

            return hasName;
        }

        //Note: Remember to prefix each option with a space. We don't want compiler options glued together.
        private string PrepareCompilerOptions(CompilerOptionsBuilder optionsBuilder)
        {
            StringBuilder compilerOptions = new StringBuilder();

            if (this.DelaySign == true)
                compilerOptions.Append(" /delaysign+");

            if (this.KeyContainer != null && this.KeyContainer.Trim().Length > 0)
                compilerOptions.AppendFormat(" /keycontainer:{0}", this.KeyContainer);

            if (this.KeyFile != null && this.KeyFile.Trim().Length > 0)
                compilerOptions.AppendFormat(" /keyfile:\"{0}\"", Path.Combine(this.ProjectDirectory, this.KeyFile));

            if (this.compilationOptions != null && this.compilationOptions.Length > 0)
            {
                foreach (ITaskItem option in this.compilationOptions)
                {
                    optionsBuilder.AddCustomOption(compilerOptions, option);
                }
            }

            if (this.resourceFiles != null && this.resourceFiles.Length > 0)
            {
                foreach (ITaskItem resourceFile in this.resourceFiles)
                {
                    string manifestResourceName;

                    if (HasManifestResourceName(resourceFile, out manifestResourceName))
                    {
                        compilerOptions.AppendFormat(" /resource:\"{0}\",{1}",
                            Path.Combine(this.ProjectDirectory, resourceFile.ItemSpec), manifestResourceName);
                    }
                    else
                    {
                        compilerOptions.AppendFormat(" /resource:\"{0}\"",
                            Path.Combine(this.ProjectDirectory, resourceFile.ItemSpec));
                    }
                }
            }

            if (this.ProjectType == SupportedLanguages.VB)
            {
                if (!string.IsNullOrEmpty(this.RootNamespace))
                    compilerOptions.AppendFormat(" /rootnamespace:{0}", this.RootNamespace);
                compilerOptions.AppendFormat(" /imports:{0}", this.Imports.Replace(';', ','));
            }

            if (compilerOptions.Length > 0)
            {
                if (char.IsWhiteSpace(compilerOptions[0]))
                {
                    compilerOptions.Remove(0, 0);
                }
            }

            return compilerOptions.ToString();
        }

        private bool ShouldKeepTempFiles()
        {
            bool retVal = false;

            // See comments for the CompileWorkflowCleanupTask class for reasons why we must keep the temp file for VB.
            if (this.ProjectType == SupportedLanguages.VB)
                retVal = true;
            else
            {
                try
                {
                    RegistryKey winoeKey = Registry.LocalMachine.OpenSubKey(Helpers.ProductRootRegKey);
                    if (winoeKey != null)
                    {
                        object obj = winoeKey.GetValue("KeepTempFiles");
                        retVal = (Convert.ToInt32(obj, CultureInfo.InvariantCulture) != 0);
                    }
                }
                catch
                {
                }
            }

            return retVal;
        }

        #endregion

        class CompilerOptionsBuilder
        {
            public CompilerOptionsBuilder()
            {
            }

            public void AddCustomOption(StringBuilder options, ITaskItem option)
            {
                string optionName;
                string optionValue;
                string optionDelimiter;
                GetOptionInfo(option, out optionName, out optionValue, out optionDelimiter);
                if (!string.IsNullOrWhiteSpace(optionName))
                {
                    if (string.IsNullOrEmpty(optionValue))
                    {
                        options.AppendFormat(" /{0}", optionName);
                    }
                    else if (string.IsNullOrEmpty(optionDelimiter))
                    {
                        options.AppendFormat(" /{0}{1}", optionName, optionValue);
                    }
                    else
                    {
                        options.AppendFormat(" /{0}{1}{2}", optionName, optionDelimiter, optionValue);
                    }
                }
            }

            protected virtual void GetOptionInfo(ITaskItem option, out string optionName, out string optionValue, out string optionDelimiter)
            {
                optionName = option.ItemSpec;
                optionValue = option.GetMetadata("value");
                optionDelimiter = option.GetMetadata("delimiter");
            }
        }
        abstract class VBCompilerOptionsBuilder : CompilerOptionsBuilder
        {
            const string SuppressWarningOption = "nowarn";

            protected VBCompilerOptionsBuilder()
                : base()
            {
            }

            sealed protected override void GetOptionInfo(ITaskItem option, out string optionName, out string optionValue, out string optionDelimiter)
            {
                base.GetOptionInfo(option, out optionName, out optionValue, out optionDelimiter);
                if ((string.Compare(optionName, SuppressWarningOption, StringComparison.OrdinalIgnoreCase) == 0) &&
                    !string.IsNullOrWhiteSpace(optionValue))
                {
                    string[] warnings = optionValue.Split(',');
                    StringBuilder validWarnings = new StringBuilder();
                    for (int i = 0; i < warnings.Length; i++)
                    {
                        string warning = warnings[i].Trim();
                        if (IsValidWarning(warning))
                        {
                            if (validWarnings.Length == 0)
                            {
                                validWarnings.Append(warning);
                            }
                            else
                            {
                                validWarnings.AppendFormat(",{0}", warning);
                            }
                        }
                    }
                    optionValue = validWarnings.ToString();
                    if (string.IsNullOrWhiteSpace(optionValue))
                    {
                        optionName = string.Empty;
                    }
                }
            }

            protected abstract bool IsValidWarning(string warning);
        }
        class WhidbeyVBCompilerOptionsBuilder : VBCompilerOptionsBuilder
        {
            static HashSet<string> validWarnings = new HashSet<string>(StringComparer.Ordinal)
                { "40000", "40003", "40004", "40005", "40007", "40008", "40009", "40010", "40011", "40012", "40014", "40018", "40019",
                    "40020", "40021", "40022", "40023", "40024", "40025", "40026", "40027", "40028", "40029", "40030", "40031", "40032",
                    "40033", "40034", "40035", "40038", "40039", "40040", "40041", "40042", "40043", "40046", "40047", "40048", "40049",
                    "40050", "40051", "40052", "40053", "40054", "40055", "40056", "40057", 
                    "41000", "41001", "41002", "41003", "41004", "41005", "41006", "41998", "41999",
                    "42000", "42001", "42002", "42003", "42004", "42014", "42015", "42016", "42017", "42018", "42019", "42020", "42021",
                    "42022", "42024", "42025", "42026", "42028", "42029", "42030", "42031", "42032", "42033", "42034", "42035", "42036",
                    "42101", "42102", "42104", "42105", "42106", "42107", "42108", "42109", "42200", "42203", "42204", "42205", "42206",
                    "42300", "42301", "42302", "42303", "42304", "42305", "42306", "42307", "42308", "42309", "42310", "42311", "42312",
                    "42313", "42314", "42315", "42316", "42317", "42318", "42319", "42320", "42321" };

            public WhidbeyVBCompilerOptionsBuilder()
                : base()
            {
            }

            protected override bool IsValidWarning(string warning)
            {
                return validWarnings.Contains(warning);
            }
        }
        class OrcasVBCompilerOptionsBuilder : VBCompilerOptionsBuilder
        {
            static HashSet<string> validWarnings = new HashSet<string>(StringComparer.Ordinal)
                { "40000", "40003", "40004", "40005", "40007", "40008", "40009", "40010", "40011", "40012", "40014", "40018", "40019",
                    "40020", "40021", "40022", "40023", "40024", "40025", "40026", "40027", "40028", "40029", "40030", "40031", "40032",
                    "40033", "40034", "40035", "40038", "40039", "40040", "40041", "40042", "40043", "40046", "40047", "40048", "40049",
                    "40050", "40051", "40052", "40053", "40054", "40055", "40056", "40057",
                    "41000", "41001", "41002", "41003", "41004", "41005", "41006", "41007", "41008", "41998", "41999",
                    "42000", "42001", "42002", "42004", "42014", "42015", "42016", "42017", "42018", "42019", "42020", "42021", "42022",
                    "42024", "42025", "42026", "42028", "42029", "42030", "42031", "42032", "42033", "42034", "42035", "42036", "42099",
                    "42101", "42102", "42104", "42105", "42106", "42107", "42108", "42109", "42110", "42111", "42200", "42203", "42204",
                    "42205", "42206", "42207", "42300", "42301", "42302", "42303", "42304", "42305", "42306", "42307", "42308", "42309",
                    "42310", "42311", "42312", "42313", "42314", "42315", "42316", "42317", "42318", "42319", "42320", "42321", "42322",
                    "42324", "42326", "42327", "42328" };

            public OrcasVBCompilerOptionsBuilder()
                : base()
            {
            }

            protected override bool IsValidWarning(string warning)
            {
                return validWarnings.Contains(warning);
            }
        }
    }
    #endregion

    internal sealed class CreateWorkflowManifestResourceNameForCSharp : CreateCSharpManifestResourceName
    {
        private bool lastAskedFileWasXoml = false;

        [Output]
        public new ITaskItem[] ResourceFilesWithManifestResourceNames
        {
            get
            {
                for (int i = 0; i < base.ResourceFilesWithManifestResourceNames.Length; i++)
                {
                    ITaskItem item = base.ResourceFilesWithManifestResourceNames[i];
                    item.SetMetadata("LogicalName", item.GetMetadata("ManifestResourceName"));
                }

                return base.ResourceFilesWithManifestResourceNames;
            }
            set
            {
                base.ResourceFilesWithManifestResourceNames = value;
            }
        }

        override protected string CreateManifestName(string fileName, string linkFileName, string rootNamespace, string dependentUponFileName, Stream binaryStream)
        {
            string manifestName = string.Empty;
            if (!this.lastAskedFileWasXoml)
            {
                manifestName = base.CreateManifestName(fileName, linkFileName, rootNamespace, dependentUponFileName, binaryStream);
            }
            else
            {
                manifestName = TasksHelper.GetXomlManifestName(fileName, linkFileName, rootNamespace, binaryStream);
            }

            string extension = Path.GetExtension(fileName);
            if (String.Compare(extension, ".rules", StringComparison.OrdinalIgnoreCase) == 0 ||
                String.Compare(extension, WorkflowDesignerLoader.DesignerLayoutFileExtension, StringComparison.OrdinalIgnoreCase) == 0)
                manifestName += extension;

            this.lastAskedFileWasXoml = false;
            return manifestName;
        }

        override protected bool IsSourceFile(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (String.Compare(extension, ".xoml", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.lastAskedFileWasXoml = true;
                return true;
            }
            return base.IsSourceFile(fileName);
        }
    }

    internal sealed class CreateWorkflowManifestResourceNameForVB : CreateVisualBasicManifestResourceName
    {
        private bool lastAskedFileWasXoml = false;

        override protected string CreateManifestName(string fileName, string linkFileName, string rootNamespace, string dependentUponFileName, Stream binaryStream)
        {
            string manifestName = string.Empty;
            if (!this.lastAskedFileWasXoml)
            {
                manifestName = base.CreateManifestName(fileName, linkFileName, rootNamespace, dependentUponFileName, binaryStream);
            }
            else
            {
                manifestName = TasksHelper.GetXomlManifestName(fileName, linkFileName, rootNamespace, binaryStream);
            }

            string extension = Path.GetExtension(fileName);
            if (String.Compare(extension, ".rules", StringComparison.OrdinalIgnoreCase) == 0 ||
                String.Compare(extension, WorkflowDesignerLoader.DesignerLayoutFileExtension, StringComparison.OrdinalIgnoreCase) == 0)
                manifestName += extension;

            this.lastAskedFileWasXoml = false;
            return manifestName;
        }

        override protected bool IsSourceFile(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (String.Compare(extension, ".xoml", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.lastAskedFileWasXoml = true;
                return true;
            }
            return base.IsSourceFile(fileName);
        }
    }

    internal static class TasksHelper
    {
        internal static string GetXomlManifestName(string fileName, string linkFileName, string rootNamespace, Stream binaryStream)
        {
            string manifestName = string.Empty;

            // Use the link file name if there is one, otherwise, fall back to file name.
            string embeddedFileName = linkFileName;
            if (embeddedFileName == null || embeddedFileName.Length == 0)
                embeddedFileName = fileName;

            Culture.ItemCultureInfo info = Culture.GetItemCultureInfo(embeddedFileName);

            if (binaryStream != null)
            {
                // Resource depends on a form. Now, get the form's class name fully 
                // qualified with a namespace.
                string name = null;
                try
                {
                    Xml.XmlTextReader reader = new Xml.XmlTextReader(binaryStream);
                    if (reader.MoveToContent() == System.Xml.XmlNodeType.Element)
                    {
                        if (reader.MoveToAttribute("Class", StandardXomlKeys.Definitions_XmlNs))
                            name = reader.Value;
                    }
                }
                catch
                {
                    // ignore it for now
                }

                if (name != null && name.Length > 0)
                {
                    manifestName = name;

                    // Append the culture if there is one.        
                    if (info.culture != null && info.culture.Length > 0)
                    {
                        manifestName = manifestName + "." + info.culture;
                    }
                }
            }

            // If there's no manifest name at this point, then fall back to using the
            // RootNamespace+Filename_with_slashes_converted_to_dots         
            if (manifestName.Length == 0)
            {
                // If Rootnamespace was null, then it wasn't set from the project resourceFile.
                // Empty namespaces are allowed.
                if (!string.IsNullOrEmpty(rootNamespace))
                    manifestName = rootNamespace + ".";

                // Replace spaces in the directory name with underscores. Needed for compatibility with Everett.
                // Note that spaces in the file name itself are preserved.
                string everettCompatibleDirectoryName = CreateManifestResourceName.MakeValidEverettIdentifier(Path.GetDirectoryName(info.cultureNeutralFilename));

                // only strip extension for .resx files
                if (0 == String.Compare(Path.GetExtension(info.cultureNeutralFilename), ".resx", StringComparison.OrdinalIgnoreCase))
                {
                    manifestName += Path.Combine(everettCompatibleDirectoryName, Path.GetFileNameWithoutExtension(info.cultureNeutralFilename));

                    // Replace all '\' with '.'
                    manifestName = manifestName.Replace(Path.DirectorySeparatorChar, '.');
                    manifestName = manifestName.Replace(Path.AltDirectorySeparatorChar, '.');

                    // Append the culture if there is one.        
                    if (info.culture != null && info.culture.Length > 0)
                    {
                        manifestName = manifestName + "." + info.culture;
                    }
                }
                else
                {
                    manifestName += Path.Combine(everettCompatibleDirectoryName, Path.GetFileName(info.cultureNeutralFilename));

                    // Replace all '\' with '.'
                    manifestName = manifestName.Replace(Path.DirectorySeparatorChar, '.');
                    manifestName = manifestName.Replace(Path.AltDirectorySeparatorChar, '.');

                    // Prepend the culture as a subdirectory if there is one.        
                    if (info.culture != null && info.culture.Length > 0)
                    {
                        manifestName = info.culture + Path.DirectorySeparatorChar + manifestName;
                    }
                }
            }
            return manifestName;
        }

    }

    internal static class Culture
    {
        static private string[] cultureInfoStrings;

        internal struct ItemCultureInfo
        {
            internal string culture;
            internal string cultureNeutralFilename;
        };

        internal static ItemCultureInfo GetItemCultureInfo(string name)
        {
            ItemCultureInfo info;
            info.culture = null;

            // If the item is defined as "Strings.en-US.resx", then ...

            // ... base file name will be "Strings.en-US" ...
            string baseFileNameWithCulture = Path.GetFileNameWithoutExtension(name);

            // ... and cultureName will be ".en-US".
            string cultureName = Path.GetExtension(baseFileNameWithCulture);

            // See if this is a valid culture name.
            bool validCulture = false;
            if ((cultureName != null) && (cultureName.Length > 1))
            {
                // ... strip the "." to make "en-US"
                cultureName = cultureName.Substring(1);
                validCulture = IsValidCultureString(cultureName);
            }
            if (validCulture)
            {
                // A valid culture was found.
                if (info.culture == null || info.culture.Length == 0)
                {
                    info.culture = cultureName;
                }

                // Copy the assigned file and make it culture-neutral
                string extension = Path.GetExtension(name);
                string baseFileName = Path.GetFileNameWithoutExtension(baseFileNameWithCulture);
                string baseFolder = Path.GetDirectoryName(name);
                string fileName = baseFileName + extension;
                info.cultureNeutralFilename = Path.Combine(baseFolder, fileName);
            }
            else
            {
                // No valid culture was found. In this case, the culture-neutral
                // name is the just the original file name.
                info.cultureNeutralFilename = name;
            }
            return info;
        }

        private static bool IsValidCultureString(string cultureString)
        {
            if (cultureInfoStrings == null)
            {
                CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);

                cultureInfoStrings = new string[cultureInfos.Length];
                for (int i = 0; i < cultureInfos.Length; i++)
                {
                    cultureInfoStrings[i] = cultureInfos[i].ToString().ToLowerInvariant();
                }
                Array.Sort(cultureInfoStrings);
            }

            bool valid = true;

            if (Array.BinarySearch(cultureInfoStrings, cultureString.ToLowerInvariant()) < 0)
            {
                valid = false;
            }

            return valid;
        }
    }

    #region Class CompileWorkflowCleanupTask
    // This cleanup task is a work-around for VB compilation only.

    // Due to a limitation for VB.Net, we can not delete the temp file.  VB does back-ground compilation for
    // supporting intellisense.  It re-compiles when there is a file change event that happens to each source
    // file.  The temp file must be added to the OutputFiles collection in order for the compiler to pick it up.
    // This adds the temp file to the VB compiler project who would report an error if the file is deleted
    // when re-compilation happens in the back-ground.

    // However, if we don't delete the temp file, we have another problem.  When we're in code-seperation mode, 
    // we compile our xoml files on the fly and add the buffer that contains 
    // the code generated based on the xoml to the project.  This code conflicts with the code in the temp file, 
    // thus causing all sorts of type conflicting errors.  

    // Because the two reasons above, we wrote this cleanup task to keep the temp file but clear out the content
    // of the file, thus make it work for both cases.
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CompileWorkflowCleanupTask : Microsoft.Build.Utilities.Task, ITask
    {

        #region Members and Constructors

        private ITaskItem[] temporaryFiles = null;

        public CompileWorkflowCleanupTask()
            :
            base(new ResourceManager("System.Workflow.ComponentModel.BuildTasksStrings",
                                     Assembly.GetExecutingAssembly()))
        {
        }

        #endregion

        #region Input parameters
        public ITaskItem[] TemporaryFiles
        {
            get
            {
                return this.temporaryFiles;
            }
            set
            {
                this.temporaryFiles = value;
            }
        }
        #endregion

        #region Public method overrides
        public override bool Execute()
        {
            if (this.temporaryFiles != null)
            {
                foreach (ITaskItem tempFileTask in this.temporaryFiles)
                {
                    string tempFile = tempFileTask.ItemSpec;
                    if (File.Exists(tempFile))
                    {
                        FileStream fileStream = File.Open(tempFile, FileMode.Truncate);
                        fileStream.Close();
                    }
                }
            }
            return true;
        }

        #endregion
    }
    #endregion
}
