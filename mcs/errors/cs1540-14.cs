// CS1540: Cannot access protected member `object.MemberwiseClone()' via a qualifier of type `anonymous type'. The qualifier must be of type `A' or derived from it
// Line: 9

class A
{
	public A ()
	{
		var x = new { s = "-" };
		x.MemberwiseClone();
	}
}
