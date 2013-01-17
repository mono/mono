public class AdapterType
{
	protected virtual void DoSomething ()
	{
	}
}

public static class Extensions
{
	public static void DoSomething (this AdapterType obj)
	{
	}
}

public abstract class Dummy : AdapterType
{
	public virtual bool Refresh ()
	{
		AdapterType someObj = null;
		someObj.DoSomething ();
		return true;
	}

	public static void Main ()
	{

	}
}