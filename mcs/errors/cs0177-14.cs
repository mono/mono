// CS0177: The out parameter `val' must be assigned to before control leaves the current method
// Line: 12

public class A
{
	public bool GetValue (out int val)
	{
		val = 0;
		return true;
	}

	public void ReallyGetValue (out int val)
	{
		if (AlwaysReturnTrue () || GetValue (out val)) {
		}
	}

	public bool AlwaysReturnTrue ()
	{
		return true;
	}
}