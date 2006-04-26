// cs1706-2.cs: Anonymous methods are not allowed in the attribute declaration
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