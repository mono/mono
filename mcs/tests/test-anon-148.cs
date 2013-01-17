public delegate TResult Func<TResult> ();

public delegate void GeneratorNext<T> (ref T current);

public class GeneratorEnumerable<T>
{
	public GeneratorEnumerable (Func<GeneratorNext<T>> next) { }
}

public class GeneratorExpression { }

public class GeneratorInvoker
{
	public GeneratorInvoker (GeneratorExpression generator) { }
	public void Invoke<T> (ref T current) { }
}

public static class Interpreter
{
	public static object InterpretGenerator<T> (GeneratorExpression generator)
	{
		return new GeneratorEnumerable<T> (
			() => new GeneratorInvoker (generator).Invoke
		);
	}

	public static int Main ()
	{
		InterpretGenerator<int> (new GeneratorExpression ());
		return 0;
	}
}
