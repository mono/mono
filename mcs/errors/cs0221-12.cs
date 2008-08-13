// CS0221: Constant value `-3' cannot be converted to a `ushort' (use `unchecked' syntax to override)
// Line: 12

class C
{
	delegate void D ();

	static void Main ()
	{
		D d = checked (delegate {
			const ushort s = (ushort) -3;
		});
	}
}
