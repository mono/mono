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

// C5 example: topological sorting 2005-09-09

// Compile with 
//   csc /r:C5.dll Toposort.cs 

using System;
using System.Text;
using C5;
using SCG = System.Collections.Generic;
using SDD = System.Diagnostics.Debug;

namespace Toposort {
  class TestToposort {
    public static void Main(String[] args) {
      Node<String> 
        d = new Node<String>("d"), 
        e = new Node<String>("e"),
        c = new Node<String>("c", d, e),
        b = new Node<String>("b", d),
        a = new Node<String>("a", d, b, c);
      foreach (Node<String> n in Toposort0(a))
        Console.WriteLine(n);
      Console.WriteLine();
      foreach (Node<String> n in Toposort1(a))
        Console.WriteLine(n);
      Console.WriteLine();
      foreach (Node<String> n in Toposort2(a))
        Console.WriteLine(n);
    }

    // Toposort 0, adding each node when finished, after its descendants.
    // Classic depth-first search.  Does not terminate on cyclic graphs.
    
    public static IList<Node<T>> Toposort0<T>(params Node<T>[] starts) {
      HashedLinkedList<Node<T>> sorted = new HashedLinkedList<Node<T>>();
      foreach (Node<T> start in starts) 
        if (!sorted.Contains(start)) 
          AddNode0(sorted, start);
      return sorted; 
    }

    private static void AddNode0<T>(IList<Node<T>> sorted, Node<T> node) {
      SDD.Assert(!sorted.Contains(node));
      foreach (Node<T> child in node.children) 
        if (!sorted.Contains(child)) 
          AddNode0(sorted, child);
      sorted.InsertLast(node);
    }

    // Toposort 1, using hash index to add each node before its descendants.
    // Terminates also on cyclic graphs.
    
    public static IList<Node<T>> Toposort1<T>(params Node<T>[] starts) {
      HashedLinkedList<Node<T>> sorted = new HashedLinkedList<Node<T>>();
      foreach (Node<T> start in starts) 
        if (!sorted.Contains(start)) {
          sorted.InsertLast(start);
          AddNode1(sorted, start);
        }
      return sorted; 
    }

    private static void AddNode1<T>(IList<Node<T>> sorted, Node<T> node) {
      SDD.Assert(sorted.Contains(node));
      foreach (Node<T> child in node.children) 
        if (!sorted.Contains(child)) {
          sorted.ViewOf(node).InsertFirst(child);
          AddNode1(sorted, child);
        }
    }

    // Toposort 2, node rescanning using a view.
    // Uses no method call stack and no extra data structures, but slower.

    public static IList<Node<T>> Toposort2<T>(params Node<T>[] starts) {
      HashedLinkedList<Node<T>> sorted = new HashedLinkedList<Node<T>>();
      foreach (Node<T> start in starts) 
        if (!sorted.Contains(start)) {
          sorted.InsertLast(start);
          using (IList<Node<T>> cursor = sorted.View(sorted.Count-1,1)) {
            do { 
              Node<T> child;
              while (null != (child = PendingChild(sorted, cursor.First))) {
                cursor.InsertFirst(child);
                cursor.Slide(0,1);
              }
            } while (cursor.TrySlide(+1));
          }
        }
      return sorted; 
    }

    static Node<T> PendingChild<T>(IList<Node<T>> sorted, Node<T> node) {
      foreach (Node<T> child in node.children) 
        if (!sorted.Contains(child))
          return child;
      return null;
    }
  }

  class Node<T> { 
    public readonly T id;
    public readonly Node<T>[] children;

    public Node(T id, params Node<T>[] children) { 
      this.id = id; this.children = children;
    }

    public override String ToString() { 
      return id.ToString();
    }

    public Node<T> this[int i] {
      set { children[i] = value; }
      get { return children[i]; }
    }
  }
}
