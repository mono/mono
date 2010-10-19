// cs0466.cs:  `Base.I.M(params int[])': the explicit interface implementation cannot introduce the params modifier
// Line: 10

interface I
{
	void M(int[] values);
}
class Base : I
{
	void I.M(params int[] values) {}
}