// CS0029: Cannot implicitly convert type `string' to `int'
// Line: 8

class A<T> where T : CB, IA
{
	void Foo (T t)
	{
		t.Prop = "3";
	}
}

class CB : CA
{
}

class CA
{
	public int Prop { get; set; }
}

interface IA
{
	string Prop { get; set; }
}