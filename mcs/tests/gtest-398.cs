using System;


public interface IFace
{
	void Tst (IFace b);
}

public delegate string ToStr (string format, IFormatProvider format_provider);


public class GenericClass<T> where T : IFormattable
{
	T field;

	public GenericClass (T t)
	{
		this.field = t;
	}

	public void Method ()
	{
		ToStr str = new ToStr (field.ToString);

		Console.WriteLine (str ("x", null));
	}

	public void Test (T t) { }
}



public class Foo
{
	public static void Main (string [] args)
	{
		GenericClass<int> example = new GenericClass<int> (99);
		example.Method ();
	}
}

