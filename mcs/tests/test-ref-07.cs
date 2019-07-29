// Compiler options: -langversion:latest

public struct TestMain
{
	public static void Main () => Test.MainMethod();
}

public readonly ref partial struct Test
{
	public static void MainMethod ()
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
