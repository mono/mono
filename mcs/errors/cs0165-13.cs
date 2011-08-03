// CS0165: Use of unassigned local variable `foo'
// Line: 9

struct Rectangle { int x; public int X { set { } } }
public class Foo {
  public static void Main (string[] args)
  {
    Rectangle foo;
    foo.X = 5;
  }
}
