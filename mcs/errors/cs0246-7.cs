// cs0246-7.cs: Neither 'A' nor 'AAttribute' is an attribute class
// Line: 6

using System;
public class A {
	[A]
	public static void Main() {
	}
}
public class AAttribute {}
