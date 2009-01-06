// CS0626: `A.~A()' is marked as an external but has no DllImport attribute. Consider adding a DllImport attribute to specify the external implementation
// Line: 7
// Compiler options: -warnaserror

public sealed class A
{
	extern ~A ();
}
