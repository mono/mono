class Stack<T> {
}

class Test {
}

class T {
	Stack<Test> a;
	Stack<Test,Test> b;
	Stack<Test,Stack<Test>> c;

	public void Foo (Stack<Test> d)
	{ }

	static void Main()
	{
		Stack<Test> e;
		Stack<Test,Stack<Test>> f;
	}
}
