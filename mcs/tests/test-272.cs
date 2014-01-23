enum Foo { Bar };

class BazAttribute : System.Attribute 
{
	public BazAttribute () {}
	public BazAttribute (Foo foo1) {}
	public Foo foo2;
	public Foo foo3 { set {} get { return Foo.Bar; } }
};

class Test 
{
	[Baz (Foo.Bar)]        void f0() {}
	[Baz ((Foo) 1)]        void f1() {}
	[Baz (foo2 = (Foo) 2)] void f2() {}
	[Baz (foo3 = (Foo) 3)] void f3() {}
	public static void Main() { }
}
