// CS3024: Constraint type `I' is not CLS-compliant
// Line: 13
// Compiler options: -warn:1 -warnaserror

using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (false)]
public interface I
{
}

public class C<T> where T : I
{
}

