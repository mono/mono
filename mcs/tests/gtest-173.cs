class List <t> {
  public void foo <b> (List <t> x) {
    System.Console.WriteLine ("{0} - {1}", typeof (t), x.GetType ());
  }
}

class C {}
class D {}


class M {
  public static void Main () {
    List <D> x = new List<D> ();
    x.foo <C> (x);
    List <string> y = new List<string> ();
    y.foo <C> (y);
  }
}
