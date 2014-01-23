public class TestRefKeywords
{
	static int Main ()
	{
		int i = 0;
		System.TypedReference r = __makeref(i);
		System.Type t = __reftype(r);
		int j = __refvalue( r,int);

		__refvalue(r, int) = 4;
		var x = ( __refvalue(r, int) += 1);

		if (x != 5)
			return 1;

		return 0;
	}
}