//---------------------------------------------------------------------
// <copyright file="UndirectedGraph.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Common.Utils;
using System.Collections.Generic;
using System.Text;
namespace System.Data.Mapping.Update.Internal {
    // Maintains a graph where the direction of the edges is not important
    class UndirectedGraph<TVertex> : InternalBase {

        #region Constructor
        internal UndirectedGraph(IEqualityComparer<TVertex> comparer) {
            m_graph = new Graph<TVertex>(comparer);
            m_comparer = comparer;
        }
        #endregion

        #region Fields
        private Graph<TVertex> m_graph; // Directed graph where we added both edges
        private IEqualityComparer<TVertex> m_comparer;
        #endregion

        #region Properties
        internal IEnumerable<TVertex> Vertices {
            get { return m_graph.Vertices; }
        }

        /// <summary>
        /// Returns the edges of the graph
        /// </summary>
        internal IEnumerable<KeyValuePair<TVertex, TVertex>> Edges {
            get {
                return m_graph.Edges;
            }
        }
        #endregion

        #region Methods
        // effects: Adds a new node to the graph. Does nothing if the vertex already exists.
        internal void AddVertex(TVertex vertex) {
            m_graph.AddVertex(vertex);
        }

        // requires: first and second must exist. An edge between first and
        // second must not already exist
        // effects: Adds a new unidirectional edge to the graph. 
        internal void AddEdge(TVertex first, TVertex second) {
            m_graph.AddEdge(first, second);
            m_graph.AddEdge(second, first);
        }


        // effects: Given a graph of T, returns a map such that nodes in the
        // same connected component are in the same list in the KeyToListMap
        internal KeyToListMap<int, TVertex> GenerateConnectedComponents() {
            int count = 0;
            // Set the "component number" for each node
            Dictionary<TVertex, ComponentNum> componentMap = new Dictionary<TVertex, ComponentNum>(m_comparer);
            foreach (TVertex vertex in Vertices) {
                componentMap.Add(vertex, new ComponentNum(count));
                count++;
            }

            // Run the connected components algorithm (Page 441 of the CLR -- Cormen, Rivest, Lieserson)
            foreach (KeyValuePair<TVertex, TVertex> edge in Edges) {
                if (componentMap[edge.Key].componentNum != componentMap[edge.Value].componentNum) {
                    // Set the component numbers of both of the nodes to be the same
                    int oldValue = componentMap[edge.Value].componentNum;
                    int newValue = componentMap[edge.Key].componentNum;
                    componentMap[edge.Value].componentNum = newValue;
                    // Since we are resetting edge.Value's component number, find all components whose value
                    // is oldValue and reset it to the new value
                    foreach (TVertex vertex in componentMap.Keys) {
                        if (componentMap[vertex].componentNum == oldValue) {
                            componentMap[vertex].componentNum = newValue;
                        }
                    }
                }
            }

            // Now just grab the vertices which have the same set numbers
            KeyToListMap<int, TVertex> result = new KeyToListMap<int, TVertex>(EqualityComparer<int>.Default);
            foreach (TVertex vertex in Vertices) {
                int componentNum = componentMap[vertex].componentNum;
                result.Add(componentNum, vertex);
            }
            return result;
        }




        internal override void ToCompactString(StringBuilder builder) {
            builder.Append(m_graph.ToString());
        }


        // A class just for ensuring that we do not modify the hash table
        // while iterating over it. Keeps track of the component number for a
        // connected component
        private class ComponentNum {
            internal ComponentNum(int compNum) {
                componentNum = compNum;
            }
            internal int componentNum;
            public override string ToString() {
                return StringUtil.FormatInvariant("{0}", componentNum);
            }

        };
        #endregion
    }
}
