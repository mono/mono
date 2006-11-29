// sestoft@dina.kvl.dk * 2004-08

using System;

class MyTest {
  public static void Main(String[] args) {
    Foo<int?> fni1 = new Foo<int?>(null);
    Console.WriteLine(fni1.Fmt());
    Foo<int?> fni2 = new Foo<int?>(17);
    Console.WriteLine(fni2.Fmt());
    Foo<int> fi = new Foo<int>(7);
    Console.WriteLine(fi.Fmt());
    Foo<String> fs1 = new Foo<String>(null);
    Console.WriteLine(fs1.Fmt());
    Foo<String> fs2 = new Foo<String>("haha");
    Console.WriteLine(fs2.Fmt());
  }
}

class Foo<T> {
  T x;
  public Foo(T x) { 
    this.x = x;
  }
  
  // This shows how to deal with tests for null in a generic setting
  // where null may mean both `null reference' and `null value of a
  // nullable type'.  Namely, the test (x == null) will always be
  // false if the generic type parameter t is instantiated with a
  // nullable type.  Reason: the null literal will be considered a
  // null reference and x will be boxed if a value type, and hence the
  // comparison will be false...

  public String Fmt() {
    if (x != null)
      return x.ToString();
    else
      return "null";
  }  
}
