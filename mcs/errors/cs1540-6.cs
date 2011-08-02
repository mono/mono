// CS1540: Cannot access protected member `A.A(A)' via a qualifier of type `A'. The qualifier must be of type `B' or derived from it
// Line: 25

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
