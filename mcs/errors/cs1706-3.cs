// CS1706: Anonymous methods and lambda expressions cannot be used in the current context
// Line: 8

delegate void D ();

class C
{
	const object c = new D (delegate {});
}
