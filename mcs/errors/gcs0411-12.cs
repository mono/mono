// CS0411: The type arguments for method `Foo<U>.DoSomething<U>(System.Func<U,U>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 15

using System;

public class Foo<T>
{
	public void DoSomething<U> (Func<U, T> valueExpression) { }
}

public class Bar
{
	protected void DoAnything<T, U> (U value)
	{
		new Foo<U> ().DoSomething (value);
	}
}
