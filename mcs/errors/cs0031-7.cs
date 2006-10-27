// CS0031: Constant value `42' cannot be converted to a `string'
// Line: 5

class A {
  public static implicit operator string (A a) { return 42; }
}
