using System;

public class AAttribute : Attribute
{
	[AAttribute (1)]
	public class BAttribute : AAttribute
	{
		public BAttribute ()
			: base ()
		{
		}

		public BAttribute (int a)
			: base (a)
		{
		}
	}

	public AAttribute ()
	{
	}

	protected AAttribute (int a)
	{
	}

	public static int Main ()
	{
		typeof (BAttribute).GetCustomAttributes (true);
		return 0;
	}
}
