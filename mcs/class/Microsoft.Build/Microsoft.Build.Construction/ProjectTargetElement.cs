//
// ProjectTargetElement.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Internal;

namespace Microsoft.Build.Construction
{
        [System.Diagnostics.DebuggerDisplayAttribute ("Name={Name} #Children={Count} Condition={Condition}")]
        public class ProjectTargetElement : ProjectElementContainer
        {
                internal ProjectTargetElement (string name, ProjectRootElement containingProject)
                        : this(containingProject)
                {
                        Name = name;
                }
                internal ProjectTargetElement (ProjectRootElement containingProject)
                {
                        ContainingProject = containingProject;
                }
                public string AfterTargets { get; set; }
                public string BeforeTargets { get; set; }
                public string DependsOnTargets { get; set; }
                public string Inputs { get; set; }
                public ICollection<ProjectItemGroupElement> ItemGroups {
                        get { return new CollectionFromEnumerable<ProjectItemGroupElement> (
                                new FilteredEnumerable<ProjectItemGroupElement> (Children)); }
                }
                public string KeepDuplicateOutputs { get; set; }
                public string Name { get; set; }
                public ICollection<ProjectOnErrorElement> OnErrors {
                        get { return new CollectionFromEnumerable<ProjectOnErrorElement> (
                                new FilteredEnumerable<ProjectOnErrorElement> (Children)); }
                }
                public string Outputs { get; set; }
                public ICollection<ProjectPropertyGroupElement> PropertyGroups {
                        get { return new CollectionFromEnumerable<ProjectPropertyGroupElement> (
                                new FilteredEnumerable<ProjectPropertyGroupElement> (Children)); }
                }
                public string Returns { get; set; }
                public ICollection<ProjectTaskElement> Tasks {
                        get { return new CollectionFromEnumerable<ProjectTaskElement> (
                                new FilteredEnumerable<ProjectTaskElement> (Children)); }
                }
                public ProjectItemGroupElement AddItemGroup ()
                {
                        var item = ContainingProject.CreateItemGroupElement ();
                        AppendChild (item);
                        return item;
                }
                public ProjectPropertyGroupElement AddPropertyGroup ()
                {
                        var property = ContainingProject.CreatePropertyGroupElement ();
                        AppendChild (property);
                        return property;
                }
                public ProjectTaskElement AddTask (string taskName)
                {
                        var task = ContainingProject.CreateTaskElement (taskName);
                        AppendChild (task);
                        return task;
                }
                internal override string XmlName {
                        get { return "Target"; }
                }

                internal override ProjectElement LoadChildElement (string name)
                {
                        switch (name) {
                        case "OnError":
                                var error = new ProjectOnErrorElement (ContainingProject);
                                AppendChild (error);
                                return error;
                        case "PropertyGroup":
                                return AddPropertyGroup ();
                        case "ItemGroup":
                                return AddItemGroup ();
                        default:
                                return AddTask (name);
                        }
                }
                internal override void LoadAttribute (string name, string value)
                {
                        switch (name) {
                        case "Name":
                                Name = value;
                                break;
                        case "DependsOnTargets":
                                DependsOnTargets = value;
                                break;
                        case "Returns":
                                Returns = value;
                                break;
                        case "Inputs":
                                Inputs = value;
                                break;
                        case "Outputs":
                                Outputs = value;
                                break;
                        case "BeforeTargets":
                                BeforeTargets = value;
                                break;
                        case "AfterTargets":
                                AfterTargets = value;
                                break;
                        case "KeepDuplicateOutputs":
                                KeepDuplicateOutputs = value;
                                break;
                        default:
                                base.LoadAttribute (name, value);
                                break;
                        }
                }
                internal override void SaveValue (System.Xml.XmlWriter writer)
                {
                        SaveAttribute (writer, "Name", Name);
                        SaveAttribute (writer, "DependsOnTargets", DependsOnTargets);
                        SaveAttribute (writer, "Returns", Returns);
                        SaveAttribute (writer, "Inputs", Inputs);
                        SaveAttribute (writer, "Outputs", Outputs);
                        SaveAttribute (writer, "BeforeTargets", BeforeTargets);
                        SaveAttribute (writer, "AfterTargets", AfterTargets);
                        SaveAttribute (writer, "KeepDuplicateOutputs", KeepDuplicateOutputs);
                        base.SaveValue (writer);
                }
        }
}
