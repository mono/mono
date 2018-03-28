using System;

interface IFoo<T>
{
	T Call ();
}

class C1
{
	public void Foo<T> (IFoo<T> t) where T : class
	{
		t?.Call ();
		var x = t?.Call ();
	}

	public void Foo2<T> (IFoo<T> t)
	{
		t?.Call ();
	}	
}

class C2<T> where T : class
{
	C2<T> i;
	T field;

	public void Foo ()
	{
		var x = i?.field;
	}
}

class Program
{
	static void Test<T>(Func<T> func) where T : struct
	{
		var r = func?.Invoke ();
	}

	static void Test2<T>(Func<T> func)
	{
		func?.Invoke ();
	}

	static void Main()
	{
		new C1 ().Foo<Program> (null);
		new C1 ().Foo2<Program> (null);

		new C2<string> ().Foo ();

		Test (() => 1);
		Test (() => 2);
	}
}