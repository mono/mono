//
// ProjectChooseElement.cs
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
using Microsoft.Build.Exceptions;
using Microsoft.Build.Internal;

namespace Microsoft.Build.Construction
{
        [System.Diagnostics.DebuggerDisplayAttribute ("ProjectChooseElement (#Children={Count} "
                                                      + "HasOtherwise={OtherwiseElement != null})")]
        public class ProjectChooseElement : ProjectElementContainer
        {
                internal ProjectChooseElement (ProjectRootElement containingProject)
                {
                        ContainingProject = containingProject;
                }
                public override string Condition { get { return null; } set { throw new InvalidOperationException(
                        "Can not set Condition."); } }
                public ProjectOtherwiseElement OtherwiseElement {
                        get { return LastChild as ProjectOtherwiseElement; }
                }
                public ICollection<ProjectWhenElement> WhenElements {
                        get { return new CollectionFromEnumerable<ProjectWhenElement> (
                                new FilteredEnumerable<ProjectWhenElement> (Children)); }
                }
                internal override string XmlName {
                        get { return "Choose"; }
                }
                internal override ProjectElement LoadChildElement (string name)
                {
                        switch (name) {
                        case "Otherwise":
                                var other = ContainingProject.CreateOtherwiseElement ();
                                AppendChild (other);
                                return other;
                        case "When":
                                var when = ContainingProject.CreateWhenElement (null);
                                PrependChild (when);
                                return when;
                        default:
                                throw new InvalidProjectFileException (string.Format (
                                        "Child \"{0}\" is not a known node type.", name));
                        }
                }
        }
}
