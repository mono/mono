// Compiler options: -langversion:experimental
class D(string arg) : Base (arg)
{
}

abstract class Base (object obj)
{
	public string Prop { get; } = obj.ToString ();
}

class X
{
	public static int Main ()
	{
		var d = new D ("test");
		if (d.Prop != "test")
			return 1;

		return 0;
	}
}