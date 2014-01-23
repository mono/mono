// Using an array of a type parameter.

class Stack<T>
{
	int size;
	T[] data;

	public Stack ()
	{
		data = new T [200];
	}

	public void Push (T item)
	{
		data [size++] = item;
	}

	public T Pop ()
	{
		return data [--size];
	}

	public void Hello (T t)
	{
		System.Console.WriteLine ("Hello: {0}", t);
	}
}

class Test
{
	public static void Main ()
	{
	}
}
