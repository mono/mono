// Compiler options: -doc:xml-071.xml

namespace N
{
	public class G<U>
	{	
	}
}

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

class Test<T> : Z<T, N.G<T[][,,]>>
{
	/// <summary>This is the consume method.</summary>
	N.G<T[][,,]> X<N.G<T[][,,]>>.Consume (Y<N.G<T[][,,]>> target)
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