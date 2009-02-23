public class A
{
	public delegate ADelegate ADelegate (bool ok);

	public static ADelegate Delegate2 (bool ok)
	{
		return ok ? Delegate2 : (ADelegate) null;
	}

	public static void Main ()
	{
	}
}
