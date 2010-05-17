public class Prop
{
}

public interface I
{
	void Foo (int[] i, bool b);
}

internal static class HelperExtensions
{
	public static void Foo (this I from, I to)
	{
	}
}

public class C
{
	public I Prop {
		get { return null; }
	}
	
	public int[] Loc {
		get { return null; }
	}
	
	void Test ()
	{
		Prop.Foo (null);
	}
	
	public static void Main ()
	{
	}
}
