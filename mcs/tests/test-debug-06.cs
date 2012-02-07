using System;

class Test
{
	public static void Main ()
	{
	}

	void TryCatch_1 ()
	{
		try
		{
		}
		catch
		{
		}
	}
	
	void TryCatch_2 ()
	{
		try
		{
		}
		catch (Exception e)
		{
			e = null;
		}
	}
	
	void TryCatch_3 ()
	{
		try
		{
		}
		catch (ArgumentException e)
		{
			e = null;
		}
		catch (Exception e)
		{
			return;
		}
	}
	
	void TryFinally_1 ()
	{
		try
		{
		}
		finally
		{
		}
	}

	void TryFinally_2 ()
	{
		try
		{
		}
		catch (Exception e)
		{
			e = null;
		}
		finally
		{
		}
	}
}