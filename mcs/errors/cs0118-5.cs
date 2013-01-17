// CS0118: `A.s' is a `field' but a `type' was expected
// Line: 11

class A
{
	public string s;
}

class X : A
{
	s MyProperty {
		get {
			return s;
		}
	}
}
