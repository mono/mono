// CS1503: Argument `#1' cannot convert `System.RuntimeArgumentHandle' expression to type `__arglist'
// Line: 10

using System;

class C
{
	void Foo (__arglist)
	{
		InstanceArgList (__arglist);
	}
	
	int InstanceArgList (__arglist)
	{
		return 54;
	}
}
