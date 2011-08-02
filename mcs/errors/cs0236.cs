// CS0236: A field initializer cannot reference the nonstatic field, method, or property `X.Foo'
// Line: 12

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

