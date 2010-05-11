partial class A<T>
{
	void Test ()
	{
		this.CurrentItem = null;
	}
}

partial class A<T> where T : class
{
	T CurrentItem { get; set; }
}

class C
{
	public static void Main ()
	{
	}
}
