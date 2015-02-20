//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Activities.Presentation;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using Microsoft.Activities.Presentation.Xaml;

    // The graph is completely defined by a collection of vertices and a collection of edges. The back pointers are not part of the graph
    // definition, but as an auxiliary to quickly trace back to the root vertex if it is reachable from the root. A vertex should have
    // no back pointers if it's not reachable from the root.
    // This abstract base class is responsible for managing back pointers while the dervied class is responsible for managing vertices and edges.
    internal abstract class GraphManager<TVertex, TEdge, TBackPointer> where TVertex : class
    {
        protected abstract TVertex Root { get; }

        internal void VerifyBackPointers()
        {
            ICollection<TVertex> reachableVertices = this.CalculateReachableVertices(true);

            foreach (TVertex vertex in this.GetVertices())
            {
                if (reachableVertices.Contains(vertex))
                {
                    foreach (TBackPointer backPointer in this.GetBackPointers(vertex))
                    {
                        if (!reachableVertices.Contains(this.GetDestinationVertexFromBackPointer(backPointer)))
                        {
                            Fx.Assert(false, "a reachable vertex's back pointer should not point to a vertex that is not reachable");
                        }

                        if (!this.HasAssociatedEdge(backPointer))
                        {
                            Fx.Assert(false, "a reachable vertex doesn't have an outgoing edge to one of the vertex that have a back pointer to it");
                        }
                    }
                }
                else
                {
                    if (this.GetBackPointers(vertex).Count() != 0)
                    {
                        Fx.Assert(false, "an unreachable vertex should not have any back pointer");
                    }
                }
            }
        }

        protected ICollection<TVertex> CalculateReachableVertices(bool verifyBackPointers = false)
        {
            HashSet<TVertex> reachableVertices = new HashSet<TVertex>(ObjectReferenceEqualityComparer<TVertex>.Default);

            if (this.Root == null)
            {
                return reachableVertices;
            }

            Queue<TVertex> queue = new Queue<TVertex>();
            queue.Enqueue(this.Root);
            reachableVertices.Add(this.Root);

            while (queue.Count > 0)
            {
                TVertex vertex = queue.Dequeue();

                foreach (TEdge edge in this.GetOutEdges(vertex))
                {
                    if (verifyBackPointers && !this.HasBackPointer(edge))
                    {
                        Fx.Assert(false, "a reachable vertex doesn't have a back pointer to one of its incoming edges");
                    }

                    TVertex to = this.GetDestinationVertexFromEdge(edge);
                    if (!reachableVertices.Contains(to))
                    {
                        reachableVertices.Add(to);
                        queue.Enqueue(to);
                    }
                }
            }

            return reachableVertices;
        }

        protected void OnRootChanged(TVertex oldRoot, TVertex newRoot)
        {
            if (oldRoot != null)
            {
                this.RemoveBackPointers(oldRoot, true);
            }

            if (newRoot != null)
            {
                this.AddBackPointers(newRoot);
            }
        }

        protected abstract IEnumerable<TVertex> GetVertices();

        protected abstract IEnumerable<TEdge> GetOutEdges(TVertex vertex);

        protected abstract IEnumerable<TBackPointer> GetBackPointers(TVertex vertex);

        protected abstract TVertex GetDestinationVertexFromEdge(TEdge edge);

        protected abstract TVertex GetSourceVertexFromEdge(TEdge edge);

        protected abstract TVertex GetDestinationVertexFromBackPointer(TBackPointer backPointer);

        // call this method when an edge is removed
        protected void OnEdgeRemoved(TEdge edgeRemoved)
        {
            Fx.Assert(edgeRemoved != null, "edgeRemoved should not be null");

            TVertex sourceVertex = this.GetSourceVertexFromEdge(edgeRemoved);
            if (!this.CanReachRootViaBackPointer(sourceVertex))
            {
                return;
            }

            this.RemoveAssociatedBackPointer(edgeRemoved);
            TVertex destinationVertex = this.GetDestinationVertexFromEdge(edgeRemoved);

            this.RemoveBackPointers(destinationVertex);
        }

        // call this method when an edge is added
        protected void OnEdgeAdded(TEdge edgeAdded)
        {
            Fx.Assert(edgeAdded != null, "edgeAdded should not be null");

            TVertex sourceVertex = this.GetSourceVertexFromEdge(edgeAdded);
            if (!this.CanReachRootViaBackPointer(sourceVertex))
            {
                return;
            }

            TVertex destinationVertex = this.GetDestinationVertexFromEdge(edgeAdded);
            bool wasReachable = this.CanReachRootViaBackPointer(destinationVertex);
            this.AddAssociatedBackPointer(edgeAdded);

            if (wasReachable)
            {
                return;
            }

            this.AddBackPointers(destinationVertex);
        }

        protected abstract void RemoveAssociatedBackPointer(TEdge edge);

        protected abstract void AddAssociatedBackPointer(TEdge edge);

        protected abstract bool HasBackPointer(TEdge edge);

        protected abstract bool HasAssociatedEdge(TBackPointer backPointer);

        protected abstract void OnVerticesBecameReachable(IEnumerable<TVertex> reachableVertices);

        protected abstract void OnVerticesBecameUnreachable(IEnumerable<TVertex> unreachableVertices);

        private bool CanReachRootViaBackPointer(TVertex vertex)
        {
            Fx.Assert(vertex != null, "vertex should not be null");

            if (vertex == this.Root)
            {
                return true;
            }

            HashSet<TVertex> visited = new HashSet<TVertex>(ObjectReferenceEqualityComparer<TVertex>.Default);
            Queue<TVertex> queue = new Queue<TVertex>();

            visited.Add(vertex);
            queue.Enqueue(vertex);

            while (queue.Count > 0)
            {
                TVertex current = queue.Dequeue();
                foreach (TBackPointer backPointer in this.GetBackPointers(current))
                {
                    TVertex destinationVertex = this.GetDestinationVertexFromBackPointer(backPointer);
                    if (object.ReferenceEquals(destinationVertex, this.Root))
                    {
                        return true;
                    }

                    if (!visited.Contains(destinationVertex))
                    {
                        visited.Add(destinationVertex);
                        queue.Enqueue(destinationVertex);
                    }
                }
            }

            return false;
        }

        // traverse the sub-graph starting from vertex and add back pointers
        private void AddBackPointers(TVertex vertex)
        {
            HashSet<TVertex> verticesBecameReachable = new HashSet<TVertex>(ObjectReferenceEqualityComparer<TVertex>.Default);
            Queue<TVertex> queue = new Queue<TVertex>();

            verticesBecameReachable.Add(vertex);
            queue.Enqueue(vertex);

            while (queue.Count > 0)
            {
                TVertex currentVertex = queue.Dequeue();

                foreach (TEdge edge in this.GetOutEdges(currentVertex))
                {
                    TVertex destinationVertex = this.GetDestinationVertexFromEdge(edge);
                    bool wasReachable = this.GetBackPointers(destinationVertex).Count() > 0;
                    this.AddAssociatedBackPointer(edge);
                    if (!wasReachable && !verticesBecameReachable.Contains(destinationVertex))
                    {
                        verticesBecameReachable.Add(destinationVertex);
                        queue.Enqueue(destinationVertex);
                    }
                }
            }

            this.OnVerticesBecameReachable(verticesBecameReachable);
        }

        // traverse the sub-graph starting from vertex, if a vertex is reachable then stop traversing its descendents,
        // otherwise remove back pointers that pointer to it and continue traversing its descendents
        private void RemoveBackPointers(TVertex vertex, bool isAllVerticesUnreachable = false)
        {
            ICollection<TVertex> reachableVertices = new HashSet<TVertex>(ObjectReferenceEqualityComparer<TVertex>.Default);

            if (!isAllVerticesUnreachable)
            {
                reachableVertices = this.CalculateReachableVertices();
            }

            if (reachableVertices.Contains(vertex))
            {
                return;
            }

            Queue<TVertex> queue = new Queue<TVertex>();
            HashSet<TVertex> unreachableVertices = new HashSet<TVertex>(ObjectReferenceEqualityComparer<TVertex>.Default);

            unreachableVertices.Add(vertex);
            queue.Enqueue(vertex);

            while (queue.Count > 0)
            {
                TVertex unreachableVertex = queue.Dequeue();
                foreach (TEdge edge in this.GetOutEdges(unreachableVertex))
                {
                    this.RemoveAssociatedBackPointer(edge);
                    TVertex to = this.GetDestinationVertexFromEdge(edge);
                    if (isAllVerticesUnreachable || !reachableVertices.Contains(to))
                    {
                        if (!unreachableVertices.Contains(to))
                        {
                            unreachableVertices.Add(to);
                            queue.Enqueue(to);
                        }
                    }
                }
            }

            this.OnVerticesBecameUnreachable(unreachableVertices);
        }
    }
}
