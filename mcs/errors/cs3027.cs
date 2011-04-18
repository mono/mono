// CS3027: `I' is not CLS-compliant because base interface `I2' is not CLS-compliant
// Line: 17
// Compiler options: -warn:1 -warnaserror

using System;
[assembly: CLSCompliant (true)]

public interface I1
{
}

[CLSCompliant (false)]
public interface I2
{
}

public interface I: I1, I2
{
}

public class Foo
{
	static void Main () {}
}
