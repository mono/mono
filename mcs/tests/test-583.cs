class Program
{
	public static int Main ()
	{
		IExtContainer e = null;
		ObjectContainerBase b = null;
		return (e == b ? 0 : 1);
	}
}

public interface IContainer
{
}

public interface IExtContainer : IContainer
{
}

public abstract class ObjectContainerBase : IContainer
{
}
