// CS0188: The `this' object cannot be used before all of its fields are assigned to
// Line: 16

struct Foo
{
	int bar;
	int baz;

	public int this [int i] {
		get { return bar; }
	}

	public Foo (int baz)
	{
		this.baz = baz;
		bar = this [2] - 1;
	}
}
