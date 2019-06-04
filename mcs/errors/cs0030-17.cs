// CS0030: Cannot convert type `object' to `S'
// Line: 13
// Compiler options: -langversion:latest

ref struct S
{
}

class X
{
	public static void Foo (object o)
	{
		var res = (S) o;
	}
}