// CS1525: Unexpected symbol `==', expecting `(' or `type'
// Line: 8

class A
{
	public static implicit operator == (A a, bool b)
	{
		return false;
	}
}
