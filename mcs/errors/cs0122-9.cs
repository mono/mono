// CS0122: `X.a' is inaccessible due to its protection level
// Line: 16

public class X {
	private int a {
		get {
			return 1;
		}
	}
}

internal class Y : X
{
	int D (X x)
	{
		if (x.a == 2)
			return 0;
		return 0;
	}
}
