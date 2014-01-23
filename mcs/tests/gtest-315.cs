// Bug #80731
public partial class A<T>
{
	public class B {}
}

public partial class A<T>
{
	public B Test;
}

class X
{
	public static void Main ()
	{
		A<int> a = new A<int> ();
		a.Test = new A<int>.B ();
	}
}
