class M
{
	public static int Main ()
	{
		new SomeGenericClass<int>().FailsToCompile ();
		return 0;
	}
}

class SomeGenericClass<SomeType>
{
	object someValue;
	delegate void SomeHandlerType ();

	void Invoke (SomeHandlerType h)
	{
		h ();
	}

	public void FailsToCompile ()
	{
		Invoke (delegate {
			object someObject = 1;
			Invoke (delegate {
				someValue = someObject;
			});
		});
	}
}

