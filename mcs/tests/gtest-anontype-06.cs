

// "cast by example" test

class CastByExample
{
	public static void Main()
	{
		object o = new { Foo = "Data" };
		// Cast object to anonymous type
		var typed = Cast(o, new { Foo = "" });
	}

	static T Cast<T>(object obj, T type)
	{
		return (T)obj;
	}
}
