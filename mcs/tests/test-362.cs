class C
{
	public static void Main()
	{	
		try {
		    Test ();
		} catch
		{
		}
	}
	
	static void Test ()
	{
		try
		{
			throw new System.ArgumentException();
		}
		catch
		{
			try
			{
			    throw;
			}
			finally
			{
				
			}
		}
	}
}
