public partial struct STuple<Ta> { }
public partial struct STuple<Ta>
{
	private readonly Ta a;
	public STuple (Ta a) { this.a = a; }
}

class C
{
	public static int Main ()
	{
		new STuple<int> ();
		return 0;
	}
}