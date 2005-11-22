// cs0631-2.cs: ref and out are not valid in this context
// Line: 5

class X {
	public static explicit operator X (ref X[] foo)
	{
		return null;
	}
}
