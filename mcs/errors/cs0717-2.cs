// CS0717: `System.Console' is not a valid constraint. Static classes cannot be used as constraints
// Line: 6

abstract class C
{
	public abstract void Foo<U> () where U : System.Console;
}
