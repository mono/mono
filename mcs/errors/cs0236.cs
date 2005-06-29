// cs0236.cs: A field initializer cannot reference the nonstatic field, method, or property `X.Foo'

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

