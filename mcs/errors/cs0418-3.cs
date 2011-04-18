// CS0418: `Foo': an abstract class cannot be sealed or static
// Line: 3
public abstract sealed partial class Foo
{
	public string myId;
}

public class PartialAbstractCompilationError
{
	public static void Main ()
	{
		System.Console.WriteLine (typeof (Foo).IsSealed);
		System.Console.WriteLine (typeof (Foo).IsAbstract);
	}
}

