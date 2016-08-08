//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Threading;
    using System.Xaml;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Microsoft.VisualStudio.Activities;

    [Fx.Tag.XamlVisible(true)]
    public class PartialClassGenerationTask : Task
    {
        const string DefaultGeneratedSourceExtension = "g";
        List<ITaskItem> generatedResources = new List<ITaskItem>();
        List<ITaskItem> generatedCodeFiles = new List<ITaskItem>();

        // We will do Dev10 behavior if the new required property MSBuildProjectDirectory is NOT specified. This can happen
        // if a Dev10 version of the Microsoft.Xaml.Targets file is being used with Dev11 installed.
        bool supportExtensions = false;
        string msBuildProjectDirectory;

        public PartialClassGenerationTask()
        {
            this.GeneratedSourceExtension = DefaultGeneratedSourceExtension;
        }

        [Fx.Tag.KnownXamlExternal]
        [Output]
        public ITaskItem[] ApplicationMarkup { get; set; }

        public string AssemblyName
        { get; set; }

        public string[] KnownReferencePaths { get; set; }

        [Fx.Tag.KnownXamlExternal]
        [Output]
        public ITaskItem[] GeneratedResources
        {
            get
            {
                return generatedResources.ToArray();
            }

            set
            {
                generatedResources = new List<ITaskItem>(value);
            }
        }
        
        [Fx.Tag.KnownXamlExternal]
        [Output]
        public ITaskItem[] GeneratedCodeFiles
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

        public string GeneratedSourceExtension
        { get; set; }

        [Required]
        public string Language
        { get; set; }

        [Required]
        public string OutputPath
        { get; set; }

        // This is Required for Dev11, but to allow things to work with a Dev10 targets file, we are not marking it required.
        public string MSBuildProjectDirectory
        {
            get
            {
                return this.msBuildProjectDirectory;
            }

            set
            {
                this.msBuildProjectDirectory = value;
                // The fact that this property is being set indicates that a Dev11 version of the targets
                // file is being used, so we should not do Dev10 behavior.
                this.supportExtensions = true;
            }
        }

        [Fx.Tag.KnownXamlExternal]
        public ITaskItem[] References
        { get; set; }

        public string RootNamespace
        { get; set; }

        [Fx.Tag.KnownXamlExternal]
        public ITaskItem[] SourceCodeFiles
        { get; set; }

        [Output]
        public bool RequiresCompilationPass2
        { get; set; }

        public string BuildTaskPath
        { get; set; }

        public bool IsInProcessXamlMarkupCompile
        { get; set; }

        public ITaskItem[] XamlBuildTypeGenerationExtensionNames
        { get; set; }

        public ITaskItem[] XamlBuildTypeInspectionExtensionNames
        { get; set; }

        private static AppDomain inProcessAppDomain;
        private static Dictionary<string, DateTime> referencesTimeStampCache;
        private Object referencesCacheLock = new Object();

        public override bool Execute()
        {
            VSDesignerPerfEventProvider perfEventProvider = new VSDesignerPerfEventProvider();
            perfEventProvider.WriteEvent(VSDesignerPerfEvents.XamlBuildTaskExecuteStart);

            try
            {
                if (IsInProcessXamlMarkupCompile)
                {
                    bool acquiredLock = false;
                    try
                    {
                        Monitor.TryEnter(referencesCacheLock, ref acquiredLock);
                        if (acquiredLock)
                        {
                            return ReuseAppDomainAndExecute();
                        }
                        else
                        {
                            return GetAppDomainAndExecute();
                        }
                    }
                    finally
                    {
                        if (acquiredLock)
                        {
                            Monitor.Exit(referencesCacheLock);
                        }
                    }
                }
                else
                {
                    return GetAppDomainAndExecute();
                }
            }
            finally
            {
                perfEventProvider.WriteEvent(VSDesignerPerfEvents.XamlBuildTaskExecuteEnd);
            }
        }
            
        bool ReuseAppDomainAndExecute()
        {
            AppDomain appDomain = null;
            bool createdNewAppDomain = false;
            try
            {
                try
                {
                    appDomain = GetInProcessAppDomain(out createdNewAppDomain);
                    bool ret = ExecuteInternal(appDomain);
                    return ret;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (createdNewAppDomain)
                    {
                        XamlBuildTaskServices.LogException(this.Log, e.Message);
                        return false;
                    }
                    else
                    {
                        if (inProcessAppDomain != null)
                        {
                            AppDomain.Unload(inProcessAppDomain);
                            inProcessAppDomain = null;
                        }
                        return GetAppDomainAndExecute();
                    }
                }
            }
            finally
            {
                if (Log != null)
                {
                    Log.MarkAsInactive();
                }
            }
        }

        bool GetAppDomainAndExecute()
        {
            AppDomain appDomain = null;
            try
            {
                appDomain = CreateNewAppDomain();
                bool ret = ExecuteInternal(appDomain);
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

        bool ExecuteInternal(AppDomain appDomain)
        {
            PartialClassGenerationTaskInternal wrapper = (PartialClassGenerationTaskInternal)appDomain.CreateInstanceAndUnwrap(
                                                        Assembly.GetExecutingAssembly().FullName,
                                                        typeof(PartialClassGenerationTaskInternal).FullName);

            PopulateBuildArtifacts(wrapper);

            bool ret = wrapper.Execute();

            if (ret)
            {
                ExtractBuiltArtifacts(wrapper);
            }
            return ret;
        }

        AppDomain CreateNewAppDomain()
        {
            return XamlBuildTaskServices.CreateAppDomain("PartialClassAppDomain_" + Guid.NewGuid(), BuildTaskPath);
        }
        
        // For Intellisense builds, we re-use the AppDomain for successive builds instead of creating a new one every time, 
        // if the references have not changed (there are no new references and they have not been updated since the last build)
        // This method accesses the static referencesTimeStampCache (indirectly). 
        // To ensure thread safety, this method should be called inside a lock/monitor
        AppDomain GetInProcessAppDomain(out bool newAppDomain)
        {
            newAppDomain = false;
            if (inProcessAppDomain == null)
            {
                inProcessAppDomain = CreateNewAppDomain();
                newAppDomain = true;
                UpdateReferenceCache();
            }
            else if (AreReferencesChanged())
            {
                AppDomain.Unload(inProcessAppDomain);
                inProcessAppDomain = CreateNewAppDomain();
                newAppDomain = true;
                UpdateReferenceCache();
            }
            return inProcessAppDomain;
        }

        // This method accesses the static referencesTimeStampCache.
        // To ensure thread safety, this method should be called inside a lock/monitor
        bool AreReferencesChanged()
        {
            bool refsChanged = false;
            if (referencesTimeStampCache == null || referencesTimeStampCache.Count != References.Length)
            {
                refsChanged = true;
            }
            else
            {
                foreach (var reference in References)
                {
                    string fullPath = Path.GetFullPath(reference.ItemSpec);
                    DateTime timeStamp = File.GetLastWriteTimeUtc(fullPath);
                    if (!referencesTimeStampCache.ContainsKey(fullPath)
                        || timeStamp > referencesTimeStampCache[fullPath]
                        || timeStamp == DateTime.MinValue)
                    {
                        refsChanged = true;
                        break;
                    }
                }
            }
            return refsChanged;
        }

        // This method accesses the static referencesTimeStampCache.
        // To ensure thread safety, this method should be called inside a lock/monitor
        void UpdateReferenceCache()
        {
            referencesTimeStampCache = new Dictionary<string, DateTime>();
            foreach (var reference in References)
            {
                string fullPath = Path.GetFullPath(reference.ItemSpec);
                DateTime timeStamp = File.GetLastWriteTimeUtc(fullPath);
                referencesTimeStampCache.Add(fullPath, timeStamp);
            }
        }

        void PopulateBuildArtifacts(PartialClassGenerationTaskInternal wrapper)
        {
            IList<ITaskItem> applicationMarkup = null;
            if (this.ApplicationMarkup != null)
            {
                applicationMarkup = this.ApplicationMarkup
                    .Select(i => new DelegatingTaskItem(i) as ITaskItem).ToList();
            }
            wrapper.ApplicationMarkup = applicationMarkup;

            wrapper.BuildLogger = this.Log;

            wrapper.References = this.References
                .Select(i => new DelegatingTaskItem(i) as ITaskItem).ToList();

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

            wrapper.Language = this.Language;
            wrapper.AssemblyName = this.AssemblyName;
            wrapper.OutputPath = this.OutputPath;
            wrapper.RootNamespace = this.RootNamespace;
            wrapper.GeneratedSourceExtension = this.GeneratedSourceExtension;
            wrapper.IsInProcessXamlMarkupCompile = this.IsInProcessXamlMarkupCompile;
            wrapper.MSBuildProjectDirectory = this.MSBuildProjectDirectory;
            wrapper.XamlBuildTaskTypeGenerationExtensionNames = XamlBuildTaskServices.GetXamlBuildTaskExtensionNames(this.XamlBuildTypeGenerationExtensionNames);
            if (this.XamlBuildTypeInspectionExtensionNames != null && this.XamlBuildTypeInspectionExtensionNames.Length > 0)
            {
                wrapper.MarkupCompilePass2ExtensionsPresent = true;
            }

            wrapper.SupportExtensions = this.supportExtensions;
        }        

        void ExtractBuiltArtifacts(PartialClassGenerationTaskInternal wrapper)
        {
            foreach (string resource in wrapper.GeneratedResources)
            {
                this.generatedResources.Add(new TaskItem(resource));
            }

            foreach (string code in wrapper.GeneratedCodeFiles)
            {
                this.generatedCodeFiles.Add(new TaskItem(code));
            }

            this.RequiresCompilationPass2 = wrapper.RequiresCompilationPass2 ||
                (this.XamlBuildTypeInspectionExtensionNames != null && this.XamlBuildTypeInspectionExtensionNames.Length > 0);
        }
    }
}
