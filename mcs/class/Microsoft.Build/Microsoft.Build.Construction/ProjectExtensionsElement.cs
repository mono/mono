//
// ProjectExtensionsElement.cs
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
using System.Text;

namespace Microsoft.Build.Construction
{
        public class ProjectExtensionsElement : ProjectElement
        {
                internal ProjectExtensionsElement (ProjectRootElement containingProject)
                {
                        ContainingProject = containingProject;
                }
                public override string Condition {
                        get { return null; }
                        set {
                                throw new InvalidOperationException ("Can not set Condition.");
                        }
                }
                public string Content {
                        get { return element.InnerXml; }
                        set { element.InnerXml = value; }
                }
                public string this[string name] {
                        get {
                                var child = element[name];
                                return child == null ? string.Empty : child.InnerXml;
                        }
                        set {
                                var child = element[name];
                                if (child == null) {
                                        if (string.IsNullOrEmpty (name))
                                                return;
                                        child = document.CreateElement (name);
                                        element.AppendChild (child);
                                }
                                if (string.IsNullOrEmpty (value))
                                        element.RemoveChild (child);
                                else
                                        child.InnerXml = value;
                        }
                }
                internal override void Load (XmlReader reader)
                {
                        while (reader.Read () && reader.NodeType != XmlNodeType.Element)
                                ;
                        using (XmlReader subReader = reader.ReadSubtree ()) {
                                document = new XmlDocument ();
                                document.Load (subReader);
                                element = document.DocumentElement;
                        }
                }
                internal override void SaveValue (XmlWriter writer)
                {
                        element.WriteContentTo (writer);
                }
                internal override string XmlName {
                        get { return "ProjectExtensions"; }
                }
                XmlDocument document;
                XmlElement element;
        }
}
