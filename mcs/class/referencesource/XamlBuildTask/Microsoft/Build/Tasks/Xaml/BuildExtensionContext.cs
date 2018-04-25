//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.Build.Utilities;

    public class BuildExtensionContext
    {
        static readonly IList<string> emptyList = new List<string>(0);
        List<string> references;
        List<string> sourceCodeFiles;
        List<string> generatedFiles;
        List<string> generatedResourceFiles;

        internal BuildExtensionContext()
        {
        }

        public string AssemblyName
        { 
            get; 
            internal set; 
        }

        public bool IsInProcessXamlMarkupCompile
        { 
            get; 
            internal set; 
        }

        public string Language
        { 
            get; 
            internal set; 
        }

        public string OutputPath
        { 
            get; 
            internal set; 
        }

        public ReadOnlyCollection<string> References
        {
            get
            {
                if (this.references == null)
                {
                    this.references = new List<string>();
                }
                return this.references.AsReadOnly();
            }
        }

        public string RootNamespace
        { 
            get; 
            internal set; 
        }

        public ReadOnlyCollection<string> SourceCodeFiles
        {
            get
            {
                if (this.sourceCodeFiles == null)
                {
                    this.sourceCodeFiles = new List<string>();
                }
                return this.sourceCodeFiles.AsReadOnly();
            }
        }

       

        public string LocalAssembly
        {
            get;
            internal set;
        }

        public TaskLoggingHelper XamlBuildLogger
        {
            get;
            internal set;
        }

        public ReadOnlyCollection<string> GeneratedFiles
        {
            get
            {
                if (this.generatedFiles == null)
                {
                    this.generatedFiles = new List<string>();
                }
                return this.generatedFiles.AsReadOnly();
            }
        }

        public ReadOnlyCollection<string> GeneratedResourceFiles
        {
            get
            {
                if (this.generatedResourceFiles == null)
                {
                    this.generatedResourceFiles = new List<string>();
                }
                return this.generatedResourceFiles.AsReadOnly();
            }
        }

        public void AddGeneratedFile(string fileName)
        {
            if (this.generatedFiles == null)
            {
                this.generatedFiles = new List<string>();
            }
            this.generatedFiles.Add(fileName);         
        }

        public void AddGeneratedResourceFile(string fileName)
        {
            if (this.generatedResourceFiles == null)
            {
                this.generatedResourceFiles = new List<string>();
            }
            this.generatedResourceFiles.Add(fileName);
        }

        internal void AddReferences(IList<string> references)
        {
            if (references != null)
            {
                if (this.references == null)
                {
                    this.references = new List<string>();
                }
                this.references.AddRange(references);
            }
        }

        internal void AddSourceCodeFiles(IList<string> sourceCodeFiles)
        {
            if (sourceCodeFiles != null)
            {
                if (this.sourceCodeFiles == null)
                {
                    this.sourceCodeFiles = new List<string>();
                }
                this.sourceCodeFiles.AddRange(sourceCodeFiles);
            }
        }
    }
}
