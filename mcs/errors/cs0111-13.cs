// cs111.cs : Class 'Test' already defines a member called 'add_XX' with the same parameter types
// Line : 12

public class Test
{
	public delegate void MyEvent ();
	public event MyEvent XX {
		add { }
		remove { }
	}
        
	public void add_XX (MyEvent e) { return; }
}



