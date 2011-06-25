//
// ProjectUsingTaskElement.cs
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
using Microsoft.Build.Exceptions;

namespace Microsoft.Build.Construction
{
        [System.Diagnostics.DebuggerDisplayAttribute ("TaskName={TaskName} AssemblyName={AssemblyName} "
                                                      + "AssemblyFile={AssemblyFile} Condition={Condition} "
                                                      + "Runtime={RequiredRuntime} Platform={RequiredPlatform}")]
        public class ProjectUsingTaskElement : ProjectElementContainer
        {
                internal ProjectUsingTaskElement (string taskName, string assemblyFile, string assemblyName,
                                                ProjectRootElement containingProject)
                {
                        TaskName = taskName;
                        AssemblyFile = assemblyFile;
                        AssemblyName = assemblyName;
                        ContainingProject = containingProject;
                }
                string assemblyFile;
                public string AssemblyFile {
                        get { return assemblyFile ?? String.Empty; }
                        set { assemblyFile = value; }
                }
                string assemblyName;
                public string AssemblyName {
                        get { return assemblyName ?? String.Empty; }
                        set { assemblyName = value; }
                }
                public UsingTaskParameterGroupElement ParameterGroup {
                        get { return FirstChild as UsingTaskParameterGroupElement; }
                }
                public ProjectUsingTaskBodyElement TaskBody {
                        get { return LastChild as ProjectUsingTaskBodyElement; }
                }
                string taskFactory;
                public string TaskFactory { get { return taskFactory ?? String.Empty; } set { taskFactory = value; } }
                string taskName;
                public string TaskName { get { return taskName ?? String.Empty; } set { taskName = value; } }
                public UsingTaskParameterGroupElement AddParameterGroup ()
                {
                        var @group = ContainingProject.CreateUsingTaskParameterGroupElement ();
                        PrependChild (@group);
                        return @group;
                }
                public ProjectUsingTaskBodyElement AddUsingTaskBody (string evaluate, string taskBody)
                {
                        var body = ContainingProject.CreateUsingTaskBodyElement (evaluate, taskBody);
                        AppendChild (body);
                        return body;
                }
                internal override void LoadAttribute (string name, string value)
                {
                        switch (name) {
                        case "AssemblyName":
                                AssemblyName = value;
                                break;
                        case "AssemblyFile":
                                AssemblyFile = value;
                                break;
                        case "TaskFactory":
                                TaskFactory = value;
                                break;
                        case "TaskName":
                                TaskName = value;
                                break;
                        default:
                                base.LoadAttribute (name, value);
                                break;
                        }
                }
                internal override void SaveValue (System.Xml.XmlWriter writer)
                {
                        SaveAttribute (writer, "AssemblyName", AssemblyName);
                        SaveAttribute (writer, "AssemblyFile", AssemblyFile);
                        SaveAttribute (writer, "TaskFactory", TaskFactory);
                        SaveAttribute (writer, "TaskName", TaskName);
                        base.SaveValue (writer);
                }
                internal override string XmlName {
                        get { return "UsingTask"; }
                }
                internal override ProjectElement LoadChildElement (string name)
                {
                        switch (name) {
                        case "ParameterGroup":
                                return AddParameterGroup ();
                        case "Task":
                                return AddUsingTaskBody (null, null);
                        default:
                                throw new InvalidProjectFileException (string.Format (
                                        "Child \"{0}\" is not a known node type.", name));
                        }
                }
        }
}
