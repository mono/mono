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

using System;
using System.Diagnostics;
using C5;
using SCG = System.Collections.Generic;

namespace PointLocation
{
  //public enum Site { Cell,Edge,Outside}
  /// <summary>
  /// A line segment with associated data of type T for the cell 
  /// to its right respectively left.
  /// </summary>
  public struct Edge<T>
    {
      public double xs, ys, xe, ye;
      
      public T right, left;
      
      public Edge(double xs, double ys, double xe, double ye, T right, T left)
	{
	  this.xs = xs;
	  this.ys = ys;
	  this.xe = xe;
	  this.ye = ye;
	  this.right = right;
	  this.left = left;
	}
      
      
      public T Cell(bool upper)
	{
	  return (DoubleComparer.StaticCompare(xs, xe) < 0) == upper ? left : right;
	}
      
      
      public override string ToString()
	{
	  return String.Format("[({0:G5};{1:G5})->({2:G5};{3:G5})/R:{4} L:{5}]", xs, ys, xe, ye, right, left);
	}
    }
  
  
  
  /// <summary>
  /// A data structure for point location in a plane divided into
  /// cells by edges. This is the classical use of persistent trees
  /// by Sarnak and Tarjan [?]. See de Berg et al for alternatives.
  /// 
  /// The internal data is an outer sorted dictionary that maps each
  /// x coordinate of an endpoint of some edge to an inner sorted set
  /// of the edges crossing or touching the vertical line at that x
  /// coordinate, the edges being ordered by their y coordinates
  /// to the immediate right of x. Lookup of a point (x,y) is done by
  /// finding the predecessor of x cell the outer dictionary and then locating
  /// the edges above and below (x,y) by searching in the inner sorted set.
  /// 
  /// The creation of the inner sorted sets is done by maintaining a
  /// (persistent) tree of edges, inserting and deleting edges according
  /// to a horzontal sweep of the edges while saving a snapshot of the
  /// inner tree in the outer dictionary at each new x coordinate.
  ///
  /// If there are n edges, there will be 2n updates to the inner tree,
  /// and in the worst case, the inner tree will have size Omega(n) for
  /// Omega(n) snapshots. We will use O(n*logn) time and O(n) space for
  /// sorting the endpoints. If we use a nodecopying persistent inner tree,
  /// we will use O(n) space and time for building the data structure proper.
  /// If we use a pathcopy persistent tree, we will use O(n*logn) time and
  /// space for the data struicture. Finally, if we use a non-persistent
  /// tree with a full copy for snapshot, we may use up to O(n^2) space and
  /// time for building the datastructure.
  ///
  /// Lookup will take O(logn) time in any case, but taking the memory
  /// hierarchy into consideration, a low space use is very beneficial
  /// for large problems.
  ///
  /// The code assumes that the given set of edges is correct, in particular
  /// that they do not touch at interior points (e.g. cross or coincide). 
  /// </summary>
	
  public class PointLocator<T>
  {
    private TreeDictionary<double,ISorted<Edge<T>>> htree;
    
    private EdgeComparer<T> lc = new EdgeComparer<T>();
    
    private SCG.IComparer<EndPoint> epc = new EndPoint(0, 0, true, 0);
    
    private DoubleComparer dc = new DoubleComparer();
    
    private TreeDictionary<EndPoint,Edge<T>> endpoints;
    
    private int count;
    
    private bool built = false;
    
    public PointLocator()
    {
      //htree = new TreeDictionary<double,TreeSet<Edge<T>>>(dc);
      endpoints = new TreeDictionary<EndPoint,Edge<T>>(epc);
    }
    
    public PointLocator(SCG.IEnumerable<Edge<T>> edges)
    {
      //htree = new TreeDictionary<double,TreeSet<Edge<T>>>(dc);
      endpoints = new TreeDictionary<EndPoint,Edge<T>>(epc);
      foreach (Edge<T> edge in edges)
	add(edge);
    }
    
    private void add(Edge<T> edge)
    {
      int c = DoubleComparer.StaticCompare(edge.xs, edge.xe);
      
      if (c == 0)
	return;
      
      endpoints.Add(new EndPoint(edge.xs, edge.ys, c < 0, count), edge);
      endpoints.Add(new EndPoint(edge.xe, edge.ye, c > 0, count++), edge);
    }

    public void Add(Edge<T> edge)
    {
      if (built)
	throw new InvalidOperationException("PointLocator static when built");
      add(edge);
    }
    
    public void AddAll(SCG.IEnumerable<Edge<T>> edges)
    {
      if (built)
	throw new InvalidOperationException("PointLocator static when built");
      
      foreach (Edge<T> edge in edges)
	add(edge);
    }
    
    public void Build()
    {
      //htree.Clear();
      htree = new TreeDictionary<double,ISorted<Edge<T>>>(dc);
      
      TreeSet<Edge<T>> vtree = new TreeSet<Edge<T>>(lc);
      double lastx = Double.NegativeInfinity;
      
      foreach (KeyValuePair<EndPoint,Edge<T>> p in endpoints)
	{
	  if (dc.Compare(p.Key.x,lastx)>0)
	    {
	      //Put an empty snapshot at -infinity!
	      htree[lastx] = (ISorted<Edge<T>>)(vtree.Snapshot());
	      lc.X = lastx = p.Key.x;
	      lc.compareToRight = false;
	    }
	  
	  if (p.Key.start)
	    {
	      if (!lc.compareToRight)
		lc.compareToRight = true;
	      Debug.Assert(vtree.Check());
	      bool chk = vtree.Add(p.Value);
	      Debug.Assert(vtree.Check());
	      
	      Debug.Assert(chk,"edge was not added!",""+p.Value);
	    }
	  else
	    {
	      Debug.Assert(!lc.compareToRight);
	      
	      Debug.Assert(vtree.Check("C"));
	      
	      bool chk = vtree.Remove(p.Value);
	      Debug.Assert(vtree.Check("D"));
	      
	      Debug.Assert(chk,"edge was not removed!",""+p.Value);
	    }
	}
      lc.compareToRight = true;
      
      htree[lastx] = (TreeSet<Edge<T>>)(vtree.Snapshot());
      built = true;
    }
    
    
    /*public void Clear()
      {
      endpoints.Clear();
      htree.Clear();
      }*/
    /// <summary>
    /// Find the cell, if any, containing (x,y).
    /// </summary>
    /// <param name="x">x coordinate of point</param>
    /// <param name="y">y coordinate of point</param>
    /// <param name="below">Associate data of cell according to edge below</param>
    /// <param name="above">Associate data of cell according to edge above</param>
    /// <returns>True if point is inside some cell</returns>
    public bool Place(double x, double y, out T cell)
    {
      if (!built)
	throw new InvalidOperationException("PointLocator must be built first");
      
      KeyValuePair<double,ISorted<Edge<T>>> p = htree.WeakPredecessor(x);
      
      //if (DoubleComparer.StaticCompare(cell.key,x)==0)
      //Just note it, we have thrown away the vertical edges!
      Edge<T> low, high;
      bool lval, hval;
      PointComparer<T> c = new PointComparer<T>(x, y);
      
      //Return value true here means we are at an edge.
      //But note that if x is in htree.Keys, we may be at a
      //vertical edge even if the return value is false here.
      //Therefore we do not attempt to sort out completely the case
      //where (x,y) is on an edge or even on several edges,
      //and just deliver some cell it is in.
      p.Value.Cut(c, out low, out lval, out high, out hval);
      if (!lval || !hval)
	{
	  cell = default(T);
	  return false;
	}
      else
	{
	  cell = low.Cell(true);//high.Cell(false);
	  return true;
	}
    }
    
    public void Place(double x, double y, out T upper, out bool hval, out T lower, out bool lval)
    {
      if (!built)
	throw new InvalidOperationException("PointLocator must be built first");
      
      KeyValuePair<double,ISorted<Edge<T>>> p = htree.WeakPredecessor(x);
      
      //if (DoubleComparer.StaticCompare(cell.key,x)==0)
      //Just note it, we have thrown away the vertical edges!
      Edge<T> low, high;
      PointComparer<T> c = new PointComparer<T>(x, y);
      
      //Return value true here means we are at an edge.
      //But note that if x is in htree.Keys, we may be at a
      //vertical edge even if the return value is false here.
      //Therefore we do not attempt to sort out completely the case
      //where (x,y) is on an edge or even on several edges,
      //and just deliver some cell it is in.
      p.Value.Cut(c, out low, out lval, out high, out hval);
      upper = hval ? high.Cell(false) : default(T);
      lower = lval ? low.Cell(true) : default(T);
      return; 
    }
    
    public void Test(double x, double y)
    {
      T cell;
      
      if (Place(x, y, out cell))
	Console.WriteLine("({0}; {1}): <- {2} ", x, y, cell);
      else
	Console.WriteLine("({0}; {1}): -", x, y);
    }
    
    /// <summary>
    /// Endpoint of an edge with ordering/comparison according to x
    /// coordinates with arbitration by the id field. 
    /// The client is assumed to keep the ids unique.
    /// </summary>
    public /*private*/ struct EndPoint: SCG.IComparer<EndPoint>
    {
      public double x, y;
      
      public bool start;
      
      private int id;
      
      
      public EndPoint(double x, double y, bool left, int id)
	{
	  this.x = x;this.y = y;this.start = left;this.id = id;
	}
      
      
      public int Compare(EndPoint a, EndPoint b)
	{
	  int c = DoubleComparer.StaticCompare(a.x, b.x);
	  
	  return c != 0 ? c : (a.start && !b.start) ? 1 : (!a.start && b.start) ? -1 : a.id < b.id ? -1 : a.id > b.id ? 1 : 0;
	}
    }
  }

  /// <summary>
  /// Compare two doubles with tolerance. 
  /// </summary>
  class DoubleComparer: SCG.IComparer<double>
  {
    private const double eps = 1E-10;
    
    public int Compare(double a, double b)
    {
      return a > b + eps ? 1 : a < b - eps ? -1 : 0;
    }

    public static int StaticCompare(double a, double b)
    {
      return a > b + eps ? 1 : a < b - eps ? -1 : 0;
    }
  }

  /// <summary>
  /// Compare a given point (x,y) to edges: is the point above, at or below
  /// the edge. Assumes edges not vertical. 
  /// </summary>
  class PointComparer<T>: IComparable<Edge<T>>
  {
    private double x, y;
    
    public PointComparer(double x, double y)
    {
      this.x = x; this.y = y;
    }
    
    public int CompareTo(Edge<T> a)
    {
      double ya = (a.ye - a.ys) / (a.xe - a.xs) * (x - a.xs) + a.ys;
      
      return DoubleComparer.StaticCompare(y, ya);
    }
    
    public bool Equals(Edge<T> a) { return CompareTo(a) == 0; }
  }

  /// <summary>
  /// Compare two edges at a given x coordinate:
  /// Compares the y-coordinate  to the immediate right of x of the two edges.
  /// Assumes edges to be compared are not vertical.
  /// </summary>
  class EdgeComparer<T>: SCG.IComparer<Edge<T>>
  {
    private double x;
    
    public bool compareToRight = true;
    
    public double X { get { return x; } set { x = value; } }
    
    public int Compare(Edge<T> line1, Edge<T> line2)
    {
      double a1 = (line1.ye - line1.ys) / (line1.xe - line1.xs);
      double a2 = (line2.ye - line2.ys) / (line2.xe - line2.xs);
      double ya = a1 * (x - line1.xs) + line1.ys;
      double yb = a2 * (x - line2.xs) + line2.ys;
      int c = DoubleComparer.StaticCompare(ya, yb);
      
      return c != 0 ? c : (compareToRight ? 1 : -1) * DoubleComparer.StaticCompare(a1, a2);
    }
  }

  namespace Test
    {
      public class Ugly : EnumerableBase<Edge<int>>, SCG.IEnumerable<Edge<int>>, SCG.IEnumerator<Edge<int>>
      {
	private int level = -1, maxlevel;
	
	private bool leftend = false;
	
	public Ugly(int maxlevel)
	{
	  this.maxlevel = maxlevel;
	}
	
	public override SCG.IEnumerator<Edge<int>> GetEnumerator()
	{
	  return (SCG.IEnumerator<Edge<int>>)MemberwiseClone();
	}
	
	public void Reset()
	{
	  level = -1;
	  leftend = false;
	}
	
	public bool MoveNext()
	{
	  if (level > maxlevel)
	    throw new InvalidOperationException();
	  
	  if (leftend)
	    {
	      leftend = false;
	      return true;
	    }
	  else
	    {
	      leftend = true;
	      return ++level <= maxlevel;
	    }
	}
	
	public Edge<int> Current
	{
	  get
	    {
	      if (level < 0 || level > maxlevel)
		throw new InvalidOperationException();
	      
	      double y = (level * 37) % maxlevel;
	      double deltax = leftend ? 1 : maxlevel;
	      
	      if (leftend)
		return new Edge<int>(0, y, level, y - 0.5, 0, 0);
	      else
		return new Edge<int>(level, y - 0.5, level, y, 0, 0);
	    }
	}
	
	
	public void Dispose() { }
	
#region IEnumerable Members

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
	  throw new Exception("The method or operation is not implemented.");
	}

#endregion
	
#region IEnumerator Members
	
	object System.Collections.IEnumerator.Current
	{
	  get { throw new Exception("The method or operation is not implemented."); }
	}
	
	void System.Collections.IEnumerator.Reset()
	{
	  throw new Exception("The method or operation is not implemented.");
	}
	
#endregion
      }

      public class TestUgly
      {
	private Ugly ugly;
	
	private int d;
	
	private PointLocator<int> pointlocator;
	
	
	public TestUgly(int d)
	{
	  this.d = d;
	  ugly = new Ugly(d);
	}
	
	
	public double Traverse()
	{
	  double xsum = 0;
	  
	  foreach (Edge<int> e in ugly)	xsum += e.xe;
	  
	  return xsum;
	}
	
	public bool LookUp(int count, int seed)
	{
	  Random random = new Random(seed);
	  bool res = false;
	  
	  for (int i = 0; i < count; i++)
	    {
	      int cell;
	      
	      res ^= pointlocator.Place(random.NextDouble() * d, random.NextDouble() * d, out cell);
	    }
	  
	  return res;
	}

	public static void Run(string[] args)
	{
	  int d = args.Length >= 2 ? int.Parse(args[1]) : 400;//00;
	  int repeats = args.Length >= 3 ? int.Parse(args[2]) : 10;
	  int lookups = args.Length >= 4 ? int.Parse(args[3]) : 500;//00;
	  
	  new TestUgly(d).run(lookups);
	}
	
	
	public void run(int lookups)
	{
	  double s = 0;
	  
	  s += Traverse();
	  
	  pointlocator = new PointLocator<int>(ugly);
	  pointlocator.Build();
	  
	  LookUp(lookups, 567);
	}
      }
      
      public class Lattice : EnumerableBase<Edge<string>>, SCG.IEnumerable<Edge<string>>, SCG.IEnumerator<Edge<string>>, System.Collections.IEnumerator
      {
	private int currenti = -1, currentj = 0, currentid = 0;
	
	private bool currenthoriz = true;
	
	private int maxi, maxj;
	
	private double a11 = 1, a21 = -1, a12 = 1, a22 = 1;
	
	public Lattice(int maxi, int maxj, double a11, double a21, double a12, double a22)
	{
	  this.maxi = maxi;
	  this.maxj = maxj;
	  this.a11 = a11;
	  this.a12 = a12;
	  this.a21 = a21;
	  this.a22 = a22;
	}

	public Lattice(int maxi, int maxj)
	{
	  this.maxi = maxi;
	  this.maxj = maxj;
	}
	
	public override SCG.IEnumerator<Edge<string>> GetEnumerator()
	{
	  return (SCG.IEnumerator<Edge<string>>)MemberwiseClone();
	}
	
	public void Reset()
	{
	  currenti = -1;
	  currentj = 0;
	  currentid = -1;
	  currenthoriz = true;
	}
	
	public bool MoveNext()
	{
	  currentid++;
	  if (currenthoriz)
	    {
	      if (++currenti >= maxi)
		{
		  if (currentj >= maxj)
		    return false;
		  
		  currenti = 0;
		  currenthoriz = false;
		}
	      
	      return true;
	    }
	  else
	    {
	      if (++currenti > maxi)
		{
		  currenti = 0;
		  currenthoriz = true;
		  if (++currentj > maxj)
		    return false;
		}
	      
	      return true;
	    }
	}
	
	
	private string i2l(int i)
	{
	  int ls = 0, j = i;
	  
	  do { ls++; j = j / 26 - 1; } while (j >= 0);
	  
	  char[] res = new char[ls];
	  
	  while (ls > 0) { res[--ls] = (char)(65 + i % 26); i = i / 26 - 1; }
	  
	  //res[0]--;
	  return new String(res);
	}
	
	
	private string fmtid(int i, int j)
	{
	  return "";//cell + ";" + cell;
	  /*if (cell < 0 || cell < 0 || cell >= maxi || cell >= maxj)
	    return "Outside";
	    
	    return String.Format("{0}{1}", i2l(cell), cell);*/
	}
	
	
	public Edge<string> Current
	{
	  get
	    {
	      if (currenti >= maxi && currentj >= maxj)
		throw new InvalidOperationException();
	      
	      double xs = currenti * a11 + currentj * a12;
	      double ys = currenti * a21 + currentj * a22;
	      double deltax = currenthoriz ? a11 : a12;
	      double deltay = currenthoriz ? a21 : a22;
	      string r = fmtid(currenti, currenthoriz ? currentj - 1 : currentj);
	      string l = fmtid(currenthoriz ? currenti : currenti - 1, currentj);
	      
	      return new Edge<string>(xs, ys, xs + deltax, ys + deltay, r, l);
	    }
	}
	
	
	public void Dispose() { }
	
#region IEnumerable Members
	
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
	  throw new Exception("The method or operation is not implemented.");
	}
	
#endregion
	
#region IEnumerator Members
	
	object System.Collections.IEnumerator.Current
	{
	  get { throw new Exception("The method or operation is not implemented."); }
	}
	
	bool System.Collections.IEnumerator.MoveNext()
	{
	  throw new Exception("The method or operation is not implemented.");
	}
	
	void System.Collections.IEnumerator.Reset()
	{
	  throw new Exception("The method or operation is not implemented.");
	}
	
#endregion
      }
      
      public class TestLattice
      {
	private Lattice lattice;
	
	private int d;
	
	private PointLocator<string> pointlocator;
	
	
	public TestLattice(int d)
	{
	  this.d = d;
	  lattice = new Lattice(d, d, 1, 0, 0, 1);
	}

	public TestLattice(int d, double shear)
	{
	  this.d = d;
	  lattice = new Lattice(d, d, 1, 0, shear, 1);
	}

	public double Traverse()
	{
	  double xsum = 0;
	  
	  foreach (Edge<string> e in lattice)	xsum += e.xe;
	  
	  return xsum;
	}
	
	
	public bool LookUp(int count, int seed)
	{
	  Random random = new Random(seed);
	  bool res = false;
	  
	  for (int i = 0; i < count; i++)
	    {
	      string cell;
	      
	      res ^= pointlocator.Place(random.NextDouble() * d, random.NextDouble() * d, out cell);
	    }
	  
	  return res;
	}
	
	
	public static void Run()
	{
 	  int d = 200;
 	  int repeats = 2;
 	  int lookups = 50000;
 	  TestLattice tl = null;
	  
 	  Console.WriteLine("TestLattice Run({0}), means over {1} repeats:", d, repeats);
 	  tl = new TestLattice(d, 0.000001);

 	  tl.Traverse();
	  
 	  tl.pointlocator = new PointLocator<string>();
	  
 	  tl.pointlocator.AddAll(tl.lattice);
	  
 	  tl.pointlocator.Build();
	  
 	  tl.LookUp(lookups, 567);
	}
	
	
	public void BasicRun()
	{
	  pointlocator.Test(-0.5, -0.5);
	  pointlocator.Test(-0.5, 0.5);
	  pointlocator.Test(-0.5, 1.5);
	  pointlocator.Test(0.5, -0.5);
	  pointlocator.Test(0.5, 0.5);
	  pointlocator.Test(0.5, 1.5);
	  pointlocator.Test(1.5, -0.5);
	  pointlocator.Test(1.5, 0.5);
	  pointlocator.Test(1.5, 1.5);
	  pointlocator.Test(1.5, 4.99);
	  pointlocator.Test(1.5, 5);
	  pointlocator.Test(1.5, 5.01);
	  pointlocator.Test(1.99, 4.99);
	  pointlocator.Test(1.99, 5);
	  pointlocator.Test(1.99, 5.01);
	  pointlocator.Test(2, 4.99);
	  pointlocator.Test(2, 5);
	  pointlocator.Test(2, 5.01);
	  pointlocator.Test(2.01, 4.99);
	  pointlocator.Test(2.01, 5);
	  pointlocator.Test(2.01, 5.01);
	}
      }
    }

  public class TestPointLocation {
    public static void Main(String[] args) {
      Test.TestUgly.Run(new String[0]);
    }
  }
}

