// CS0221: Constant value `-1' cannot be converted to a `ushort' (use `unchecked' syntax to override)
// Line: 11

class C
{
	delegate void D ();

	static void Main ()
	{
		D d = unchecked (delegate {
			const ushort s = (ushort) -1;
		});
	}
}
