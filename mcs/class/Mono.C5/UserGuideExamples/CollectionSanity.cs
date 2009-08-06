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

// C5 example: anagrams 2004-12-08

// Compile with 
//   csc /r:C5.dll CollectionSanity.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace CollectionSanity {
  class CollectionSanity {
    public static void Main(String[] args) {
      IList<int> col1 = new LinkedList<int>(),
        col2 = new LinkedList<int>(), col3 = new LinkedList<int>();
      col1.AddAll<int>(new int[] { 7, 9, 13 });
      col2.AddAll<int>(new int[] { 7, 9, 13 });
      col3.AddAll<int>(new int[] { 9, 7, 13 });

      HashSet<IList<int>> hs1 = new HashSet<IList<int>>();
      hs1.Add(col1); hs1.Add(col2); hs1.Add(col3);
      Console.WriteLine("hs1 is sane: {0}", EqualityComparerSanity<int,IList<int>>(hs1));
    }
    
    // When colls is a collection of collections, this method checks
    // that all `inner' collections use the exact same equalityComparer.  Note
    // that two equalityComparer objects may be functionally (extensionally)
    // identical, yet be distinct objects.  However, if the equalityComparers
    // were obtained from EqualityComparer<T>.Default, there will be at most one
    // equalityComparer for each type T.

    public static bool EqualityComparerSanity<T,U>(ICollectionValue<U> colls) 
      where U : IExtensible<T>
    {
      SCG.IEqualityComparer<T> equalityComparer = null;
      foreach (IExtensible<T> coll in colls) {
        if (equalityComparer == null) 
          equalityComparer = coll.EqualityComparer;
        if (equalityComparer != coll.EqualityComparer) 
          return false;
      }
      return true;
    }
  }
}
