// cs1718.cs: Comparison made to same variable; did you mean to compare something else?
// Line: 10
// Compiler options: -warnaserror -warn:3

class C
{
	public static void Main () 
	{ 
		int a = 20;
		if (a > a) {
			return;
		}
	}
}
 
