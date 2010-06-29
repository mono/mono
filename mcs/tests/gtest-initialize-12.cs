class C : B
{
	public override int Foo { set {} }
}

abstract class B
{
	public abstract int Foo { set; }
}

public class Test
{
	static int Foo { set {} }

	public static void Main ()
	{
		var c = new C () { Foo = 1 };
	}
}