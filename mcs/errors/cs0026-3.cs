// CS0026: Keyword `this' is not valid in a static property, static method, or static field initializer
// Line: 7

class A : B
{
	public A () 
		: base (this)
	{
	}
}

class B
{
	public B (B b)
	{
	}
}