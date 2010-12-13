interface IA<T>
{
	T Method (int index);
}

interface IB
{
	void Method (int index);
}

interface IC : IA<string>, IB
{
	void Method (params int[] index);
}

class M : IC
{

	void IC.Method (params int[] index)
	{
	}

	string IA<string>.Method (int index)
	{
		throw new System.NotImplementedException ();
	}

	void IB.Method (int index)
	{
		throw new System.NotImplementedException ();
	}

	public static void Main ()
	{
		IC ic = new M ();
		ic.Method (1);
	}
}