// CS0742: A throw statement with no argument is only allowed in a catch clause nested inside of the innermost catch clause
// Line: 14

class C
{
	static void Test()
	{
		try
		{
			throw new System.Exception();
		}
		catch
		{
			try
			{
			}
			finally
			{
				throw;
			}
		}
	}
}