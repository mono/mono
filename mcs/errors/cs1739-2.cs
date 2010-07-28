// CS1739: The best overloaded method match for `A.this[int]' does not contain a parameter named `value'
// Line: 17

class A
{
	public int this [int id] {
		set {
		}
	}
}

class B
{
	public static void Main ()
	{
		A a = new A ();
		a [value:1] = 9;
	}
}
