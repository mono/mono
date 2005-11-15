using System;
using System.Reflection;

public struct Container<T>
{
	public T content;

	public Container (T content)
	{
		this.content = content;
	}
}

public class A
{
	public Container<long> field;

	public A ()
	{
		field = new Container<long> (0xdeadbeaf);
	}
}

public class M
{
	public static int Main()
	{
		A a = new A();

		if (a.field.content != 0xdeadbeaf)
			return 1;

		FieldInfo fi = a.GetType().GetField("field");
		object o = fi.GetValue (a);
		Container<long> unboxed = (Container<long>) o;

		if (unboxed.content != 0xdeadbeaf)
			return 2;

		return 0;
	}
}
