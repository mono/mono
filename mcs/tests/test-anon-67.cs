public class ClassOne
{
	public delegate string ReturnStringDelegate ();

	public ClassOne (ReturnStringDelegate d)
	{
	}

	public ClassOne (string s)
		: this (new ReturnStringDelegate (delegate () { return s; }))
	{
	}

	public static void Main () { }
}