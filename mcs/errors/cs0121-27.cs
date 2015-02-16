// CS0121: The call is ambiguous between the following methods or properties: `G<int>.Foo()' and `G<string>.Foo()'
// Line: 18

using static G<int>;
using static G<string>;

class G<T>
{
	public static void Foo ()
	{
	}
}

class Test
{
	public static void Main ()
	{
		Foo ();
	}
}