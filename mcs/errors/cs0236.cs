//cs0236: A field initializer cannot reference the non-static field, method or property `X.Foo'.

class X
{
	int Foo {
		get {
			return 9;
		}
	}

	long Bar = Foo;

	static void Main () {
	}
}
