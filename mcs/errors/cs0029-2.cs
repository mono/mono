public class Foo {
  enum MyEnumType { MyValue }

  public void Bar ()
  {
    uint my_uint_var;
    switch (my_uint_var) {
    case MyEnumType.MyValue:
      break;
    default:
      break;
    }
  }

  static void Main () {}
}






