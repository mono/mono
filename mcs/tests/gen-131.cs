//-- ex-nullable

using System;

class MyTest {
  public static void Main(String[] args) {
    Console.WriteLine("Note that null prints as blank or []\n");
    int? i1 = 11, i2 = 22, i3 = null, i4 = i1+i2, i5 = i1+i3;
    // Values: 11 22 null 33 null
    Console.WriteLine("[{0}] [{1}] [{2}] [{3}] [{4}]", i1, i2, i3, i4, i5);
    int i6 = (int)i1;                           // Legal
    // int i7 = (int)i5;                        // Legal but fails at run-time
    // int i8 = i1;                             // Illegal

    int?[] iarr = { i1, i2, i3, i4, i5 };
    i2 += i1;
    i2 += i4;
    Console.WriteLine("i2 = {0}", i2);          // 66 = 11+22+33

    int sum = 0;
    for (int i=0; i<iarr.Length; i++)
      sum += iarr[i] != null ? iarr[i].Value : 0;
      //      sum += iarr[i] ?? 0;
    Console.WriteLine("sum = {0}", sum);        // 66 = 11+22+33

    for (int i=0; i<iarr.Length; i++)
      if (iarr[i] > 11)
        Console.Write("[{0}] ", iarr[i]);       // 22 33
    Console.WriteLine();

    for (int i=0; i<iarr.Length; i++)
      if (iarr[i] != i1)
        Console.Write("[{0}] ", iarr[i]);       // 22 null 33 null
    Console.WriteLine();
    Console.WriteLine();
    int?[] ivals = { null, 2, 5 };
    Console.WriteLine("{0,6} {1,6} {2,6} {3,6} {4,-6} {5,-6} {6,-6} {7,-6}", 
                      "x", "y", "x+y", "x-y", "x<y", "x>=y", "x==y", "x!=y");
    Console.WriteLine();
    foreach (int? x in ivals) 
      foreach (int? y in ivals) 
        Console.WriteLine("{0,6} {1,6} {2,6} {3,6} {4,-6} {5,-6} {6,-6} {7,-6}", 
                          x, y, x+y, x-y, (x<y), (x>=y), x==y, x!=y);
  }
}
