// CS8082: An argument to nameof operator cannot include sub-expression
// Line: 9

class C
{
	void Foo ()
	{
		dynamic o = null;
		var s = nameof (o.ToString ().Equals);
	}
}