//
// We should also allow overrides to work on protected methods.
// Only private is not considered part of the override process.
//
abstract class A {
        protected abstract int Foo ();
}

class B : A {
        protected override int Foo ()
	{
		return 0;
	}

	public int M ()
	{
		return Foo ();
	}
}

class Test {
        public static int Main ()
	{
		return new B ().M ();
        }
}


