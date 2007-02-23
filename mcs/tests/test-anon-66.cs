public class Test
{
	public delegate void TestEventHandler ();
	public event TestEventHandler testEvent;

	public event TestEventHandler TestEvent
	{
		add
		{
			TestEventHandler fun = delegate () { value (); };
			fun ();
		}
		remove { }
	}
	
	public static void Main () { }
}
