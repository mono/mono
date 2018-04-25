//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System.Reflection;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using Microsoft.Build.BuildEngine;

    [Fx.Tag.XamlVisible(true)]
    public class GenerateTemporaryAssemblyTask : Task
    {
        const string MSBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        // We will do Dev10 behavior if the new required property GeneratedResourceFiles is NOT specified. This can happen
        // if a Dev10 version of the Microsoft.Xaml.Targets file is being used with Dev11 installed.
        bool supportExtensions = false;
        ITaskItem[] generatedResourceFiles;

        public GenerateTemporaryAssemblyTask()
        {
        }

        [Required]
        public string AssemblyName
        { get; set; }

        [Required]
        public string OutputPath
        { get; set; }

        [Required]
        public string CurrentProject
        { get; set; }

        [Fx.Tag.KnownXamlExternal]
        [Required]
        public ITaskItem[] SourceCodeFiles { get; set; }

        [Required]
        public string CompileTargetName
        { get; set; }

        // Required in Dev11, but for backward compatibility with a Dev10 targets file, not marking as required.
        public ITaskItem[] GeneratedResourcesFiles 
        {
            get
            {
                return this.generatedResourceFiles;
            }

            set
            {
                this.generatedResourceFiles = value;
                this.supportExtensions = true;
            }
        }

        [Fx.Tag.KnownXamlExternal]
        public ITaskItem[] ReferencePaths
        { get; set; }

        [Required]
        public string ApplicationMarkupTypeName
        { get; set; }

        public override bool Execute()
        {
            bool retVal;
            try
            {
                XDocument projectDocument = XDocument.Load(this.CurrentProject);
                if (projectDocument != null)
                {
                    XElement projectElement = projectDocument.Element(XName.Get("Project", MSBuildNamespace));
                    if (projectElement != null)
                    {
                        RemoveItemsByName(projectElement, this.ApplicationMarkupTypeName);
                        RemoveItemsByName(projectElement, "Reference");
                        RemoveItemsByName(projectElement, "ProjectReference");
                        if (this.supportExtensions)
                        {
                            AddNewResourceItems(projectElement, this.GeneratedResourcesFiles);
                        }
                        AddNewItems(projectElement, "Compile", this.SourceCodeFiles);
                        AddNewItems(projectElement, "ReferencePath", this.ReferencePaths);

                        RemovePropertyByName(projectElement, "OutputType");
                        RemovePropertyByName(projectElement, "AssemblyName");
                        AddNewProperties(projectElement,
                            new ProjectProperty[] {
                                new ProjectProperty() { Name = "OutputType", Value = "Library" },
                                new ProjectProperty() { Name = "AssemblyName", Value = this.AssemblyName },
                                new ProjectProperty() { Name = "Utf8Output", Value = "true", Condition = "'$(Utf8Output)' == ''" }
                            });
                    }
                }

                string randomName = Path.GetRandomFileName();
                randomName = Path.ChangeExtension(randomName, "");
                string filename = Path.ChangeExtension(randomName, ".tmp_proj");
                projectDocument.Save(filename);
                Hashtable globalProperties = new Hashtable();
                globalProperties["IntermediateOutputPath"] = this.OutputPath;
                globalProperties["AssemblyName"] = this.AssemblyName;
                globalProperties["OutputType"] = "Library";
                retVal = base.BuildEngine.BuildProjectFile(filename, new string[] { this.CompileTargetName }, globalProperties, null);
                File.Delete(filename);
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
                XamlBuildTaskServices.LogException(this.Log, e.Message);
                retVal = false;
            }
            return retVal;
        }

        void RemoveItemsByName(XElement project, string itemName)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                IEnumerable<XElement> itemGroups = project.Elements(XName.Get("ItemGroup", MSBuildNamespace));
                itemGroups.Elements(XName.Get(itemName, MSBuildNamespace)).Remove();
            }
        }

        void AddNewItems(XElement project, string itemName, ITaskItem[] items)
        {
            if (!string.IsNullOrEmpty(itemName) && items != null)
            {
                XElement newItemGroup = new XElement(XName.Get("ItemGroup", MSBuildNamespace));
                project.Add(newItemGroup);
                foreach (ITaskItem item in items)
                {
                    XElement newElement = new XElement(XName.Get(itemName, MSBuildNamespace));
                    XAttribute include = new XAttribute("Include", item.ItemSpec);
                    newElement.Add(include);
                    newItemGroup.Add(newElement);
                }
            }
        }

        //<ItemGroup>
        //    <EmbeddedResource Include="@(XamlGeneratedResources)">
        //        <Type>Non-Resx</Type>
        //        <WithCulture>false</WithCulture>
        //    </EmbeddedResource>
        //</ItemGroup>
        void AddNewResourceItems(XElement project, ITaskItem[] items)
        {
            if (items != null && items.Length > 0)
            {
                XElement newItemGroup = new XElement(XName.Get("ItemGroup", MSBuildNamespace));
                project.Add(newItemGroup);
                foreach (ITaskItem item in items)
                {
                    XElement newResource = new XElement(XName.Get("EmbeddedResource", MSBuildNamespace));
                    newResource.Add(new XAttribute("Include", item.ItemSpec));

                    XElement type = new XElement(XName.Get("Type", MSBuildNamespace), "Non-Resx");
                    newResource.Add(type);

                    XElement withCulture = new XElement(XName.Get("WithCulture", MSBuildNamespace), "false");
                    newResource.Add(withCulture);

                    newItemGroup.Add(newResource);
                }
            }
        }
        
        void RemovePropertyByName(XElement project, string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                IEnumerable<XElement> itemGroups = project.Elements(XName.Get("PropertyGroup", MSBuildNamespace));
                itemGroups.Elements(XName.Get(propertyName, MSBuildNamespace)).Remove();
            }
        }

        void AddNewProperties(XElement project, IEnumerable<ProjectProperty> properties)
        {
            if (properties != null)
            {
                XElement newPropertyGroup = new XElement(XName.Get("PropertyGroup", MSBuildNamespace));
                project.Add(newPropertyGroup);
                foreach (ProjectProperty prop in properties)
                {
                    if (!string.IsNullOrEmpty(prop.Name) && prop.Value != null)
                    {
                        XElement newElement = new XElement(XName.Get(prop.Name, MSBuildNamespace));
                        newElement.Value = prop.Value;
                        if (!string.IsNullOrEmpty(prop.Condition))
                        {
                            newElement.SetAttributeValue(XName.Get("Condition", string.Empty), prop.Condition);
                        }
                        newPropertyGroup.Add(newElement);
                    }
                }
            }
        }

        class ProjectProperty
        {
            public string Name
            { get; set; }
            public string Value
            { get; set; }
            public string Condition
            { get; set; }
        }
    }
}
