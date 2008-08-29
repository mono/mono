// CS0152: The label `case 0:' already occurs in this switch statement
// Line: 13

// https://bugzilla.novell.com/show_bug.cgi?id=363791

class Test {
  enum Foo { MUL, JL }
  static Foo f;
  public static void Main ()
  {
    switch (f) {
    case Foo.MUL: break;
    case Foo.MUL: break;
    }
  }
}
