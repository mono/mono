//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using XamlBuildTask;

    [Fx.Tag.XamlVisible(true)]
    public class CompilationPass2Task : Task
    {
        List<ITaskItem> generatedCodeFiles = new List<ITaskItem>();

        // We will do Dev10 behavior if NONE of the new required properties are specified. This can happen
        // if a Dev10 version of the Microsoft.Xaml.Targets file is being used with Dev11 installed.
        // The new required properties for Dev11 are:
        //    Language
        //    OutputPath
        //    MSBuildProjectDirectory
        //    ApplicationMarkupWithTypeName
        //
        // If ANY of these are specified, then ALL must be specified.
        bool supportExtensions = false;
        string language;
        string outputPath;
        string msBuildProjectDirectory;
        ITaskItem[] applicationMarkupWithTypeName;

        public CompilationPass2Task()
        {
        }

        [Fx.Tag.KnownXamlExternal]
        public ITaskItem[] ApplicationMarkup { get; set; }

        public string AssemblyName
        { get; set; }

        [Fx.Tag.KnownXamlExternal]
        public ITaskItem[] References { get; set; }

        // Required in Dev11, but for backward compatibility with a Dev10 targets file, not marking as required.
        public string Language
        {
            get
            {
                return this.language;
            }

            set
            {
                this.language = value;
                this.supportExtensions = true;
            }
        }

        // Required in Dev11, but for backward compatibility with a Dev10 targets file, not marking as required.
        public string OutputPath
        {
            get
            {
                return this.outputPath;
            }

            set
            {
                this.outputPath = value;
                this.supportExtensions = true;
            }
        }

        // Required in Dev11, but for backward compatibility with a Dev10 targets file, not marking as required.
        public string MSBuildProjectDirectory
        {
            get
            {
                return this.msBuildProjectDirectory;
            }

            set
            {
                this.msBuildProjectDirectory = value;
                this.supportExtensions = true;
            }
        }

        public bool IsInProcessXamlMarkupCompile
        { get; set; }

        [Fx.Tag.KnownXamlExternal]
        public ITaskItem[] SourceCodeFiles
        { get; set; }
        
        public ITaskItem[] XamlBuildTypeInspectionExtensionNames
        { get; set; }

        // Required in Dev11, but for backward compatibility with a Dev10 targets file, not marking as required.
        public ITaskItem[] ApplicationMarkupWithTypeName
        {
            get
            {
                return this.applicationMarkupWithTypeName;
            }

            set
            {
                this.applicationMarkupWithTypeName = value;
                this.supportExtensions = true;
            }
        }

        public string LocalAssemblyReference
        { get; set; }

        public string RootNamespace
        { get; set; }

        public string BuildTaskPath
        { get; set; }

        [Output]
        public ITaskItem[] ExtensionGeneratedCodeFiles
        {
            get
            {
                return generatedCodeFiles.ToArray();
            }
            set
            {
                generatedCodeFiles = new List<ITaskItem>(value);
            }
        }

        public override bool Execute()
        {
            AppDomain appDomain = null;

            try
            {
                ValidateRequiredDev11Properties();

                appDomain = XamlBuildTaskServices.CreateAppDomain("CompilationPass2AppDomain_" + Guid.NewGuid(), BuildTaskPath);

                CompilationPass2TaskInternal wrapper = (CompilationPass2TaskInternal)appDomain.CreateInstanceAndUnwrap(
                    Assembly.GetExecutingAssembly().FullName,
                    typeof(CompilationPass2TaskInternal).FullName);

                PopulateBuildArtifacts(wrapper);

                bool ret = wrapper.Execute();

                ExtractBuiltArtifacts(wrapper);

                if (!ret)
                {
                    foreach (LogData logData in wrapper.LogData)
                    {
                        XamlBuildTaskServices.LogException(
                            this.Log,
                            logData.Message,
                            logData.FileName,
                            logData.LineNumber,
                            logData.LinePosition);
                    }
                }

                return ret;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                XamlBuildTaskServices.LogException(this.Log, e.Message);
                return false;
            }
            finally
            {
                if (appDomain != null)
                {
                    AppDomain.Unload(appDomain);
                }
            }
        }

        string SeparateWithComma(string originalString, string stringToAppend)
        {
            if (!String.IsNullOrEmpty(originalString))
            {
                return String.Join(", ", originalString, stringToAppend);
            }
            else
            {
                return stringToAppend;
            }
        }

        void ValidateRequiredDev11Properties()
        {
            if (this.supportExtensions)
            {
                string requiredPropertiesNotSpecified = "";
                if (this.language == null)
                {
                    requiredPropertiesNotSpecified = SeparateWithComma(requiredPropertiesNotSpecified, "Language");
                }
                if (this.outputPath == null)
                {
                    requiredPropertiesNotSpecified = SeparateWithComma(requiredPropertiesNotSpecified, "OutputPath");
                }
                if (this.msBuildProjectDirectory == null)
                {
                    requiredPropertiesNotSpecified = SeparateWithComma(requiredPropertiesNotSpecified, "MSBuildProjectDirectory");
                }
                if (this.applicationMarkupWithTypeName == null)
                {
                    requiredPropertiesNotSpecified = SeparateWithComma(requiredPropertiesNotSpecified, "ApplicationMarkupWithTypeName");
                }
                if (!String.IsNullOrEmpty(requiredPropertiesNotSpecified))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.MissingRequiredParametersCompilationPass2Task(requiredPropertiesNotSpecified)));
                }
            }
        }

        void PopulateBuildArtifacts(CompilationPass2TaskInternal wrapper)
        {
            if (!this.supportExtensions)
            {
                IList<string> applicationMarkup = new List<string>(this.ApplicationMarkup.Length);
                foreach (ITaskItem taskItem in this.ApplicationMarkup)
                {
                    applicationMarkup.Add(taskItem.ItemSpec);
                }
                wrapper.ApplicationMarkup = applicationMarkup;
            }

            wrapper.SupportExtensions = this.supportExtensions;

            wrapper.BuildLogger = this.Log;

            wrapper.References = this.References
                .Select(i => new DelegatingTaskItem(i) as ITaskItem).ToList();

            wrapper.LocalAssemblyReference = this.LocalAssemblyReference;

            wrapper.AssemblyName = this.AssemblyName;

            wrapper.RootNamespace = this.RootNamespace;

            wrapper.Language = this.Language;

            wrapper.OutputPath = this.OutputPath;

            wrapper.IsInProcessXamlMarkupCompile = this.IsInProcessXamlMarkupCompile;

            wrapper.MSBuildProjectDirectory = this.MSBuildProjectDirectory;

            IList<string> sourceCodeFiles = null;
            if (this.SourceCodeFiles != null)
            {
                sourceCodeFiles = new List<string>(this.SourceCodeFiles.Length);
                foreach (ITaskItem taskItem in this.SourceCodeFiles)
                {
                    sourceCodeFiles.Add(taskItem.ItemSpec);
                }
            }
            wrapper.SourceCodeFiles = sourceCodeFiles;

            if (this.supportExtensions)
            {
                wrapper.XamlBuildTaskTypeInspectionExtensionNames = XamlBuildTaskServices.GetXamlBuildTaskExtensionNames(this.XamlBuildTypeInspectionExtensionNames);

                // Here we create a Dictionary of Type Full Name and corresponding TaskItem
                // This is passed to the extensions which enables them to look up 
                // metadata about a type like file name.
                IDictionary<string, ITaskItem> applicationMarkupWithTypeName = null;
                if (this.ApplicationMarkupWithTypeName != null)
                {
                    applicationMarkupWithTypeName = new Dictionary<string, ITaskItem>();
                }
                foreach (ITaskItem taskItem in this.ApplicationMarkupWithTypeName)
                {
                    string typeName = taskItem.GetMetadata("typeName");
                    if (!String.IsNullOrWhiteSpace(typeName))
                    {
                        applicationMarkupWithTypeName.Add(typeName, new DelegatingTaskItem(taskItem));
                    }
                }
                wrapper.ApplicationMarkupWithTypeName = applicationMarkupWithTypeName;
            }
        }

        void ExtractBuiltArtifacts(CompilationPass2TaskInternal wrapper)
        {
            if (wrapper.GeneratedCodeFiles == null)
            {
                return;
            }
            foreach (string code in wrapper.GeneratedCodeFiles)
            {
                this.generatedCodeFiles.Add(new TaskItem(code));
            }
        }
    }
}
