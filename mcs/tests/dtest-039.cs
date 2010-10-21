class A
{
	public virtual object Foo ()
	{
		return null;
	}
	
	public virtual object[] FooArray ()
	{
		return null;
	}
	
	internal virtual object Prop {
		get {
			return 9;
		}
		set {
		}
	}
	
	public virtual object[] PropArray {
		get {
			return null;
		}
	}
	
	internal virtual object this [int arg] {
		get {
			return 5;
		}
		set {
		}
	}
}

class B : A
{
	public override dynamic Foo ()
	{
		return 5;
	}
	
	public override dynamic[] FooArray ()
	{
		return new object [] { 'a', 'b' , 'z' };
	}
	
	internal override dynamic Prop {
		set {
		}
	}
	
	public override dynamic[] PropArray {
		get {
			return new object [] { 'a', 'b' };
		}
	}
	
	internal override dynamic this [int arg] {
		set {
		}
	}
}

class MainClass : B
{
	void Test ()
	{
		char ch;
		ch = Prop;
		ch = PropArray [1];
		ch = this [1];
	}
	
	public static int Main ()
	{
		B b = new B ();
		int res;
		res = b.Foo ();
		if (res != 5)
			return 1;
		
		char ch = b.FooArray () [1];
		if (ch != 'b')
			return 2;
		
		++b.Prop;
		res = b.Prop;
		if (res != 9)
			return 3;
		
		ch = b.PropArray [1];
		if (ch != 'b')
			return 4;
		
		res = b [3];
		if (res != 5)
			return 5;
		
		return 0;
	}
}