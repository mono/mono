// CS0649: Field `C.s' is never assigned to, and will always have its default value `null'
// Line: 7
// Compiler options: -warnaserror -warn:4

class C
{
	int? s;
	
	void Test ()
	{
		System.Console.WriteLine (s == null);
	}
}
