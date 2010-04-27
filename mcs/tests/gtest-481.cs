public class TestClass<T>
{
	private T[][] m_data;

	public TestClass (int arrSize)
	{
		Add (ref m_data);
	}

	private static void Add (ref T[][] arr)
	{
	}
}

class C
{
	public static void Main ()
	{
		new TestClass<decimal> (4);
	}
}
