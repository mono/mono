// Compiler options: -t:library

public class Foo<T>
{
	public void Hello (T t)
	{ }
}

public class Bar<T,U> : Foo<U>
{
	public void Test (T t, U u)
	{ }
}
