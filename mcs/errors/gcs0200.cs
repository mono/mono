// CS0200: Property or indexer `A.Counter' cannot be assigned to (it is read only)
// Line: 9

class Program
{
	static void Main()
	{
		A a = new A();
		a.Counter++;
	}
}

class A {
	private int? _counter;
	public int? Counter {
		get { return _counter; }
	}
}
