
class A<AA>
{
	public virtual AA Foo<U> (U u)
	{
		return default (AA);
	}
}

class B : A<object>
{
	public override dynamic Foo<T> (T t)
	{
		return 'c';
	}
}

public class MainClass
{
	public static int Main ()
	{
		B b = new B ();
		char res = b.Foo<int> (5);
		return 0;
	}
}