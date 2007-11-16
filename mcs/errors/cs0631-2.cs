// CS0631: The parameter modifier `ref' is not valid in this context
// Line: 5

class X {
	public static explicit operator X (ref X[] foo)
	{
		return null;
	}
}
