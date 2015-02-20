//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Xml;
    using System.Reflection;
    using System.Runtime;
    using System.Globalization;
    using Microsoft.Build.Utilities;
    using XamlBuildTask;
    using Microsoft.Build.Framework;

    internal class CompilationPass2TaskInternal : MarshalByRefObject 
    {
        IList<string> applicationMarkup;
        IList<ITaskItem> references;
        IList<LogData> logData;
        IList<string> sourceCodeFiles;
        IList<string> generatedCodeFiles;
        XamlBuildTypeInspectionExtensionContext buildContextForExtensions;
        IDictionary<string, ITaskItem> applicationMarkupWithTypeName;

        public IList<string> ApplicationMarkup
        {
            get
            {
                if (this.applicationMarkup == null)
                {
                    this.applicationMarkup = new List<string>();
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

        public string LocalAssemblyReference
        { get; set; }

        public string RootNamespace
        { get; set; }

        public string MSBuildProjectDirectory
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

        public IDictionary<string, ITaskItem> ApplicationMarkupWithTypeName
        {
            get
            {
                if (this.applicationMarkupWithTypeName == null)
                {
                    this.applicationMarkupWithTypeName = new Dictionary<string, ITaskItem>();
                }
                return applicationMarkupWithTypeName;
            }
            set
            {
                this.applicationMarkupWithTypeName = value;
            }
        }

        public string OutputPath
        { get; set; }

        public string Language
        { get; set; }

        public bool IsInProcessXamlMarkupCompile
        { get; set; }

        public IList<Tuple<string, string, string>> XamlBuildTaskTypeInspectionExtensionNames
        { get; set; }

        public IList<Tuple<AssemblyName, Assembly>> ReferencedAssemblies
        { get; set; }

        public bool SupportExtensions
        { get; set; }

        public XamlBuildTypeInspectionExtensionContext BuildContextForExtensions
        {
            get
            {
                if (this.buildContextForExtensions == null)
                {
                    XamlBuildTypeInspectionExtensionContext local = new XamlBuildTypeInspectionExtensionContext();              
                    local.AssemblyName = this.AssemblyName;
                    local.IsInProcessXamlMarkupCompile = this.IsInProcessXamlMarkupCompile;
                    local.Language = this.Language;
                    local.OutputPath = this.OutputPath;
                    local.RootNamespace = this.RootNamespace;
                    local.AddSourceCodeFiles(this.SourceCodeFiles);
                    local.LocalAssembly = this.LocalAssemblyReference;
                    local.XamlBuildLogger = this.BuildLogger;
                    local.AddReferences(XamlBuildTaskServices.GetReferences(this.references));
                    local.AddApplicationMarkupWithTypeName(this.ApplicationMarkupWithTypeName);

                    this.buildContextForExtensions = local;
                }
                return this.buildContextForExtensions;
            }
        }

        public bool Execute()
        {
            try
            {
                if ((!this.SupportExtensions) && ((this.ApplicationMarkup == null) || this.ApplicationMarkup.Count == 0))
                {
                    return true;
                }
                else if (this.ApplicationMarkupWithTypeName == null || this.ApplicationMarkupWithTypeName.Count == 0)
                {
                    return true;
                }

                IList<Assembly> loadedAssemblyList = null;
                if (this.References != null)
                {
                    loadedAssemblyList = XamlBuildTaskServices.Load(this.References, false);
                }

                Assembly localAssembly = null;
                if (LocalAssemblyReference != null)
                {
                    try
                    {
                        localAssembly = XamlBuildTaskServices.Load(LocalAssemblyReference);
                        loadedAssemblyList.Add(localAssembly);
                    }
                    catch (FileNotFoundException e)
                    {
                        XamlBuildTaskServices.LogException(this.BuildLogger, e.Message, e.FileName, 0, 0);
                        return false;
                    }
                }

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(XamlBuildTaskServices.ReflectionOnlyAssemblyResolve);
                XamlNsReplacingContext wxsc = new XamlNsReplacingContext(loadedAssemblyList, localAssembly.GetName().Name, this.AssemblyName);

                bool foundValidationErrors = false;
                if (!this.SupportExtensions)
                {
                    foreach (string app in ApplicationMarkup)
                    {
                        try
                        {
                            if (!ProcessMarkupItem(app, wxsc, localAssembly))
                            {
                                foundValidationErrors = true;
                            }
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                            XamlBuildTaskServices.LogException(this.BuildLogger, e.Message, app, 0, 0);
                            return false;
                        }
                    }
                }
                else
                {
                    foreach (ITaskItem app in this.ApplicationMarkupWithTypeName.Values)
                    {
                        string inputMarkupFile = app.ItemSpec;
                        try
                        {
                            if (!ProcessMarkupItem(inputMarkupFile, wxsc, localAssembly))
                            {
                                foundValidationErrors = true;
                            }
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                            XamlBuildTaskServices.LogException(this.BuildLogger, e.Message, inputMarkupFile, 0, 0);
                            return false;
                        }
                    }
                    if (!foundValidationErrors)
                    {
                        foundValidationErrors = !ExecuteExtensions();
                        if (!foundValidationErrors)
                        {
                            foundValidationErrors = this.BuildLogger.HasLoggedErrors;
                        }
                    }
                }
                return !foundValidationErrors;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                // Log unknown errors that do not originate from the task.
                // Assumes that all known errors are logged when the exception is thrown.
                XamlBuildTaskServices.LogException(this.BuildLogger, e.Message);
                return false;
            }
        }

        bool ProcessMarkupItem(string markupItem, XamlNsReplacingContext wxsc, Assembly localAssembly)
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings() { LocalAssembly = localAssembly, ProvideLineInfo = true, AllowProtectedMembersOnRoot = true };
            using (StreamReader streamReader = new StreamReader(markupItem))
            {
                var xamlReader = new XamlXmlReader(XmlReader.Create(streamReader), wxsc, settings);
                ClassValidator validator = new ClassValidator(markupItem, localAssembly, this.RootNamespace);
                IList<LogData> validationErrors = null;
                if (validator.ValidateXaml(xamlReader, false, this.AssemblyName, out validationErrors))
                {
                    return true;
                }
                else
                {
                    foreach (LogData logData in validationErrors)
                    {
                        this.LogData.Add(logData);
                    }
                    return false;
                }
            }
        }
               
        bool ExecuteExtensions()
        {
            ResolveAssemblyHelper resolveAssemblyHelper = new ResolveAssemblyHelper(XamlBuildTaskServices.GetReferences(this.References));
            AppDomain.CurrentDomain.AssemblyResolve += resolveAssemblyHelper.ResolveLocalProjectReferences;

            bool extensionExecutedSuccessfully = true;
            try
            {                
                IEnumerable<IXamlBuildTypeInspectionExtension> extensions =
                    XamlBuildTaskServices.GetXamlBuildTaskExtensions<IXamlBuildTypeInspectionExtension>(
                    this.XamlBuildTaskTypeInspectionExtensionNames,
                    this.BuildLogger,
                    this.MSBuildProjectDirectory);                

                foreach (IXamlBuildTypeInspectionExtension extension in extensions)
                {
                    try
                    {
                        extensionExecutedSuccessfully &= extension.Execute(this.BuildContextForExtensions);
                    }
                    catch (FileNotFoundException e)
                    {
                        throw FxTrace.Exception.AsError(new LoggableException(SR.ExceptionThrownInExtension(extension.ToString(), e.GetType().ToString(), SR.AssemblyNotFound(ResolveAssemblyHelper.FileNotFound))));
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
                if (!this.BuildLogger.HasLoggedErrors && extensionExecutedSuccessfully)
                {
                    foreach (string file in this.BuildContextForExtensions.GeneratedFiles)
                    {
                        this.GeneratedCodeFiles.Add(file);
                    }
                }
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolveAssemblyHelper.ResolveLocalProjectReferences;
            }
            return extensionExecutedSuccessfully;
        }
    }
}
