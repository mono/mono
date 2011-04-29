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
                List<ProjectElement> children = new List<ProjectElement> ();

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
                        get { return children; }
                }

                public ICollection<ProjectElement> ChildrenReversed {
                        get { return new CollectionFromEnumerable<ProjectElement> (
                                new ReverseEnumerable<ProjectElement> (children)); }
                }

                public int Count {
                        get { return children.Count; }
                }
                public ProjectElement FirstChild {
                        get { return children.FirstOrDefault (); }
                        private set { }
                }
                public ProjectElement LastChild {
                        get { return Count > 0 ? children[Count - 1] : null; }
                        private set { }
                }

                public void AppendChild (ProjectElement child)
                {
                        children.Add (child);
                        child.Parent = this;
                        if (Count != 1) {
                                child.PreviousSibling = children[Count - 2];
                                children[Count - 2].NextSibling = child;
                        }
                }

                public void InsertAfterChild (ProjectElement child, ProjectElement reference)
                {
                        if (reference == null) {
                                PrependChild (child);
                        } else {
                                child.Parent = this;
                                var referenceIndex = children.IndexOf (reference);
                                children.Insert (referenceIndex + 1, child);
                                child.PreviousSibling = reference;
                                reference.NextSibling = child;
                                
                                if (referenceIndex + 2 < Count) {
                                        child.NextSibling = children[referenceIndex + 2];
                                        children[referenceIndex + 2].PreviousSibling = child;
                                }
                        }
                }

                public void InsertBeforeChild (ProjectElement child, ProjectElement reference)
                {
                        if (reference == null) {
                                AppendChild (child);
                        } else {
                                child.Parent = this;
                                var referenceIndex = children.IndexOf (reference);
                                children.Insert (referenceIndex, child);
                                child.NextSibling = reference;
                                reference.PreviousSibling = child;
                                
                                if (referenceIndex > 0) {
                                        child.PreviousSibling = children[referenceIndex - 1];
                                        children[referenceIndex - 1].NextSibling = child;
                                }
                        }
                }

                public void PrependChild (ProjectElement child)
                {
                        children.Insert (0, child);
                        child.Parent = this;
                        if (Count != 1) {
                                child.NextSibling = children[1];
                                children[1].PreviousSibling = child;
                        }
                }

                public void RemoveAllChildren ()
                {
                        foreach (var child in children)
                                RemoveChild (child);
                }

                public void RemoveChild (ProjectElement child)
                {
                        child.Parent = null;
                        children.Remove(child);
                        if (child.NextSibling != null)
                                child.NextSibling.PreviousSibling = child.PreviousSibling;
                        if (child.PreviousSibling != null)
                                child.PreviousSibling.NextSibling = child.NextSibling;
                }

                internal override void SaveValue (XmlWriter writer)
                {
                        base.SaveValue (writer);
                        foreach (var child in Children)
                                child.Save (writer);
                }

                internal override void LoadValue (XmlReader reader)
                {
                        while (reader.Read ()) {
                                if (!reader.IsStartElement ())
                                        continue;
                                var child = LoadChildElement (reader.Name);
                                child.Load (reader.ReadSubtree ());
                        }
                }

                internal abstract ProjectElement LoadChildElement (string name);
        }
}
