// CS0266: Cannot implicitly convert type `Foo.MyEnumType' to `uint'. An explicit conversion exists (are you missing a cast?)
// Line: 11

public class Foo {
  enum MyEnumType { MyValue }

  public void Bar ()
  {
    uint my_uint_var = 0;
    switch (my_uint_var) {
    case MyEnumType.MyValue:
      break;
    default:
      break;
    }
  }

  static void Main () {}
}






