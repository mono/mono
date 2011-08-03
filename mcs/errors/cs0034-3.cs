// CS0034: Operator `+' is ambiguous on operands of type `Y' and `X'
// Line: 22
public class Y {
	public static implicit operator int (Y y) {
		return 0;
	}

	public static implicit operator string (Y y) {
		return null;
	}

	public static implicit operator Y (string y) {
		return null;
	}

	public static implicit operator Y (int y) {
		return null;
	}
}

public class X {
	public static implicit operator int (X x) {
		return 0;
	}

	public static implicit operator string (X x) {
		return null;
	}
}

public class C {
	public static void Main ()
	{
		Y y = new Y () + new X ();
	}
}

