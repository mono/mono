public class C
{
	void Method ()
	{
	}
	
	public static int Main ()
	{
		dynamic d = new C ();
		var a = new [] { d, (object) null };
		a[0].Method();
		return 0;
	}
}