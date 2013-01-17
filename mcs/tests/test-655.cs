public class D
{
	byte v;

	byte this [int idx] {
		get {
			return v;
		}
		set {
			v = value;
		}
	}

	public static int Main ()
	{
		D d = new D ();
		byte b = 1;
		d [0] += 1;
		d [0] += b;
		if (d [0] != 2)
			return 1;

		return 0;
	}
}
