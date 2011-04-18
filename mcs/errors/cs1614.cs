// CS1614: `A' is ambiguous between `A' and `AAttribute'. Use either `@A' or `AAttribute'
// Line: 6
// Bug #56456

using System;
public class A : Attribute {
	[A]
	public static void Main() {
	}
}
public class AAttribute : Attribute {}
