public delegate void DelegateHandler();

public interface EventInterface 
{
	event DelegateHandler OnEvent;
}

public class BaseClass 
{
	public event DelegateHandler OnEvent;
}

public class ExtendingClass : BaseClass, EventInterface 
{
	public static void Main()
	{
	}
}