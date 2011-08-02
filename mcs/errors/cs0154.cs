// CS0154: The property or indexer `A.name' cannot be used in this context because it lacks the `get' accessor
// Line: 21

public class A
{
	public string name 
	{
		set
		{
			name = value;
		}
	}
}

public class B
{
	public static void Main ()
	{
		A a = new A ();
		string b = a.name;
	}
}
			
