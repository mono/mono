// cs0153.cs: A goto case is only valid inside a switch statement
// Line:
class X {
	void Main ()
	{
		goto default;
	}
}
