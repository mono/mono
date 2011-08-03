// CS0441: `Foo': a class cannot be both static and sealed
// Line: 3
public sealed partial class Foo
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

