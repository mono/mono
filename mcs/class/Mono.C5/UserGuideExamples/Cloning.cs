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

// C5 example: anagrams 2004-12-

// Compile with 
//   csc /r:C5.dll Cloning.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace MyCloningTest {
  class MyTest {
    public static void Main(String[] args) {
      IList<int> lst = new ArrayList<int>();
      lst.AddAll(new int[] { 2, 3, 5, 7, 11, 13 });
      Console.WriteLine(lst);
      IList<int> v1 = lst.ViewOf(7);
      Console.WriteLine(v1);
      IList<int> v2 = (IList<int>)v1.Clone();
      v2.Slide(1);
      Console.WriteLine(v1);
      Console.WriteLine(v2);
    }
  }
}
