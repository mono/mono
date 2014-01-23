using System;

public class Class1<T>
	where T : MyType
{
	public void MethodOfClass1 (T a, MyType b)
	{
		a.MethodOfMyBaseType ();
	}
}

public class MyType : MyBaseType
{
	public override void MethodOfMyBaseType ()
	{
	}
}

public abstract class MyBaseType
{
	public abstract void MethodOfMyBaseType ();
}

class X
{
	public static void Main ()
	{ }
}
