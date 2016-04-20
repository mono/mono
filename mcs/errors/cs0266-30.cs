// CS0266: Cannot implicitly convert type `Foo<int>.FooEvent' to `Foo<string>.FooEvent'. An explicit conversion exists (are you missing a cast?)
// Line: 12

class Foo<T> {
	public event FooEvent Event;
	public delegate T FooEvent();
}

class CompilerCrashTest {
	static void Main() {
		Foo<string> foo = new Foo<string>();
		foo.Event += new Foo<int>.FooEvent (() => 0);
	}
}
