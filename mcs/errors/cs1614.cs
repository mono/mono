// cs1614.cs: 'A': is ambiguous;  use either '@A' or 'AAttribute'
// Line: 6
// Bug #56456

using System;
public class A : Attribute {
	[A]
	public static void Main() {
	}
}
public class AAttribute : Attribute {}
