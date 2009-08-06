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

// C5 example: ReadOnlyPatterns.cs for pattern chapter

// Compile with 
//   csc /r:C5.dll ReadOnlyPatterns.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace ReadOnlyPatterns {
  class ReadOnlyPatterns {
    public static void Main(String[] args) {
      GuardHashSet<int>();
      GuardTreeSet<int>();
      GuardList<int>();
      GuardHashDictionary<int,int>();
      GuardSortedDictionary<int,int>();
    }

    // Read-only access to a hash-based collection
    
    static void GuardHashSet<T>() {
      ICollection<T> coll = new HashSet<T>();
      DoWork(new GuardedCollection<T>(coll));
    }

    static void DoWork<T>(ICollection<T> gcoll) { 
      // Use gcoll ... 
    }

    // Read-only access to an indexed sorted collection

    static void GuardTreeSet<T>() {
      IIndexedSorted<T> coll = new TreeSet<T>(); 
      DoWork(new GuardedIndexedSorted<T>(coll));
    }
    
    static void DoWork<T>(IIndexedSorted<T> gcoll) { 
      // Use gcoll ...
    }

    // Read-only access to a list

    static void GuardList<T>() {
      IList<T> coll = new ArrayList<T>(); 
      DoWork(new GuardedList<T>(coll));
    }
    
    static void DoWork<T>(IList<T> gcoll) { 
      // Use gcoll ...
    }

    // Read-only access to a dictionary

    static void GuardHashDictionary<K,V>() {
      IDictionary<K,V> dict = new HashDictionary<K,V>(); 
      DoWork(new GuardedDictionary<K,V>(dict));
    }
    
    static void DoWork<K,V>(IDictionary<K,V> gdict) { 
      // Use gdict ...
    }

    // Read-only access to a sorted dictionary

    static void GuardSortedDictionary<K,V>() {
      ISortedDictionary<K,V> dict = new TreeDictionary<K,V>(); 
      DoWork(new GuardedSortedDictionary<K,V>(dict));
    }
    
    static void DoWork<K,V>(ISortedDictionary<K,V> gdict) { 
      // Use gdict ...
    }
  }
}
