// cs0154.cs: The property 'name' can not be used in this context because
//            it lacks a get accessor.
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
			
