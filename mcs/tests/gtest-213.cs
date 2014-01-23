public interface SomeInterface
{
	int Foo { get; set; }
}

public struct SomeStruct : SomeInterface
{
	int x;
	public int Foo {
		get { return x; }
		set { x = value; }
	}
}

public class Test
{
	public static void Fun<T> (T t)
		where T : SomeInterface
	{
		if (++t.Foo != 1)
			throw new System.Exception ("not 1");
		if (t.Foo != 1)
			throw new System.Exception ("didn't update 't'");
	}

	public static void Main()
	{
		Fun (new SomeStruct ());
	}
}
