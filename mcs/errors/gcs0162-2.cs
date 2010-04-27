// CS0162: Unreachable code detected
// Line: 10
// Compiler options: -warnaserror -warn:2

class C
{
	static int Main () 
	{
		while (!new bool {});
		return 1;
	}
}
