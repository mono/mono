// CS0724: A throw statement with no arguments is not allowed inside of a finally clause nested inside of the innermost catch clause
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
