using System;
using System.Reflection;

public class TestAttribute : Attribute
{
    public Type type;

    public TestAttribute(Type type)
    {
        this.type = type;
    }
}

class C<T>
{
    [Test(typeof(C<string>))]
    public static void Foo()
    {
    }
}

public class C
{
	public static int Main ()
	{
		MethodInfo mi = typeof (C<>).GetMethod ("Foo");
		object[] a = mi.GetCustomAttributes (false);
		if (((TestAttribute)a[0]).type.ToString() != "C`1[System.String]")
			return 1;

		Console.WriteLine("OK");
		return 0;
	}
}