using C1 = N1.C1;

public class Test {
	private static C1 c1 = null;

	public static C1 C1 {
		get {
			return c1;
		}
	}

	public static int Main() {
		C1 tmp = C1;
		return 0;
	}
}

namespace N1 {
    public class C1 {
    }
}
