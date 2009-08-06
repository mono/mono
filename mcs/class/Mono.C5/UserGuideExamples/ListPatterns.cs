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

// C5 example: ListPatterns.cs for pattern chapter

// Compile with 
//   csc /r:C5.dll ListPatterns.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace ListPatterns {
  class ListPatterns {
    public static void Main(String[] args) {
      IList<int> list = new ArrayList<int>();
      list.AddAll(new int[] { 23, 29, 31, 37, 41, 43, 47, 53 });
      // Reversing and swapping
      Console.WriteLine(list);
      list.Reverse();
      Console.WriteLine(list);
      ReverseInterval(list, 2, 3);
      Console.WriteLine(list);
      SwapInitialFinal(list, 2);
      Console.WriteLine(list);
      // Clearing all or part of list
      list.CollectionCleared 
	+= delegate(Object c, ClearedEventArgs eargs) {
	     ClearedRangeEventArgs ceargs = eargs as ClearedRangeEventArgs;
	     if (ceargs != null) 
	       Console.WriteLine("Cleared [{0}..{1}]", 
				 ceargs.Start, ceargs.Start+ceargs.Count-1);
	   };
      RemoveSublist1(list, 1, 2);
      Console.WriteLine(list);
      RemoveSublist2(list, 1, 2);
      Console.WriteLine(list);
      RemoveTail1(list, 3);
      Console.WriteLine(list);
      RemoveTail2(list, 2);
      Console.WriteLine(list);
    }

    // Reverse list[i..i+n-1]

    public static void ReverseInterval<T>(IList<T> list, int i, int n) {
      list.View(i,n).Reverse();
    }

    // Swap list[0..i-1] with list[i..Count-1]

    public static void SwapInitialFinal<T>(IList<T> list, int i) {
      list.View(0,i).Reverse();
      list.View(i,list.Count-i).Reverse();
      list.Reverse();
    }

    // Remove sublist of a list

    public static void RemoveSublist1<T>(IList<T> list, int i, int n) {
      list.RemoveInterval(i, n);
    }

    public static void RemoveSublist2<T>(IList<T> list, int i, int n) {
      list.View(i, n). Clear();
    }


    // Remove tail of a list

    public static void RemoveTail1<T>(IList<T> list, int i) {
      list.RemoveInterval(i, list.Count-i);
    }

    public static void RemoveTail2<T>(IList<T> list, int i) {
      list.View(i, list.Count-i).Clear();
    }

    // Pattern for finding and using first (leftmost) x in list

    private static void PatFirst<T>(IList<T> list, T x) { 
      int j = list.IndexOf(x);
      if (j >= 0) { 
	// x is a position j in list
      } else {
	// x is not in list
      }
    }

    // Pattern for finding and using last (rightmost) x in list

    private static void PatLast<T>(IList<T> list, T x) { 
      int j = list.LastIndexOf(x);
      if (j >= 0) { 
	// x is at position j in list
      } else {
	// x is not in list
      }
    }

    // Pattern for finding and using first (leftmost) x in list[i..i+n-1]

    private static void PatFirstSublist<T>(IList<T> list, T x, int i, int n) { 
      int j = list.View(i,n).IndexOf(x);
      if (j >= 0) { 
	// x is at position j+i in list
      } else {
	// x is not in list[i..i+n-1]
      }
    }

    // Pattern for finding and using last (rightmost) x in list[i..i+n-1]

    private static void PatLastSublist<T>(IList<T> list, T x, int i, int n) { 
      int j = list.View(i,n).LastIndexOf(x);
      if (j >= 0) { 
	// x is at position j+i in list
      } else {
	// x is not in list[i..i+n-1]
      }
    }
  }
}
