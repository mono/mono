// CS0243: Conditional not valid on `MyClass.GetHashCode()' because it is an override method
// Line: 6

public class MyClass
{
	[System.Diagnostics.Conditional ("WOOHOO")]
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
}
