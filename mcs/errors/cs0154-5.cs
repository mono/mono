// CS0154: The property or indexer `BugReport.MyProperty' cannot be used in this context because it lacks the `get' accessor
// Line: 16

static class BugReport
{
	static float MyProperty {
		set { }
	}

	static void MyExtension (this float val)
	{
	}

	public static void Main ()
	{
		MyProperty.MyExtension ();
	}
}

