// CS0831:  An expression tree may not contain a base access
// Line: 14

using System;
using System.Linq.Expressions;

class B
{
	protected bool Core {
		get {
			return true;
		}
	}
}

class C : B
{
	public void Test ()
	{
		Expression<Func<bool>> e = () => base.Core;
	}
}
