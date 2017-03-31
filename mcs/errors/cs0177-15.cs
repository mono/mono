// CS0177: The out parameter `x' must be assigned to before control leaves the current method
// Line: 6

public class GotoWithOut
{
	public static void Test (bool cond, out int x)
	{
		if (cond)
		{
			goto Label2;
		}
		else
		{
			goto Label;
		}
		Label:
		x = 0;
		Label2:
		return;
	}
}