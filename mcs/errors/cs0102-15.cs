// cs0102.cs: The class 'Test' already contains a definition for 'add_XX'
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

