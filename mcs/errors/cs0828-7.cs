// CS0827: An anonymous type property `Prop' cannot be initialized with `(int, method group)'
// Line: 9

class XX
{
	public static void Main ()
	{
		var m = new {
			Prop = (1, Main)
		};
	}
}