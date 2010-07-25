// CS1706: Anonymous methods and lambda expressions cannot be used in the current context
// Line: 14

public delegate void Proc();

public class AAttribute : System.Attribute
{
	public AAttribute(Proc p)
	{ }
}

public class Class
{
	[A((object)delegate { return; })]
	public void Foo()
	{
	}
} 
