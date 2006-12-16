// CS3005: Identifier `Foo.main()' differing only in case is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror

using System;
[assembly: CLSCompliant(false)]

[CLSCompliant(true)]
public class Foo {
	public static void Main () {}
	public static void main () {}
}