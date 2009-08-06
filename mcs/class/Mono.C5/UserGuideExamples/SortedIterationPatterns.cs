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

// C5 example: SortedIterationPatterns.cs for pattern chapter

// Compile with 
//   csc /r:C5.dll SortedIterationPatterns.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace SortedIterationPatterns {
  class SortedIterationPatterns {
    public static void Main(String[] args) {
      ISorted<int> sorted = new TreeSet<int>();
      sorted.AddAll(new int[] { 23, 29, 31, 37, 41, 43, 47, 53 });
      Console.WriteLine(sorted);
      if (args.Length == 1) { 
        int n = int.Parse(args[0]);
        int res;
        if (Predecessor(sorted, n, out res))
          Console.WriteLine("{0} has predecessor {1}", n, res);
        if (WeakPredecessor(sorted, n, out res))
          Console.WriteLine("{0} has weak predecessor {1}", n, res);
        if (Successor(sorted, n, out res))
          Console.WriteLine("{0} has successor {1}", n, res);
        if (WeakSuccessor(sorted, n, out res))
          Console.WriteLine("{0} has weak successor {1}", n, res);
      }
      IterBeginEnd(sorted);
      IterBeginEndBackwards(sorted);
      IterIncExc(sorted, 29, 47);
      IterIncExcBackwards(sorted, 29, 47);
      IterIncEnd(sorted, 29);
      IterBeginExc(sorted, 47);
      IterIncInc(sorted, 29, 47);
      IterBeginInc(sorted, 47);
      IterExcExc(sorted, 29, 47);
      IterExcEnd(sorted, 29);
      IterExcInc(sorted, 29, 47);
    }

    // --- Predecessor and successor patterns --------------------

    // Find weak successor of y in coll, or return false

    public static bool WeakSuccessor<T>(ISorted<T> coll, T y, out T ySucc) 
      where T : IComparable<T>
    {
      T yPred;
      bool hasPred, hasSucc, 
        hasY = coll.Cut(y, out yPred, out hasPred, out ySucc, out hasSucc);
      if (hasY)
        ySucc = y;
      return hasY || hasSucc;
    }

    // Find weak predecessor of y in coll, or return false

    public static bool WeakPredecessor<T>(ISorted<T> coll, T y, out T yPred) 
      where T : IComparable<T>
    {
      T ySucc;
      bool hasPred, hasSucc, 
        hasY = coll.Cut(y, out yPred, out hasPred, out ySucc, out hasSucc);
      if (hasY) 
        yPred = y;
      return hasY || hasPred;
    }

    // Find (strict) successor of y in coll, or return false

    public static bool Successor<T>(ISorted<T> coll, T y, out T ySucc) 
      where T : IComparable<T>
    {
      bool hasPred, hasSucc;
      T yPred;
      coll.Cut(y, out yPred, out hasPred, out ySucc, out hasSucc);
      return hasSucc;
    }

    // Find (strict) predecessor of y in coll, or return false

    public static bool Predecessor<T>(ISorted<T> coll, T y, out T yPred) 
      where T : IComparable<T>
    {
      bool hasPred, hasSucc;
      T ySucc;
      coll.Cut(y, out yPred, out hasPred, out ySucc, out hasSucc);
      return hasPred;
    }

    // --- Sorted iteration patterns -----------------------------

    // Iterate over all items
    
    public static void IterBeginEnd<T>(ISorted<T> coll) {
      foreach (T x in coll) { 
        Console.Write("{0} ", x);
      }
      Console.WriteLine();
    }

    // Iterate over all items, backwards
    
    public static void IterBeginEndBackwards<T>(ISorted<T> coll) {
      foreach (T x in coll.Backwards()) { 
        Console.Write("{0} ", x);
      }
      Console.WriteLine();
    }

    // Iterate over [x1,x2[
    
    public static void IterIncExc<T>(ISorted<T> coll, T x1, T x2) {
      foreach (T x in coll.RangeFromTo(x1, x2)) { 
        Console.Write("{0} ", x);
      }
      Console.WriteLine();
    }

    // Iterate over [x1,x2[, backwards
    
    public static void IterIncExcBackwards<T>(ISorted<T> coll, T x1, T x2) {
      foreach (T x in coll.RangeFromTo(x1, x2).Backwards()) { 
        Console.Write("{0} ", x);
      }
      Console.WriteLine();
    }

    // Iterate over [x1...]
    
    public static void IterIncEnd<T>(ISorted<T> coll, T x1) {
      foreach (T x in coll.RangeFrom(x1)) { 
        Console.Write("{0} ", x);
      }
      Console.WriteLine();
    }

    // Iterate over [...x2[
    
    public static void IterBeginExc<T>(ISorted<T> coll, T x2) {
      foreach (T x in coll.RangeTo(x2)) { 
        Console.Write("{0} ", x);
      }
      Console.WriteLine();
    }

    // Iterate over [x1...x2]
    
    public static void IterIncInc<T>(ISorted<T> coll, T x1, T x2) 
      where T : IComparable<T>
    {
      T x2Succ; 
      bool x2HasSucc = Successor(coll, x2, out x2Succ);
      IDirectedEnumerable<T> range = 
        x2HasSucc ? coll.RangeFromTo(x1, x2Succ) : coll.RangeFrom(x1);
      foreach (T x in range) {
        Console.Write("{0} ", x);
      } 
      Console.WriteLine();
    }

    // Iterate over [...x2]
    
    public static void IterBeginInc<T>(ISorted<T> coll, T x2) 
      where T : IComparable<T>
    {
      T x2Succ; 
      bool x2HasSucc = Successor(coll, x2, out x2Succ);
      IDirectedEnumerable<T> range = 
        x2HasSucc ? coll.RangeTo(x2Succ) : coll.RangeAll();
      foreach (T x in range) {
        Console.Write("{0} ", x);
      } 
      Console.WriteLine();
    }

    // Iterate over ]x1...x2[
    
    public static void IterExcExc<T>(ISorted<T> coll, T x1, T x2)
      where T : IComparable<T>
    {
      T x1Succ;
      bool x1HasSucc = Successor(coll, x1, out x1Succ);
      IDirectedEnumerable<T> range = 
        x1HasSucc ? coll.RangeFromTo(x1Succ, x2) : new ArrayList<T>();
      foreach (T x in range) {
        Console.Write("{0} ", x);
      } 
      Console.WriteLine();
    }

    // Iterate over ]x1...]
    
    public static void IterExcEnd<T>(ISorted<T> coll, T x1) 
      where T : IComparable<T>
    {
      T x1Succ;
      bool x1HasSucc = Successor(coll, x1, out x1Succ);
      IDirectedEnumerable<T> range = 
        x1HasSucc ? coll.RangeFrom(x1Succ) : new ArrayList<T>();
      foreach (T x in range) {
        Console.Write("{0} ", x);
      } 
      Console.WriteLine();
    }

    // Iterate over ]x1...x2]
    
    public static void IterExcInc<T>(ISorted<T> coll, T x1, T x2) 
      where T : IComparable<T>
    {
      T x1Succ, x2Succ;
      bool x1HasSucc = Successor(coll, x1, out x1Succ),
           x2HasSucc = Successor(coll, x2, out x2Succ);
      IDirectedEnumerable<T> range = 
        x1HasSucc ? (x2HasSucc ? coll.RangeFromTo(x1Succ, x2Succ) 
                               : coll.RangeFrom(x1Succ))
                  : new ArrayList<T>();
      foreach (T x in range) {
        Console.Write("{0} ", x);
      } 
      Console.WriteLine();
    }

  }
}
