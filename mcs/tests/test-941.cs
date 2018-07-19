public class GotoCodeFlowBug
{
	public static void Test (bool cond, out int x)
	{
		if (cond)
		{
			goto Label;
		}
		Label:
		x = 0;
	}

	public static void Test2 (bool cond, out int x)
	{
		if (cond)
		{
			goto Label;
		}
		else
		{
			goto Label;
		}
		Label:
		x = 0;
	}

	public static void Main ()
	{
	}
}