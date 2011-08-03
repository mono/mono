public interface ITest
{
	void Run ();
}

public class A
{
	public void Run ()
	{
	}
}

public class B : A, ITest
{
}