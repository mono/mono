// CS0831: An expression tree may not contain a base access
// Line: 20

using System;
using System.Linq.Expressions;

class B
{
	protected B this [int i] {
		get {
			return null;
		}
	}
}

class C : B
{
	public void Test ()
	{
		Expression<Func<B>> e = () => base [8];
	}
}
