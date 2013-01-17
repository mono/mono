// CS1612: Cannot modify a value type return value of `Test.v(bool)'. Consider storing the value in a temporary variable
// Line: 28

public struct V
{
	public int this [int i] {
		set {
		}
	}
	
	public int x;
}

class Test
{
	V m_value;

	public static V v(bool b) { return new V (); }

	public Test ()
	{
		m_value = new V ();
	}

	public static void Main ()
	{
		Test t = new Test ();
		Test.v(true).x = 9;
	}
}
