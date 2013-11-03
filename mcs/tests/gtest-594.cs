using System;

public interface IFoo<U>
{
	void Foo<T> () where T : C;
}

public class C : IA
{
}

public interface IA
{
}

class Y : IFoo<int>
{
	public void Foo<T> () where T : C
	{
	}
}

class X
{
	public static void Main()
	{
		new Y ();
	}
}