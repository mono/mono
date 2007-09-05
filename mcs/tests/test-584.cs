public class Style
{
	public static Style CurrentStyle
	{
		get { return null; }
		set { }
	}

	private static bool LoadCurrentStyle ()
	{
		return ((CurrentStyle = Load ()) != null);
	}

	public static Style Load ()
	{
		return null;
	}
	
	public static int Main ()
	{
		return LoadCurrentStyle () ? 1 : 0;
	}	
}
