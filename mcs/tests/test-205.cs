using System;

[AttributeUsage (AttributeTargets.All)]
public class A: Attribute {

	public A (object o) {
	}
}
  
[A ((object)AttributeTargets.All)]
public class Test {
        static public void Main() {
	}
}
