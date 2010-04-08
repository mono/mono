public class Tests
{
	string s;
	
	private Tests (string arg = "long")
	{
		this.s = arg;
	}
	
	public Tests (int other)
	{
	}
	
	public static int Main ()
	{
		var v = new Tests ();
		if (v.s != "long")
			return 1;
		
		return 0;
	}
}
