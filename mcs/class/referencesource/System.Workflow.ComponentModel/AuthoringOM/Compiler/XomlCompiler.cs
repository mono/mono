namespace System.Workflow.ComponentModel.Compiler
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.CodeDom.Compiler;
    using System.Reflection;
    using System.ComponentModel.Design.Serialization;
    using System.Xml;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Security.Policy;
    using System.Runtime.Versioning;
    using System.Configuration;
    using System.Collections.ObjectModel;

    #endregion

    #region WorkflowMarkupSourceAttribute Class
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowMarkupSourceAttribute : Attribute
    {
        private string fileName;
        private string md5Digest;

        public WorkflowMarkupSourceAttribute(string fileName, string md5Digest)
        {
            this.fileName = fileName;
            this.md5Digest = md5Digest;
        }

        public string FileName
        {
            get
            {
                return this.fileName;
            }
        }

        public string MD5Digest
        {
            get
            {
                return this.md5Digest;
            }
        }
    }
    #endregion

    #region Interface IWorkflowCompilerOptionsService
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IWorkflowCompilerOptionsService
    {
        string RootNamespace { get; }
        string Language { get; }
        bool CheckTypes { get; }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowCompilerOptionsService : IWorkflowCompilerOptionsService
    {
        internal const string DefaultLanguage = "CSharp";

        public virtual string RootNamespace
        {
            get
            {
                return string.Empty;
            }
        }
        public virtual string Language
        {
            get
            {
                return WorkflowCompilerOptionsService.DefaultLanguage;
            }
        }
        public virtual bool CheckTypes
        {
            get
            {
                return false;
            }
        }
        public virtual string TargetFrameworkMoniker
        {
            get
            {
                return string.Empty;
            }
        }
    }
    #endregion

    #region WorkflowCompilationContext

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowCompilationContext
    {
        [ThreadStatic]
        static WorkflowCompilationContext current = null;

        ContextScope scope;
        ReadOnlyCollection<AuthorizedType> authorizedTypes;

        WorkflowCompilationContext(ContextScope scope)
        {
            this.scope = scope;
        }

        public static WorkflowCompilationContext Current
        {
            get
            {
                return WorkflowCompilationContext.current;
            }
            private set
            {
                WorkflowCompilationContext.current = value;
            }
        }

        public string RootNamespace
        {
            get
            {
                return this.scope.RootNamespace;
            }
        }
        public string Language
        {
            get
            {
                return this.scope.Language;
            }
        }
        public bool CheckTypes
        {
            get
            {
                return this.scope.CheckTypes;
            }
        }

        internal FrameworkName TargetFramework
        {
            get
            {
                return this.scope.TargetFramework;
            }
        }
        internal Version TargetFrameworkVersion
        {
            get
            {
                FrameworkName fx = this.scope.TargetFramework;
                if (fx != null)
                {
                    return fx.Version;
                }
                else
                {
                    return MultiTargetingInfo.DefaultTargetFramework;
                }
            }
        }
        internal IServiceProvider ServiceProvider
        {
            get
            {
                return this.scope;
            }
        }

        public static IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            IWorkflowCompilerOptionsService optionsService = serviceProvider.GetService(typeof(IWorkflowCompilerOptionsService)) as IWorkflowCompilerOptionsService;
            if (optionsService != null)
            {
                return CreateScope(serviceProvider, optionsService);
            }
            else
            {
                return new DefaultContextScope(serviceProvider);
            }
        }

        public IList<AuthorizedType> GetAuthorizedTypes()
        {
            if (this.authorizedTypes == null)
            {
                try
                {
                    IList<AuthorizedType> authorizedTypes;

                    IDictionary<string, IList<AuthorizedType>> authorizedTypesDictionary =
                        ConfigurationManager.GetSection("System.Workflow.ComponentModel.WorkflowCompiler/authorizedTypes") as IDictionary<string, IList<AuthorizedType>>;
                    Version targetVersion = null;
                    FrameworkName framework = this.scope.TargetFramework;
                    if (framework != null)
                    {
                        targetVersion = framework.Version;
                    }
                    else
                    {
                        targetVersion = MultiTargetingInfo.DefaultTargetFramework;
                    }

                    string normalizedVersionString = string.Format(CultureInfo.InvariantCulture, "v{0}.{1}", targetVersion.Major, targetVersion.Minor);

                    if (authorizedTypesDictionary.TryGetValue(normalizedVersionString, out authorizedTypes))
                    {
                        this.authorizedTypes = new ReadOnlyCollection<AuthorizedType>(authorizedTypes);
                    }
                }
                catch
                {
                }
            }
            return this.authorizedTypes;
        }

        internal static IDisposable CreateScope(IServiceProvider serviceProvider, WorkflowCompilerParameters parameters)
        {
            return new ParametersContextScope(serviceProvider, parameters);
        }

        static IDisposable CreateScope(IServiceProvider serviceProvider, IWorkflowCompilerOptionsService optionsService)
        {
            WorkflowCompilerOptionsService standardService = optionsService as WorkflowCompilerOptionsService;
            if (standardService != null)
            {
                return new StandardContextScope(serviceProvider, standardService);
            }
            else
            {
                return new InterfaceContextScope(serviceProvider, optionsService);
            }
        }

        abstract class ContextScope : IDisposable, IServiceProvider
        {
            IServiceProvider serviceProvider;
            WorkflowCompilationContext currentContext;
            bool disposed;

            protected ContextScope(IServiceProvider serviceProvider)
            {
                this.serviceProvider = serviceProvider;
                this.currentContext = WorkflowCompilationContext.Current;
                WorkflowCompilationContext.Current = new WorkflowCompilationContext(this);
            }
            ~ContextScope()
            {
                DisposeImpl();
            }

            public abstract string RootNamespace { get; }
            public abstract string Language { get; }
            public abstract bool CheckTypes { get; }
            public abstract FrameworkName TargetFramework { get; }

            public void Dispose()
            {
                DisposeImpl();
                GC.SuppressFinalize(this);
            }
            public object GetService(Type serviceType)
            {
                return this.serviceProvider.GetService(serviceType);
            }

            void DisposeImpl()
            {
                if (!this.disposed)
                {
                    WorkflowCompilationContext.Current = this.currentContext;
                    this.disposed = true;
                }
            }
        }
        class InterfaceContextScope : ContextScope
        {
            IWorkflowCompilerOptionsService service;

            public InterfaceContextScope(IServiceProvider serviceProvider, IWorkflowCompilerOptionsService service)
                : base(serviceProvider)
            {
                this.service = service;
            }

            public override string RootNamespace
            {
                get
                {
                    return this.service.RootNamespace;
                }
            }
            public override string Language
            {
                get
                {
                    return this.service.Language;
                }
            }
            public override bool CheckTypes
            {
                get
                {
                    return this.service.CheckTypes;
                }
            }
            public override FrameworkName TargetFramework
            {
                get
                {
                    return null;
                }
            }
        }
        class StandardContextScope : ContextScope
        {
            WorkflowCompilerOptionsService service;
            FrameworkName fxName;

            public StandardContextScope(IServiceProvider serviceProvider, WorkflowCompilerOptionsService service)
                : base(serviceProvider)
            {
                this.service = service;
            }

            public override string RootNamespace
            {
                get
                {
                    return this.service.RootNamespace;
                }
            }
            public override string Language
            {
                get
                {
                    return this.service.Language;
                }
            }
            public override bool CheckTypes
            {
                get
                {
                    return this.service.CheckTypes;
                }
            }
            public override FrameworkName TargetFramework
            {
                get
                {
                    if (this.fxName == null)
                    {
                        string fxName = this.service.TargetFrameworkMoniker;
                        if (!string.IsNullOrEmpty(fxName))
                        {
                            this.fxName = new FrameworkName(fxName);
                        }
                    }
                    return this.fxName;
                }
            }
        }
        class ParametersContextScope : ContextScope
        {
            WorkflowCompilerParameters parameters;

            public ParametersContextScope(IServiceProvider serviceProvider, WorkflowCompilerParameters parameters)
                : base(serviceProvider)
            {
                this.parameters = parameters;
            }

            public override string RootNamespace
            {
                get
                {
                    return WorkflowCompilerParameters.ExtractRootNamespace(this.parameters);
                }
            }
            public override string Language
            {
                get
                {
                    return this.parameters.LanguageToUse;
                }
            }

            public override bool CheckTypes
            {
                get
                {
                    return this.parameters.CheckTypes;
                }
            }
            public override FrameworkName TargetFramework
            {
                get
                {
                    if (this.parameters.MultiTargetingInformation != null)
                    {
                        return this.parameters.MultiTargetingInformation.TargetFramework;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        class DefaultContextScope : ContextScope
        {
            public DefaultContextScope(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public override string RootNamespace
            {
                get
                {
                    return string.Empty;
                }
            }
            public override string Language
            {
                get
                {
                    return WorkflowCompilerOptionsService.DefaultLanguage;
                }
            }
            public override bool CheckTypes
            {
                get
                {
                    return false;
                }
            }
            public override FrameworkName TargetFramework
            {
                get
                {
                    return null;
                }
            }
        }
    }

    #endregion

    #region Class WorkflowCompiler

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowCompiler
    {
        public WorkflowCompilerResults Compile(WorkflowCompilerParameters parameters, params string[] files)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (files == null)
                throw new ArgumentNullException("files");

            string createdDirectoryName = null;
            string createdTempFileName = null;

            AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
            setup.LoaderOptimization = LoaderOptimization.MultiDomainHost;
            AppDomain compilerDomain = AppDomain.CreateDomain("CompilerDomain", null, setup);

            bool generateInMemory = false;
            string originalOutputAssembly = parameters.OutputAssembly;

            try
            {
                if (parameters.GenerateInMemory)
                {
                    generateInMemory = true;
                    parameters.GenerateInMemory = false;

                    if (string.IsNullOrEmpty(parameters.OutputAssembly))
                    {
                        // We need to remember the filename generated by Path.GetTempFileName so we can clean it up.
                        createdTempFileName = Path.GetTempFileName();
                        parameters.OutputAssembly = createdTempFileName + ".dll";
                    }
                    else
                    {
                        int tries = 0;
                        while (true)
                        {
                            try
                            {
                                tries++;
                                createdDirectoryName = Path.GetTempPath() + "\\" + Guid.NewGuid();
                                DirectoryInfo info = Directory.CreateDirectory(createdDirectoryName);
                                parameters.OutputAssembly = info.FullName + "\\" + parameters.OutputAssembly;
                                break;
                            }
                            catch
                            {
                                // If we have tried 10 times without success, give up. Something must be wrong
                                // with what gets returned by GetTempPath or we have exceeded max_path by appending
                                // the GUID.
                                if (tries >= 10)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }

                WorkflowCompilerInternal compiler = (WorkflowCompilerInternal)compilerDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(WorkflowCompilerInternal).FullName);
                WorkflowCompilerResults results = compiler.Compile(parameters, files);

                if (generateInMemory && !results.Errors.HasErrors)
                {
                    results.CompiledAssembly = Assembly.Load(File.ReadAllBytes(results.PathToAssembly));
                    results.PathToAssembly = null;
                }

                return results;
            }
            finally
            {
                string outputAssembly = parameters.OutputAssembly;

                if (generateInMemory)
                {
                    parameters.GenerateInMemory = true;
                    parameters.OutputAssembly = originalOutputAssembly;
                }

                AppDomain.Unload(compilerDomain);

                // The temp file must be deleted after the app domain is unloaded, or else it will
                // be "busy", causing the delete to throw an access exception.
                if (generateInMemory)
                {
                    try
                    {
                        // There will always be an outputAssemblyName to delete.
                        File.Delete(outputAssembly);

                        // If we created a temp file name with Path.GetTempFileName, we need to delete it here.
                        if (createdTempFileName != null)
                        {
                            File.Delete(createdTempFileName);
                        }

                        // If we created a directory, delete it.
                        if (createdDirectoryName != null)
                        {
                            Directory.Delete(createdDirectoryName, true);
                        }
                    }
                    catch
                    { }
                }
            }
        }
    }

    #endregion

    #region Class WorkflowCompilerInternal

    internal sealed class WorkflowCompilerInternal : MarshalByRefObject
    {
        #region Lifetime service

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region File based compilation

        public WorkflowCompilerResults Compile(WorkflowCompilerParameters parameters, string[] allFiles)
        {
            WorkflowCompilerResults results = new WorkflowCompilerResults(parameters.TempFiles);

            // Split the xoml files from cs/vb files.
            StringCollection xomlFiles = new StringCollection();
            StringCollection userCodeFiles = new StringCollection();
            foreach (string file in allFiles)
            {
                if (file.EndsWith(".xoml", StringComparison.OrdinalIgnoreCase))
                    xomlFiles.Add(file);
                else
                    userCodeFiles.Add(file);
            }

            string[] files = new string[xomlFiles.Count];
            xomlFiles.CopyTo(files, 0);
            string[] codeFiles = new string[userCodeFiles.Count];
            userCodeFiles.CopyTo(codeFiles, 0);

            string mscorlibPath = typeof(object).Assembly.Location;
            ServiceContainer serviceContainer = new ServiceContainer();
            MultiTargetingInfo mtInfo = parameters.MultiTargetingInformation;
            if (mtInfo == null)
            {
                XomlCompilerHelper.FixReferencedAssemblies(parameters, results, parameters.LibraryPaths);
            }
            string mscorlibName = Path.GetFileName(mscorlibPath);

            // Add assembly resolver.
            ReferencedAssemblyResolver resolver = new ReferencedAssemblyResolver(parameters.ReferencedAssemblies, parameters.LocalAssembly);
            AppDomain.CurrentDomain.AssemblyResolve += resolver.ResolveEventHandler;

            // prepare service container
            TypeProvider typeProvider = new TypeProvider(new ServiceContainer());
            int mscorlibIndex = -1;
            if ((parameters.ReferencedAssemblies != null) && (parameters.ReferencedAssemblies.Count > 0))
            {
                for (int i = 0; i < parameters.ReferencedAssemblies.Count; i++)
                {
                    string assemblyPath = parameters.ReferencedAssemblies[i];
                    if ((mscorlibIndex == -1) && (string.Compare(mscorlibName, Path.GetFileName(assemblyPath), StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        mscorlibIndex = i;
                        mscorlibPath = assemblyPath;
                    }
                    typeProvider.AddAssemblyReference(assemblyPath);
                }
            }
            // a note about references to mscorlib:
            //  If we found mscorlib in the list of reference assemblies, we should remove it prior to sending it to the CodeDOM compiler.
            //  The CodeDOM compiler would add the right mscorlib [based on the version of the provider we use] and the duplication would
            //  cause a compilation error.
            //  If we didn't found a reference to mscorlib we need to add it to the type-provider, though, so we will support exposing
            //  those known types.
            if (mscorlibIndex != -1)
            {
                parameters.ReferencedAssemblies.RemoveAt(mscorlibIndex);
                if (string.IsNullOrEmpty(parameters.CoreAssemblyFileName))
                {
                    parameters.CoreAssemblyFileName = mscorlibPath;
                }
            }
            else
            {
                typeProvider.AddAssemblyReference(mscorlibPath);
            }

            serviceContainer.AddService(typeof(ITypeProvider), typeProvider);
            
            TempFileCollection intermediateTempFiles = null;
            string localAssemblyPath = string.Empty;
            string createdDirectoryName = null;

            try
            {
                using (WorkflowCompilationContext.CreateScope(serviceContainer, parameters))
                {
                    parameters.LocalAssembly = GenerateLocalAssembly(files, codeFiles, parameters, results, out intermediateTempFiles, out localAssemblyPath, out createdDirectoryName);
                    if (parameters.LocalAssembly != null)
                    {
                        // WinOE 

                        resolver.SetLocalAssembly(parameters.LocalAssembly);

                        // Work around HERE!!!
                        // prepare type provider
                        typeProvider.SetLocalAssembly(parameters.LocalAssembly);
                        typeProvider.AddAssembly(parameters.LocalAssembly);

                        results.Errors.Clear();
                        XomlCompilerHelper.InternalCompileFromDomBatch(files, codeFiles, parameters, results, localAssemblyPath);
                    }
                }
            }
            catch (Exception e)
            {
                results.Errors.Add(new WorkflowCompilerError(String.Empty, -1, -1, ErrorNumbers.Error_UnknownCompilerException.ToString(CultureInfo.InvariantCulture), SR.GetString(SR.Error_CompilationFailed, e.Message)));
            }
            finally
            {
                // Delate the temp files.
                if (intermediateTempFiles != null && parameters.TempFiles.KeepFiles == false)
                {
                    foreach (string file in intermediateTempFiles)
                    {
                        try
                        {
                            System.IO.File.Delete(file);
                        }
                        catch
                        {
                        }
                    }

                    try
                    {
                        // GenerateLocalAssembly may have created a directory, so let's try to delete it
                        // We can't just delete Path.GetDirectoryName(localAssemblyPath) because it might be the Temp directory.
                        if (createdDirectoryName != null)
                        {
                            Directory.Delete(createdDirectoryName, true);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return results;
        }

        #endregion

        #region Code for Generating Local Assembly

        private static ValidationErrorCollection ValidateIdentifiers(IServiceProvider serviceProvider, Activity activity)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();
            Dictionary<string, int> names = new Dictionary<string, int>();

            Walker walker = new Walker();
            walker.FoundActivity += delegate(Walker walker2, WalkerEventArgs e)
            {
                Activity currentActivity = e.CurrentActivity;
                if (!currentActivity.Enabled)
                {
                    e.Action = WalkerAction.Skip;
                    return;
                }

                ValidationError identifierError = null;

                if (names.ContainsKey(currentActivity.QualifiedName))
                {
                    if (names[currentActivity.QualifiedName] != 1)
                    {
                        identifierError = new ValidationError(SR.GetString(SR.Error_DuplicatedActivityID, currentActivity.QualifiedName), ErrorNumbers.Error_DuplicatedActivityID, false, "Name");
                        identifierError.UserData[typeof(Activity)] = currentActivity;
                        validationErrors.Add(identifierError);
                        names[currentActivity.QualifiedName] = 1;
                    }

                    return;
                }
                // Undone: AkashS - remove this check when we allow root activities to not have a name.
                if (!string.IsNullOrEmpty(currentActivity.Name))
                {
                    names[currentActivity.Name] = 0;
                    identifierError = ValidationHelpers.ValidateIdentifier("Name", serviceProvider, currentActivity.Name);
                    if (identifierError != null)
                    {
                        identifierError.UserData[typeof(Activity)] = currentActivity;
                        validationErrors.Add(identifierError);
                    }
                }
            };

            walker.Walk(activity as Activity);
            return validationErrors;
        }

        private Assembly GenerateLocalAssembly(string[] files, string[] codeFiles, WorkflowCompilerParameters parameters, WorkflowCompilerResults results, out TempFileCollection tempFiles2, out string localAssemblyPath, out string createdDirectoryName)
        {
            localAssemblyPath = string.Empty;
            createdDirectoryName = null;
            tempFiles2 = null;

            // Generate code for the markup files.
            CodeCompileUnit markupCompileUnit = GenerateCodeFromFileBatch(files, parameters, results);
            if (results.Errors.HasErrors)
                return null;

            SupportedLanguages language = CompilerHelpers.GetSupportedLanguage(parameters.LanguageToUse);

            // Convert all compile units to source files.
            CodeDomProvider codeDomProvider = CompilerHelpers.GetCodeDomProvider(language, parameters.CompilerVersion);

            // Clone the parameters.
            CompilerParameters clonedParams = XomlCompilerHelper.CloneCompilerParameters(parameters);
            clonedParams.TempFiles.KeepFiles = true;
            tempFiles2 = clonedParams.TempFiles;

            clonedParams.GenerateInMemory = true;

            if (string.IsNullOrEmpty(parameters.OutputAssembly))
                localAssemblyPath = clonedParams.OutputAssembly = clonedParams.TempFiles.AddExtension("dll");
            else
            {
                string tempAssemblyDirectory = clonedParams.TempFiles.BasePath;
                int postfix = 0;
                while (true)
                {
                    try
                    {
                        if (Directory.Exists(tempAssemblyDirectory))
                        {
                            break;
                        }
                        Directory.CreateDirectory(tempAssemblyDirectory);
                        createdDirectoryName = tempAssemblyDirectory;
                        break;
                    }
                    catch
                    {
                        // If we have tried 10 times without success, give up. Something must be wrong
                        // with what gets returned by TempFiles.BasePath
                        if (postfix >= 10)
                        {
                            throw;
                        }
                        tempAssemblyDirectory = clonedParams.TempFiles.BasePath + postfix++;
                    }
                    
                }
                localAssemblyPath = clonedParams.OutputAssembly = tempAssemblyDirectory + "\\" + Path.GetFileName(clonedParams.OutputAssembly);
                clonedParams.TempFiles.AddFile(localAssemblyPath, true);

                // Working around the fact that when the OutputAssembly is specified, the
                // codeDomProvider.CompileAssemblyFromFile call below does NOT add the pdb file
                // to the clonedParams.TempFiles collection. Instead, it looks as though it
                // does a clonedParams.TempFiles.BasePath.AddExtension("pdb"), which is a file
                // that doesn't actually get created.
                // We need to add the pdb file to the clonedParameters.TempFiles collection so that
                // it gets deleted, even in the case where we didn't end up creating the tempAssemblyDirectory above.
                string pdbFilename = Path.GetFileNameWithoutExtension(localAssemblyPath) + ".pdb";
                clonedParams.TempFiles.AddFile(Path.GetDirectoryName(localAssemblyPath) + "\\" + pdbFilename, true);
            }

            // Explictily ignore warnings (in case the user set this property in the project options).
            clonedParams.TreatWarningsAsErrors = false;

            if (clonedParams.CompilerOptions != null && clonedParams.CompilerOptions.Length > 0)
            {
                // Need to remove /delaysign option together with the /keyfile or /keycontainer
                // the temp assembly should not be signed or we'll have problems loading it.

                // Custom splitting: need to take strings like '"one two"' into account 
                // even though it has a space inside, it should not be split.

                string source = clonedParams.CompilerOptions;
                ArrayList optionsList = new ArrayList();
                int begin = 0;
                int end = 0;
                bool insideString = false;
                while (end < source.Length)
                {
                    int currentLength = end - begin;
                    if (source[end] == '"')
                    {
                        insideString = !insideString;
                    }
                    else if (source[end] == ' ' && !insideString)
                    {
                        // Split only if not inside string like in "inside some string".
                        // Split here. Ignore multiple spaces.
                        if (begin == end)
                        {
                            begin++; // end will get incremented in the end of the loop.
                        }
                        else
                        {
                            string substring = source.Substring(begin, end - begin);
                            optionsList.Add(substring);
                            begin = end + 1; // end will get incremented in the end of the loop
                        }
                    }

                    end++;
                }

                // The remaining sub-string.
                if (begin != end)
                {
                    string substring = source.Substring(begin, end - begin);
                    optionsList.Add(substring);
                }

                string[] options = optionsList.ToArray(typeof(string)) as string[];

                clonedParams.CompilerOptions = string.Empty;
                foreach (string option in options)
                {
                    if (option.Length > 0 &&
                        !option.StartsWith("/delaysign", StringComparison.OrdinalIgnoreCase) &&
                        !option.StartsWith("/keyfile", StringComparison.OrdinalIgnoreCase) &&
                        !option.StartsWith("/keycontainer", StringComparison.OrdinalIgnoreCase))
                    {
                        clonedParams.CompilerOptions += " " + option;
                    }
                }
            }

            // Disable compiler optimizations, but include debug information.
            clonedParams.CompilerOptions = (clonedParams.CompilerOptions == null) ? "/optimize-" : clonedParams.CompilerOptions + " /optimize-";
            clonedParams.IncludeDebugInformation = true;

            if (language == SupportedLanguages.CSharp)
                clonedParams.CompilerOptions += " /unsafe";

            // Add files.
            ArrayList ccus = new ArrayList((ICollection)parameters.UserCodeCompileUnits);
            ccus.Add(markupCompileUnit);
            ArrayList userCodeFiles = new ArrayList();
            userCodeFiles.AddRange(codeFiles);
            userCodeFiles.AddRange(XomlCompilerHelper.GenerateFiles(codeDomProvider, clonedParams, (CodeCompileUnit[])ccus.ToArray(typeof(CodeCompileUnit))));

            // Generate the temporary assembly.
            CompilerResults results2 = codeDomProvider.CompileAssemblyFromFile(clonedParams, (string[])userCodeFiles.ToArray(typeof(string)));
            if (results2.Errors.HasErrors)
            {
                results.AddCompilerErrorsFromCompilerResults(results2);
                return null;
            }


            return results2.CompiledAssembly;
        }

        internal static CodeCompileUnit GenerateCodeFromFileBatch(string[] files, WorkflowCompilerParameters parameters, WorkflowCompilerResults results)
        {
            WorkflowCompilationContext context = WorkflowCompilationContext.Current;
            if (context == null)
                throw new Exception(SR.GetString(SR.Error_MissingCompilationContext));

            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            foreach (string fileName in files)
            {
                Activity rootActivity = null;
                try
                {
                    DesignerSerializationManager manager = new DesignerSerializationManager(context.ServiceProvider);
                    using (manager.CreateSession())
                    {
                        WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(manager);
                        xomlSerializationManager.WorkflowMarkupStack.Push(parameters);
                        xomlSerializationManager.LocalAssembly = parameters.LocalAssembly;
                        using (XmlReader reader = XmlReader.Create(fileName))
                            rootActivity = WorkflowMarkupSerializationHelpers.LoadXomlDocument(xomlSerializationManager, reader, fileName);

                        if (parameters.LocalAssembly != null)
                        {
                            foreach (object error in manager.Errors)
                            {
                                if (error is WorkflowMarkupSerializationException)
                                {
                                    results.Errors.Add(new WorkflowCompilerError(fileName, (WorkflowMarkupSerializationException)error));
                                }
                                else
                                {
                                    results.Errors.Add(new WorkflowCompilerError(fileName, -1, -1, ErrorNumbers.Error_SerializationError.ToString(CultureInfo.InvariantCulture), error.ToString()));
                                }
                            }
                        }

                    }
                }
                catch (WorkflowMarkupSerializationException xomlSerializationException)
                {
                    results.Errors.Add(new WorkflowCompilerError(fileName, xomlSerializationException));
                    continue;
                }
                catch (Exception e)
                {
                    results.Errors.Add(new WorkflowCompilerError(fileName, -1, -1, ErrorNumbers.Error_SerializationError.ToString(CultureInfo.InvariantCulture), SR.GetString(SR.Error_CompilationFailed, e.Message)));
                    continue;
                }

                if (rootActivity == null)
                {
                    results.Errors.Add(new WorkflowCompilerError(fileName, 1, 1, ErrorNumbers.Error_SerializationError.ToString(CultureInfo.InvariantCulture), SR.GetString(SR.Error_RootActivityTypeInvalid)));
                    continue;
                }

                bool createNewClass = (!string.IsNullOrEmpty(rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string));
                if (!createNewClass)
                {
                    results.Errors.Add(new WorkflowCompilerError(fileName, 1, 1, ErrorNumbers.Error_SerializationError.ToString(CultureInfo.InvariantCulture), SR.GetString(SR.Error_CannotCompile_No_XClass)));
                    continue;
                }

                //NOTE: CompileWithNoCode is meaningless now. It means no x:Code in a XOML file. It exists until the FP migration is done
                //Ideally FP should just use XOML files w/o X:Class and run them w/o ever compiling them
                if ((parameters.CompileWithNoCode) && XomlCompilerHelper.HasCodeWithin(rootActivity))
                {
                    ValidationError error = new ValidationError(SR.GetString(SR.Error_CodeWithinNotAllowed), ErrorNumbers.Error_CodeWithinNotAllowed);
                    error.UserData[typeof(Activity)] = rootActivity;
                    results.Errors.Add(XomlCompilerHelper.CreateXomlCompilerError(error, parameters));
                }

                ValidationErrorCollection errors = new ValidationErrorCollection();

                errors = ValidateIdentifiers(context.ServiceProvider, rootActivity);
                foreach (ValidationError error in errors)
                    results.Errors.Add(XomlCompilerHelper.CreateXomlCompilerError(error, parameters));

                if (results.Errors.HasErrors)
                    continue;

                codeCompileUnit.Namespaces.AddRange(WorkflowMarkupSerializationHelpers.GenerateCodeFromXomlDocument(rootActivity, fileName, context.RootNamespace, CompilerHelpers.GetSupportedLanguage(context.Language), context.ServiceProvider));
            }

            WorkflowMarkupSerializationHelpers.FixStandardNamespacesAndRootNamespace(codeCompileUnit.Namespaces, context.RootNamespace, CompilerHelpers.GetSupportedLanguage(context.Language));
            return codeCompileUnit;
        }
        #endregion
    }

    #endregion
}
