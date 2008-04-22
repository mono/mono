public class Test
{
	delegate void D ();
	static D d;
	
	static public void TestFunc ()
	{
		return;
		
		string testStr;

		d += delegate ()
		{
        	testStr = "sss";
		};
   }

	public static void Main (string[] args)
	{
		TestFunc ();
	}
}
