// CS0185: `method group' is not a reference type as required by the lock statement
// Line: 15

class MainClass
{
	public static void Main ()
	{
		lock (Bar.Buzz) {
		}
	}
}

class Bar
{
	internal void Buzz ()
	{
	}
}