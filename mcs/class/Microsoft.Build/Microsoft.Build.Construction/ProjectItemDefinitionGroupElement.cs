//
// ProjectItemDefinitionGroupElement.cs
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
        [System.Diagnostics.DebuggerDisplayAttribute ("#ItemDefinitions={Count} Condition={Condition} Label={Label}")]
        public class ProjectItemDefinitionGroupElement : ProjectElementContainer
        {
                internal ProjectItemDefinitionGroupElement (ProjectRootElement containingProject)
                {
                        ContainingProject = containingProject;
                }
                public ICollection<ProjectItemDefinitionElement> ItemDefinitions {
                        get { return new CollectionFromEnumerable<ProjectItemDefinitionElement> (
                                new FilteredEnumerable<ProjectItemDefinitionElement> (Children)); }
                }
                public ProjectItemDefinitionElement AddItemDefinition (string itemType)
                {
                        var definition = ContainingProject.CreateItemDefinitionElement (itemType);
                        AppendChild (definition);
                        return definition;
                }
                internal override string XmlName {
                        get { return "ItemDefinitionGroup"; }
                }
                internal override ProjectElement LoadChildElement (string name)
                {
                        return AddItemDefinition (name);
                }
        }
}
