

partial class C
{
	static partial void Partial (int i);
	
	public static int Main ()
	{
		Partial (1);
		return 0;
	}
}