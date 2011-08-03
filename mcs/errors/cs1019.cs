// CS1019: Overloadable unary operator expected
// Line: 18

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

	public static MyClass operator/ (MyClass i)
	{
	
	}

	public static implicit operator MyClass (Object o)
	{

	}
}
