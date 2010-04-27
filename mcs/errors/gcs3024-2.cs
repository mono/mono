// CS3024: Constraint type `A' is not CLS-compliant
// Line: 15
// Compiler options: -warn:1 -warnaserror

using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (false)]
public abstract class A
{
}

public class C
{
	public static void Foo<T>() where T : A
	{
	}
}

