/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

// C5 example: Graph representation with basic algorithms using C5 


// Compile with 
//   csc /r:C5.dll Graph.cs 


// The code is structured as a rudimentary Graph library, with an interface
// for (edge)weighted graphs and a single implementation based on an
// adjacency list representation using hash dictionaries. 


// The algorithms implemented include:
//
// Breadth-First-Search and Depth-First-Search, with an interface based on actions
// to be taken as edges are traversed. Applications are checking for connectedness,
// counting components
//
// Priority-First-Search, where edges are traversed according to either weight or 
// accumulated weight from the start of the search.
// An application of the non-accumulating version is the construction of a minimal
// spanning tree and therefore the following approximation algorithm. 
// Applications of the accumulating version are the construction of a shortest path
// and the computation of the distance from one vertex to another one.
//
// An approximation algorithm for Travelling Salesman Problems, 
// where the weights satisfies the triangle inequality. 


// Pervasive generic parameters:
//                      V: The type of a vertex in a graph. Vertices are identified
//                         by the Equals method inherited from object (or overridden).
//                      E: The type of additional data associated with edges in a graph.
//                      W: The type of values of weights on edges in a weighted graph, 
//                         in practise usually int or double. Must be comparable and 
//                         there must be given a compatible way to add values.
           
// Interfaces:
//                      IGraph<V,E,W>: The interface for a graph implementation with
//                         vertex type V, edge dat type E and weight values of type W.           
//                      IWeight<E,W>: The interface of a weight function 

// Classes:
//                      HashGraph<V,E,W>: An implementation of IWeightedGraph<V,E,W> based on 
//                          adjacency lists represented as hash dictionaries.
//                      HashGraph<V,E,W>.EdgesValue: A helper class for the Edges() method
//                      CountWeight<E>: A 
//                      IntWeight:
//                      DoubleWeight:

// Value Types:
//                      Edge<V,E>: 
//                      EdgeAction<V,E,U>:

// Some notes:
// The code only supports "natural" equality of vertices.

using C5;
using System;
using SCG = System.Collections.Generic;

namespace Graph
{
  /// <summary>
  /// (Duer ikke)
  /// </summary>
  /// <typeparam name="V"></typeparam>
  /// <typeparam name="E"></typeparam>
  interface IGraphVertex<V, E,W> where W : IComparable<W>
  {
    V Value { get;}
    IGraph<V, E, W> Graph { get;}
    ICollectionValue<KeyValuePair<V, E>> Adjacent { get;}

  }

  class Vertex<V>
  {
    //V v;
  }
/// <summary>
/// 
/// </summary>
/// <typeparam name="V"></typeparam>
/// <typeparam name="E"></typeparam>
/// <typeparam name="W"></typeparam>
  interface IGraph<V, E, W> where W : IComparable<W>
  {
    /// <summary>
    /// 
    /// </summary>
    /// <value>The weight object for this graph</value>
    IWeight<E, W> Weight { get;}

    /// <summary>
    /// 
    /// </summary>
    /// <value>The number of vertices in this graph</value>
    int VertexCount { get;}

    /// <summary>
    /// 
    /// </summary>
    /// <value>The number of edges in this graph</value>
    int EdgeCount { get;}

    /// <summary>
    /// The collection of vertices of this graph.
    /// The return value is a snapshot, not a live view onto the graph.
    /// </summary>
    /// <returns></returns>
    ICollectionValue<V> Vertices();

    /// <summary>
    /// The collection of edges incident to a particular vertex.
    /// The return value is a snapshot og (endvertex, edgedata) pairs.
    /// </summary>
    /// <param name="vertex"></param>
    /// <returns></returns>
    ICollectionValue<KeyValuePair<V, E>> Adjacent(V vertex);

    /// <summary>
    /// The collection of all edges in the graph. The return value is a snapshot
    /// of Edge values. Each edge is present once for an undefined direction.
    /// </summary>
    /// <returns></returns>
    ICollectionValue<Edge<V, E>> Edges();

    /// <summary>
    /// Add a(n isolated) vertex to the graph Ignore if vertex is already in the graph.
    /// </summary>
    /// <param name="vertex"></param>
    /// <returns>True if the vertex was added.</returns>
    bool AddVertex(V vertex);

    /// <summary>
    /// Add an edge to the graph. If the edge is already in the graph, update the 
    /// edge data. Add vertices as needed.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="edgedata"></param>
    /// <returns>True if the edge was added</returns>
    bool AddEdge(V start, V end, E edgedata);

    /// <summary>
    /// Remove a vertex and all its incident edges from the graph.
    /// </summary>
    /// <returns>True if the vertex was already in the graph and hence was removed</returns>
    bool RemoveVertex(V vertex);

    /// <summary>
    /// Remove an edge from the graph. Do not remove the vertices if they become isolated.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="edgedata">On output, the edge data associated with the removed edge.</param>
    /// <returns>True if </returns>
    bool RemoveEdge(V start, V end, out E edgedata);

    /// <summary>
    /// Is there an edge from start to end in this graph, and if so, what is 
    /// the data on that edge.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="edge"></param>
    /// <returns></returns>
    bool FindEdge(V start, V end, out E edge);

    /// <summary>
    /// Construct the subgraph corresponding to a set of vertices.
    /// </summary>
    /// <param name="vs"></param>
    /// <returns></returns>
    IGraph<V, E, W> SubGraph(ICollectionValue<V> vs);

    /// <summary>
    /// 
    /// </summary>
    /// <value>True if graph is connected</value>
    bool IsConnected { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <value>The number of connected components of this graph</value>
    int ComponentCount { get;}

    /// <summary>
    /// Compute the connnected components of this graph. 
    /// </summary>
    /// <returns>A collection of (vertex,component) pairs, where the first part of the
    /// pair is some vertex in the component.</returns>
    ICollectionValue<KeyValuePair<V, IGraph<V, E, W>>> Components();

    /// <summary>
    /// Traverse the connected component containing the <code>start</code> vertex, 
    /// in either BFS or DFS order, beginning at <code>start</code> and performing the action 
    /// <code>act</code> on each edge part of the constructed tree.
    /// </summary>
    /// <param name="bfs">True if BFS, false if DFS</param>
    /// <param name="start">The vertex to start at</param>
    /// <param name="act">The action to perform at each node</param>
    void TraverseVertices(bool bfs, V start, Action<Edge<V, E>> act);

    /// <summary>
    /// Traverse an undirected graph in either BFS or DFS order, performing the action 
    /// <code>act</code> on each vertex. 
    /// The start vertex of each component of the graph is undefinded. 
    /// </summary>
    /// <param name="bfs">True if BFS, false if DFS</param>
    /// <param name="act"></param>
    void TraverseVertices(bool bfs, Action<V> act);

    /// <summary>
    /// Traverse an undirected graph in either BFS or DFS order, performing the action 
    /// <code>act</code> on each edge in the traversal and beforecomponent/aftercomponent 
    /// at the start and end of each component (with argument: the start vertex of the component).
    /// </summary>
    /// <param name="bfs">True if BFS, false if DFS</param>
    /// <param name="act"></param>
    /// <param name="beforecomponent"></param>
    /// <param name="aftercomponent"></param>
    void TraverseVertices(bool bfs, Action<Edge<V, E>> act, Action<V> beforecomponent, Action<V> aftercomponent);

    /// <summary>
    /// A more advanced Depth First Search traversal.
    /// </summary>
    /// <param name="start">The vertex to start the search at</param>
    /// <param name="beforevertex">Action to perform when a vertex is first encountered.</param>
    /// <param name="aftervertex">Action to perform when all edges out of a vertex has been handles.</param>
    /// <param name="onfollow">Action to perform as an edge is traversed.</param>
    /// <param name="onfollowed">Action to perform when an edge is travesed back.</param>
    /// <param name="onnotfollowed">Action to perform when an edge (a backedge)is seen, but not followed.</param>
    void DepthFirstSearch(V start, Action<V> beforevertex, Action<V> aftervertex,
            Action<Edge<V, E>> onfollow, Action<Edge<V, E>> onfollowed, Action<Edge<V, E>> onnotfollowed);

    //TODO: perhaps we should avoid exporting this?
    /// <summary>
    /// Traverse the part of the graph reachable from start in order of least distance 
    /// from start according to the weight function. Perform act on the edges of the 
    /// traversal as they are recognised.
    /// </summary>
    /// <typeparam name="W"></typeparam>
    /// <param name="weight"></param>
    /// <param name="start"></param>
    /// <param name="act"></param>
    void PriorityFirstTraverse(bool accumulating, V start, EdgeAction<V, E, W> act);

    /// <summary>
    /// Compute the (a) shortest path from start to end. THrow an exception if end cannot be reached rom start.
    /// </summary>
    /// <param name="weight"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    ICollectionValue<Edge<V, E>> ShortestPath(V start, V end);

    /// <summary>
    /// Compute the Distance from start to end, i.e. the total weight of a shortest path from start to end. 
    /// Throw an exception if end cannot be reached rom start.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    W Distance(V start, V end);

    /// <summary>
    /// Compute a minimum spanning tree for the graph.
    /// Throw an exception if this graph is not connected.
    /// </summary>
    /// <param name="root">(The starting point of the PFS, to be removed)</param>
    /// <returns></returns>
    IGraph<V, E, W> MinimumSpanningTree(out V root);

    /// <summary>
    /// Compute a factor 2 approximation to a Minimum Weight
    /// Perfect Matching in a graph using NNs
    /// </summary>
    /// <returns></returns>
    ICollectionValue<Edge<V, E>> ApproximateMWPM();

    /// <summary>
    /// Construct a closed Euler tour of this graph if one exists, i.e. if 
    /// the graph is connected and all vertices have even degrees. Throw an
    /// ArgumentException if no closed Euler tour exists.
    /// </summary>
    /// <returns>A list of vertices in an Euler tour of this graph.</returns>
    IList<V> EulerTour();

    /// <summary>
    /// This is intended for implementations of the very simple factor 2 approximation
    /// algorithms for the travelling salesman problem for Euclidic weight/distance 
    /// functions, i.e. distances that satisfy the triangle inequality. (We do not do 3/2)
    /// </summary>
    /// <returns></returns>
    IDirectedCollectionValue<V> ApproximateTSP();

    /// <summary>
    /// Pretty-print the graph to the console (for debugging purposes).
    /// </summary>
    void Print(System.IO.TextWriter output);
  }

/// <summary>
/// The type of an edge in a graph. An edge is identified by its pair of 
/// vertices. The pair is considered ordered, and so an Edge really describes
/// an edge of the graph in a particular traversal direction.
/// </summary>
/// <typeparam name="V">The type of a vertex.</typeparam>
/// <typeparam name="E">The type of data asociated with edges.</typeparam>
  struct Edge<V, E>
  {
    static SCG.IEqualityComparer<V> vequalityComparer = EqualityComparer<V>.Default;
    public readonly V start, end;
    public readonly E edgedata;
    public Edge(V start, V end, E edgedata)
    {
      if (vequalityComparer.Equals(start, end))
        throw new ArgumentException("Illegal: start and end are equal");
      this.start = start; this.end = end; this.edgedata = edgedata;
    }

    public Edge<V, E> Reverse()
    {
      return new Edge<V, E>(end, start, edgedata);
    }

    public override string ToString()
    {
      return String.Format("(start='{0}', end='{1}', edgedata='{2}')", start, end, edgedata); ;
    }

    public override bool Equals(object obj)
    {
      if (obj is Edge<V, E>)
      {
        Edge<V, E> other = (Edge<V, E>)obj;
        return vequalityComparer.Equals(start, other.start) && vequalityComparer.Equals(end, other.end);
      }
      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      //TODO: should we use xor? Or a random factor?
      return start.GetHashCode() + 148712671 * end.GetHashCode();
    }

    /// <summary>
    /// The unordered equalityComparer compares edges independent of the order of the vertices.
    /// </summary>
    public class UnorderedEqualityComparer : SCG.IEqualityComparer<Edge<V, E>>
    {
      /// <summary>
      /// Check if two edges have the same vertices irrespective of order.
      /// </summary>
      /// <param name="i1"></param>
      /// <param name="i2"></param>
      /// <returns></returns>
      public bool Equals(Edge<V, E> i1, Edge<V, E> i2)
      {
        return (vequalityComparer.Equals(i1.start, i2.start) && vequalityComparer.Equals(i1.end, i2.end)) ||
            (vequalityComparer.Equals(i1.end, i2.start) && vequalityComparer.Equals(i1.start, i2.end));
      }

      /// <summary>
      /// Return a hash code compatible with the unordered equals.
      /// </summary>
      /// <param name="item"></param>
      /// <returns></returns>
      public int GetHashCode(Edge<V, E> item)
      {
        return item.start.GetHashCode() ^ item.end.GetHashCode();
      }
    }
  }

/// <summary>
/// The type of the weight object of a graph. This consists of a function mapping
/// edge data values to weight values, and an operation to add two weight values.
/// It is required that weight values are comparable.
/// 
/// The user must assure that the add operation is commutative and fulfills 
/// Add(w1,w2) &le; w1 for all w1 and w2 that can appear as weights or sums of
/// weights. In practise, W will be int or double, all weight values will be 
/// non-negative and the addition will be the natural addition on W.
/// </summary>
/// <typeparam name="E"></typeparam>
/// <typeparam name="W"></typeparam>
  interface IWeight<E, W> where W : IComparable<W>
  {
    /// <summary>
    /// Compute the weight value corresponding to specific edge data.
    /// </summary>
    /// <param name="edgedata"></param>
    /// <returns></returns>
    W Weight(E edgedata);

    /// <summary>
    /// Add two weight values.
    /// </summary>
    /// <param name="w1"></param>
    /// <param name="w2"></param>
    /// <returns></returns>
    W Add(W w1, W w2);
  }

/// <summary>
/// An action to perform when an edge is encountered during a traversal of the graph.
/// The "extra" parameter is for additional information supplied by the traversal 
/// algorithm. 
/// The intention of the bool return value is that returning false is a signal to the
/// traversal algorithm to abandon the traversl because the user has already found
/// what he was looking for.
/// </summary>
/// <typeparam name="V"></typeparam>
/// <typeparam name="E"></typeparam>
/// <typeparam name="U"></typeparam>
/// <param name="edge"></param>
/// <param name="extra"></param>
/// <returns></returns>
  delegate bool EdgeAction<V, E, U>(Edge<V, E> edge, U extra);


/*
  For a dense graph, we would use data fields:

  E'[,] or E'[][] for the matrix. Possibly E'[][] for a triangular one!
  Here E' = struct{E edgedata, bool present} or class{E edgedata}, or if E is a class just E.
  Thus E' is E! for value types. Or we could have two matrices: E[][] and bool[][]. 

  HashDictionary<V,int> to map vertex ids to indices.
  ArrayList<V> for the map the other way.
  Or simply a HashedArrayList<V> to get both?

  PresentList<int>, FreeList<int> or similar, if we do not want to compact the indices in the matrix on each delete.
  If we compact, we always do a delete on the vertex<->index map by a replace and a removelast:
    vimap[ind]=vimap[vimap.Count]; vimap.RemoveLast(); //also reorder matrix!
  

*/

/// <summary>
/// An implementation of IGraph&le;V,E,W&ge; based on an adjacency list representation using hash dictionaries.
/// As a consequence, this will be most efficient for sparse graphs.
/// </summary>
/// <typeparam name="V"></typeparam>
/// <typeparam name="E"></typeparam>
/// <typeparam name="W"></typeparam>
  class HashGraph<V, E, W> : IGraph<V, E, W> where W : IComparable<W>
  {
    int edgecount;

    HashDictionary<V, HashDictionary<V, E>> graph;

    IWeight<E, W> weight;

    public IWeight<E, W> Weight { get { return weight; } }


    /// <summary>
    /// Create an initially empty graph.
    /// </summary>
    /// <param name="weight"></param>
    [UsedBy("testTSP")]
    public HashGraph(IWeight<E, W> weight)
    {
      this.weight = weight;
      edgecount = 0;
      graph = new HashDictionary<V, HashDictionary<V, E>>();
    }

    /// <summary>
    /// Constructing a graph with no isolated vertices given a collection of edges.
    /// </summary>
    /// <param name="edges"></param>
    [UsedBy()]
    public HashGraph(IWeight<E, W> weight, SCG.IEnumerable<Edge<V, E>> edges) : this(weight)
    {
      foreach (Edge<V, E> edge in edges)
      {
        if (edge.start.Equals(edge.end))
          throw new ApplicationException("Edge has equal start and end");
        {
          HashDictionary<V, E> edgeset;
          //TODO: utilize upcoming FindOrAddSome operation
          if (!graph.Find(edge.start, out edgeset))
            graph.Add(edge.start, edgeset = new HashDictionary<V, E>());
          if (!edgeset.UpdateOrAdd(edge.end, edge.edgedata))
            edgecount++;
          if (!graph.Find(edge.end, out edgeset))
            graph.Add(edge.end, edgeset = new HashDictionary<V, E>());
          edgeset.UpdateOrAdd(edge.start, edge.edgedata);
        }
      }
    }

    /// <summary>
    /// This constructs a graph with a given set of vertices.
    /// Will only allow these vertices.
    /// Duplicate edges are allowed.
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="edges"></param>
    public HashGraph(IWeight<E, W> weight, SCG.IEnumerable<V> vertices, SCG.IEnumerable<Edge<V, E>> edges) : this(weight)
    {
      foreach (V v in vertices)
        graph.Add(v, new HashDictionary<V, E>());
      foreach (Edge<V, E> edge in edges)
      {
        HashDictionary<V, E> edgeset;
        if (edge.start.Equals(edge.end))
          throw new ApplicationException("Edge has equal start and end");
        if (!graph.Find(edge.start, out edgeset))
          throw new ApplicationException("Edge has unknown start");
        if (!edgeset.UpdateOrAdd(edge.end, edge.edgedata))
          edgecount++;
        if (!graph.Find(edge.end, out edgeset))
          throw new ApplicationException("Edge has unknown end");
        edgeset.UpdateOrAdd(edge.start, edge.edgedata);
      }
    }

    [UsedBy("testCOMP")]
    public int VertexCount { get { return graph.Count; } }

    [UsedBy("testCOMP")]
    public int EdgeCount { get { return edgecount; } }

    public ICollectionValue<V> Vertices()
    {
      return new GuardedCollectionValue<V>(graph.Keys);
    }

    public ICollectionValue<KeyValuePair<V, E>> Adjacent(V vertex)
    {
      return new GuardedCollectionValue<KeyValuePair<V, E>>(graph[vertex]);
    }

    class EdgesValue : CollectionValueBase<Edge<V, E>>
    {
      HashGraph<V, E, W> graph;
      internal EdgesValue(HashGraph<V, E, W> g) { graph = g; }

      public override bool IsEmpty { get { return graph.edgecount == 0; } }

      public override int Count { get { return graph.edgecount; } }

      public override Speed CountSpeed { get { return Speed.Constant; } }


      public override Edge<V, E> Choose()
      {
        KeyValuePair<V, HashDictionary<V, E>> adjacent = graph.graph.Choose();
        KeyValuePair<V, E> otherend = graph.graph[adjacent.Key].Choose();
        return new Edge<V, E>(adjacent.Key, otherend.Key, otherend.Value);
      }

      public override SCG.IEnumerator<Edge<V, E>> GetEnumerator()
      {
        HashSet<Edge<V, E>> seen = new HashSet<Edge<V, E>>(new Edge<V, E>.UnorderedEqualityComparer());
        foreach (V v in graph.graph.Keys)
          foreach (KeyValuePair<V, E> p in graph.graph[v])
          {
            Edge<V, E> edge = new Edge<V, E>(v, p.Key, p.Value);
            if (!seen.FindOrAdd(ref edge))
              yield return edge;
          }
      }
    }

    public ICollectionValue<Edge<V, E>> Edges()
    {
      return new EdgesValue(this);
    }

    public bool AddVertex(V v)
    {
      if (graph.Contains(v))
        return false;
      graph.Add(v, new HashDictionary<V, E>());
      return true;
    }

    //Note: no warning on update of edgedata!
    //TODO: Shouldn´t Update or Add return the old value?
    //Then it would be easy to check for updates 
    public bool AddEdge(V start, V end, E edgedata)
    {
      bool retval = false;
      HashDictionary<V, E> edgeset;
      if (graph.Find(start, out edgeset))
        retval = !edgeset.UpdateOrAdd(end, edgedata);
      else
      {
        graph[start] = edgeset = new HashDictionary<V, E>();
        edgeset[end] = edgedata;
        retval = true;
      }
      if (graph.Find(end, out edgeset))
        edgeset.UpdateOrAdd(start, edgedata);
      else
      {
        graph[end] = edgeset = new HashDictionary<V, E>();
        edgeset[start] = edgedata;
      }
      if (retval)
        edgecount++;
      return retval;
    }

    public bool RemoveVertex(V vertex)
    {
      HashDictionary<V, E> edgeset;
      if (!graph.Find(vertex, out edgeset))
        return false;
      foreach (V othervertex in edgeset.Keys)
        graph[othervertex].Remove(vertex); //Assert retval==true
      edgecount -= edgeset.Count;
      graph.Remove(vertex);
      return true;
    }

    public bool RemoveEdge(V start, V end, out E edgedata)
    {
      HashDictionary<V, E> edgeset;
      if (!graph.Find(start, out edgeset))
      {
        edgedata = default(E);
        return false;
      }
      if (!edgeset.Remove(end, out edgedata))
        return false;
      graph[end].Remove(start);
      edgecount--;
      return true;
    }

    public bool FindEdge(V start, V end, out E edgedata)
    {
      HashDictionary<V, E> edges;
      if (!graph.Find(start, out edges))
      {
        edgedata = default(E);
        return false;
      }
      return edges.Find(end, out edgedata);
    }

    public IGraph<V, E, W> SubGraph(ICollectionValue<V> vs)
    {
      HashSet<V> vertexset = vs as HashSet<V>;
      if (vertexset == null)
      {
        vertexset = new HashSet<V>();
        vertexset.AddAll(vs);
      }

      return new HashGraph<V, E, W>(weight,
          vs,
          Edges().Filter(delegate(Edge<V, E> e) { return vertexset.Contains(e.start) && vertexset.Contains(e.end); }));
    }

    public bool IsConnected
    {
      //TODO: optimize: needs to change Action<Edge<V,E>> to EdgeAction to be able to break out
      get { return ComponentCount <= 1; }
    }

    public int ComponentCount
    {
      get
      {
        int components = 0;
        TraverseVertices(false, null, delegate(V v) { components++; }, null);
        return components;
      }
    }

    public ICollectionValue<KeyValuePair<V, IGraph<V, E, W>>> Components()
    {
      ArrayList<KeyValuePair<V, IGraph<V, E, W>>> retval = new ArrayList<KeyValuePair<V, IGraph<V, E, W>>>();
      HashGraph<V, E, W> component;
      ArrayList<V> vertices = null;
      Action<Edge<V, E>> edgeaction = delegate(Edge<V, E> e)
      {
        vertices.Add(e.end);
      };
      Action<V> beforecomponent = delegate(V v)
      {
        vertices = new ArrayList<V>();
        vertices.Add(v);
      };
      Action<V> aftercomponent = delegate(V v)
      {
        //component = SubGraph(vertices);
        component = new HashGraph<V, E, W>(weight);
        foreach (V start in vertices)
        {
          //component.graph[start] = graph[start].Clone();
          HashDictionary<V, E> edgeset = component.graph[start] = new HashDictionary<V, E>();
          foreach (KeyValuePair<V, E> adjacent in graph[start])
            edgeset[adjacent.Key] = adjacent.Value;
        }
        retval.Add(new KeyValuePair<V, IGraph<V, E, W>>(v, component));
      };
      TraverseVertices(false, edgeaction, beforecomponent, aftercomponent);
      return retval;
    }

    [UsedBy("test1")]
    public void TraverseVertices(bool bfs, V start, Action<Edge<V, E>> act)
    {
      if (!graph.Contains(start))
        throw new ArgumentException("start Vertex not in graph");
      IList<Edge<V, E>> todo = new LinkedList<Edge<V, E>>();
      todo.FIFO = bfs;
      HashSet<V> seen = new HashSet<V>();
      V v;
      while (!todo.IsEmpty || seen.Count == 0)
      {
        if (seen.Count > 1)
        {
          Edge<V, E> e = todo.Remove();
          if (act != null)
            act(e);
          v = e.end;
        }
        else
        {
          seen.Add(start);
          v = start;
        }

        HashDictionary<V, E> adjacent;
        if (graph.Find(v, out adjacent))
        {
          foreach (KeyValuePair<V, E> p in adjacent)
          {
            V end = p.Key;
            if (!seen.FindOrAdd(ref end))
              todo.Add(new Edge<V, E>(v, end, p.Value));
          }
        }
      }
    }

    public void TraverseVertices(bool bfs, Action<V> act)
    {
      TraverseVertices(bfs, delegate(Edge<V, E> e) { act(e.end); }, act, null);
    }

    //TODO: merge the hash set here with the intra omponent one?
    public void TraverseVertices(bool bfs, Action<Edge<V, E>> act, Action<V> beforecomponent, Action<V> aftercomponent)
    {
      HashSet<V> missing = new HashSet<V>();
      missing.AddAll(Vertices());
      Action<Edge<V, E>> myact = act + delegate(Edge<V, E> e) { missing.Remove(e.end); };
      Action<V> mybeforecomponent = beforecomponent + delegate(V v) { missing.Remove(v); };
      while (!missing.IsEmpty)
      {
        V start = default(V);
        foreach (V v in missing)
        { start = v; break; }
        mybeforecomponent(start);
        TraverseVertices(bfs, start, myact);
        if (aftercomponent != null)
          aftercomponent(start);
      }
    }

    delegate void Visitor(V v, V parent, bool atRoot);

    //TODO: allow actions to be null
    [UsedBy("testDFS")]
    public void DepthFirstSearch(V start, Action<V> before, Action<V> after,
        Action<Edge<V, E>> onfollow, Action<Edge<V, E>> onfollowed, Action<Edge<V, E>> onnotfollowed)
    {
      HashSet<V> seen = new HashSet<V>();
      seen.Add(start);
      //If we do not first set visit = null, the compiler will complain at visit(end)
      //that visit is uninitialized
      Visitor visit = null;
      visit = delegate(V v, V parent, bool atRoot)
      {
        before(v);
        HashDictionary<V, E> adjacent;
        if (graph.Find(v, out adjacent))
          foreach (KeyValuePair<V, E> p in adjacent)
          {
            V end = p.Key;
            Edge<V, E> e = new Edge<V, E>(v, end, p.Value);
            if (!seen.FindOrAdd(ref end))
            {
              onfollow(e);
              visit(end, v, false);
              onfollowed(e);
            }
            else
            {
              if (!atRoot && !parent.Equals(end))
                onnotfollowed(e);
            }
          }
        after(v);
      };
      visit(start, default(V), true);
    }

    public void PriorityFirstTraverse(bool accumulated, V start, EdgeAction<V, E, W> act)
    {
      if (!graph.Contains(start))
        throw new ArgumentException("Graph does not contain start");
      IPriorityQueue<W> fringe = new IntervalHeap<W>();
      HashDictionary<V, IPriorityQueueHandle<W>> seen = new HashDictionary<V, IPriorityQueueHandle<W>>();
      HashDictionary<IPriorityQueueHandle<W>, Edge<V, E>> bestedge = new HashDictionary<IPriorityQueueHandle<W>, Edge<V, E>>();

      IPriorityQueueHandle<W> h = null;
      V current;
      W currentdist;
      while (!fringe.IsEmpty || seen.Count == 0)
      {
        if (seen.Count == 0)
        {
          seen.Add(start, h);
          current = start;
          currentdist = default(W);
        }
        else
        {
          currentdist = fringe.DeleteMin(out h);
          Edge<V, E> e = bestedge[h];
          if (!act(e, currentdist))
            break;
          bestedge.Remove(h);
          current = e.end;
        }
        HashDictionary<V, E> adjacentnodes;
        if (graph.Find(current, out adjacentnodes))
          foreach (KeyValuePair<V, E> adjacent in adjacentnodes)
          {
            V end = adjacent.Key;
            E edgedata = adjacent.Value;
            W dist = weight.Weight(edgedata), olddist;
            if (accumulated && !current.Equals(start)) dist = weight.Add(currentdist, weight.Weight(edgedata));
            if (!seen.Find(end, out h))
            {
              h = null;
              fringe.Add(ref h, dist);
              seen[end] = h;
              bestedge[h] = new Edge<V, E>(current, end, edgedata);
            }
            else if (fringe.Find(h, out olddist) && dist.CompareTo(olddist) < 0)
            {
              fringe[h] = dist;
              bestedge[h] = new Edge<V, E>(current, end, edgedata);
            }
          }
      }
    }

    public W Distance(V start, V end)
    {
      W dist = default(W);
      bool found = false;
      PriorityFirstTraverse(true, start, delegate(Edge<V, E> e, W w)
      {
        if (end.Equals(e.end)) { dist = w; found = true; return false; }
        else return true;
      });
      if (found)
        return dist;
      throw new ArgumentException(String.Format("No path from {0} to {1}", start, end));
    }


    public ICollectionValue<Edge<V, E>> ShortestPath(V start, V end)
    {
      HashDictionary<V, Edge<V, E>> backtrack = new HashDictionary<V, Edge<V, E>>();
      PriorityFirstTraverse(true, start, delegate(Edge<V, E> e, W w) { backtrack[e.end] = e; return !end.Equals(e.end); });
      ArrayList<Edge<V, E>> path = new ArrayList<Edge<V, E>>();
      Edge<V, E> edge;
      V v = end;
      while (backtrack.Find(v, out edge))
      {
        path.Add(edge);
        v = edge.start;
      }
      if (path.IsEmpty)
        throw new ArgumentException(String.Format("No path from {0} to {1}", start, end));
      path.Reverse();
      return path;
    }

    /// <summary>
    /// NB: assume connected, throw exception if not
    /// </summary>
    /// <typeparam name="W"></typeparam>
    /// <param name="edgeWeight"></param>
    /// <returns></returns>
    public IGraph<V, E, W> MinimumSpanningTree(out V start)
    {
      ArrayList<Edge<V, E>> edges = new ArrayList<Edge<V, E>>();
      start = default(V);
      foreach (V v in graph.Keys)
      { start = v; break; }
      PriorityFirstTraverse(false, start, delegate(Edge<V, E> e, W w) { edges.Add(e); return true; });
      if (edges.Count != graph.Count - 1)
        throw new ArgumentException("Graph not connected");
      return new HashGraph<V, E, W>(weight, edges);
    }

    public ICollectionValue<Edge<V, E>> ApproximateMWPM()
    {
      //Assume graph complete and even number of vertices
      HashGraph<V, E, W> clone = new HashGraph<V, E, W>(weight, Edges());
      ArrayList<Edge<V, E>> evenpath = new ArrayList<Edge<V, E>>();
      ArrayList<Edge<V, E>> oddpath = new ArrayList<Edge<V, E>>();
      V start = default(V);
      foreach (V v in clone.Vertices()) { start = v; break; }
      V current = start;
      W evenweight, oddweight;
      evenweight = oddweight = default(W);
      bool even = true;
      while (clone.VertexCount > 0)
      {
        V bestvertex = default(V);
        E bestedge = default(E);
        W bestweight = default(W);
        if (clone.VertexCount == 1)
        {
          bestvertex = start;
          bestedge = graph[current][start];
          bestweight = weight.Weight(bestedge);
        }
        else
        {
          bool first = true;
          foreach (KeyValuePair<V, E> p in clone.graph[current])
          {
            W thisweight = weight.Weight(p.Value);
            if (first || bestweight.CompareTo(thisweight) > 0)
            {
              bestvertex = p.Key;
              bestweight = thisweight;
              bestedge = p.Value;
            }
            first = false;
          }
        }
        clone.RemoveVertex(current);
        //Console.WriteLine("-* {0} / {1} / {2}", bestvertex, bestweight, tour.Count);
        if (even)
        {
          evenweight = evenpath.Count < 1 ? bestweight : weight.Add(evenweight, bestweight);
          evenpath.Add(new Edge<V, E>(current, bestvertex, bestedge));
        }
        else
        {
          oddweight = oddpath.Count < 1 ? bestweight : weight.Add(oddweight, bestweight);
          oddpath.Add(new Edge<V, E>(current, bestvertex, bestedge));
        }
        current = bestvertex;
        even = !even;
      }
      //Console.WriteLine("Totalweights: even: {0} and odd: {1}", evenweight, oddweight);
      return evenweight.CompareTo(oddweight) < 0 ? evenpath : oddpath;
    }

    /// <summary>
    /// The construction is performed as follows: 
    /// Start at some vertex. Greedily construct a path starting there by 
    /// following edges at random until no more unused edges are available
    /// from the current node, which must be the start node. Then follow 
    /// the path constructed so far and whenever we meet a vertex with 
    /// unused edges, construct a path from there greedily as above, 
    /// inserting into the path in front of us.
    /// 
    /// The algorithm will use constant time for each vertex added 
    /// to the path and 
    /// 
    /// Illustrates use of views as a safe version of listnode pointers 
    /// and hashed linked lists for choosing some item in constant time combined 
    /// with (expected) constant time remove.
    /// </summary>
    /// <returns></returns>
    public IList<V> EulerTour()
    {
      bool debug = false;
      //Assert connected and all degrees even. (Connected is checked at the end)
      string fmt = "Graph does not have a closed Euler tour: vertex {0} has odd degree {1}";
      foreach (KeyValuePair<V, HashDictionary<V, E>> adj in graph)
        if (adj.Value.Count % 2 != 0)
          throw new ArgumentException(String.Format(fmt, adj.Key, adj.Value.Count));

      LinkedList<V> path = new LinkedList<V>();
      //Clone the graph data to keep track of used edges.
      HashDictionary<V, HashedArrayList<V>> edges = new HashDictionary<V, HashedArrayList<V>>();
      V start = default(V);
      HashedArrayList<V> adjacent = null;
      foreach (KeyValuePair<V, HashDictionary<V, E>> p in graph)
      {
        adjacent = new HashedArrayList<V>();
        adjacent.AddAll(p.Value.Keys);
        start = p.Key;
        edges.Add(start, adjacent);
      }

      path.Add(start);
      IList<V> view = path.View(0, 1);
      while (adjacent.Count > 0)
      {
        V current = view[0];
        if (debug) Console.WriteLine("==> {0}", current);
        //Augment the path (randomly) until we return here and all edges
        while (adjacent.Count > 0)
        {
          if (debug) Console.WriteLine(" => {0}, {1}", current, path.Count);
          V next = adjacent.RemoveFirst();
          view.Add(next);
          if (debug) Console.WriteLine("EDGE: " + current + "->" + next);
          if (!edges[next].Remove(current))
            Console.WriteLine("Bad");
          current = next;
          adjacent = edges[current];
        }
        //When we get here, the view contains a closed path, i.e.
        //view[view.Count-1] == view[0] and we have followed all edges from view[0]
        //We slide forward along the rest of the path constructed so far and stop at the
        //first vertex with still unfollwed edges.
        while (view.Offset < path.Count - 1 && adjacent.Count == 0)
        {
          view.Slide(1, 1);
          if (debug) Console.WriteLine(" -> {0}, {1}", view[0], path.Count);
          adjacent = edges[view[0]];
        }
      }
      if (path.Count <= edges.Count)
        throw new ArgumentException("Graph was not connected");
      return path;
    }

    /// <summary>
    /// The purpose of this struct is to be able to create and add new,
    /// synthetic vertices to a graph. 
    /// </summary>
    struct Vplus : IEquatable<Vplus>
    {
      internal readonly V vertex; internal readonly int id;
      static SCG.IEqualityComparer<V> vequalityComparer = EqualityComparer<V>.Default;
      internal Vplus(V v) { vertex = v; id = 0; }
      internal Vplus(int i) { vertex = default(V); id = i; }
      //We should override Equals and GetHashCode

      public override string ToString()
      { return id == 0 ? String.Format("real({0})", vertex) : String.Format("fake({0})", id); }

      public override bool Equals(object obj) { throw new NotImplementedException(); }

      public bool Equals(Vplus other) { return vequalityComparer.Equals(vertex, other.vertex) && id == other.id; }

      public override int GetHashCode() { return vequalityComparer.GetHashCode(vertex) + id; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IDirectedCollectionValue<V> ApproximateTSP2()
    {
      /* Construct a minimum spanning tree for the graph */
      V root;
      IGraph<V, E, W> tree = MinimumSpanningTree(out root);

      //Console.WriteLine("========= Matching of odd vertices of mst =========");
      ArrayList<V> oddvertices = new ArrayList<V>();
      foreach (V v in tree.Vertices())
        if (tree.Adjacent(v).Count % 2 != 0)
          oddvertices.Add(v);

      ICollectionValue<Edge<V, E>> matching = SubGraph(oddvertices).ApproximateMWPM();

      //Console.WriteLine("========= Fuse matching and tree =========");
      //We must split the edges of the matching with fake temporary vertices
      //since the matching and the tree may have common edges

      HashGraph<Vplus, E, W> fused = new HashGraph<Vplus, E, W>(weight);
      foreach (Edge<V, E> e in tree.Edges())
        fused.AddEdge(new Vplus(e.start), new Vplus(e.end), e.edgedata);
      int fakeid = 1;
      foreach (Edge<V, E> e in matching)
      {
        Vplus fakevertex = new Vplus(fakeid++);
        fused.AddEdge(new Vplus(e.start), fakevertex, e.edgedata);
        fused.AddEdge(fakevertex, new Vplus(e.end), e.edgedata);
      }
      fused.Print(Console.Out);

      //Console.WriteLine("========= Remove fake vertices and perform shortcuts =========");
      IList<Vplus> fusedtour = fused.EulerTour();
      HashSet<V> seen = new HashSet<V>();
      IList<V> tour = new ArrayList<V>();

      foreach (Vplus vplus in fused.EulerTour())
      {
        V v = vplus.vertex;
        if (vplus.id == 0 && !seen.FindOrAdd(ref v))
          tour.Add(v);
      }
      return tour;
    }

    /// <summary>
    /// (Refer to the litterature, Vazirani)
    /// 
    /// (Describe: MST+Euler+shortcuts)
    /// </summary>
    /// <returns></returns>
    [UsedBy("testTSP")]
    public IDirectedCollectionValue<V> ApproximateTSP()
    {
      /* Construct a minimum spanning tree for the graph */
      V root;
      IGraph<V, E, W> tree = MinimumSpanningTree(out root);

      /* (Virtually) double all edges of MST and construct an Euler tour of the vertices*/
      LinkedList<V> tour = new LinkedList<V>();
      tour.Add(root);
      tour.Add(root);
      IList<V> view = tour.View(1, 1);

      Action<Edge<V, E>> onfollow = delegate(Edge<V, E> e)
      {
        //slide the view until it points to the last copy of e.start
        while (!view[0].Equals(e.start))
          view.Slide(1);
        //then insert two copies of e.end and slide the view one backwards
        view.InsertFirst(e.end);
        view.InsertFirst(e.end);
        view.Slide(1, 1);
      };

      tree.TraverseVertices(false, root, onfollow);

      /* Finally, slide along the Euler tour and shortcut by removing vertices already seen*/
      HashSet<V> seen = new HashSet<V>();
      view = tour.View(0, tour.Count);
      while (view.Offset < tour.Count - 1)
      {
        V v = view[0];
        if (seen.FindOrAdd(ref v))
          view.RemoveFirst();
        else
          view.Slide(1, view.Count - 1);
      }
      return tour;
    }

    public void Print(System.IO.TextWriter output)
    {
      output.WriteLine("Graph has {0} vertices, {1} edges, {2} components", graph.Count, edgecount, ComponentCount);
      foreach (KeyValuePair<V, HashDictionary<V, E>> p in graph)
      {
        output.Write(" {0} ->  ", p.Key);
        foreach (KeyValuePair<V, E> p2 in p.Value)
          output.Write("{1} (data {2}), ", p.Key, p2.Key, p2.Value);
        output.WriteLine();
      }
    }
  }

/// <summary>
/// A weight on a graph that assigns the weight 1 to every edge.
/// </summary>
/// <typeparam name="E">(Ignored) type of edgedata</typeparam>
  class CountWeight<E> : IWeight<E, int>
  {
    public int Weight(E edgedata) { return 1; }

    public int Add(int w1, int w2) { return w1 + w2; }
  }

/// <summary>
/// A weight on an IGraph&lt;V,int&gt; that uses the value of the edgedata as the weight value.
/// </summary>
  class IntWeight : IWeight<int, int>
  {

    public int Weight(int edgedata) { return edgedata; }

    public int Add(int w1, int w2) { return w1 + w2; }

  }

/// <summary>
/// A weight on an IGraph&lt;V,double&gt; that uses the value of the edgedata as the weight value.
/// </summary>
  class DoubleWeight : IWeight<double, double>
  {

    public double Weight(double edgedata) { return edgedata; }

    public double Add(double w1, double w2) { return w1 + w2; }

  }

/// <summary>
/// Attribute used for marking which examples use a particuler graph method
/// </summary>
  class UsedByAttribute : Attribute
  {
    string[] tests;
    internal UsedByAttribute(params string[] tests) { this.tests = tests; }

    public override string ToString()
    {
      System.Text.StringBuilder sb = new System.Text.StringBuilder();
      for (int i = 0; i < tests.Length; i++)
      {
        if (i > 0)
          sb.Append(", ");
        sb.Append(tests[i]);
      }
      return sb.ToString();
    }
  }

/// <summary>
/// Attribute for marking example methods with a description
/// </summary>
  class ExampleDescriptionAttribute : Attribute
  {
    string text;
    internal ExampleDescriptionAttribute(string text) { this.text = text; }

    public override string ToString() { return text; }
  }

/// <summary>
/// A selection of test cases
/// </summary>
  class Test
  {
    static SCG.IEnumerable<Edge<int, int>> Grid(int n)
    {
      Random ran = new Random(1717);
      for (int i = 1; i <= n; i++)
      {
        for (int j = 1; j <= n; j++)
        {
          yield return new Edge<int, int>(1000 * i + j, 1000 * (i + 1) + j, ran.Next(1, 100));
          yield return new Edge<int, int>(1000 * i + j, 1000 * i + j + 1, ran.Next(1, 100));
        }
      }
    }

    static SCG.IEnumerable<Edge<string, int>> Snake(int n)
    {
      for (int i = 1; i <= n; i++)
      {
        yield return new Edge<string, int>("U" + i, "L" + i, 1);
        yield return new Edge<string, int>("U" + i, "U" + (i + 1), i % 2 == 0 ? 1 : 10);
        yield return new Edge<string, int>("L" + i, "L" + (i + 1), i % 2 == 0 ? 10 : 1);
      }
    }

    /// <summary>
    /// Create the edges of a forest of complete binary trees.
    /// </summary>
    /// <param name="treeCount">Number of trees</param>
    /// <param name="height">Height of trees</param>
    /// <returns></returns>
    static SCG.IEnumerable<Edge<string, int>> Forest(int treeCount, int height)
    {
      for (int i = 0; i < treeCount; i++)
      {
        IList<string> q = new ArrayList<string>();
        string root = String.Format("[{0}]", i);
        int lmax = height + root.Length;
        q.Add(root);
        while (!q.IsEmpty)
        {
          string s = q.Remove();
          string s2 = s + "L";
          if (s2.Length < lmax)
            q.Add(s2);
          yield return new Edge<string, int>(s, s2, 0);
          s2 = s + "R";
          if (s2.Length < lmax)
            q.Add(s2);
          yield return new Edge<string, int>(s, s2, 0);
        }
      }
    }

    /// <summary>
    /// Create edges of a graph correspondingto a "wheel" shape: the ecnter and equidistant 
    /// points around the perimeter. The edgedata and edge weights are the euclidean distances.
    /// </summary>
    /// <param name="complete">True means the graph will be complete, false that the graph
    /// will have edges for the spokes and between neighboring perimeter vetices.</param>
    /// <param name="n">The number of perimeter vertices, must be at least 3.</param>
    /// <returns>An enumerable with the edges</returns>
    static SCG.IEnumerable<Edge<string, double>> Wheel(bool complete, int n)
    {
      if (n < 3)
        throw new ArgumentOutOfRangeException("n must be at least 3");
      string center = "C";
      string[] perimeter = new string[n];
      for (int i = 0; i < n; i++)
      {
        perimeter[i] = "P" + i;
        yield return new Edge<string, double>(perimeter[i], center, 1);
      }
      if (complete)
        for (int i = 0; i < n - 1; i++)
          for (int j = i + 1; j < n; j++)
            yield return new Edge<string, double>(perimeter[i], perimeter[j], 2 * Math.Sin((j - i) * Math.PI / n));
      else
      {
        for (int i = 0; i < n - 1; i++)
          yield return new Edge<string, double>(perimeter[i], perimeter[i + 1], 2 * Math.Sin(Math.PI / n));
        yield return new Edge<string, double>(perimeter[n - 1], perimeter[0], 2 * Math.Sin(Math.PI / n));
      }
    }


    /// <summary>
    /// []
    /// </summary>
    [ExampleDescription("Basic BFS and DFS using TraverseVertices method")]
    static void test1()
    {
      IGraph<int, int, int> g = new HashGraph<int, int, int>(new CountWeight<int>(), Grid(3));
      Console.WriteLine("Edge count: {0}", g.Edges().Count);
      Console.WriteLine("BFS:");
      g.TraverseVertices(true, 1001, delegate(Edge<int, int> e) { Console.WriteLine(e); });
      Console.WriteLine("DFS:");
      g.TraverseVertices(false, 1001, delegate(Edge<int, int> e) { Console.WriteLine(e); });
    }

    /// <summary>
    /// 
    /// </summary>
    [ExampleDescription("Component methods")]
    static void testCOMP()
    {
      IGraph<string, int, int> g = new HashGraph<string, int, int>(new CountWeight<int>(), Forest(2, 2));
      Console.WriteLine("Forest has: Vertices: {0}, Edges: {1}, Components: {2}", g.VertexCount, g.EdgeCount, g.ComponentCount);
      //g.Print(Console.Out);
      foreach (KeyValuePair<string, IGraph<string, int, int>> comp in g.Components())
      {
        Console.WriteLine("Component of {0}:", comp.Key);
        comp.Value.Print(Console.Out);
      }
    }

    //TODO: remove?
    static void test3()
    {
      HashGraph<int, int, int> g = new HashGraph<int, int, int>(new CountWeight<int>(), Grid(5));
      g.Print(Console.Out);
      //EdgeWeight<int, IntWeight> weight = delegate(int i) { return i; };
      Console.WriteLine("========= PFS accum =========");
      g.PriorityFirstTraverse(
          true,
          2002,
          delegate(Edge<int, int> e, int d) { Console.WriteLine("Edge: {0}, at distance {1}", e, d); return true; });
      Console.WriteLine("========= PFS not accum =========");
      g.PriorityFirstTraverse(
          false,
          2002,
          delegate(Edge<int, int> e, int d) { Console.WriteLine("Edge: {0}, at distance {1}", e, d); return true; });
      Console.WriteLine("========= MST =========");
      int root;
      g.MinimumSpanningTree(out root).Print(Console.Out);
      Console.WriteLine("========= SP =========");
      foreach (Edge<int, int> edge in g.ShortestPath(1001, 5005))
        Console.WriteLine(edge);
    }

    static void test4()
    {
      IGraph<string, int, int> g = new HashGraph<string, int, int>(new IntWeight(), Snake(5));
      Console.WriteLine("Edge count: {0}", g.Edges().Count);
      Console.WriteLine("========= PFS =========");
      g.PriorityFirstTraverse(false,
          "U3",
          delegate(Edge<string, int> e, int d) { Console.WriteLine("Edge: {0}, at distance {1}", e, d); return true; });
      Console.WriteLine("========= MST =========");
      string root;
      IGraph<string, int, int> mst = g.MinimumSpanningTree(out root);
      mst.Print(Console.Out);
      Console.WriteLine("DFS:");
      mst.TraverseVertices(false, root, delegate(Edge<string, int> e) { Console.WriteLine(e); });
      Console.WriteLine("ATSP:");
      int first = 0;
      foreach (string s in g.ApproximateTSP())
      {
        Console.Write((first++ == 0 ? "" : " -> ") + s);
      }
    }

    /// <summary>
    /// This example examines the two variants of a priority-first search:
    ///  with accumulated weights, leading to shortest paths from start;
    ///  with non-acumulated weights, leading to a minimum spanning tree.
    /// </summary>
    [ExampleDescription("Priority-first-search with and without accumulated weights")]
    static void testPFS()
    {
      IGraph<string, double, double> g = new HashGraph<string, double, double>(new DoubleWeight(), Wheel(false, 10));
      g.Print(Console.Out);
      Console.WriteLine("========= PFS non-accumulated weights (-> MST) =========");
      g.PriorityFirstTraverse(false,
          "P0",
          delegate(Edge<string, double> e, double d) { Console.WriteLine("Edge: {0}, at distance {1}", e, d); return true; });
      Console.WriteLine("========= PFS accumulated weights (-> Shortst paths from start) =========");
      g.PriorityFirstTraverse(true,
          "P0",
          delegate(Edge<string, double> e, double d) { Console.WriteLine("Edge: {0}, at distance {1}", e, d); return true; });
    }

    /// <summary>
    /// 
    /// 
    /// (Refer to Vazirani, or do that where ApproximateTSP is implemented)
    /// 
    /// Note that the tour created is not optimal. [Describe why]
    /// </summary>
    [ExampleDescription("Approximate TSP")]
    static void testTSP()
    {
      IGraph<string, double, double> g = new HashGraph<string, double, double>(new DoubleWeight(), Wheel(true, 10));
      //g.Print(Console.Out);
      Console.WriteLine("========= MST =========");
      string root;
      IGraph<string, double, double> mst = g.MinimumSpanningTree(out root);
      mst.TraverseVertices(false,
         root,
         delegate(Edge<string, double> e) { Console.WriteLine("Edge: {0} -> {1}", e.start, e.end); });
      Console.WriteLine("========= Approximate TSP =========");
      int first = 0;
      foreach (string s in g.ApproximateTSP())
      {
        Console.Write((first++ == 0 ? "" : " -> ") + s);
      }
    }

    /// <summary>
    /// 
    /// 
    /// (Refer to Vazirani, or do that where ApproximateTSP is implemented)
    /// 
    /// Note that the tour created is not optimal. [Describe why]
    /// </summary>
    [ExampleDescription("Approximate TSP2")]
    static void testTSP2()
    {
      HashGraph<string, double, double> g = new HashGraph<string, double, double>(new DoubleWeight(), Wheel(true, 20));

      foreach (string s in g.ApproximateTSP2())
        Console.WriteLine("# " + s);
      //g.Print(Console.Out);
      /*
        Console.WriteLine("========= MST =========");
        string root;
        IGraph<string, double, double> mst = g.MinimumSpanningTree(out root);
        mst.TraverseVertices(false,
           root,
           delegate(Edge<string, double> e) { Console.WriteLine("Edge: {0} -> {1}", e.start, e.end); });
        ArrayList<string> oddvertices = new ArrayList<string>();
        foreach (string v in mst.Vertices())
            if (mst.Adjacent(v).Count % 2 != 0)
                oddvertices.Add(v);

        Console.WriteLine("========= Matching of odd vertices of mst =========");
        ICollectionValue<Edge<string, double>> matching = g.SubGraph(oddvertices).ApproximateMWPM();

        Console.WriteLine("========= Add matching to mst =========");
        //We must split the edges of the matchin with fake temporary vertices
        //(For a general vertex type, we would have to augment it to Pair<V,int> 
        int fake = 0;
        foreach (Edge<string, double> e in matching)
        {
            string fakevertex = "_" + (fake++);
            mst.AddEdge(e.start, fakevertex, 0);
            mst.AddEdge(fakevertex, e.end, e.edgedata);
        }
        //mst.Print(Console.Out);

        IList<string> tour = mst.EulerTour(), view = tour.View(1, tour.Count - 1);

        //Remove fake vertices
         while (view.Count > 0)
            if (view[0].StartsWith("_"))
                view.RemoveFirst();
            else
                view.Slide(1, view.Count - 1);

        Console.WriteLine("========= Approximate TSP 2 =========");
        //Short cut
        view = tour.View(1, tour.Count - 1);
        HashSet<string> seen = new HashSet<string>();

        while (view.Count > 0)
        {
            string s = view[0];
            if (seen.FindOrAdd(ref s))
                view.RemoveFirst();
            else
                view.Slide(1, view.Count - 1);
        }

        foreach (string s in tour)
            Console.WriteLine(". " + s);*/
    }

    /// <summary>
    /// 
    /// </summary>
    static void testEuler()
    {
      HashGraph<string, double, double> g = new HashGraph<string, double, double>(new DoubleWeight(), Wheel(true, 6));
      foreach (string s in g.EulerTour())
        Console.Write(s + " ");
      Console.WriteLine();
    }

    /// <summary>
    /// An articulation point in a graph is a vertex, whose removal will 
    /// disconnect the graph (or its component). This example uses the 
    /// extended DepthFirstSearch method to compute articulation points
    /// of a graph.
    /// 
    /// (Refer to Sedgewick. )
    /// 
    /// Each vertex is given an index in traversal order. 
    /// For each vertex, we compute the least index reachable by going downwards
    /// in the DFS tree and then following a single non-dfs edge. 
    /// 
    /// Since we cannot store the DFS indices in the vertices without changing the 
    /// (assumed given) vertex type, V, we remember the indices in a V-&gt;int 
    /// hash dictionary, index. The "least reachable" values are then stored in an 
    /// array keyed by the index.
    /// 
    /// The root of the search is an articulation point if it has more than one
    /// outgoing DFS edges. Other articulation points are non-root vertices, va,
    /// with an outgoing DFS edge, where the the least reachable index of the other 
    /// vertex is greater or equal to the index of va. 
    /// </summary>
    [ExampleDescription("Using the advanced DFS to compute articulation points")]
    static void testDFS()
    {
      HashGraph<string, int, int> g = new HashGraph<string, int, int>(new IntWeight());
      g.AddEdge("A", "B", 0);
      g.AddEdge("A", "E", 0);
      g.AddEdge("B", "E", 0);
      g.AddEdge("B", "C", 0);
      g.AddEdge("B", "H", 0);
      g.AddEdge("H", "I", 0);
      g.AddEdge("B", "D", 0);
      g.AddEdge("C", "D", 0);
      g.AddEdge("C", "F", 0);
      g.AddEdge("C", "G", 0);
      g.AddEdge("F", "G", 0);

      HashDictionary<string, int> index = new HashDictionary<string, int>();
      int[] leastIndexReachableFrom = new int[g.VertexCount];
      int nextindex = 0;
      int outgoingFromRoot = 0;
      Action<string> beforevertex = delegate(string v)
      {
        int i = (index[v] = nextindex++);
        leastIndexReachableFrom[i] = i;
      };
      Action<string> aftervertex = delegate(string v)
      {
        int i = index[v];
        if (i == 0 && outgoingFromRoot > 1)
          Console.WriteLine("Articulation point: {0} ({1}>1 outgoing DFS edges from start)",
              v, outgoingFromRoot);
      };
      Action<Edge<string, int>> onfollow = delegate(Edge<string, int> e)
      {
      };
      Action<Edge<string, int>> onfollowed = delegate(Edge<string, int> e)
      {
        int startind = index[e.start], endind = index[e.end];
        if (startind == 0)
          outgoingFromRoot++;
        else
        {
          int leastIndexReachable = leastIndexReachableFrom[endind];
          if (leastIndexReachable >= startind)
            Console.WriteLine("Articulation point: {0} (least index reachable via {3} is {1} >= this index {2})",
                e.start, leastIndexReachable, startind, e);
          if (leastIndexReachableFrom[startind] > leastIndexReachable)
            leastIndexReachableFrom[startind] = leastIndexReachable;
        }
      };
      Action<Edge<string, int>> onnotfollowed = delegate(Edge<string, int> e)
      {
        int startind = index[e.start], endind = index[e.end];
        if (leastIndexReachableFrom[startind] > endind)
          leastIndexReachableFrom[startind] = endind;
      };

      string root = "C";
      g.DepthFirstSearch(root, beforevertex, aftervertex, onfollow, onfollowed, onnotfollowed);
      Console.WriteLine("Edges:");
      foreach (Edge<string, int> e in g.Edges())
        Console.WriteLine("/ {0}", e);
    }

    static void Main(String[] args)
    {
      if (args.Length != 1)
      {
        Console.WriteLine("Usage: Graph.exe testno");
        System.Reflection.MethodInfo[] mis = typeof(Test).GetMethods(
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        foreach (System.Reflection.MethodInfo mi in mis)
        {
          if (mi.GetParameters().Length == 0 && mi.ReturnType == typeof(void) && mi.Name.StartsWith("test"))
          {
            object[] attrs = mi.GetCustomAttributes(typeof(ExampleDescriptionAttribute), false);
            Console.WriteLine(" {0} : {1}", mi.Name.Substring(4), attrs.Length > 0 ? attrs[0] : "");
          }
        }
      }
      else
      {
        string testMethodName = String.Format("test{0}", args[0]);

        System.Reflection.MethodInfo mi = typeof(Test).GetMethod(testMethodName,
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        if (mi == null)
          Console.WriteLine("No such testmethod, {0}", testMethodName);
        else
        {
          object[] attrs = mi.GetCustomAttributes(typeof(ExampleDescriptionAttribute), false);
          Console.WriteLine("============================== {0}() ==================================", testMethodName);
          Console.WriteLine("Description: {0}", attrs.Length > 0 ? attrs[0] : "None");
          Console.WriteLine("===========================================================================");
          mi.Invoke(null, null);
        }
      }
    }
  }
}
