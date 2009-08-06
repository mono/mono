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

// C5 example: GettingStarted 2005-01-18

// Compile with 
//   csc /r:C5.dll GettingStarted.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace GettingStarted {
  class GettingStarted {
    public static void Main(String[] args) {
      IList<String> names = new ArrayList<String>();
      names.AddAll(new String[] { "Hoover", "Roosevelt", 
                                  "Truman", "Eisenhower", "Kennedy" });
      // Print list:
      Console.WriteLine(names);
      // Print item 1 ("Roosevelt") in the list:
      Console.WriteLine(names[1]);
      // Create a list view comprising post-WW2 presidents:
      IList<String> postWWII = names.View(2, 3);
      // Print item 2 ("Kennedy") in the view:
      Console.WriteLine(postWWII[2]);
      // Enumerate and print the list view in reverse chronological order:
      foreach (String name in postWWII.Backwards()) 
        Console.WriteLine(name);
    }
  }
}
