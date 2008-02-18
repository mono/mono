// CS0748: All lambda parameters must be typed either explicitly or implicitly
// Line: 11


public class C
{
	delegate void E ();
	
	public static void Main ()
	{
		e = (ref int E, v) => {};
	}
}
