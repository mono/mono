// CS0626: `C.this[int]' is marked as an external but has no DllImport attribute. Consider adding a DllImport attribute to specify the external implementation
// Line: 7
// Compiler options: -warnaserror -warn:1

class C
{
	public extern char this[int index]
	{
		get;
	}
}