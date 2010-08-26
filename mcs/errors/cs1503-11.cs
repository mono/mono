// CS1503: Argument `#1' cannot convert `null' expression to type `__arglist'
// Line: 8

class C
{
	void Foo ()
	{
		InstanceArgList (null);
	}
	
	int InstanceArgList (__arglist)
	{
		return 54;
	}
}
