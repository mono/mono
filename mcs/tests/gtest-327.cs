interface ITest
{
	void Test ();
}

class Tester<T> where T : ITest, new ()
{
	public void Do ()
	{
		new T ().Test ();
	}
}

class Reference : ITest
{
	public void Test ()
	{
	}
}

struct Value : ITest
{
	public void Test ()
	{
	}
}

class C
{
	public static void Main ()
	{
		new Tester<Reference> ().Do ();
		new Tester<Value> ().Do ();
	}
}


