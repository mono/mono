public interface I
{
	dynamic D ();
	object D2 ();
}

public class C : I
{
	public object D ()
	{
		return null;
	}
	
	public dynamic D2 ()
	{
		return null;
	}
	
	public static int Main ()
	{
		return 0;
	}
}

abstract class AC
{
	public abstract void Foo(dynamic[] d);
}

class BC : AC
{
	public override void Foo(params object[] d)
	{
	}
}
