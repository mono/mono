// Compiler options: -t:library

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
}
