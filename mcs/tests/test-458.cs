// test-458.cs : for bug #75723
using System;


// The attribute
internal class MyAttr : Attribute {
  internal MyAttr() { }
  internal MyAttr(Type type) { }
  internal MyAttr(string name) { }
  internal MyAttr(int i) { }
}

// The Classes
[MyAttr()]
internal class ClassA  { }

[MyAttr(typeof(string))]
internal class ClassB  { }

[MyAttr("abc")]
internal class ClassC  { }

[MyAttr(3)]
internal class ClassD  { }

// Just to compile
internal class Top  { 
  public static void Main (string[] strgs){}
}
