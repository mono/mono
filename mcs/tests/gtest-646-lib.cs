// Compiler options: -t:library

public class CompilerBug<T>
{
	public int Foo (CompilerBug<T> p1, CompilerBug<T> p2)
	{
		return 1;
	}

	public int Foo (CompilerBug<object> p1, CompilerBug<T> p2)
	{
		return 2;
	}
}