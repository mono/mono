using System;

class X
{
	public static void Main ()
	{
		Func<object> f = () => throw null;
	}

	public int Test () => throw null;

	object Foo ()
	{
		return null;
	}

	public object Test2 () => Foo () ?? throw null;

	static void Test3 (out int z) => throw null;

	int this [int x] {
		get => throw null;
	}    

	public event Action Event {
		add => throw null; 
		remove => throw null;
	}

	void TestExpr_1 (bool b)
	{
		int x = b ? throw new NullReferenceException () : 1;        
	}

	void TestExpr_2 (bool b)
	{
		int x = b ? 2 : throw new NullReferenceException ();
	}

	void TestExpr_3 (string s)
	{
		s = s ?? throw new NullReferenceException ();
	}

	void TestExpr_4 ()
	{
		throw new ApplicationException () ?? throw new NullReferenceException() ?? throw null;
	}

	void TestExpr_5 ()
	{
		Action a = () => throw new ApplicationException () ?? throw new NullReferenceException() ?? throw null;
	}

	static int TestExpr_6 (out int z) => throw null;

	int TestExpr_7 (out int z)
	{
		return true ? throw new NullReferenceException () : 1;
	}
}