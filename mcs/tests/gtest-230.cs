//
// This example is from bug: 72908
//
// Showed a problem with the use of "<" in certain expressions
// that were confusing the parser
//

class A { }
  
public class B {
  static int goo <T1, T2, T3, T4, T5> (int d) { return 3; }

  static void foo (int x) { }
  static void foo (bool h, int x, int y, int j, bool k) { }

  public void Add (object x) { }

  public static void Main () {
    int A = 4;
    int goo1 = 3;
    foo (goo <A, A, A, A, A> (1));
    foo (goo1 <A, A, A, A, A > goo1);
  }
}



