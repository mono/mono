//
// Tests that we validate the unchecked state during constatn resolution
//
class X {
	public static void Main ()
	{
		unchecked {
			const int val = (int)0x800B0109;
		}
	}
}
