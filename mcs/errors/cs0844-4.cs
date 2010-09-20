// CS0844: A local variable `s' cannot be used before it is declared. Consider renaming the local variable when it hides the member `C.s'
// Line: 10

class C
{
	const string s = "s";

	public void Test ()
	{
		s = "x";
		string s = "a";
	}
}
