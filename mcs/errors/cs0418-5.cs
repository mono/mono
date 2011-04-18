// CS0418: `Foo': an abstract class cannot be sealed or static
// Line: 3
public abstract partial class Foo
{
	public string myId;
}

public static partial class Foo
{
	public string Id { get { return myId; } }
}

public class PartialAbstractCompilationError
{
	public static void Main ()
	{
		System.Console.WriteLine (typeof (Foo).IsSealed);
		System.Console.WriteLine (typeof (Foo).IsAbstract);
	}
}

