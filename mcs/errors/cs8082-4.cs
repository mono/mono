// CS8082: An argument to nameof operator cannot include sub-expression
// Line: 8

class C
{
	void Foo ()
	{
		var s = nameof (1.ToString);
	}
}