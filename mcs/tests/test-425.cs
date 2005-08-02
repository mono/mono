// Test for bug #57047
using System;
public class A : Attribute {
	[@A]
	public static void Main() {
	}
}
public class AAttribute : Attribute {}

