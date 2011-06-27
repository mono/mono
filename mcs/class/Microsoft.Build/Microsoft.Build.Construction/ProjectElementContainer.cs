//
// ProjectElementContainer.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Build.Internal;

namespace Microsoft.Build.Construction
{
        public abstract class ProjectElementContainer : ProjectElement
        {
                internal ProjectElementContainer () {}
                LinkedList<ProjectElement> children = new LinkedList<ProjectElement> ();

                public IEnumerable<ProjectElement> AllChildren {
                        get {
                                foreach (var child in Children) {
                                        var container = child as ProjectElementContainer;
                                        if (container != null)
                                                foreach (var containersChild in container.AllChildren)
                                                        yield return containersChild;
                                        yield return child;
                                }
                        }
                }

                public ICollection<ProjectElement> Children {
                        get { return new CollectionFromEnumerable<ProjectElement> (
                                children.Where (p => !(p is ProjectCommentElement))); }
                }

                public ICollection<ProjectElement> ChildrenReversed {
                        get { return new CollectionFromEnumerable<ProjectElement> (
                                new ReverseEnumerable<ProjectElement> (children)); }
                }

                public int Count {
                        get { return children.Count; }
                }
                public ProjectElement FirstChild {
                        get { return children.First == null ? null : children.First.Value; }
                        private set { }
                }
                public ProjectElement LastChild {
                        get { return children.Last == null ? null: children.Last.Value; }
                        private set { }
                }

                public void AppendChild (ProjectElement child)
                {
                        children.AddLast (child.LinkedListNode);
                        child.Parent = this;
                }

                public void InsertAfterChild (ProjectElement child, ProjectElement reference)
                {
                        if (reference == null) {
                                PrependChild (child);
                        } else {
                                child.Parent = this;
                                children.AddAfter (reference.LinkedListNode, child.LinkedListNode);
                        }
                }

                public void InsertBeforeChild (ProjectElement child, ProjectElement reference)
                {
                        if (reference == null) {
                                AppendChild (child);
                        } else {
                                child.Parent = this;
                                children.AddBefore (reference.LinkedListNode, child.LinkedListNode);
                        }
                }

                public void PrependChild (ProjectElement child)
                {
                        children.AddFirst (child.LinkedListNode);
                        child.Parent = this;
                }

                public void RemoveAllChildren ()
                {
                        foreach (var child in children)
                                RemoveChild (child);
                }

                public void RemoveChild (ProjectElement child)
                {
                        child.Parent = null;
                        children.Remove (child.LinkedListNode);
                }

                internal override void SaveValue (XmlWriter writer)
                {
                        base.SaveValue (writer);
                        foreach (var child in children)
                                child.Save (writer);
                }

                internal override void LoadValue (XmlReader reader)
                {
                        while (reader.Read ()) {
                                if (reader.NodeType == XmlNodeType.Element) {
                                        var child = LoadChildElement (reader.Name);
                                        child.Load (reader.ReadSubtree ());
                                } else if (reader.NodeType == XmlNodeType.Comment) {
                                        var commentElement = new ProjectCommentElement (ContainingProject);
                                        commentElement.Load (reader);
                                        AppendChild (commentElement);
                                }
                        }
                }

                internal abstract ProjectElement LoadChildElement (string name);
        }
}
