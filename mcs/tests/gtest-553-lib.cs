// Compiler options: -t:library

public class A
{
	public class C<T> : IA
	{
	}
	
	private interface IA
	{
	}
}

public class B
{
	public class C<T> : IA<T>
	{
	}
	
	private interface IA<T>
	{
	}
}
