// CS0082: A member `Test.add_XX(Test.MyEvent)' is already reserved
// Line: 12

public class Test
{
	public delegate void MyEvent ();
	public event MyEvent XX {
		add { }
		remove { }
	}
        
	public void add_XX (MyEvent e) { return; }
}

