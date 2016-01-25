class X
{
	public object MyChildObject;
}

interface ISomeInterface
{
}

class MainClass
{
	public static void Main ()
	{
		X myObject = null;
		var x = (myObject?.MyChildObject is ISomeInterface);
	}
}
