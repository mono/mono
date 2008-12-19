// This code issues CS3014 error in csc ersion 1.1

using System;

[assembly: CLSCompliant(false)]

[CLSCompliant(true)]
public class Foo {
	public static void Main () {}
}