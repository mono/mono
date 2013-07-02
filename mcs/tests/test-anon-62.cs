// Bug #79702
public delegate void FooHandler();

public class X
{
	private string a;

	public X(string a)
	{
		this.a = a;
	}

	public static void Main()
	{
	}
}

public class Y : X
{
	private Z a;
	
	public Y(Z a) : base(a.A)
	{
		this.a = a;

		FooHandler handler = delegate {
			a.Hello();
		};
	}
}

public class Z
{
	public string A;
	
	public void Hello ()
	{
	}
}
