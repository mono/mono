// The standard says this doesn't have to have the 'sealed' modifier
public partial class Foo
{
	public string myId;
}

public sealed partial class Foo
{
	public string Id { get { return myId; } }
}

public class PartialAbstractCompilationError
{
	public static void Main ()
	{
		if (typeof (Foo).IsAbstract || !typeof (Foo).IsSealed)
			throw new System.ApplicationException ();
	}
}

