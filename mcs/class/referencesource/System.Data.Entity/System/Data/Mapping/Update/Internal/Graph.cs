//---------------------------------------------------------------------
// <copyright file="Graph.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Common.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Globalization;
using System.Linq;
namespace System.Data.Mapping.Update.Internal
{
    /// <summary>
    /// A directed graph class.
    /// </summary>
    /// <remarks>
    /// Notes on language (in case you're familiar with one or the other convention):
    /// 
    /// node == vertex
    /// arc == edge
    /// predecessor == incoming
    /// successor == outgoing
    /// </remarks>
    /// <typeparam name="TVertex">Type of nodes in the graph</typeparam>
    internal class Graph<TVertex>
    {
        #region Constructors
        /// <summary>
        /// Initialize a new graph
        /// </summary>
        /// <param name="comparer">Comparer used to determine if two node references are 
        /// equivalent</param>
        internal Graph(IEqualityComparer<TVertex> comparer)
        {
            EntityUtil.CheckArgumentNull(comparer, "comparer");

            m_comparer = comparer;
            m_successorMap = new Dictionary<TVertex, HashSet<TVertex>>(comparer);
            m_predecessorCounts = new Dictionary<TVertex, int>(comparer);
            m_vertices = new HashSet<TVertex>(comparer);
        }
        #endregion

        #region Fields
        /// <summary>
        /// Gets successors of the node (outgoing edges).
        /// </summary>
        private readonly Dictionary<TVertex, HashSet<TVertex>> m_successorMap;

        /// <summary>
        /// Gets number of predecessors of the node.
        /// </summary>
        private readonly Dictionary<TVertex, int> m_predecessorCounts;

        /// <summary>
        /// Gets the vertices that exist in the graph.
        /// </summary>
        private readonly HashSet<TVertex> m_vertices;
        private readonly IEqualityComparer<TVertex> m_comparer;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the vertices of the graph.
        /// </summary>
        internal IEnumerable<TVertex> Vertices
        {
            get { return m_vertices; }
                
        }

        /// <summary>
        /// Returns the edges of the graph in the form: [from, to]
        /// </summary>
        internal IEnumerable<KeyValuePair<TVertex, TVertex>> Edges
        {
            get
            {
                foreach (KeyValuePair<TVertex, HashSet<TVertex>> successors in m_successorMap)
                {
                    foreach (TVertex vertex in successors.Value)
                    {
                        yield return new KeyValuePair<TVertex, TVertex>(successors.Key, vertex);
                    }
                }
            }
                
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new node to the graph. Does nothing if the vertex already exists.
        /// </summary>
        /// <param name="vertex">New node</param>
        internal void AddVertex(TVertex vertex)
        {
            m_vertices.Add(vertex);
        }

        /// <summary>
        /// Adds a new edge to the graph. NOTE: only adds edges for existing vertices.
        /// </summary>
        /// <param name="from">Source node</param>
        /// <param name="to">Target node</param>
        internal void AddEdge(TVertex from, TVertex to)
        {
            // Add only edges relevant to the current graph vertices
            if (m_vertices.Contains(from) && m_vertices.Contains(to))
            {
                HashSet<TVertex> successors;
                if (!m_successorMap.TryGetValue(from, out successors))
                {
                    successors = new HashSet<TVertex>(m_comparer);
                    m_successorMap.Add(from, successors);
                }
                if (successors.Add(to))
                {
                    // If the edge does not already exist, increment the count of incoming edges (predecessors).
                    int predecessorCount;
                    if (!m_predecessorCounts.TryGetValue(to, out predecessorCount))
                    {
                        predecessorCount = 1;
                    }
                    else
                    {
                        ++predecessorCount;
                    }
                    m_predecessorCounts[to] = predecessorCount;
                }
            }
        }

        /// <summary>
        /// DESTRUCTIVE OPERATION: performing a sort modifies the graph
        /// Performs topological sort on graph. Nodes with no remaining incoming edges are removed
        /// in sort order (assumes elements implement IComparable(Of TVertex))
        /// </summary>
        /// <returns>true if the sort succeeds; false if it fails and there is a remainder</returns>
        internal bool TryTopologicalSort(out IEnumerable<TVertex> orderedVertices, out IEnumerable<TVertex> remainder)
        {
            // populate all predecessor-less nodes to root queue
            var rootsPriorityQueue = new SortedSet<TVertex>(Comparer<TVertex>.Default);

            foreach (TVertex vertex in m_vertices)
            {
                int predecessorCount;
                if (!m_predecessorCounts.TryGetValue(vertex, out predecessorCount) || 0 == predecessorCount)
                {
                    rootsPriorityQueue.Add(vertex);
                }
            }

            var result = new TVertex[m_vertices.Count];
            int resultCount = 0;

            // perform sort
            while (0 < rootsPriorityQueue.Count)
            {
                // get the vertex that is next in line in the secondary ordering
                TVertex from = rootsPriorityQueue.Min;
                rootsPriorityQueue.Remove(from);

                // remove all outgoing edges (free all vertices that depend on 'from')
                HashSet<TVertex> toSet;
                if (m_successorMap.TryGetValue(from, out toSet))
                {
                    foreach (TVertex to in toSet)
                    {
                        int predecessorCount = m_predecessorCounts[to] - 1;
                        m_predecessorCounts[to] = predecessorCount;
                        if (predecessorCount == 0)
                        {
                            // 'to' contains no incoming edges, so it is now a root
                            rootsPriorityQueue.Add(to);
                        }
                    }

                    // remove the entire successor set since it has been emptied
                    m_successorMap.Remove(from);
                }

                // add the freed vertex to the result and remove it from the graph
                result[resultCount++] = from;
                m_vertices.Remove(from);
            }

            // check that all elements were yielded
            if (m_vertices.Count == 0)
            {
                // all vertices were ordered
                orderedVertices = result;
                remainder = Enumerable.Empty<TVertex>();
                return true;
            }
            else
            {
                orderedVertices = result.Take(resultCount);
                remainder = m_vertices;
                return false;
            }
        }

        /// <summary>
        /// For debugging purposes.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<TVertex, HashSet<TVertex>> outgoingEdge in m_successorMap)
            {
                bool first = true;

                sb.AppendFormat(CultureInfo.InvariantCulture, "[{0}] --> ", outgoingEdge.Key);
            
                foreach (TVertex vertex in outgoingEdge.Value)
                {
                    if (first) { first = false; }
                    else { sb.Append(", "); }
                    sb.AppendFormat(CultureInfo.InvariantCulture, "[{0}]", vertex);
                }

                sb.Append("; ");
            }
            
            return sb.ToString();
        }
        #endregion
    }
}
