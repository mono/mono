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

// C5 example: ViewPatterns 2005-07-22

// Compile with 
//   csc /r:C5.dll ViewPatterns.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace ViewPatterns {
  class Views {
    public static void Main(String[] args) {
      IList<char> lst = new ArrayList<char>();
      lst.AddAll<char>(new char[] { 'a', 'b', 'c', 'd' });
      IList<char> v1 = lst.View(1, 1);
      Console.WriteLine("v1 = {0}", v1);
      InsertBeforeFirst(v1, '<', 'b');
      InsertAfterFirst(v1, '>', 'b');
      Console.WriteLine("v1 = {0}", v1);
      char x; 
      if (SequencePredecessor(v1, 'b', out x)) 
        Console.WriteLine("Predecessor of b is " + x);
      if (SequenceSuccessor(v1, 'b', out x)) 
        Console.WriteLine("Successor of b is " + x);
      if (!SequencePredecessor(v1, 'c', out x)) 
        Console.WriteLine("c has no predecessor");
      if (!SequenceSuccessor(v1, 'a', out x)) 
        Console.WriteLine("a has no successor");
      IList<char> lst2 = new ArrayList<char>();
      lst2.AddAll<char>(new char[] { 'a', 'b', 'c', 'A', 'a', 'd', 'a' });
      foreach (int i in IndexesOf(lst2, 'a')) 
        Console.Write("{0} ", i);
      Console.WriteLine();
      foreach (int i in ReverseIndexesOf(lst2, 'a')) 
        Console.Write("{0} ", i);
      Console.WriteLine();
      Console.WriteLine(lst2);
      IList<char> view = lst2.View(2,0);
      InsertAtView(lst2, view, 'y');
      Console.WriteLine(lst2);
      InsertIntoView(view, 'x');
      Console.WriteLine(lst2);
    }

    // --- Patterns for zero-item views -----------------------------

    // Number of items before zero-item view
   
    public static int ItemsBefore<T>(IList<T> view) {
      return view.Offset;
    }

    // Number of items after zero-item view
   
    public static int ItemsAfter<T>(IList<T> view) {
      return view.Underlying.Count - view.Offset;
    }

    // Move (zero-item) view one item to the left
   
    public static void MoveLeft<T>(IList<T> view) {
      // One of these:
      view.Slide(-1);
      view.TrySlide(-1);
    }

    // Move (zero-item) view one item to the right
   
    public static void MoveRight<T>(IList<T> view) {
      // One of these:
      view.Slide(+1);
      view.TrySlide(+1);
    }

    // Test whether (zero-item) view is at beginning of list
   
    public static bool AtBeginning<T>(IList<T> view) {
      return view.Offset == 0;
    }

    // Test whether (zero-item) view is at end of list
   
    public static bool AtEnd<T>(IList<T> view) {
      return view.Offset == view.Underlying.Count;
    }

    // Insert x into zero-item view and into underlying list
   
    public static void InsertIntoView<T>(IList<T> view, T x) {
      view.Add(x);
    }

    // Insert x into list at zero-item view 
   
    public static void InsertAtView<T>(IList<T> list, IList<T> view, T x) {
      list.Insert(view, x);
    }

    // Delete the item before zero-item view 
   
    public static void DeleteBefore<T>(IList<T> view) {
      view.Slide(-1,1).RemoveFirst();
    }

    // Delete the item after zero-item view 
   
    public static void DeleteAfter<T>(IList<T> view) {
      view.Slide(0,1).RemoveFirst();
    }

    // Get the zero-item view at left endpoint.  Succeeds on all lists
    // and valid views.

    public static IList<T> LeftEndView<T>(IList<T> list) {
      return list.View(0,0);
    }

    // Get the zero-item view at right endpoint.  Succeeds on all
    // lists and valid views.

    public static IList<T> RightEndView<T>(IList<T> list) {
      return list.View(list.Count,0);
    }


    // --- Patterns for one-item views ------------------------------

    // Find the sequence predecessor x of y; or throw exception

    public static T SequencePredecessor<T>(IList<T> list, T y) {
      return list.ViewOf(y).Slide(-1)[0];
    }  
    
    // Find the sequence predecessor x of y; or return false 

    public static bool SequencePredecessor<T>(IList<T> list, T y, out T x) {
      IList<T> view = list.ViewOf(y);
      bool ok = view != null && view.TrySlide(-1);
      x = ok ? view[0] : default(T);
      return ok;
    }  

    // Find the sequence successor x of y; or throw exception

    public static T SequenceSuccessor<T>(IList<T> list, T y) {
      return list.ViewOf(y).Slide(+1)[0];
    }  

    // Find the sequence successor x of y; or return false

    public static bool SequenceSuccessor<T>(IList<T> list, T y, out T x) {
      IList<T> view = list.ViewOf(y);
      bool ok = view != null && view.TrySlide(+1);
      x = ok ? view[0] : default(T);
      return ok;
    }  

    // Insert x into list after first occurrence of y (or throw
    // NullReferenceException).
    
    public static void InsertAfterFirst<T>(IList<T> list, T x, T y) {
      list.Insert(list.ViewOf(y), x);
    }

    // Insert x into list before first occurrence of y (or throw
    // NullReferenceException)
    
    public static void InsertBeforeFirst<T>(IList<T> list, T x, T y) {
      list.Insert(list.ViewOf(y).Slide(0, 0), x);
    }

    // Insert x into list after last occurrence of y (or throw
    // NullReferenceException).
    
    public static void InsertAfterLast<T>(IList<T> list, T x, T y) {
      list.Insert(list.LastViewOf(y), x);
    }

    // Insert x into list before last occurrence of y (or throw
    // NullReferenceException)
    
    public static void InsertBeforeLast<T>(IList<T> list, T x, T y) {
      list.Insert(list.LastViewOf(y).Slide(0, 0), x);
    }

    // Same meaning as InsertBeforeFirst on a proper list, but not on
    // a view

    public static void InsertBeforeFirstAlt<T>(IList<T> list, T x, T y) {
      list.ViewOf(y).InsertFirst(x);
    }

    // Delete the sequence predecessor of first y; or throw exception

    public static T RemovePredecessorOfFirst<T>(IList<T> list, T y) {
      return list.ViewOf(y).Slide(-1).Remove();
    }

    // Delete the sequence successor of first y; or throw exception

    public static T RemoveSuccessorOfFirst<T>(IList<T> list, T y) {
      return list.ViewOf(y).Slide(+1).Remove();
    }

    // --- Other view patterns --------------------------------------

    // Replace the first occurrence of each x from xs by y in list:
    
    public static void ReplaceXsByY<T>(HashedLinkedList<T> list, T[] xs, T y) {
      foreach (T x in xs) {
        using (IList<T> view = list.ViewOf(x)) {
          if (view != null) { 
            view.Remove();
            view.Add(y);
          }
        }
      }
    }

    // Get index in underlying list of view's left end
    
    public static int LeftEndIndex<T>(IList<T> view) { 
      return view.Offset;
    }

    // Get index in underlying list of view's right end

    public static int RightEndIndex<T>(IList<T> view) { 
      return view.Offset + view.Count;
    }

    // Test whether views overlap 

    public static bool Overlap<T>(IList<T> u, IList<T> w) { 
      if (u.Underlying == null || u.Underlying != w.Underlying) 
        throw new ArgumentException("views must have same underlying list");
      else
        return u.Offset < w.Offset+w.Count && w.Offset < u.Offset+u.Count;
    }

    // Find the length of the overlap between two views

    public static int OverlapLength<T>(IList<T> u, IList<T> w) { 
      if (Overlap(u, w))
        return Math.Min(u.Offset+u.Count, w.Offset+w.Count) 
             - Math.Max(u.Offset, w.Offset);
      else
        return -1; // No overlap
    }

    // Test whether view u contains view v 

    public static bool ContainsView<T>(IList<T> u, IList<T> w) { 
      if (u.Underlying == null || u.Underlying != w.Underlying) 
        throw new ArgumentException("views must have same underlying list");
      else
        if (w.Count > 0)
          return u.Offset <= w.Offset && w.Offset+w.Count <= u.Offset+u.Count;
        else
          return u.Offset < w.Offset && w.Offset < u.Offset+u.Count;
    }

    // Test whether views u and v have (or are) the same underlying list

    public static bool SameUnderlying<T>(IList<T> u, IList<T> w) { 
      return (u.Underlying ?? u) == (w.Underlying ?? w);
    }

    // Find the index of the first item that satisfies p

    public static int FindFirstIndex<T>(IList<T> list, Fun<T,bool> p) {
      using (IList<T> view = list.View(0, 0)) {
        while (view.TrySlide(0, 1)) {
          if (p(view.First)) 
            return view.Offset;
          view.Slide(+1, 0);
        }
      }
      return -1;
    }

    // Find the index of the last item that satisfies p
    
    public static int FindLastIndex<T>(IList<T> list, Fun<T,bool> p) {
      using (IList<T> view = list.View(list.Count, 0)) {
        while (view.TrySlide(-1, 1)) {
          if (p(view.First)) 
            return view.Offset;
        }
      }
      return -1;
    }

    // Yield indexes of all items equal to x, in list order:

    public static SCG.IEnumerable<int> IndexesOf<T>(IList<T> list, T x) { 
      IList<T> tail = list.View(0, list.Count);
      tail = tail.ViewOf(x);
      while (tail != null) {
        yield return tail.Offset;
        tail = tail.Slide(+1,0).Span(list);
        tail = tail.ViewOf(x); 
      }
    }

    // Yield indexes of items equal to x, in reverse list order.

    public static SCG.IEnumerable<int> ReverseIndexesOf<T>(IList<T> list, T x) {
      IList<T> head = list.View(0, list.Count);
      head = head.LastViewOf(x);
      while (head != null) {
        yield return head.Offset;
        head = list.Span(head.Slide(0,0));
        head = head.LastViewOf(x);
      }
    }

  }
}
