public class C
{
	static object ViewState;
	
	public static void Main ()
	{
		var v1 = (bool)ViewState != null;
		var v2 = null != (bool)ViewState;
	}
}
