using System.Collections.Generic;

// comment this line to see another bug in gmcs (unrelated)
interface IB { bool foo (); }


class B : IB { public bool foo () { return true; } }

interface Filter <T> where T : IB {
  T Is (IB x);

}

struct K : IB {
  public bool foo () { return false; }

}

class MyFilter : Filter <K> {
  public K Is (IB x) { return new K(); }
}

class MyBFilter : Filter <B> {
  public B Is (IB x) { return new B(); }
}

class M {
 
  static List<T> foo1 <T> (Filter <T> x) where T : IB {
    List <T> result = new List <T>();
    T maybe = x.Is (new B());
    if (maybe != null)
      result.Add (maybe);
    return result;
  }
 
  public static void Main () {
       MyFilter m = new MyFilter ();
        System.Console.WriteLine (foo1 <K> (m).Count);
        MyBFilter mb = new MyBFilter ();
        System.Console.WriteLine (foo1 <B> (mb).Count);
  }
}
