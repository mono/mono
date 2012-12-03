class MYAttr : System.Attribute {
}

[MYAttr]
partial class A {
  public static void Main () {
  }
}

partial class A {
  int i;
}
