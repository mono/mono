// Not used -- ex-nullable-struct

// Converting a struct from S to S? creates a copy of the struct.
// Getting the struct out of the non-null value creates another copy.

using System;

struct S {
  private int x;
  public int X {
    get { return x; }
    set { this.x = value; }	// Cannot be used on non-variable ns.Value
  }
  public void Set(int x) {
    this.x = x;
  }
}

class MyTest {
  public static void Main(String[] args) {
    S s = new S();
    s.Set(11);
    Console.WriteLine("s.X = {0}", s.X);
    S? ns = s;
    Console.WriteLine("s.X = {0} ns.Value.X = {1}", s.X, ns.Value.X);
    ns.Value.Set(22);
    Console.WriteLine("s.X = {0} ns.Value.X = {1}", s.X, ns.Value.X);
    s.Set(33);
    Console.WriteLine("s.X = {0} ns.Value.X = {1}", s.X, ns.Value.X);
  }
}
