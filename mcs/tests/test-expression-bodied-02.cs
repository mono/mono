using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct Cs0842ExpressionBodyGetterBug
{
	[FieldOffset(0)]
	public int DummyVariable;

	public int MyGetter => 5;
}

class C
{
	public static void Main ()
	{
	}
}