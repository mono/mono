//
// ProjectOutputElement.cs
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

using System.Xml;
namespace Microsoft.Build.Construction
{
        [System.Diagnostics.DebuggerDisplayAttribute ("Name={Name} TaskParameter={TaskParameter} ItemName={ItemName} "
                                                      + "PropertyName={PropertyName} Condition={Condition}")]
        public class ProjectOutputElement : ProjectElement
        {
                internal ProjectOutputElement (string taskParameter, string itemType, string propertyName,
                                               ProjectRootElement containintProject)
                {
                        TaskParameter = taskParameter;
                        ItemType = itemType;
                        PropertyName = propertyName;
                        ContainingProject = containintProject;
                }
                public bool IsOutputItem {
                        get { return true; }
                }
                public bool IsOutputProperty {
                        get { return !IsOutputItem; }
                }
                public string ItemType { get; set; }
                public string PropertyName { get; set; }
                public string TaskParameter { get; set; }
                internal override string XmlName {
                        get { return "Output"; }
                }
                internal override void SaveValue (XmlWriter writer)
                {
                        base.SaveValue (writer);
                        SaveAttribute (writer, "TaskParameter", TaskParameter);
                        if (IsOutputProperty)
                                SaveAttribute (writer, "PropertyName", PropertyName);
                        else
                                SaveAttribute (writer, "ItemName", ItemType);
                }
                internal override void LoadAttribute (string name, string value)
                {
                        switch (name) {
                        case "TaskParameter":
                                TaskParameter = value;
                                break;
                        case "PropertyName":
                                PropertyName = value;
                                break;
                        case "ItemName":
                                ItemType = value;
                                break;
                        default:
                                base.LoadAttribute (name, value);
                                break;
                        }
                }
        }
}
