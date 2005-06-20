class list <a> {
}

class Nil <a> : list <a> {
  public static Nil <a> single;
  static Nil () {
    single = new Nil <a> ();
  }
}


public class Test {
   public static void Main()  {
     list <int>[,] x = new list<int>[10,10];
     x[0,0] = Nil <int>.single;

   }
}
