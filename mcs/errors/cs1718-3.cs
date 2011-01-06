// CS1718: A comparison made to same variable. Did you mean to compare something else?
// Line: 10
// Compiler options: -warnaserror -warn:3

class C
{
	public static void Main () 
	{ 
		int? a = 20;
		if (a > a) {
			return;
		}
	}
}
 
