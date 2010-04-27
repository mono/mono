// CS0842: Automatically implemented property `S.Value' cannot be used inside a type with an explicit StructLayout attribute
// Line: 10

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
class S
{
	public int Value {
		get; set;
	}
}
