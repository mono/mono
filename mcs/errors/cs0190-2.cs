// CS0190: The __arglist construct is valid only within a variable argument method
// Line: 11

public class Test
{
	public static void Foo (__arglist)
	{
		System.RuntimeArgumentHandle o;
		{
			System.Action a = delegate () {
				o = __arglist; 
			};
			
			a ();
		}
	}
	
	public static void Main ()
	{
		Foo (__arglist ());
	}
}