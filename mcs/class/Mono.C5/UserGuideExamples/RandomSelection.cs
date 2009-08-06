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

// C5 example: RandomSelection.cs for pattern chapter

// Compile with 
//   csc /r:C5.dll RandomSelection.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace RandomSelection {
  class RandomSelection {
    public static void Main(String[] args) {
      ArrayList<int> list = new ArrayList<int>(), copy1, copy2;
      list.AddAll(new int[] { 2, 3, 5, 7, 11, 13, 17, 19 });
      copy1 = (ArrayList<int>)list.Clone();
      copy2 = (ArrayList<int>)list.Clone();
      const int N = 7;
      Console.WriteLine("-- With replacement:");
      foreach (int x in RandomWith(list, N))
        Console.Write("{0} ", x);
      Console.WriteLine("\n-- Without replacement:");
      foreach (int x in RandomWithout1(copy1, N))
        Console.Write("{0} ", x);
      Console.WriteLine("\n-- Without replacement:");
      foreach (int x in RandomWithout2(copy2, N))
        Console.Write("{0} ", x);
      Console.WriteLine();
    }

    private static readonly C5Random rnd = new C5Random();

    // Select N random items from coll, with replacement.
    // Does not modify the given list.

    public static SCG.IEnumerable<T> RandomWith<T>(IIndexed<T> coll, int N) {
      for (int i=N; i>0; i--) { 
        T x = coll[rnd.Next(coll.Count)];
        yield return x;
      }
    }

    // Select N random items from list, without replacement.
    // Modifies the given list.

    public static SCG.IEnumerable<T> RandomWithout1<T>(IList<T> list, int N) {
      list.Shuffle(rnd);     
      foreach (T x in list.View(0, N)) 
        yield return x;
    }

    // Select N random items from list, without replacement.
    // Faster when list is efficiently indexable and modifiable.
    // Modifies the given list.

    public static SCG.IEnumerable<T> RandomWithout2<T>(ArrayList<T> list, int N) {
      for (int i=N; i>0; i--) { 
        int j = rnd.Next(list.Count);
        T x = list[j], replacement = list.RemoveLast();
        if (j < list.Count) 
          list[j] = replacement;
        yield return x;
      }
    }
  }
}
