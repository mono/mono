// CS0310: The type `string' must have a public parameterless constructor in order to use it as parameter `T' in the generic type or method `Program.Ret<T>()'
// Line: 10

public static class Program
{
	static void Main ()
	{
		Ret<string> ();
	}

	static T Ret<T> () where T : new ()
	{
		return new T ();
	}
} 
