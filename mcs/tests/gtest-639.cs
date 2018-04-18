class A<T> where T : CB, IA
{
	void Foo (T t)
	{
		t.Prop = 3;
		long l = t.Prop2;
		t["1"] = "2";
	}
}

class A2<T, U> 
	where T : CB, U
	where U : IA
{
	void Foo (T t)
	{
		t.Prop = 3;
		long l = t.Prop2;
		t["1"] = "2";
	}
}

class CB : CA
{
}

class CA
{
	public int Prop { get; set; }

	public string this [byte b] { get { return ""; } }
}

interface IA
{
	string Prop { get; set; }
	long Prop2 { get; }

	string this [string b] { get; set; }
}

class X
{
	public static void Main ()
	{
	}
}