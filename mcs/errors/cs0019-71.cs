// CS0019: Operator `==' cannot be applied to operands of type `External' and `int'
// Line: 11
// Compiler options: -r:CS0019-71-lib.dll

class X
{
	public static void Main ()
	{
		var t1 = new External ();
		int t2 = 0;
		bool b = t1 == t2;
	}
}