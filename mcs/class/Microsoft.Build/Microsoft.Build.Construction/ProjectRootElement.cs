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

namespace Microsoft.Build.Construction
{
        public class ProjectRootElement : ProjectElementContainer
        {
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
                        get { return new CollectionFromEnumerable<ProjectPropertyElement> (AllChildren.
                                Where (p => p as ProjectPropertyElement != null).
                                Select (p => (ProjectPropertyElement)p)); }
                }

                public ICollection<ProjectChooseElement> ChooseElements {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public Encoding Encoding {
                        get { return Encoding.UTF8; }
                }

                public bool HasUnsavedChanges {
                        get { return true; }
                }

                public ICollection<ProjectImportGroupElement> ImportGroups {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public ICollection<ProjectImportGroupElement> ImportGroupsReversed {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public ICollection<ProjectImportElement> Imports {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public string InitialTargets { get; set; }

                public ICollection<ProjectItemDefinitionGroupElement> ItemDefinitionGroups {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public ICollection<ProjectItemDefinitionGroupElement> ItemDefinitionGroupsReversed {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public ICollection<ProjectItemDefinitionElement> ItemDefinitions {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public ICollection<ProjectItemGroupElement> ItemGroups {
                        get { return new CollectionFromEnumerable<ProjectItemGroupElement> (Children.
                                Where (p => p as ProjectItemGroupElement != null).
                                Select (p => (ProjectItemGroupElement)p)); }
                }

                public ICollection<ProjectItemGroupElement> ItemGroupsReversed {
                        get { return new CollectionFromEnumerable<ProjectItemGroupElement> (ChildrenReversed.
                                Where (p => p as ProjectItemGroupElement != null).
                                Select (p => (ProjectItemGroupElement)p)); }
                }

                public ICollection<ProjectItemElement> Items {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                public DateTime LastWriteTimeWhenRead {
                        get { return DateTime.MinValue; }
                }

                public ICollection<ProjectPropertyGroupElement> PropertyGroups {
                        get { return new CollectionFromEnumerable<ProjectPropertyGroupElement> (Children.
                                Where (p => p as ProjectPropertyGroupElement != null).
                                Select (p => (ProjectPropertyGroupElement)p)); }
                }

                public ICollection<ProjectPropertyGroupElement> PropertyGroupsReversed {
                        get { return new CollectionFromEnumerable<ProjectPropertyGroupElement> (ChildrenReversed.
                                Where (p => p as ProjectPropertyGroupElement != null).
                                Select (p => (ProjectPropertyGroupElement)p)); }
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
                        get {
                                throw new NotImplementedException ();
                        }
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
                        get {
                                throw new NotImplementedException ();
                        }
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
                        return Create (XmlReader.Create (path));
                }

                public static ProjectRootElement Create (XmlReader xmlReader)
                {
                        throw new NotImplementedException ();
                }

                public static ProjectRootElement Create (string path, ProjectCollection projectCollection)
                {
                        throw new NotImplementedException ();
                }

                public static ProjectRootElement Create (XmlReader xmlReader, ProjectCollection projectCollection)
                {
                        throw new NotImplementedException ();
                }

                public ProjectImportElement AddImport (string project)
                {
                        var import = CreateImportElement (project);
                        AppendChild (import);
                        return import;
                }

                public ProjectImportGroupElement AddImportGroup ()
                {
                        throw new NotImplementedException ();
                }

                public ProjectItemElement AddItem (string itemType, string include)
                {
                        return AddItem (itemType, include, null);
                }

                public ProjectItemElement AddItem (string itemType, string include,
                                                   IEnumerable<KeyValuePair<string, string>> metadata)
                {
                        throw new NotImplementedException ();
                }

                public ProjectItemDefinitionElement AddItemDefinition (string itemType)
                {
                        throw new NotImplementedException ();
                }

                public ProjectItemDefinitionGroupElement AddItemDefinitionGroup ()
                {
                        throw new NotImplementedException ();
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
                        throw new NotImplementedException ();
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
                        throw new NotImplementedException ();
                }

                public ProjectUsingTaskElement AddUsingTask (string name, string assemblyFile, string assemblyName)
                {
                        throw new NotImplementedException ();
                }

                public ProjectChooseElement CreateChooseElement ()
                {
                        throw new NotImplementedException ();
                }

                public ProjectImportElement CreateImportElement (string project)
                {
                        return new ProjectImportElement (project, this);
                }

                public ProjectImportGroupElement CreateImportGroupElement ()
                {
                        throw new NotImplementedException ();
                }

                public ProjectItemDefinitionElement CreateItemDefinitionElement (string itemType)
                {
                        throw new NotImplementedException ();
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
                        throw new NotImplementedException ();
                }

                public ProjectOtherwiseElement CreateOtherwiseElement ()
                {
                        throw new NotImplementedException ();
                }

                public ProjectOutputElement CreateOutputElement (string taskParameter, string itemType,
                                                                 string propertyName)
                {
                        throw new NotImplementedException ();
                }

                public ProjectExtensionsElement CreateProjectExtensionsElement ()
                {
                        throw new NotImplementedException ();
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
                        throw new NotImplementedException ();
                }

                public ProjectTaskElement CreateTaskElement (string name)
                {
                        throw new NotImplementedException ();
                }

                public ProjectUsingTaskBodyElement CreateUsingTaskBodyElement (string evaluate, string body)
                {
                        throw new NotImplementedException ();
                }

                public ProjectUsingTaskElement CreateUsingTaskElement (string taskName, string assemblyFile,
                                                                       string assemblyName)
                {
                        throw new NotImplementedException ();
                }

                public ProjectUsingTaskParameterElement CreateUsingTaskParameterElement (string name, string output,
                                                                                         string required,
                                                                                         string parameterType)
                {
                        throw new NotImplementedException ();
                }

                public UsingTaskParameterGroupElement CreateUsingTaskParameterGroupElement ()
                {
                        throw new NotImplementedException ();
                }

                public ProjectWhenElement CreateWhenElement (string condition)
                {
                        throw new NotImplementedException ();
                }

                public static ProjectRootElement Open (string path)
                {
                        throw new NotImplementedException ();
                }

                public static ProjectRootElement Open (string path, ProjectCollection projectCollection)
                {
                        throw new NotImplementedException ();
                }

                public void Save ()
                {
                        Save (Encoding);
                }

                public void Save (Encoding saveEncoding)
                {
                        using (var writer = new StreamWriter (File.OpenWrite (FullPath), saveEncoding)) {
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
                        throw new NotImplementedException ();
                }

                public static ProjectRootElement TryOpen (string path, ProjectCollection projectCollection)
                {
                        throw new NotImplementedException ();
                }

                internal override void Save (XmlWriter writer)
                {
                        writer.WriteStartElement (XmlName, "http://schemas.microsoft.com/developer/msbuild/2003");
                        if (!string.IsNullOrWhiteSpace (ToolsVersion))
                                writer.WriteAttributeString ("ToolsVersion", ToolsVersion);
                        if (!string.IsNullOrWhiteSpace (DefaultTargets))
                                writer.WriteAttributeString ("DefaultTargets", DefaultTargets);
                        foreach (var child in Children)
                                child.Save (writer);
                        writer.WriteEndElement ();
                }

                internal override string XmlName {
                        get { return "Project"; }
                }
        }
}
