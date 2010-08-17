// CS0108: `IMutableSequence.this[int]' hides inherited member `ISequence.this[int]'. Use the new keyword if hiding was intended
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