//
// Tests that we validate the unchecked state during constatn resolution
//
class X {
	static void Main ()
	{
		unchecked {
			const int val = (int)0x800B0109;
		}
	}
}
