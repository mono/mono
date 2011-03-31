// CS1540: Cannot access protected member `AAttribute.AAttribute(int)' via a qualifier of type `AAttribute'. The qualifier must be of type `BAttribute' or derived from it
// Line: 17

using System;

public class AAttribute : Attribute
{
	public AAttribute ()
	{
	}

	protected AAttribute (int a)
	{
	}
}

[AAttribute (5)]
public class BAttribute : AAttribute
{
	public BAttribute () : base ()
	{
	}
	
	public BAttribute (int a) : base (a)
	{
	}
}
