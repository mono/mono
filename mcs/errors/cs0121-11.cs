// CS0121: The call is ambiguous between the following methods or properties: `IA.Foo()' and `IB.Foo()'
// Line: 27

interface IA
{
	void Foo ();
}

interface IBB : IB
{
}

interface IB
{
	int Foo ();
}

interface IC : IA, IBB
{
}

public class Program
{
	static void Main ()
	{
		IC i = null;
		i.Foo ();
	}
}
