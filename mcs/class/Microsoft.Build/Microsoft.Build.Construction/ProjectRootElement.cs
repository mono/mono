//
// ProjectRootElement.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//
// (C) 2011 Leszek Ciesielski
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Internal;
using System.Text;
using System;
using System.Xml;
using System.IO;
using System.Linq;
using System.Globalization;
using Microsoft.Build.Exceptions;

namespace Microsoft.Build.Construction
{
        [System.Diagnostics.DebuggerDisplayAttribute("{FullPath} #Children={Count} DefaultTargets={DefaultTargets} "
                                                     + "ToolsVersion={ToolsVersion} InitialTargets={InitialTargets}")]
        public class ProjectRootElement : ProjectElementContainer
        {
                public override string Condition { get { return null; } set { throw new InvalidOperationException (
                        "Can not set Condition."); } }
                public string DefaultTargets { get; set; }

                string fullPath;
                public string FullPath {
                        get { return fullPath; }
                        set {
                                fullPath = Path.GetFullPath (value);
                                DirectoryPath = Path.GetDirectoryName (fullPath);
                        }
                }

                string directoryPath;
                public string DirectoryPath {
                        get { return directoryPath ?? String.Empty; }
                        internal set { directoryPath = value; }
                }

                public ICollection<ProjectPropertyElement> Properties {
                        get { return new CollectionFromEnumerable<ProjectPropertyElement> (
                                new FilteredEnumerable<ProjectPropertyElement> (AllChildren)); }
                }

                public ICollection<ProjectChooseElement> ChooseElements {
                        get { return new CollectionFromEnumerable<ProjectChooseElement> (
                                new FilteredEnumerable<ProjectChooseElement> (Children)); }
                }

                public Encoding Encoding {
                        get { return Encoding.UTF8; }
                }

                public bool HasUnsavedChanges {
                        get { return true; }
                }

                public ICollection<ProjectImportGroupElement> ImportGroups {
                        get { return new CollectionFromEnumerable<ProjectImportGroupElement> (
                                new FilteredEnumerable<ProjectImportGroupElement> (Children)); }
                }

                public ICollection<ProjectImportGroupElement> ImportGroupsReversed {
                        get { return new CollectionFromEnumerable<ProjectImportGroupElement> (
                                new FilteredEnumerable<ProjectImportGroupElement> (ChildrenReversed)); }
                }

                public ICollection<ProjectImportElement> Imports {
                        get { return new CollectionFromEnumerable<ProjectImportElement> (
                                new FilteredEnumerable<ProjectImportElement> (AllChildren)); }
                }

                public string InitialTargets { get; set; }

                public ICollection<ProjectItemDefinitionGroupElement> ItemDefinitionGroups {
                        get { return new CollectionFromEnumerable<ProjectItemDefinitionGroupElement> (
                                new FilteredEnumerable<ProjectItemDefinitionGroupElement> (Children)); }
                }

                public ICollection<ProjectItemDefinitionGroupElement> ItemDefinitionGroupsReversed {
                        get { return new CollectionFromEnumerable<ProjectItemDefinitionGroupElement> (
                                new FilteredEnumerable<ProjectItemDefinitionGroupElement> (ChildrenReversed)); }
                }

                public ICollection<ProjectItemDefinitionElement> ItemDefinitions {
                        get { return new CollectionFromEnumerable<ProjectItemDefinitionElement> (
                                new FilteredEnumerable<ProjectItemDefinitionElement> (AllChildren)); }
                }

                public ICollection<ProjectItemGroupElement> ItemGroups {
                        get { return new CollectionFromEnumerable<ProjectItemGroupElement> (
                                new FilteredEnumerable<ProjectItemGroupElement> (Children)); }
                }

                public ICollection<ProjectItemGroupElement> ItemGroupsReversed {
                        get { return new CollectionFromEnumerable<ProjectItemGroupElement> (
                                new FilteredEnumerable<ProjectItemGroupElement> (ChildrenReversed)); }
                }

                public ICollection<ProjectItemElement> Items {
                        get { return new CollectionFromEnumerable<ProjectItemElement> (
                                new FilteredEnumerable<ProjectItemElement> (AllChildren)); }
                }

                public DateTime LastWriteTimeWhenRead {
                        get { return DateTime.MinValue; }
                }

                public ICollection<ProjectPropertyGroupElement> PropertyGroups {
                        get { return new CollectionFromEnumerable<ProjectPropertyGroupElement> (
                                new FilteredEnumerable<ProjectPropertyGroupElement> (Children)); }
                }

                public ICollection<ProjectPropertyGroupElement> PropertyGroupsReversed {
                        get { return new CollectionFromEnumerable<ProjectPropertyGroupElement> (
                                new FilteredEnumerable<ProjectPropertyGroupElement> (ChildrenReversed)); }
                }

                public string RawXml {
                        get {
                                using (var writer = new StringWriter (CultureInfo.InvariantCulture)) {
                                        Save (writer);
                                        return writer.ToString ();
                                }
                        }
                }

                public ICollection<ProjectTargetElement> Targets {
                        get { return new CollectionFromEnumerable<ProjectTargetElement> (
                                new FilteredEnumerable<ProjectTargetElement> (Children)); }
                }

                public DateTime TimeLastChanged {
                        get { return DateTime.Now; }
                }

                string toolsVersion = "4.0";
                public string ToolsVersion {
                        get { return toolsVersion; }
                        set { toolsVersion = value; }
                }

                public ICollection<ProjectUsingTaskElement> UsingTasks {
                        get { return new CollectionFromEnumerable<ProjectUsingTaskElement> (
                                new FilteredEnumerable<ProjectUsingTaskElement> (Children)); }
                }

                public int Version {
                        get { return 0; }
                }

                ProjectRootElement (ProjectCollection projectCollection)
                {
                }

                public static ProjectRootElement Create ()
                {
                        return Create (ProjectCollection.GlobalProjectCollection);
                }

                public static ProjectRootElement Create (ProjectCollection projectCollection)
                {
                        return new ProjectRootElement (projectCollection);
                }

                public static ProjectRootElement Create (string path)
                {
                        return Create (path, ProjectCollection.GlobalProjectCollection);
                }

                public static ProjectRootElement Create (XmlReader xmlReader)
                {
                        return Create (xmlReader, ProjectCollection.GlobalProjectCollection);
                }

                public static ProjectRootElement Create (string path, ProjectCollection projectCollection)
                {
                        var result = Create (projectCollection);
                        result.FullPath = path;
                        return result;
                }

                public static ProjectRootElement Create (XmlReader xmlReader, ProjectCollection projectCollection)
                {
                        // yes, this should create en empty project
                        var result = Create (projectCollection);
                        return result;
                }

                public ProjectImportElement AddImport (string project)
                {
                        var import = CreateImportElement (project);
                        AppendChild (import);
                        return import;
                }

                public ProjectImportGroupElement AddImportGroup ()
                {
                        var importGroup = CreateImportGroupElement ();
                        AppendChild (importGroup);
                        return importGroup;
                }

                public ProjectItemElement AddItem (string itemType, string include)
                {
                        return AddItem (itemType, include, null);
                }

                public ProjectItemElement AddItem (string itemType, string include,
                                                   IEnumerable<KeyValuePair<string, string>> metadata)
                {
                        var @group = ItemGroups.
                                Where (p => string.IsNullOrEmpty (p.Condition)
                                       && p.Items.Where (s => s.ItemType.Equals (itemType,
                                                StringComparison.OrdinalIgnoreCase)).FirstOrDefault () != null).
                                        FirstOrDefault ();
                        if (@group == null)
                                @group = AddItemGroup ();
                        return @group.AddItem (itemType, include, metadata);
                }

                public ProjectItemDefinitionElement AddItemDefinition (string itemType)
                {
                        var @group = ItemDefinitionGroups.
                                Where (p => string.IsNullOrEmpty (p.Condition)
                                       && p.ItemDefinitions.Where (s => s.ItemType.Equals (itemType,
                                                StringComparison.OrdinalIgnoreCase)).FirstOrDefault () != null).
                                        FirstOrDefault ();
                        if (@group == null)
                                @group = AddItemDefinitionGroup ();
                        return @group.AddItemDefinition (itemType);
                }

                public ProjectItemDefinitionGroupElement AddItemDefinitionGroup ()
                {
                        var @group = CreateItemDefinitionGroupElement ();
                        ProjectElementContainer last = ItemDefinitionGroupsReversed.FirstOrDefault ();
                        if (last == null)
                                last = PropertyGroupsReversed.FirstOrDefault ();
                        InsertAfterChild (@group, last);
                        return @group;
                }

                public ProjectItemGroupElement AddItemGroup ()
                {
                        var @group = CreateItemGroupElement ();
                        ProjectElementContainer last = ItemGroupsReversed.FirstOrDefault ();
                        if (last == null)
                                last = PropertyGroupsReversed.FirstOrDefault ();
                        InsertAfterChild (@group, last);
                        return @group;
                }

                public ProjectPropertyElement AddProperty (string name, string value)
                {
                        ProjectPropertyGroupElement parentGroup = null;
                        foreach (var @group in PropertyGroups) {
                                if (string.IsNullOrEmpty (@group.Condition)) {
                                        if (parentGroup == null)
                                                parentGroup = @group;
                                        var property = @group.Properties.
                                                Where (p => string.IsNullOrEmpty (p.Condition)
                                                       && p.Name.Equals (name, StringComparison.OrdinalIgnoreCase)).
                                                        FirstOrDefault ();
                                        if (property != null) {
                                                property.Value = value;
                                                return property;
                                        }
                                }
                        }
                        if (parentGroup == null)
                                parentGroup = AddPropertyGroup ();
                        return parentGroup.AddProperty (name, value);
                }

                public ProjectPropertyGroupElement AddPropertyGroup ()
                {
                        var @group = CreatePropertyGroupElement ();
                        var last = PropertyGroupsReversed.FirstOrDefault ();
                        InsertAfterChild (@group, last);
                        return @group;
                }

                public ProjectTargetElement AddTarget (string name)
                {
                        var target = CreateTargetElement (name);
                        AppendChild (target);
                        return target;
                }

                public ProjectUsingTaskElement AddUsingTask (string name, string assemblyFile, string assemblyName)
                {
                        var usingTask = CreateUsingTaskElement (name, assemblyFile, assemblyName);
                        AppendChild (usingTask);
                        return usingTask;
                }

                public ProjectChooseElement CreateChooseElement ()
                {
                        return new ProjectChooseElement (this);
                }

                public ProjectImportElement CreateImportElement (string project)
                {
                        return new ProjectImportElement (project, this);
                }

                public ProjectImportGroupElement CreateImportGroupElement ()
                {
                        return new ProjectImportGroupElement (this);
                }

                public ProjectItemDefinitionElement CreateItemDefinitionElement (string itemType)
                {
                        return new ProjectItemDefinitionElement (itemType, this);
                }

                public ProjectItemDefinitionGroupElement CreateItemDefinitionGroupElement ()
                {
                        return new ProjectItemDefinitionGroupElement (this);
                }

                public ProjectItemElement CreateItemElement (string itemType)
                {
                        return new ProjectItemElement (itemType, this);
                }

                public ProjectItemElement CreateItemElement (string itemType, string include)
                {
                        var item = CreateItemElement (itemType);
                        item.Include = include;
                        return item;
                }

                public ProjectItemGroupElement CreateItemGroupElement ()
                {
                        return new ProjectItemGroupElement (this);
                }

                public ProjectMetadataElement CreateMetadataElement (string name)
                {
                        return new ProjectMetadataElement (name, this);
                }

                public ProjectMetadataElement CreateMetadataElement (string name, string unevaluatedValue)
                {
                        var metadata = CreateMetadataElement (name);
                        metadata.Value = unevaluatedValue;
                        return metadata;
                }

                public ProjectOnErrorElement CreateOnErrorElement (string executeTargets)
                {
                        return new ProjectOnErrorElement (executeTargets, this);
                }

                public ProjectOtherwiseElement CreateOtherwiseElement ()
                {
                        return new ProjectOtherwiseElement (this);
                }

                public ProjectOutputElement CreateOutputElement (string taskParameter, string itemType,
                                                                 string propertyName)
                {
                        return new ProjectOutputElement (taskParameter, itemType, propertyName, this);
                }

                public ProjectExtensionsElement CreateProjectExtensionsElement ()
                {
                        return new ProjectExtensionsElement (this);
                }

                public ProjectPropertyElement CreatePropertyElement (string name)
                {
                        return new ProjectPropertyElement (name, this);
                }

                public ProjectPropertyGroupElement CreatePropertyGroupElement ()
                {
                        return new ProjectPropertyGroupElement (this);
                }

                public ProjectTargetElement CreateTargetElement (string name)
                {
                        return new ProjectTargetElement (name, this);
                }

                public ProjectTaskElement CreateTaskElement (string name)
                {
                        return new ProjectTaskElement (name, this);
                }

                public ProjectUsingTaskBodyElement CreateUsingTaskBodyElement (string evaluate, string body)
                {
                        return new ProjectUsingTaskBodyElement (evaluate, body, this);
                }

                public ProjectUsingTaskElement CreateUsingTaskElement (string taskName, string assemblyFile,
                                                                       string assemblyName)
                {
                        return new ProjectUsingTaskElement (taskName, assemblyFile, assemblyName, this);
                }

                public ProjectUsingTaskParameterElement CreateUsingTaskParameterElement (string name, string output,
                                                                                         string required,
                                                                                         string parameterType)
                {
                        return new ProjectUsingTaskParameterElement (name, output, required, parameterType, this);
                }

                public UsingTaskParameterGroupElement CreateUsingTaskParameterGroupElement ()
                {
                        return new UsingTaskParameterGroupElement (this);
                }

                public ProjectWhenElement CreateWhenElement (string condition)
                {
                        return new ProjectWhenElement (condition, this);
                }

                public static ProjectRootElement Open (string path)
                {
                        return Open (path, ProjectCollection.GlobalProjectCollection);
                }

                public static ProjectRootElement Open (string path, ProjectCollection projectCollection)
                {
                        var result = Create (path, projectCollection);
                        using (var reader = XmlReader.Create (path))
                                result.Load (reader);
                        return result;
                }

                public void Save ()
                {
                        Save (Encoding);
                }

                public void Save (Encoding saveEncoding)
                {
                        using (var writer = new StreamWriter (File.Create (FullPath), saveEncoding)) {
                                Save (writer);
                        }
                }

                public void Save (string path)
                {
                        Save (path, Encoding);
                }

                public void Save (TextWriter writer)
                {
                        using (var xmlWriter = XmlWriter.Create (writer, new XmlWriterSettings { Indent = true,
                                NewLineChars = "\r\n" })) {
                                Save (xmlWriter);
                        }
                }

                public void Save (string path, Encoding encoding)
                {
                        FullPath = path;
                        Save (encoding);
                }

                public static ProjectRootElement TryOpen (string path)
                {
                        return TryOpen (path, ProjectCollection.GlobalProjectCollection);
                }

                public static ProjectRootElement TryOpen (string path, ProjectCollection projectCollection)
                {
                        // this should be non-null only if the project is already cached
                        // and caching is not yet implemented
                        return null;
                }

                internal override void Load (XmlReader reader)
                {
                        try {
                                base.Load (reader);
                        } catch (XmlException ex) {
                                throw new InvalidProjectFileException (FullPath, ex.LineNumber, ex.LinePosition, 0, 0,
                                        ex.Message, null, null, null);
                        }
                }

                internal override ProjectElement LoadChildElement (string name)
                {
                        switch (name) {
                        case "PropertyGroup":
                                var prop = CreatePropertyGroupElement ();
                                AppendChild (prop);
                                return prop;
                        case "ItemGroup":
                                var item = CreateItemGroupElement ();
                                AppendChild (item);
                                return item;
                        case "Import":
                                return AddImport (null);
                        case "Target":
                                return AddTarget (null);
                        case "ItemDefinitionGroup":
                                var def = CreateItemDefinitionGroupElement ();
                                AppendChild (def);
                                return def;
                        case "UsingTask":
                                return AddUsingTask (null, null, null);
                        case "Choose":
                                var choose = CreateChooseElement ();
                                AppendChild (choose);
                                return choose;
                        case "ProjectExtensions":
                                var ext = CreateProjectExtensionsElement ();
                                AppendChild (ext);
                                return ext;
                        default:
                                throw new InvalidProjectFileException (string.Format (
                                        "Child \"{0}\" is not a known node type.", name));
                        }
                }

                internal override void LoadAttribute (string name, string value)
                {
                        switch (name) {
                        case "ToolsVersion":
                                ToolsVersion = value;
                                break;
                        case "DefaultTargets":
                                DefaultTargets = value;
                                break;
                        case "InitialTargets":
                                InitialTargets = value;
                                break;
                        default:
                                base.LoadAttribute (name, value);
                                break;
                        }
                }

                internal override void Save (XmlWriter writer)
                {
                        writer.WriteStartElement (XmlName, "http://schemas.microsoft.com/developer/msbuild/2003");
                        SaveValue (writer);
                        writer.WriteEndElement ();
                }

                internal override void SaveValue (XmlWriter writer)
                {
                        SaveAttribute (writer, "ToolsVersion", ToolsVersion);
                        SaveAttribute (writer, "DefaultTargets", DefaultTargets);
                        SaveAttribute (writer, "InitialTargets", InitialTargets);
                        base.SaveValue (writer);
                }

                internal override string XmlName {
                        get { return "Project"; }
                }
        }
}
