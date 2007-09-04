delegate void D1 (int i);
delegate void D2 (out string s);

public class C
{
	static void T (D1 d)
	{
	}
	
	static void T (D2 d)
	{
	}
	
	public static void Main ()
	{
		T (delegate { });
		T (delegate (out string o) { o = "ab"; });
	}
}
