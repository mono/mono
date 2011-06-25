//
// ProjectElement.cs
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
using System.Xml;
using Microsoft.Build.Exceptions;

namespace Microsoft.Build.Construction
{
        public abstract class ProjectElement
        {
                internal ProjectElement ()
                {
                        linkedListNode = new LinkedListNode<ProjectElement> (this);
                }
                public ProjectRootElement ContainingProject { get; internal set; }
                public ProjectElement PreviousSibling {
                        get { return LinkedListNode.Previous == null ? null : LinkedListNode.Previous.Value; }
                        internal set { }
                }
                public ProjectElementContainer Parent { get; internal set; }
                public ProjectElement NextSibling {
                        get { return LinkedListNode.Next == null ? null : LinkedListNode.Next.Value; }
                        internal set { }
                }
                string label;
                public string Label { get { return label ?? String.Empty; } set { label = value; } }
                string condition;
                public virtual string Condition { get { return condition ?? String.Empty; } set { condition = value; } }
                public IEnumerable<ProjectElementContainer> AllParents {
                        get {
                                var parent = Parent;
                                while (parent != null) {
                                        yield return parent;
                                        parent = parent.Parent;
                                }
                        }
                }
                readonly LinkedListNode<ProjectElement> linkedListNode;
                internal LinkedListNode<ProjectElement> LinkedListNode { get { return linkedListNode; } }
                internal virtual void Load (XmlReader reader)
                {
                        reader.ReadToFollowing (XmlName);
                        while (reader.MoveToNextAttribute ()) {
                                LoadAttribute (reader.Name, reader.Value);
                        }
                        LoadValue (reader);
                }
                internal virtual void LoadAttribute (string name, string value)
                {
                        switch (name) {
                        case "xmlns":
                                break;
                        case "Label":
                                Label = value;
                                break;
                        case "Condition":
                                Condition = value;
                                break;
                        default:
                                throw new InvalidProjectFileException (string.Format (
                                        "Attribute \"{0}\" is not known on node \"{1}\" [type {2}].", name, XmlName,
                                        GetType ()));
                        }
                }
                internal virtual void LoadValue (XmlReader reader)
                {
                }
                internal abstract string XmlName { get; }
                internal virtual void Save (XmlWriter writer)
                {
                        writer.WriteStartElement (XmlName);
                        SaveValue (writer);
                        writer.WriteEndElement ();
                }
                internal virtual void SaveValue (XmlWriter writer)
                {
                        SaveAttribute (writer, "Label", Label);
                        SaveAttribute (writer, "Condition", Condition);
                }
                internal void SaveAttribute (XmlWriter writer, string attributeName, string attributeValue)
                {
                        if (!string.IsNullOrWhiteSpace (attributeValue))
                                writer.WriteAttributeString (attributeName, attributeValue);
                }
        }
}
