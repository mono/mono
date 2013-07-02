// CS0534: `C' does not implement inherited abstract member `B.Foo()'
// Line: 16

class A
{
	public virtual void Foo ()
	{
	}
}

abstract class B : A
{
	public abstract override void Foo ();
}

class C : B
{
}