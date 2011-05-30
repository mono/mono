//
// Project.cs
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
using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Internal;

namespace Microsoft.Build.Evaluation
{
        [DebuggerDisplay("{FullPath} EffectiveToolsVersion={ToolsVersion} #GlobalProperties="
                         +"{data.globalProperties.Count} #Properties={data.Properties.Count} #ItemTypes="
                         +"{data.ItemTypes.Count} #ItemDefinitions={data.ItemDefinitions.Count} #Items="
                         +"{data.Items.Count} #Targets={data.Targets.Count}")]
        public class Project
        {
                public Project (ProjectRootElement xml) : this(xml, null, null)
                {
                }
                public Project (ProjectRootElement xml, IDictionary<string, string> globalProperties,
                                string toolsVersion)
                        : this(xml, globalProperties, toolsVersion, ProjectCollection.GlobalProjectCollection)
                {
                }
                public Project (ProjectRootElement xml, IDictionary<string, string> globalProperties,
                                string toolsVersion, ProjectCollection projectCollection)
                        : this(xml, globalProperties, toolsVersion, projectCollection, ProjectLoadSettings.Default)
                {
                }

                public Project (ProjectRootElement xml, IDictionary<string, string> globalProperties,
                                string toolsVersion, ProjectCollection projectCollection,
                                ProjectLoadSettings loadSettings)
                {
                        ProjectCollection = projectCollection;
                        Xml = xml;
                        GlobalProperties = globalProperties;
                        ToolsVersion = toolsVersion;
                }

                public IDictionary<string, string> GlobalProperties { get; private set; }
                public ProjectCollection ProjectCollection { get; private set; }
                public string ToolsVersion { get; private set; }
                public ProjectRootElement Xml { get; private set; }

                public ICollection<ProjectItem> GetItemsIgnoringCondition (string itemType)
                {
                        return new CollectionFromEnumerable<ProjectItem> (
                                new FilteredEnumerable<ProjectItemElement> (Xml.Items).
                                Where (p => p.ItemType.Equals (itemType, StringComparison.OrdinalIgnoreCase)).
                                Select (p => new ProjectItem(p)));
                }
                public void RemoveItems (IEnumerable<ProjectItem> items)
                {
                        var removal = new List<ProjectItem> (items);
                        foreach (var item in removal) {
                                var parent = item.Xml.Parent;
                                parent.RemoveChild (item.Xml);
                                if (parent.Count == 0)
                                        parent.Parent.RemoveChild (parent);
                        }
                }
        }
}
