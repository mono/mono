// cs1717-5.cs: Assignment made to same variable; did you mean to assign something else?
// Line: 9
// Compiler options: -warnaserror -warn:3

class A
{
	public A ()
	{
		int a = a = 5;
	}
}