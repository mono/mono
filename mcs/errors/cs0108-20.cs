// CS0108: `B.Adapter' hides inherited member `A.Adapter'. Use the new keyword if hiding was intended
// Line: 14
// Compiler options: -warnaserror -warn:2

class A
{
	public abstract class Adapter
	{
	}
}

class B : A
{
	public int Adapter { get; set; }
}