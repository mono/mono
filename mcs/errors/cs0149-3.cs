// CS0149: Method name expected
// Line: 10

delegate void D ();

public class MainClass
{
	public static void Main ()
	{
		D delegateInstance = new D (Main, null);
	}
}
