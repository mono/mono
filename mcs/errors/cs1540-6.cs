public class A {
	public A ()
	{
	}

	protected A (A a)
	{
	}
}

public class B : A {
	public B () : base ()
	{
	}
	
	public B (A a) : base (a)
	{
	}
	
	public A MyA {
		get {
			A a = new A (this);
			return a;
		}
	}
}
