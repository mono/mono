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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Internal;
using System.Xml;
using Microsoft.Build.Exceptions;

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
                string exclude;
                public string Exclude { get { return exclude ?? String.Empty; } set { exclude = value; } }
                public bool HasMetadata {
                        get {
                                var metadata = Metadata.FirstOrDefault ();
                                return metadata != null;
                        }
                }
                string include;
                public string Include { get { return include ?? String.Empty; } set { include = value; } }
                string itemType;
                public string ItemType { get { return itemType ?? String.Empty; } set { itemType = value; } }
                public ICollection<ProjectMetadataElement> Metadata {
                        get { return new CollectionFromEnumerable<ProjectMetadataElement> (
                                new FilteredEnumerable<ProjectMetadataElement> (Children)); }
                }
                string @remove;
                public string Remove { get { return @remove ?? String.Empty; } set { @remove = value; } }
                string keepDuplicates;
		string keepMetadata;
		string removeMetadata;
                public
                string KeepDuplicates { get { return keepDuplicates ?? String.Empty; } set { keepDuplicates = value; } }
                public
		string KeepMetadata { get { return keepMetadata ?? String.Empty; } set { keepMetadata = value; } }
                public
		string RemoveMetadata { get { return removeMetadata ?? String.Empty; } set { removeMetadata = value; } }
                
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
                        SaveAttribute (writer, "KeepDuplicates", KeepDuplicates);
                        SaveAttribute (writer, "KeepMetadata", KeepMetadata);
                        SaveAttribute (writer, "RemoveMetadata", RemoveMetadata);
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
                        case "KeepDuplicates":
                                KeepDuplicates = value;
                                break;
                        case "KeepMetadata":
                                KeepMetadata = value;
                                break;
                        case "RemoveMetadata":
                                RemoveMetadata = value;
                                break;
                        case "Remove":
                                Remove = value;
                                break;
                        default:
                                base.LoadAttribute (name, value);
                                break;
                        }
                }
                internal override void LoadValue (XmlReader reader)
                {
                        if (string.IsNullOrWhiteSpace (Include) && string.IsNullOrEmpty (Remove))
                                throw new InvalidProjectFileException (Location, null, string.Format ("Both Include and Remove attribute are null or empty on '{0}' item", ItemType));
                        base.LoadValue (reader);
                }
                internal override ProjectElement LoadChildElement (XmlReader reader)
                {
                        var metadata = ContainingProject.CreateMetadataElement (reader.LocalName);
                        AppendChild (metadata);
                        return metadata;
                }
                 public ElementLocation ExcludeLocation { get; private set; }
                 public ElementLocation IncludeLocation { get; private set; }
                 public ElementLocation KeepDuplicatesLocation { get; private set; }
                 public ElementLocation RemoveLocation { get; private set; }
                 public ElementLocation RemoveMetadataLocation { get; private set; }
        }
}
