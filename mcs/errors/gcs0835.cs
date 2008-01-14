// CS0835: Cannot convert `lambda expression' to an expression tree of non-delegate type `string'
// Line: 10

using System.Linq.Expressions;

class C
{
	public void Foo ()
	{
		Expression<string> e = () => "a";
	}
}
