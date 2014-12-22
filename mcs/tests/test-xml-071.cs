// Compiler options: -doc:xml-071.xml

interface X<out TOutput>
{
	TOutput Consume (Y<TOutput> a);
}

interface Y<in TInput>
{
}

interface Z<in TInput, out TOutput> : Y<TInput>, X<TOutput>
{
}

class Test<T> : Z<T, T[]>
{
	/// <summary>This is the consume method.</summary>
	T[] X<T[]>.Consume (Y<T[]> target)
	{
		return null;
	}
}

class Program
{
	static void Main ()
	{
	}
}
