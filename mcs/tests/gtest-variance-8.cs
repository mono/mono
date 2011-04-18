using System;

interface IIn<in T>
{
}

interface IOut<out T>
{
}

class A<T> : IIn<T>, IOut<T>
{
}

class C
{
	public static int Main ()
	{
		IIn<string> a_string = new A<string> ();
		IIn<object> a_object = new A<object> ();

		if (!(a_string is IIn<string>))
			return 1;
		
		if ((a_string is IIn<object>))
			return 2;
		
		if (!(a_object is IIn<string>))
			return 3;
		
		if (!(a_object is IIn<object>))
			return 4;

		IOut<string> b_string = new A<string> ();
		IOut<object> b_object = new A<object> ();

		if (!(b_string is IOut<string>))
			return 10;
		
		if (!(b_string is IOut<object>))
			return 11;
		
		if (b_object is IOut<string>)
			return 12;
		
		if (!(b_object is IOut<object>))
			return 13;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
