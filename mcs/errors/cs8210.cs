// CS8210: A tuple literal cannot not contain a value of type `void'
// Line: 8

class XX
{
	public static void Main ()
	{
		var m = (1, Main ());
	}
}