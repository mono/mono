class C <A> {
  public static void foo<B> (C<B> x)
  {
    D.append (x);
  }
}

class D {
  public static void append<A> (C<A> x)
  {
  }

  public static void Main ()
  {
    C<object>.foo<int> (null);
  }
}
