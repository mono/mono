// CS0108: `B.D' hides inherited member `A.D'. Use the new keyword if hiding was intended
// Line: 15
// Compiler options: -warnaserror -warn:2

public class B : A
{
	public delegate void D ();
}

public class A
{
	public int D;
}
