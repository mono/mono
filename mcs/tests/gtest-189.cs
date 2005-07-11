interface IFoo {}
class Bar : IFoo {}

class Cont<T> {
  T f;
  public Cont(T x) { f = x; }
  public override string ToString ()
  {
    return f.ToString ();
  }
}

class M {
  public static void Main ()
  {
    Cont<IFoo> c = new Cont<IFoo> (new Bar ());
    System.Console.WriteLine (c);
  }
}
