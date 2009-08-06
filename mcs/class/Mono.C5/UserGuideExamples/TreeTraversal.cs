// C5 example
// 2004-11-09

using System;
using C5;
using SCG = System.Collections.Generic;

namespace TreeTraversal
{
  class MyTest
  {
    public static void Main(String[] args)
    {
      Tree<int> t = MakeTree(1, 15);
      Act<int> act = delegate(int val) { Console.Write("{0} ", val); };
      Console.WriteLine("Depth-first:");
      Tree<int>.DepthFirst(t, act);
      Console.WriteLine("\nBreadth-first:");
      Tree<int>.BreadthFirst(t, act);
      Console.WriteLine("\nDepth-first:");
      Tree<int>.Traverse(t, act, new ArrayList<Tree<int>>());
      Console.WriteLine("\nBreadth-first:");
      Tree<int>.Traverse(t, act, new LinkedList<Tree<int>>());
      Console.WriteLine();
    }

    // Build n-node tree with root numbered b and other nodes numbered b+1..b+n
    public static Tree<int> MakeTree(int b, int n)
    {
      if (n == 0)
        return null;
      else
      {
        int k = n / 2;
        Tree<int> t1 = MakeTree(b + 1, k), t2 = MakeTree(b + k + 1, n - 1 - k);
        return new Tree<int>(b, t1, t2);
      }
    }
  }

  class Tree<T>
  {
    private T val;
    private Tree<T> t1, t2;
    public Tree(T val) : this(val, null, null) { }
    public Tree(T val, Tree<T> t1, Tree<T> t2)
    {
      this.val = val; this.t1 = t1; this.t2 = t2;
    }

    public static void DepthFirst(Tree<T> t, Act<T> act)
    {
      IStack<Tree<T>> work = new ArrayList<Tree<T>>();
      work.Push(t);
      while (!work.IsEmpty)
      {
        Tree<T> cur = work.Pop();
        if (cur != null)
        {
          work.Push(cur.t2);
          work.Push(cur.t1);
          act(cur.val);
        }
      }
    }

    public static void BreadthFirst(Tree<T> t, Act<T> act)
    {
      IQueue<Tree<T>> work = new CircularQueue<Tree<T>>();
      work.Enqueue(t);
      while (!work.IsEmpty)
      {
        Tree<T> cur = work.Dequeue();
        if (cur != null)
        {
          work.Enqueue(cur.t1);
          work.Enqueue(cur.t2);
          act(cur.val);
        }
      }
    }

    public static void Traverse(Tree<T> t, Act<T> act, IList<Tree<T>> work)
    {
      work.Clear();
      work.Add(t);
      while (!work.IsEmpty)
      {
        Tree<T> cur = work.Remove();
        if (cur != null)
        {
          if (work.FIFO)
          {
            work.Add(cur.t1);
            work.Add(cur.t2);
          }
          else
          {
            work.Add(cur.t2);
            work.Add(cur.t1);
          }
          act(cur.val);
        }
      }
    }
  }
}