public class Program
{	
	static void Main()
	{
		new Program<object>();
	}
}

public class Program<T>
{
	public Program(Generic<T> generic = null)
	{
	}
}

public class Generic<T>
{	
}
