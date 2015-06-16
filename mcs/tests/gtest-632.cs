public class BaseClass<TSource>
{
	public void DoStuff<TInput> (TInput stuff) where TInput: TSource 
	{
	}
}

public class MyClass: BaseClass<TInterface>, MyInterface
{
	public static void Main ()
	{
	}
}

public class TInterface
{
}

public interface MyInterface 
{
	void DoStuff<TInput> (TInput stuff) where TInput: TInterface;
}