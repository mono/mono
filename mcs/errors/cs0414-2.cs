// CS0414: The private field `X.o' is assigned but its value is never used
// Line: 14
// Compiler options: -warnaserror -warn:3

partial class X
{
	public static void Main ()
	{
	}
}

partial class X
{
	int o = 4;
}
