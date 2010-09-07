public class List<X>
{
	public class Comp<Y>
	{
		public List<Y>.Comp<X> flip (Y y, X x)
		{
			return new Flip<Y> (this);
		}
	}
	public class Flip<Z> : List<Z>.Comp<X>
	{
		Comp<Z> c;
		public Flip (Comp<Z> cc) { c = cc; }
	}
}

class C
{
	public static int Main ()
	{
		var a = new List<short>.Comp<bool> ().flip (true, 3);
		return 0;
	}
}
