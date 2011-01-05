// CS0831: An expression tree may not contain a base access
// Line: 14

using System;
using System.Linq.Expressions;

class B
{
	protected int Core ()
	{
		return 4;
	}
}

class C : B
{
	public void Test ()
	{
		Expression<Func<int>> e = () => base.Core ();
	}
}
