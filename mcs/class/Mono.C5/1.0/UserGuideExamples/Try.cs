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

// C5 example: various tests 2005-01-01

// Compile with 
//   csc /r:C5.dll Try.cs 

using System;
using System.Text;
using C5;
using SCG = System.Collections.Generic;

namespace Try
{
  class MyTest
  {
    public static void Main()
    {
      IList<bool> list = new ArrayList<bool>();
      list.AddAll(new bool[] { false, false, true, true, false });
      list.CollectionCleared 
	+= delegate(Object coll, ClearedEventArgs args) {
	  ClearedRangeEventArgs crargs = args as ClearedRangeEventArgs;
	  if (crargs != null) {
	    Console.WriteLine("Cleared {0} to {1}", 
			      crargs.Start, crargs.Start+crargs.Count-1);
	  } else {
	    Console.WriteLine("Cleared {0} items", args.Count);
	  }
	};
      list.RemoveInterval(2, 2);
      HashSet<int> hash = new HashSet<int>();
      hash.ItemsRemoved 
	+= delegate {
	  Console.WriteLine("Item was removed");
	};
      hash.ItemsAdded 
	+= delegate {
	  Console.WriteLine("Item was added");
	};
      hash.UpdateOrAdd(2);
      hash.UpdateOrAdd(2);
    }
  }
}
