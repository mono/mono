//
// UsingTaskParameterGroupElement.cs
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
        [System.Diagnostics.DebuggerDisplayAttribute ("#Parameters={Count}")]
        public class UsingTaskParameterGroupElement : ProjectElementContainer
        {
                internal UsingTaskParameterGroupElement (ProjectRootElement containingProject)
                {
                        ContainingProject = containingProject;
                }
                public override string Condition { get { return null; } set { throw new InvalidOperationException(
                        "Can not set Condition."); } }
                public ICollection<ProjectUsingTaskParameterElement> Parameters {
                        get { return new CollectionFromEnumerable<ProjectUsingTaskParameterElement> (
                                new FilteredEnumerable<ProjectUsingTaskParameterElement> (Children)); }
                }
                public ProjectUsingTaskParameterElement AddParameter (string name)
                {
                        return AddParameter (name, null, null, null);
                }
                public ProjectUsingTaskParameterElement AddParameter (string name, string output, string required,
                                                                      string parameterType)
                {
                        var parameter = ContainingProject.CreateUsingTaskParameterElement (name, output, required,
                                parameterType);
                        AppendChild (parameter);
                        return parameter;
                }
                internal override string XmlName {
                        get { return "ParameterGroup"; }
                }
                internal override ProjectElement LoadChildElement (string name)
                {
                        return AddParameter (name);
                }
        }
}
