// CS8082: An argument to nameof operator cannot include sub-expression
// Line: 9

class C
{
	void Foo ()
	{
		object o = null;
		var s = nameof (o.ToString ().Equals);
	}
}