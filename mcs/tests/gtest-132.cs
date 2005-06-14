//-- ex-nullable-sqrt

using System;

class MyTest {
  public static int? Sqrt(int? x) {
    if (x.HasValue && x.Value >= 0)
      return (int)(Math.Sqrt(x.Value));
    else
      return null;
  }

  public static void Main(String[] args) {
    // Prints :2:::
    Console.WriteLine(":{0}:{1}:{2}:", Sqrt(5), Sqrt(null), Sqrt(-5));
  }
}
