// CS0834: A lambda expression with statement body cannot be converted to an expresion tree
// Line: 12

using System.Linq.Expressions;

class C
{
	delegate void D ();
	
	public void Foo ()
	{
		Expression<D> e = () => { };
	}
}
