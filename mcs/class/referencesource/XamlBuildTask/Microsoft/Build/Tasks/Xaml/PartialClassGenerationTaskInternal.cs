//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{

    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Xml;
    using System.Reflection;
    using System.Globalization;
    using System.Runtime.Remoting.Lifetime;
    using Microsoft.Build.Utilities;
    using XamlBuildTask;
    using Microsoft.Build.Framework;

    internal class PartialClassGenerationTaskInternal : MarshalByRefObject
    {
        const string UnknownExceptionErrorCode = "XC1000";

        IList<ITaskItem> applicationMarkup;
        IList<string> generatedResources;
        IList<string> generatedCodeFiles;
        IList<ITaskItem> references;
        IList<string> sourceCodeFiles;
        HashSet<string> sourceFilePaths;
        IList<LogData> logData;
        IList<Assembly> assemblyNames;
        XamlSchemaContext schemaContext;
        HashSet<string> markupFileNames;
        IEnumerable<IXamlBuildTypeGenerationExtension> xamlBuildTypeGenerationExtensions;
        XamlBuildTypeGenerationExtensionContext buildContextForExtensions;

        // Set the lease lifetime according to the environment variable with the name defined by RemotingLeaseLifetimeInMinutesEnvironmentVariableName
        public override object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();
            XamlBuildTaskLeaseLifetimeHelper.SetLeaseLifetimeFromEnvironmentVariable(lease);
            return lease;
        }

        public IList<ITaskItem> ApplicationMarkup
        {
            get
            {
                if (this.applicationMarkup == null)
                {
                    this.applicationMarkup = new List<ITaskItem>();
                }
                return this.applicationMarkup;
            }
            set
            {
                this.applicationMarkup = value;
            }
        }

        public string AssemblyName
        { get; set; }

        public TaskLoggingHelper BuildLogger
        { get; set; }

        public XamlBuildTypeGenerationExtensionContext BuildContextForExtensions
        {
            get
            {
                if (this.buildContextForExtensions == null)
                {
                    XamlBuildTypeGenerationExtensionContext local = new XamlBuildTypeGenerationExtensionContext();
                    local.AssemblyName = this.AssemblyName;
                    local.IsInProcessXamlMarkupCompile = this.IsInProcessXamlMarkupCompile;
                    local.Language = this.Language;
                    local.OutputPath = this.OutputPath;
                    local.RootNamespace = this.RootNamespace;
                    local.AddSourceCodeFiles(this.SourceCodeFiles);
                    local.AddReferences(XamlBuildTaskServices.GetReferences(this.references));
                    local.XamlBuildLogger = this.BuildLogger;

                    this.buildContextForExtensions = local;
                }
                return this.buildContextForExtensions;
            }
        }

        public IList<string> GeneratedResources
        {
            get
            {
                if (this.generatedResources == null)
                {
                    this.generatedResources = new List<string>();
                }
                return this.generatedResources;
            }
        }

        public IList<string> GeneratedCodeFiles
        {
            get
            {
                if (this.generatedCodeFiles == null)
                {
                    this.generatedCodeFiles = new List<string>();
                }
                return generatedCodeFiles;
            }
        }
        
        public string GeneratedSourceExtension
        { get; set; }

        public string Language
        { get; set; }

        public IList<LogData> LogData
        {
            get
            {
                if (this.logData == null)
                {
                    this.logData = new List<LogData>();
                }
                return this.logData;
            }
        }

        public string OutputPath
        { get; set; }

        public IList<ITaskItem> References
        {
            get
            {
                if (this.references == null)
                {
                    this.references = new List<ITaskItem>();
                }
                return this.references;
            }
            set
            {
                this.references = value;
            }
        }

        public IList<Assembly> LoadedAssemblyList
        {
            get
            {
                if (this.assemblyNames == null)
                {
                    if (IsInProcessXamlMarkupCompile)
                    {
                        this.assemblyNames = cachedAssemblyList;
                    }
                    else
                    {
                        this.assemblyNames = new List<Assembly>();
                    }
                }
                return this.assemblyNames;
            }
            set
            {
                this.assemblyNames = value;
                if (IsInProcessXamlMarkupCompile)
                {
                    cachedAssemblyList = value;
                }
            }
        }

        public string MSBuildProjectDirectory
        { get; set; }

        private static IList<Assembly> cachedAssemblyList = null;

        public bool IsInProcessXamlMarkupCompile
        { get; set; }

        public string RootNamespace
        { get; set; }

        public IList<string> SourceCodeFiles
        {
            get
            {
                if (this.sourceCodeFiles == null)
                {
                    this.sourceCodeFiles = new List<string>();
                }
                return this.sourceCodeFiles;
            }
            set
            {
                this.sourceCodeFiles = value;
            }
        }

        public bool RequiresCompilationPass2
        { get; set; }

        public bool SupportExtensions
        { get; set; }

        HashSet<string> SourceFilePaths
        {
            get
            {
                if (sourceFilePaths == null)
                {
                    sourceFilePaths = new HashSet<string>();
                    if (SourceCodeFiles != null)
                    {
                        foreach (string sourceCodeFile in SourceCodeFiles)
                        {
                            sourceFilePaths.Add(sourceCodeFile);
                        }
                    }
                }
                return sourceFilePaths;
            }
            set
            {
                this.sourceFilePaths = value;
            }
        }

        public XamlSchemaContext SchemaContext
        {
            get
            {
                if (schemaContext == null)
                {
                    if (LoadedAssemblyList.Count > 0)
                    {
                        schemaContext = new XamlSchemaContext(LoadedAssemblyList);
                    }
                    else
                    {
                        schemaContext = new XamlSchemaContext();
                    }
                }
                return schemaContext;
            }
        }

        public string HelperClassFullName
        { get; set; }

        public IList<Tuple<string, string, string>> XamlBuildTaskTypeGenerationExtensionNames
        {
            get;
            set;
        }

        public bool MarkupCompilePass2ExtensionsPresent
        {
            get;
            set;
        } 

        public bool Execute()
        {
            try
            {
                if (this.ApplicationMarkup == null || this.ApplicationMarkup.Count == 0)
                {
                    return true;
                }
                if (!CodeDomProvider.IsDefinedLanguage(this.Language))
                {
                    throw FxTrace.Exception.Argument("Language", SR.UnrecognizedLanguage(this.Language));
                }

                if (this.SupportExtensions)
                {
                    this.xamlBuildTypeGenerationExtensions = XamlBuildTaskServices.GetXamlBuildTaskExtensions<IXamlBuildTypeGenerationExtension>(
                        this.XamlBuildTaskTypeGenerationExtensionNames,
                        this.BuildLogger,
                        this.MSBuildProjectDirectory);
                }

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(XamlBuildTaskServices.ReflectionOnlyAssemblyResolve);
                bool retVal = true;
                // We load the assemblies for the real builds
                // For intellisense builds, we load them the first time only
                if (!IsInProcessXamlMarkupCompile || this.LoadedAssemblyList == null)
                {
                    if (this.References != null)
                    {
                        try
                        {
                            this.LoadedAssemblyList = XamlBuildTaskServices.Load(this.References, IsInProcessXamlMarkupCompile);
                        }
                        catch (FileNotFoundException e)
                        {
                            XamlBuildTaskServices.LogException(this.BuildLogger, e.Message, e.FileName, 0, 0);
                            retVal = false;
                        }
                    }
                }

                CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider(this.Language);

                ProcessHelperClassGeneration(codeDomProvider); 
                foreach (ITaskItem app in ApplicationMarkup)
                {
                    string inputMarkupFile = app.ItemSpec;
                    try
                    {
                        retVal &= ProcessMarkupItem(app, codeDomProvider);
                    }
                    catch (LoggableException e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        XamlBuildTaskServices.LogException(this.BuildLogger, e.Message, e.Source, e.LineNumber, e.LinePosition);
                        retVal = false;
                    }
                    catch (FileLoadException e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        XamlBuildTaskServices.LogException(this.BuildLogger, SR.AssemblyCannotBeResolved(XamlBuildTaskServices.FileNotLoaded), inputMarkupFile, 0, 0);
                        retVal = false;
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        XamlBuildTaskServices.LogException(this.BuildLogger, e.Message, inputMarkupFile, 0, 0);
                        retVal = false;
                    }
                }

                // Add the files generated from extensions
                if (this.SupportExtensions)
                {
                    if (retVal)
                    {
                        foreach (string fileName in this.BuildContextForExtensions.GeneratedFiles)
                        {
                            this.GeneratedCodeFiles.Add(fileName);
                        }

                        foreach (string fileName in this.BuildContextForExtensions.GeneratedResourceFiles)
                        {
                            this.GeneratedResources.Add(fileName);
                        }
                    }
                }

                return retVal;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                // Log unknown errors that do not originate from the task.
                // Assumes that all known errors are logged when the exception is thrown.
                if (!(e is LoggableException))
                {
                    XamlBuildTaskServices.LogException(this.BuildLogger, e.Message);
                }
                return false;
            }
        }

        void ProcessHelperClassGeneration(CodeDomProvider codeDomProvider)
        {
            string codeFileName = "_" + this.AssemblyName + GetGeneratedSourceExtension(codeDomProvider);
            codeFileName = Path.Combine(this.OutputPath, codeFileName);


            string namespaceName = "XamlStaticHelperNamespace";
            string className = "_XamlStaticHelper";

            // Generate code file
            CodeCompileUnit codeUnit = new ClassGenerator(this.BuildLogger, codeDomProvider, this.Language).GenerateHelperClass(namespaceName, className, this.LoadedAssemblyList);
            WriteCode(codeDomProvider, codeUnit, codeFileName);
            this.GeneratedCodeFiles.Add(codeFileName);

            this.HelperClassFullName = namespaceName + "." + className;
        }

        bool ProcessMarkupItem(ITaskItem markupItem, CodeDomProvider codeDomProvider)
        {
            string markupItemFileName = markupItem.ItemSpec;
            XamlBuildTaskServices.PopulateModifiers(codeDomProvider);

            XamlNodeList xamlNodes = ReadXamlNodes(markupItemFileName);
            if (xamlNodes == null)
            {
                return false;
            }

            ClassData classData = ReadClassData(xamlNodes, markupItemFileName);

            string outputFileName = GetFileName(markupItemFileName);
            string codeFileName = Path.ChangeExtension(outputFileName, GetGeneratedSourceExtension(codeDomProvider));
            string markupFileName = Path.ChangeExtension(outputFileName, GeneratedSourceExtension + XamlBuildTaskServices.XamlExtension);
            classData.EmbeddedResourceFileName = Path.GetFileName(markupFileName);
            classData.HelperClassFullName = this.HelperClassFullName;

            // Check if code file with partial class exists
            classData.SourceFileExists = UserProvidedFileExists(markupItemFileName, codeDomProvider);

            // Store the full type name as metadata on the markup item
            string rootNamespacePrefix = null;
            string namespacePrefix = null;
            string typeFullName = null;
            if (this.Language.Equals("VB") && !String.IsNullOrWhiteSpace(classData.RootNamespace))
            {
                rootNamespacePrefix = classData.RootNamespace + ".";
            }

            if (!String.IsNullOrWhiteSpace(classData.Namespace))
            {
                namespacePrefix = classData.Namespace + ".";
            }

            if (rootNamespacePrefix != null)
            {
                if (namespacePrefix != null)
                {
                    typeFullName = rootNamespacePrefix + namespacePrefix + classData.Name;
                }
                else
                {
                    typeFullName = rootNamespacePrefix + classData.Name;
                }
            }
            else
            {
                if (namespacePrefix != null)
                {
                    typeFullName = namespacePrefix + classData.Name;
                }
                else
                {
                    typeFullName = classData.Name;
                }
            }

            markupItem.SetMetadata("typeName", typeFullName);

            // Execute extensions here to give them a chance to mutate the ClassData before we generate code.
            if (this.SupportExtensions)
            {
                if (!ExecuteExtensions(classData, markupItem))
                {
                    return false;
                }
            }

            // Generate code file
            CodeCompileUnit codeUnit = new ClassGenerator(this.BuildLogger, codeDomProvider, this.Language).Generate(classData);         
            WriteCode(codeDomProvider, codeUnit, codeFileName);
            this.GeneratedCodeFiles.Add(codeFileName);

            // Generate resource file
            if (!string.IsNullOrEmpty(this.AssemblyName))
            {
                // Generate xaml "implementation" file
                XmlWriterSettings xmlSettings = new XmlWriterSettings { Indent = true, IndentChars = "  ", CloseOutput = true };
                using (XmlWriter xmlWriter = XmlWriter.Create(File.Open(markupFileName, FileMode.Create), xmlSettings))
                {
                    XamlXmlWriterSettings xamlSettings = new XamlXmlWriterSettings() { CloseOutput = true };
                    
                    // Process EmbeddedResourceXaml to remove xml:space="preserve"
                    // due to a 




                    RemoveXamlSpaceAttribute(classData);

                    using (XamlReader reader = classData.EmbeddedResourceXaml.GetReader())
                    {
                        using (XamlXmlWriter xamlWriter = new XamlXmlWriter(xmlWriter, reader.SchemaContext, xamlSettings))
                        {
                            XamlServices.Transform(reader, xamlWriter);                                                     
                        }
                    }
                }
                this.GeneratedResources.Add(markupFileName);
            }

            if (classData.RequiresCompilationPass2)
            {
                this.RequiresCompilationPass2 = true;
            }
            else
            {
                if (!this.SupportExtensions)
                {
                    if (!ValidateXaml(xamlNodes, markupItemFileName))
                    {
                        this.RequiresCompilationPass2 = true;
                    }
                }
                else
                {
                    // skip validation if we are doing in-proc compile
                    // OR if we have pass 2 extensions hooked up 
                    // as we anyway need to run pass 2 in that case
                    if (!this.IsInProcessXamlMarkupCompile && !this.MarkupCompilePass2ExtensionsPresent)
                    {
                        if (!ValidateXaml(xamlNodes, markupItemFileName))
                        {
                            this.RequiresCompilationPass2 = true;
                        }
                    }
                }
            }
            return true;
        }

        bool ExecuteExtensions(ClassData classData, ITaskItem markupItem)
        {
            // Execute pass1 extensions only 
            // we skip pass1 extensions if we are doing in-proc compile
            if (!this.IsInProcessXamlMarkupCompile)
            {
                bool extensionExecutedSuccessfully = true;
                foreach (IXamlBuildTypeGenerationExtension extension in this.xamlBuildTypeGenerationExtensions)
                {
                    if (extension == null)
                    {
                        continue;
                    }

                    this.BuildContextForExtensions.InputTaskItem = markupItem;
                    try
                    {
                        extensionExecutedSuccessfully = extension.Execute(classData, this.BuildContextForExtensions) && extensionExecutedSuccessfully;
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        throw FxTrace.Exception.AsError(new LoggableException(SR.ExceptionThrownInExtension(extension.ToString(), e.GetType().ToString(), e.Message)));
                    }
                }

                if (this.BuildLogger.HasLoggedErrors || !extensionExecutedSuccessfully)
                {
                    return false;
                }
            }

            return true;
        }

        string GetFileName(string markupItem)
        {
            if (markupFileNames == null)
            {
                markupFileNames = new HashSet<string>();
            }

            string originalMarkupItemName = Path.GetFileNameWithoutExtension(markupItem);
            string markupItemName = originalMarkupItemName;
            int i = 0;
            while (this.markupFileNames.Contains(markupItemName))
            {
                markupItemName = originalMarkupItemName + "." + (++i).ToString(CultureInfo.InvariantCulture);
            }
            this.markupFileNames.Add(markupItemName);
            markupItemName = markupItemName + Path.GetExtension(markupItem);
            if (this.OutputPath == null)
            {
                throw FxTrace.Exception.AsError(
                        new InvalidOperationException(SR.OutputPathCannotBeNull));
            }
            return Path.Combine(this.OutputPath, markupItemName);
        }

        XamlNodeList ReadXamlNodes(string xamlFileName)
        {
            XamlNodeList nodeList = new XamlNodeList(this.SchemaContext);

            try
            {
                XamlXmlReaderSettings settings = new XamlXmlReaderSettings
                {
                    AllowProtectedMembersOnRoot = true,
                    ProvideLineInfo = true
                };

                using (StreamReader streamReader = new StreamReader(xamlFileName))
                {
                    XamlReader reader = new XamlXmlReader(XmlReader.Create(streamReader), this.SchemaContext, settings);
                    XamlServices.Transform(reader, nodeList.Writer);
                }
            }
            catch (XmlException e)
            {
                XamlBuildTaskServices.LogException(this.BuildLogger, e.Message, xamlFileName, e.LineNumber, e.LinePosition); 
                return null;
            }
            catch (XamlException e)
            {
                XamlBuildTaskServices.LogException(this.BuildLogger, e.Message, xamlFileName, e.LineNumber, e.LinePosition);
                return null;
            }
            
            if (nodeList.Count > 0)
            {
                return nodeList;
            }
            else
            {
                return null;
            }
        }

        ClassData ReadClassData(XamlNodeList xamlNodes, string xamlFileName)
        {
            ClassImporter importer = new ClassImporter(xamlFileName, this.AssemblyName, this.Language.Equals("VB") ? this.RootNamespace : null);
            ClassData classData = importer.ReadFromXaml(xamlNodes);
            return classData;
        }

        void RemoveXamlSpaceAttribute(ClassData classData)
        {
            using (XamlReader reader = classData.EmbeddedResourceXaml.GetReader())
            {
                XamlNodeList newList = new XamlNodeList(reader.SchemaContext);
                using (XamlWriter writer = newList.Writer)
                {
                    bool nodesAvailable = reader.Read();
                    while (nodesAvailable)
                    {                        
                        if (reader.NodeType == XamlNodeType.StartMember && reader.Member == XamlLanguage.Space)
                        {
                            reader.Skip();
                        }
                        else
                        {
                            writer.WriteNode(reader);
                            nodesAvailable = reader.Read();
                        }
                    }
                }
                classData.EmbeddedResourceXaml = newList;
            }
        }

        bool ValidateXaml(XamlNodeList xamlNodeList, string xamlFileName)
        {
            using (XamlReader xamlReader = xamlNodeList.GetReader())
            {
                IList<LogData> validationErrors = null;
                ClassValidator validator = new ClassValidator(xamlFileName, null, null);
                return validator.ValidateXaml(xamlReader, true, this.AssemblyName, out validationErrors);
            }
        }

        void WriteCode(CodeDomProvider provider, CodeCompileUnit codeUnit, string fileName)
        {
            using (StreamWriter fileStream = new StreamWriter(fileName))
            {
                using (IndentedTextWriter tw = new IndentedTextWriter(fileStream))
                {
                    provider.GenerateCodeFromCompileUnit(codeUnit, tw, new CodeGeneratorOptions());
                }
            }
        }

        string GetGeneratedSourceExtension(CodeDomProvider codeDomProvider)
        {
            string result = null;
            if (!string.IsNullOrEmpty(this.GeneratedSourceExtension))
            {
                result = this.GeneratedSourceExtension;
                if (!result.StartsWith(".", StringComparison.Ordinal))
                {
                    result = "." + result;
                }
            }
            return result + "." + codeDomProvider.FileExtension;
        }

        bool UserProvidedFileExists(string markupItemPath, CodeDomProvider codeDomProvider)
        {
            string desiredSourceFilePath = Path.ChangeExtension(markupItemPath, "xaml." + codeDomProvider.FileExtension);
            return SourceFilePaths.Contains(desiredSourceFilePath);
        }
    }
}
