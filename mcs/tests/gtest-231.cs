//
// From bug 77032
//
class X {
	static int stored_offset, stored_len, opt_len;

	public static void Main ()
	{
	if (stored_offset >= 0 && (stored_len+4) < (opt_len >> 3))  {}

	}
}
