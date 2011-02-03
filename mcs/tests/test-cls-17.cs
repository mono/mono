// Compiler options: -warnaserror

// This code used to issue CS3014 errors in csc version 1.1

using System;

[assembly: CLSCompliant(false)]

[CLSCompliant(true)]
public enum E
{
	Value
}

[CLSCompliant(true)]
public class Foo
{
	public E e;
	public static void Main () {}
}