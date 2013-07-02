using System;
using System.Reflection;

public class Foo<S>
{ }

public struct Bar<T>
{ }

public class Test<U>
{
	public static void Func (U u)
	{
		Console.WriteLine (u);
	}
}

class X
{
	static void Test (Type t, object arg)
	{
		MethodInfo mi = t.GetMethod ("Func");
		mi.Invoke (null, new object[] { arg });
	}

	public static void Main ()
	{
		Test (typeof (Test<Foo<int>>), new Foo<int> ());
		Test (typeof (Test<Bar<int>>), new Bar<int> ());
		Test (typeof (Test<Bar<string>>), new Bar<string> ());
		Test (typeof (Test<Foo<DateTime>>), new Foo<DateTime> ());
		Test (typeof (Test<DateTime>), DateTime.Now);
		Test (typeof (Test<string>), "Hello");
	}
}
