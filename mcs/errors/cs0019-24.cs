// CS0019: Operator `+=' cannot be applied to operands of type `object' and `object'
// Line: 10

class Program
{
	static int Main ()
	{
		object[] o = null;
		int ii = 2;
		o [ii++] += new object ();
		return 0;
	}
}
