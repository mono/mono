// CS8141:
// Line: 9

public interface I<T>
{
	T Test ();
}

public class C : I<(int a, int b)>
{
	public (int c, int d) Test ()
	{
		return (1, 2);
	}
}
