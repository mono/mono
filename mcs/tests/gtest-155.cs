public interface IBase
{
	void DoSomeThing();
}

public interface IExtended : IBase
{
	void DoSomeThingElse();
}

public class MyClass<T> where T: IExtended, new()
{
	public MyClass()
	{
		T instance = new T();
		instance.DoSomeThing();
	}
}

class X
{
	public static void Main ()
	{ }
}
