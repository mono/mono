// cs0206.cs: indexers or properties can not be used as ref or out arguments
// Line:
class X {
	static int P { get { return 1; } set { } }

	static int m (out int v)
	{
		v = 1;
		return 1;
	}
	
	static void Main ()
	{
		m (out P);
	}
}
