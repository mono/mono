//
// This test is used to test that we do not use the .override
// command on abstract method implementations.
//

public abstract class Abstract {
	public abstract int A ();
}

public class Concrete : Abstract {
	public override int A () {
		return 1;
	}
}

class Test {

	public static int Main ()
	{
		Concrete c = new Concrete ();

		if (c.A () != 1)
			return 1;

		return 0;
	}
}
	
