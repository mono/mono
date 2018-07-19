// Compiler options: -langversion:latest

public readonly ref partial struct Test
{
	public static void Main ()
	{
		var m = new Test ();
		m.Method ();
	}

	Test Method ()
	{
		return new Test ();
	}
}

ref partial struct Test
{

}

ref struct Second
{
	Test field;
}

public abstract class P
{
	public abstract Test Span { get; }
}

public interface II
{
	Test Span { get; }
}
