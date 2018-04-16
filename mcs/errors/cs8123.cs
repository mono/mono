// CS8123: The tuple element name `b' is ignored because a different name or no name is specified by the target type `(long, string)'
// Line: 9
// Compiler options: -warnaserror

static class X
{
	static (long a, string x) Test ()
	{
		return (b: 1, "");
	}
}