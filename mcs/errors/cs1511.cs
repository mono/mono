// cs1511.cs: Base modifier not allowed in static code
// Line:

class Y {
	public int a;
}

class X : Y {

	static void Main ()
	{
		base.a = 1;
	}
}
