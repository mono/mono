class MYAttr : System.Attribute {
}

[MYAttr]
partial class A {
  static void Main () {
  }
}

partial class A {
  int i;
}
