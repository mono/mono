// CS0431: Alias `A' cannot be used with `::' since it denotes a type. Consider replacing `::' with `.'
// Line: 10

using A = Test;

class Test
{
	static void Main ()
	{
		A::P p;
	}
}
