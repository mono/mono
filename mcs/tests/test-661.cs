// Compiler options: -r:test-661-lib.dll

public class Test
{
	public static void Main ()
	{
	}
	
	public void TestMethod ()
	{
		SummaryInfo s = GetSummary ();
		s.set_Property (0, null);
	}

	static SummaryInfo GetSummary ()
	{
		return null;
	}
}
