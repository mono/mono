//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Runtime;
#if DEBUG_DUMP
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using System.Windows.Media;
    using System.Xml;
#endif


    /// <summary>
    /// This class efficiently stores and retrieves arbitrarily sized and positioned
    /// objects in a quad-tree data structure.  This can be used to do efficient hit
    /// detection or visiblility checks on objects in a virtualized canvas.
    /// The object does not need to implement any special interface because the Rect Bounds
    /// of those objects is handled as a separate argument to Insert.
    /// </summary>
    class QuadTree<T> where T : class
    {
        Rect bounds; // overall bounds we are indexing.
        Quadrant root;
        IDictionary<T, Quadrant> table;



        /// <summary>
        /// This determines the overall quad-tree indexing strategy, changing this bounds
        /// is expensive since it has to re-divide the entire thing - like a re-hash operation.
        /// </summary>
        public Rect Bounds
        {
            get { return this.bounds; }
            set { this.bounds = value; ReIndex(); }
        }

        /// <summary>
        /// Insert a node with given bounds into this QuadTree.
        /// </summary>
        /// <param name="node">The node to insert</param>
        /// <param name="bounds">The bounds of this node</param>
        public void Insert(T node, Rect bounds)
        {
            if (this.bounds.Width == 0 || this.bounds.Height == 0)
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.BoundsMustBeNonZero));
            }
            if (bounds.Width == 0 || bounds.Height == 0)
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.BoundsMustBeNonZero));
            }
            if (this.root == null)
            {
                this.root = new Quadrant(null, this.bounds);
            }

            Quadrant parent = this.root.Insert(node, bounds);

            if (this.table == null)
            {
                this.table = new Dictionary<T, Quadrant>();
            }
            this.table[node] = parent;


        }

        /// <summary>
        /// Get a list of the nodes that intersect the given bounds.
        /// </summary>
        /// <param name="bounds">The bounds to test</param>
        /// <returns>List of zero or mode nodes found inside the given bounds</returns>
        public IEnumerable<T> GetNodesInside(Rect bounds)
        {
            foreach (QuadNode n in GetNodes(bounds))
            {
                yield return n.Node;
            }
        }

        /// <summary>
        /// Get a list of the nodes that intersect the given bounds.
        /// </summary>
        /// <param name="bounds">The bounds to test</param>
        /// <returns>List of zero or mode nodes found inside the given bounds</returns>
        public bool HasNodesInside(Rect bounds)
        {
            if (this.root == null)
            {
                return false;                
            }
            return this.root.HasIntersectingNodes(bounds);
        }

        /// <summary>
        /// Get list of nodes that intersect the given bounds.
        /// </summary>
        /// <param name="bounds">The bounds to test</param>
        /// <returns>The list of nodes intersecting the given bounds</returns>
        IEnumerable<QuadNode> GetNodes(Rect bounds)
        {
            List<QuadNode> result = new List<QuadNode>();
            if (this.root != null)
            {
                this.root.GetIntersectingNodes(result, bounds);
            }
            return result;
        }

        /// <summary>
        /// Remove the given node from this QuadTree.
        /// </summary>
        /// <param name="node">The node to remove</param>
        /// <returns>True if the node was found and removed.</returns>
        public bool Remove(T node)
        {
            if (this.table != null)
            {
                Quadrant parent = null;
                if (this.table.TryGetValue(node, out parent))
                {
                    parent.RemoveNode(node);
                    this.table.Remove(node);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Rebuild all the Quadrants according to the current QuadTree Bounds.
        /// </summary>
        void ReIndex()
        {
            this.root = null;
            foreach (QuadNode n in GetNodes(this.bounds))
            {
                Insert(n.Node, n.Bounds);
            }
        }

        /// <summary>
        /// Each node stored in the tree has a position, width & height.
        /// </summary>
        internal class QuadNode
        {
            Rect bounds;
            QuadNode next; // linked in a circular list.
            T node; // the actual visual object being stored here.

            /// <summary>
            /// Construct new QuadNode to wrap the given node with given bounds
            /// </summary>
            /// <param name="node">The node</param>
            /// <param name="bounds">The bounds of that node</param>
            public QuadNode(T node, Rect bounds)
            {
                this.node = node;
                this.bounds = bounds;
            }

            /// <summary>
            /// The node
            /// </summary>
            public T Node
            {
                get { return this.node; }
                set { this.node = value; }
            }

            /// <summary>
            /// The Rect bounds of the node
            /// </summary>
            public Rect Bounds
            {
                get { return this.bounds; }
            }

            /// <summary>
            /// QuadNodes form a linked list in the Quadrant.
            /// </summary>
            public QuadNode Next
            {
                get { return this.next; }
                set { this.next = value; }
            }
        }


        /// <summary>
        /// The canvas is split up into four Quadrants and objects are stored in the quadrant that contains them
        /// and each quadrant is split up into four child Quadrants recurrsively.  Objects that overlap more than
        /// one quadrant are stored in the this.nodes list for this Quadrant.
        /// </summary>
        internal class Quadrant
        {
            Quadrant parent;
            Rect bounds; // quadrant bounds.

            QuadNode nodes; // nodes that overlap the sub quadrant boundaries.

            // The quadrant is subdivided when nodes are inserted that are 
            // completely contained within those subdivisions.
            Quadrant topLeft;
            Quadrant topRight;
            Quadrant bottomLeft;
            Quadrant bottomRight;

#if DEBUG_DUMP
            public void ShowQuadTree(Canvas c)
            {
                Rectangle r = new Rectangle();
                r.Width = this.bounds.Width;
                r.Height = this.bounds.Height;
                Canvas.SetLeft(r, this.bounds.Left);
                Canvas.SetTop(r, this.bounds.Top);
                r.Stroke = Brushes.DarkRed;
                r.StrokeThickness = 1;
                r.StrokeDashArray = new DoubleCollection(new double[] { 2.0, 3.0 });
                c.Children.Add(r);

                if (this.topLeft != null) this.topLeft.ShowQuadTree(c);
                if (this.topRight != null) this.topRight.ShowQuadTree(c);
                if (this.bottomLeft != null) this.bottomLeft.ShowQuadTree(c);
                if (this.bottomRight != null) this.bottomRight.ShowQuadTree(c);
            }

            public void Dump(LogWriter w)
            {
                w.WriteAttribute("Bounds", this.bounds.ToString());
                if (this.nodes != null)
                {
                    QuadNode n = this.nodes;
                    do
                    {
                        n = n.Next; // first node.
                        w.Open("node");
                        w.WriteAttribute("Bounds", n.Bounds.ToString());
                        w.Close();
                    } while (n != this.nodes);
                }
                DumpQuadrant("TopLeft", this.topLeft, w);
                DumpQuadrant("TopRight", this.topRight, w);
                DumpQuadrant("BottomLeft", this.bottomLeft, w);
                DumpQuadrant("BottomRight", this.bottomRight, w);
            }

            public void DumpQuadrant(string label, Quadrant q, LogWriter w)
            {
                if (q != null)
                {
                    w.Open("Quadrant");
                    w.WriteAttribute("Name", label);
                    q.Dump(w);
                    w.Close();
                }
            }
#endif

            /// <summary>
            /// Construct new Quadrant with a given bounds all nodes stored inside this quadrant
            /// will fit inside this bounds.  
            /// </summary>
            /// <param name="parent">The parent quadrant (if any)</param>
            /// <param name="bounds">The bounds of this quadrant</param>
            public Quadrant(Quadrant parent, Rect bounds)
            {
                this.parent = parent;
                Fx.Assert(bounds.Width != 0 && bounds.Height != 0, "Cannot have empty bound");
                if (bounds.Width == 0 || bounds.Height == 0)
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR.BoundsMustBeNonZero));
                }
                this.bounds = bounds;
            }

            /// <summary>
            /// The parent Quadrant or null if this is the root
            /// </summary>
            internal Quadrant Parent
            {
                get { return this.parent; }
            }

            /// <summary>
            /// The bounds of this quadrant
            /// </summary>
            internal Rect Bounds
            {
                get { return this.bounds; }
            }

            /// <summary>
            /// Insert the given node
            /// </summary>
            /// <param name="node">The node </param>
            /// <param name="bounds">The bounds of that node</param>
            /// <returns></returns>
            internal Quadrant Insert(T node, Rect bounds)
            {

                Fx.Assert(bounds.Width != 0 && bounds.Height != 0, "Cannot have empty bound");
                if (bounds.Width == 0 || bounds.Height == 0)
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR.BoundsMustBeNonZero));
                }

                Quadrant toInsert = this;
                while (true)
                {
                    double w = toInsert.bounds.Width / 2;
                    if (w < 1)
                    {
                        w = 1;
                    }
                    double h = toInsert.bounds.Height / 2;
                    if (h < 1)
                    {
                        h = 1;
                    }

                    // assumption that the Rect struct is almost as fast as doing the operations
                    // manually since Rect is a value type.

                    Rect topLeft = new Rect(toInsert.bounds.Left, toInsert.bounds.Top, w, h);
                    Rect topRight = new Rect(toInsert.bounds.Left + w, toInsert.bounds.Top, w, h);
                    Rect bottomLeft = new Rect(toInsert.bounds.Left, toInsert.bounds.Top + h, w, h);
                    Rect bottomRight = new Rect(toInsert.bounds.Left + w, toInsert.bounds.Top + h, w, h);

                    Quadrant child = null;

                    // See if any child quadrants completely contain this node.
                    if (topLeft.Contains(bounds))
                    {
                        if (toInsert.topLeft == null)
                        {
                            toInsert.topLeft = new Quadrant(toInsert, topLeft);
                        }
                        child = toInsert.topLeft;
                    }
                    else if (topRight.Contains(bounds))
                    {
                        if (toInsert.topRight == null)
                        {
                            toInsert.topRight = new Quadrant(toInsert, topRight);
                        }
                        child = toInsert.topRight;
                    }
                    else if (bottomLeft.Contains(bounds))
                    {
                        if (toInsert.bottomLeft == null)
                        {
                            toInsert.bottomLeft = new Quadrant(toInsert, bottomLeft);
                        }
                        child = toInsert.bottomLeft;
                    }
                    else if (bottomRight.Contains(bounds))
                    {
                        if (toInsert.bottomRight == null)
                        {
                            toInsert.bottomRight = new Quadrant(toInsert, bottomRight);
                        }
                        child = toInsert.bottomRight;
                    }

                    if (child != null)
                    {
                        toInsert = child;
                    }
                    else
                    {
                        QuadNode n = new QuadNode(node, bounds);
                        if (toInsert.nodes == null)
                        {
                            n.Next = n;
                        }
                        else
                        {
                            // link up in circular link list.
                            QuadNode x = toInsert.nodes;
                            n.Next = x.Next;
                            x.Next = n;
                        }
                        toInsert.nodes = n;
                        return toInsert;
                    }
                }
            }

            /// <summary>
            /// Returns all nodes in this quadrant that intersect the given bounds.
            /// The nodes are returned in pretty much random order as far as the caller is concerned.
            /// </summary>
            /// <param name="nodes">List of nodes found in the given bounds</param>
            /// <param name="bounds">The bounds that contains the nodes you want returned</param>
            internal void GetIntersectingNodes(List<QuadNode> nodes, Rect bounds)
            {
                if (bounds.IsEmpty) return;
                double w = this.bounds.Width / 2;
                double h = this.bounds.Height / 2;

                // assumption that the Rect struct is almost as fast as doing the operations
                // manually since Rect is a value type.

                Rect topLeft = new Rect(this.bounds.Left, this.bounds.Top, w, h);
                Rect topRight = new Rect(this.bounds.Left + w, this.bounds.Top, w, h);
                Rect bottomLeft = new Rect(this.bounds.Left, this.bounds.Top + h, w, h);
                Rect bottomRight = new Rect(this.bounds.Left + w, this.bounds.Top + h, w, h);

                // See if any child quadrants completely contain this node.
                if (topLeft.IntersectsWith(bounds) && this.topLeft != null)
                {
                    this.topLeft.GetIntersectingNodes(nodes, bounds);
                }

                if (topRight.IntersectsWith(bounds) && this.topRight != null)
                {
                    this.topRight.GetIntersectingNodes(nodes, bounds);
                }

                if (bottomLeft.IntersectsWith(bounds) && this.bottomLeft != null)
                {
                    this.bottomLeft.GetIntersectingNodes(nodes, bounds);
                }

                if (bottomRight.IntersectsWith(bounds) && this.bottomRight != null)
                {
                    this.bottomRight.GetIntersectingNodes(nodes, bounds);
                }

                GetIntersectingNodes(this.nodes, nodes, bounds);
            }

            /// <summary>
            /// Walk the given linked list of QuadNodes and check them against the given bounds.
            /// Add all nodes that intersect the bounds in to the list.
            /// </summary>
            /// <param name="last">The last QuadNode in a circularly linked list</param>
            /// <param name="nodes">The resulting nodes are added to this list</param>
            /// <param name="bounds">The bounds to test against each node</param>
            static void GetIntersectingNodes(QuadNode last, List<QuadNode> nodes, Rect bounds)
            {
                if (last != null)
                {
                    QuadNode n = last;
                    do
                    {
                        n = n.Next; // first node.
                        if (n.Bounds.IntersectsWith(bounds))
                        {
                            nodes.Add(n);
                        }
                    } while (n != last);
                }
            }

            /// <summary>
            /// Return true if there are any nodes in this Quadrant that intersect the given bounds.
            /// </summary>
            /// <param name="bounds">The bounds to test</param>
            /// <returns>boolean</returns>
            internal bool HasIntersectingNodes(Rect bounds)
            {
                if (bounds.IsEmpty) return false;
                double w = this.bounds.Width / 2;
                double h = this.bounds.Height / 2;

                // assumption that the Rect struct is almost as fast as doing the operations
                // manually since Rect is a value type.

                Rect topLeft = new Rect(this.bounds.Left, this.bounds.Top, w, h);
                Rect topRight = new Rect(this.bounds.Left + w, this.bounds.Top, w, h);
                Rect bottomLeft = new Rect(this.bounds.Left, this.bounds.Top + h, w, h);
                Rect bottomRight = new Rect(this.bounds.Left + w, this.bounds.Top + h, w, h);

                bool found = false;

                // See if any child quadrants completely contain this node.
                if (topLeft.IntersectsWith(bounds) && this.topLeft != null)
                {
                    found = this.topLeft.HasIntersectingNodes(bounds);
                }

                if (!found && topRight.IntersectsWith(bounds) && this.topRight != null)
                {
                    found = this.topRight.HasIntersectingNodes(bounds);
                }

                if (!found && bottomLeft.IntersectsWith(bounds) && this.bottomLeft != null)
                {
                    found = this.bottomLeft.HasIntersectingNodes(bounds);
                }

                if (!found && bottomRight.IntersectsWith(bounds) && this.bottomRight != null)
                {
                    found = this.bottomRight.HasIntersectingNodes(bounds);
                }
                if (!found)
                {
                    found = HasIntersectingNodes(this.nodes, bounds);
                }
                return found;
            }

            /// <summary>
            /// Walk the given linked list and test each node against the given bounds/
            /// </summary>
            /// <param name="last">The last node in the circularly linked list.</param>
            /// <param name="bounds">Bounds to test</param>
            /// <returns>Return true if a node in the list intersects the bounds</returns>
            static bool HasIntersectingNodes(QuadNode last, Rect bounds)
            {
                if (last != null)
                {
                    QuadNode n = last;
                    do
                    {
                        n = n.Next; // first node.
                        if (n.Bounds.IntersectsWith(bounds))
                        {
                            return true;
                        }
                    } while (n != last);
                }
                return false;
            }

            /// <summary>
            /// Remove the given node from this Quadrant.
            /// </summary>
            /// <param name="node">The node to remove</param>
            /// <returns>Returns true if the node was found and removed.</returns>
            internal bool RemoveNode(T node)
            {
                bool rc = false;
                if (this.nodes != null)
                {
                    QuadNode p = this.nodes;
                    while (p.Next.Node != node && p.Next != this.nodes)
                    {
                        p = p.Next;
                    }
                    if (p.Next.Node == node)
                    {
                        rc = true;
                        QuadNode n = p.Next;
                        if (p == n)
                        {
                            // list goes to empty
                            this.nodes = null;
                        }
                        else
                        {
                            if (this.nodes == n) this.nodes = p;
                            p.Next = n.Next;
                        }
                    }
                }
                return rc;
            }

        }
#if DEBUG_DUMP
        public void ShowQuadTree(Canvas container)
        {
            if (this.root != null)
            {
                this.root.ShowQuadTree(container);
            }
        }

        public void Dump(LogWriter w)
        {
            if (this.root != null)
            {
                this.root.Dump(w);
            }
        }
#endif
    }

#if DEBUG_DUMP
    public class LogWriter : IDisposable
    {
        XmlWriter xw;
        int indent;
        int maxdepth;

        public LogWriter(TextWriter w)
        {
            XmlWriterSettings s = new XmlWriterSettings();
            s.Indent = true;            
            this.xw = XmlWriter.Create(w, s);
        }

        public int MaxDepth
        {
            get
            {
                return this.maxdepth;
            }
        }

        public void Open(string label)
        {
            this.xw.WriteStartElement(label);
            this.indent++;
            if (this.indent > this.maxdepth) this.maxdepth = this.indent;

        }
        public void Close()
        {
            this.indent--;
            this.xw.WriteEndElement();
        }
        public void WriteAttribute(string name, string value)
        {
            this.xw.WriteAttributeString(name, value);
        }

    #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.xw != null)
            {
                using (this.xw)
                {
                    this.xw.Flush();
                }
                this.xw = null;
            }
        }

    #endregion
    }
#endif

}
