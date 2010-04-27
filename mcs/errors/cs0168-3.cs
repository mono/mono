// cs0168-3.cs: The variable `y2' is declared but never used
// Line: 9
// Compiler options: -warn:3 -warnaserror

class CompilerBugDemo
{
	public static object Wrong()
	{
		object y2;
		return null;
	}
}