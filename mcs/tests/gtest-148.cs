using System;

static class Test1 {
  public class IOp<T> { }
  static void Foo<S,OP>(uint v) where OP : IOp<S> { }
};

static class Test2 {
  public class IOp<T> { }
  static void Foo<T,OP>(uint v) where OP : IOp<T> { }
};

class X
{
	public static void Main ()
	{ }
}
