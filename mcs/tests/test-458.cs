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

internal class Top
{ 
  public static int Main ()
  {
  	if (typeof (ClassA).GetCustomAttributes (false).Length != 1)
  		return 1;
  		
  	return 0;
  }
}
