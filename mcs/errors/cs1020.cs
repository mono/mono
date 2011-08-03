// CS1020: Overloadable binary operator expected
// Line: 19


public class MyClass {

	public int this[int ndx] 
	{
		get { }
		set { }
	}

	public event EventHandler Click 
	{
		add { } 
		remove { }
	}

	public static MyClass operator++ (MyClass i, MyClass j)
	{
	
	}

	public static implicit operator MyClass (Object o)
	{

	}
}
