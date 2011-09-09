struct Foo
{
	public Foo (object newValue)
		: this ()
	{
		this.NewValue = newValue;
	}

	public object NewValue
	{
		get;
		private set;
	}
}

class C
{
	public static void Main ()
	{
	}
}
