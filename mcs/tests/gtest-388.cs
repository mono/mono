using System;

class Data {
  public int Value;
}

class Foo {
  static void f (Data d)
  {
    if (d.Value != 5)
      throw new Exception ();
  }

  public static void Main ()
  {
    Data d;
    f (d = new Data () { Value = 5 });
  }
}
