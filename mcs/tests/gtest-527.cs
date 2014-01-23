public static class CoalescingWithGenericsBug
{
	class Service { public void Foo () { } }

	static T Provide<T> () where T : class
	{
		return FindExisting<T> () ?? System.Activator.CreateInstance<T> ();
	}

	static T FindExisting<T> () where T : class
	{
		return null;
	}

	public static int Main ()
	{
		Provide<Service> ().Foo ();
		return 0;
	}
}
