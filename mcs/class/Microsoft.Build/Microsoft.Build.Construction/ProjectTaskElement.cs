//
// ProjectTaskElement.cs
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
using System.Xml;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Internal;

namespace Microsoft.Build.Construction
{
        [System.Diagnostics.DebuggerDisplayAttribute ("{Name} Condition={Condition} ContinueOnError={ContinueOnError} "
                                                      + "#Outputs={Count}")]
        public class ProjectTaskElement : ProjectElementContainer
        {
                internal ProjectTaskElement (string taskName, ProjectRootElement containingProject)
                        : this(containingProject)
                {
                        Name = taskName;
                }
                internal ProjectTaskElement (ProjectRootElement containingProject)
                {
                        ContainingProject = containingProject;
                }
                string continueOnError;
                public string ContinueOnError {
                        get { return continueOnError ?? String.Empty; }
                        set { continueOnError = value; }
                }
                string name;
                public string Name { get { return name ?? String.Empty; } private set { name = value; } }
                public ICollection<ProjectOutputElement> Outputs {
                        get { return new CollectionFromEnumerable<ProjectOutputElement> (
                                new FilteredEnumerable<ProjectOutputElement> (AllChildren)); }
                }
                public IDictionary<string, string> Parameters {
                        get { return parameters; }
                }
                public ProjectOutputElement AddOutputItem (string taskParameter, string itemType)
                {
                        return AddOutputItem (taskParameter, itemType, null);
                }
                public ProjectOutputElement AddOutputItem (string taskParameter, string itemType, string condition)
                {
                        var output = new ProjectOutputElement (taskParameter, itemType, null, ContainingProject);
                        if( condition != null)
                                output.Condition = condition;
                        AppendChild (output);
                        return output;
                }
                public ProjectOutputElement AddOutputProperty (string taskParameter, string propertyName)
                {
                        return AddOutputProperty (taskParameter, propertyName, null);
                }
                public ProjectOutputElement AddOutputProperty (string taskParameter, string propertyName,
                                                               string condition)
                {
                        var output = new ProjectOutputElement (taskParameter, null, propertyName, ContainingProject);
                        if( condition != null)
                                output.Condition = condition;
                        AppendChild (output);
                        return output;
                }
                public string GetParameter (string name)
                {
                        string value;
                        if (parameters.TryGetValue (name, out value))
                                return value;
                        return string.Empty;
                }
                public void RemoveAllParameters ()
                {
                        parameters.Clear ();
                }
                public void RemoveParameter (string name)
                {
                        parameters.Remove (name);
                }
                public void SetParameter (string name, string unevaluatedValue)
                {
                        parameters[name] = unevaluatedValue;
                }
                internal override string XmlName {
                        get { return Name; }
                }
                internal override ProjectElement LoadChildElement (string name)
                {
                        switch (name) {
                        case "Output":
                                var output = ContainingProject.CreateOutputElement (null, null, null);
                                AppendChild (output);
                                return output;
                        default:
                                throw new InvalidProjectFileException (string.Format (
                                        "Child \"{0}\" is not a known node type.", name));
                        }
                }
                internal override void LoadAttribute (string name, string value)
                {
                        switch (name) {
                        case "ContinueOnError":
                                ContinueOnError = value;
                                break;
                        case "xmlns":
                                break;
                        case "Label":
                                Label = value;
                                break;
                        case "Condition":
                                Condition = value;
                                break;
                        default:
                                SetParameter (name, value);
                                break;
                        }
                }
                internal override void SaveValue (XmlWriter writer)
                {
                        SaveAttribute (writer, "ContinueOnError", ContinueOnError);
                        foreach (var parameter in parameters) {
                                SaveAttribute (writer, parameter.Key, parameter.Value);
                        }
                        base.SaveValue (writer);
                }
                private Dictionary<string, string> parameters = new Dictionary<string, string> ();
        }
}
