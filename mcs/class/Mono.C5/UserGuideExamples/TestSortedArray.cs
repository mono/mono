// C5 example: This should fail because C5 does not know how to build
// a comparer for Object.

// Similarly for Rec<string,int>

// Compile with 
//   csc /r:C5.dll TestSortedArray.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace TestSortedArray {
  class TestSortedArray {
    public static void Main(String[] args) {
      //       SortedArray<Object> sarr = new SortedArray<Object>();
      SCG.IComparer<Rec<string,int>> lexico =
	new DelegateComparer<Rec<string,int>>(
	    delegate(Rec<string,int> r1, Rec<string,int> r2) { 
	      int order = r1.X1.CompareTo(r2.X1);
	      return order==0 ? r1.X2.CompareTo(r2.X2) : order;
	    });
      SortedArray<Rec<string,int>> sarr = new SortedArray<Rec<string,int>>(lexico);
      sarr.Add(new Rec<string,int>("ole", 32));
      sarr.Add(new Rec<string,int>("hans", 77));
      sarr.Add(new Rec<string,int>("ole", 63));
      foreach (Rec<string,int> r in sarr)
	Console.WriteLine(r);
    }
  }
}
