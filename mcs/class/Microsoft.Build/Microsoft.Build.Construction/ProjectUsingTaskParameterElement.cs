//
// ProjectUsingTaskParameterElement.cs
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
using System.Xml;
namespace Microsoft.Build.Construction
{
        [System.Diagnostics.DebuggerDisplayAttribute ("Name={Name} ParameterType={ParameterType} Output={Output}")]
        public class ProjectUsingTaskParameterElement : ProjectElement
        {
                internal ProjectUsingTaskParameterElement (string name, string output, string required,
                                                         string parameterType, ProjectRootElement containingProject)
                {
                        Name = name;
                        Output = output;
                        Required = required;
                        ParameterType = parameterType;
                        ContainingProject = containingProject;
                }
                string name;
                public string Name { get { return name ?? String.Empty; } set { name = value; } }
                public override string Condition { get { return null; } set { throw new InvalidOperationException (
                        "Can not set Condition."); } }
                string output;
                public string Output { get { return output ?? String.Empty; } set { output = value; } }
                string parameterType;
                public string ParameterType {
                        get { return parameterType ?? String.Empty; }
                        set { parameterType = value; }
                }
                string required;
                public string Required { get { return required ?? String.Empty; } set { required = value; } }
                internal override string XmlName {
                        get { return Name; }
                }
                internal override void LoadAttribute (string name, string value)
                {
                        switch (name) {
                        case "ParameterType":
                                ParameterType = value;
                                break;
                        case "Output":
                                Output = value;
                                break;
                        case "Required":
                                Required = value;
                                break;
                        default:
                                base.LoadAttribute (name, value);
                                break;
                        }
                }
                internal override void SaveValue (XmlWriter writer)
                {
                        base.SaveValue (writer);
                        SaveAttribute (writer, "ParameterType", ParameterType);
                        SaveAttribute (writer, "Required", Required);
                        SaveAttribute (writer, "Output", Output);
                }
        }
}
