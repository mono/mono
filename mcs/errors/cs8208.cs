// CS8208: The type `dynamic' pattern matching is not allowed
// Line: 9

static class Program
{
	public static void Main ()
	{
		object o = null;            
		if (o is dynamic res) {
		}
	}
}
