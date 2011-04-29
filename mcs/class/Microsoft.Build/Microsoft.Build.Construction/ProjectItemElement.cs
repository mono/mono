//
// ProjectItemElementa.cs
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
using System.Linq;
using Microsoft.Build.Internal;
using System.Xml;

namespace Microsoft.Build.Construction
{
        [System.Diagnostics.DebuggerDisplayAttribute ("{ItemType} Include={Include} Exclude={Exclude} "
                                                      + "#Metadata={Count} Condition={Condition}")]
        public class ProjectItemElement : ProjectElementContainer
        {
                internal ProjectItemElement (string itemType, ProjectRootElement containingProject)
                {
                        ItemType = itemType;
                        ContainingProject = containingProject;
                }
                public string Exclude { get; set; }
                public bool HasMetadata {
                        get {
                                var metadata = Metadata.FirstOrDefault ();
                                return metadata != null;
                        }
                }
                public string Include { get; set; }
                public string ItemType { get; set; }
                public ICollection<ProjectMetadataElement> Metadata {
                        get { return new CollectionFromEnumerable<ProjectMetadataElement> (
                                new FilteredEnumerable<ProjectMetadataElement> (Children)); }
                }
                public string Remove { get; set; }
                public ProjectMetadataElement AddMetadata (string name, string unevaluatedValue)
                {
                        var metadata = ContainingProject.CreateMetadataElement (name, unevaluatedValue);
                        AppendChild (metadata);
                        return metadata;
                }
                internal override string XmlName {
                        get { return ItemType; }
                }
                internal override void SaveValue (XmlWriter writer)
                {
                        SaveAttribute (writer, "Include", Include);
                        SaveAttribute (writer, "Exclude", Exclude);
                        SaveAttribute (writer, "Remove", Remove);
                        base.SaveValue (writer);
                }
                internal override void LoadAttribute (string name, string value)
                {
                        switch (name) {
                        case "Include":
                                Include = value;
                                break;
                        case "Exclude":
                                Exclude = value;
                                break;
                        case "Remove":
                                Remove = value;
                                break;
                        default:
                                base.LoadAttribute (name, value);
                                break;
                        }
                }
                internal override ProjectElement LoadChildElement (string name)
                {
                        var metadata = ContainingProject.CreateMetadataElement (name);
                        AppendChild (metadata);
                        return metadata;
                }
        }
}
