// CS0176: Static member `X.CONST' cannot be accessed with an instance reference, qualify it with a type name instead
// Line: 12

public class X {
	public const double CONST = 1;
}

public class Y: X {

	void D (X x)
	{
		double d = x.CONST;
	}
}
