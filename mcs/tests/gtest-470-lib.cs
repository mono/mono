// Compiler options: -t:library

public class A
{
	public virtual void Foo<T> ()
	{
	}
}

public class B : A
{
	public override void Foo<T> ()
	{
	}
}
