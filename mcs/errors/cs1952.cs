// CS1952: An expression tree cannot contain a method with variable arguments
// Line: 11

using System;
using System.Linq.Expressions;

class C
{
	void Foo ()
	{
		Expression<Func<int>> e = () => InstanceArgList (__arglist (0));
	}
	
	int InstanceArgList (__arglist)
	{
		return 54;
	}
}
