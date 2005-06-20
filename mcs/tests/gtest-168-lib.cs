// Compiler options: -t:library
using System;

interface IComp<a>
{ }

class opt<c>
{ }

abstract class Func<p1,r>
{
	public r apply (p1 x)
	{
		throw new System.Exception ();
	}
}

class NemerleMap<a,b>
	where a : IComp<a>
{
	public opt<b> Find (a k)
	{
		return null;
	}

	public void Fold<d> (a y)
	{
	}

	class lambda<d,aa,bb> : Func<aa,aa>
		where aa : IComp<aa>
	{
		public aa apply2 (aa x)
		{
			return x;
		}
	}
}
