using System;

public sealed class BoundAttribute : System.Attribute
{
	public BoundAttribute(double min, int i)
	{
	}
}

class C
{
    [BoundAttribute (0, 0)]
	int i;

    [BoundAttribute (3, 3)]
    double d;

    public static void Main ()
    {
    }
}
