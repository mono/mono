// CS1718: A comparison made to same variable. Did you mean to compare something else?
// Line: 12
// Compiler options: -warnaserror -warn:3

class A
{
	delegate void D ();
	D d = null;
	
	public A ()
	{
		bool b = d == d;
	}
}