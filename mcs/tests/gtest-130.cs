//-- ex-nullable-bool

using System;

class MyTest {
  public static void Main(String[] args) {
    Console.WriteLine("Note that null prints as blank or []\n");
    bool? b1 = null, b2 = false, b3 = true;
    bool? b4 = b1^b2, b5 = b1&b2, b6 = b1|b2;                     // null false null
    Console.WriteLine("[{0}] [{1}] [{2}]", b4, b5, b6);
    bool? b7 = b1^b3, b8 = b1&b3, b9 = b1|b3;                     // null null true
    Console.WriteLine("[{0}] [{1}] [{2}]", b7, b8, b9);
    Console.WriteLine(b1 != null ? "null is true" : "null is false");     // null is false
    Console.WriteLine(b1 == null ? "!null is true" : "!null is false");  // !null is false

    Console.WriteLine();
    bool?[] bvals = new bool?[] { null, false, true };
    Console.WriteLine("{0,-6} {1,-6} {2,-6} {3,-6} {4,-6}", 
                      "x", "y", "x&y", "x|y", "x^y");
    foreach (bool? x in bvals) 
      foreach (bool? y in bvals) 
        Console.WriteLine("{0,-6} {1,-6} {2,-6} {3,-6} {4,-6}", 
                          x, y, x&y, x|y, x^y);
    Console.WriteLine();
    Console.WriteLine("{0,-6} {1,-6}", "x", "!x");
    foreach (bool? x in bvals) 
      Console.WriteLine("{0,-6} {1,-6}", x, !x);
  }
}
