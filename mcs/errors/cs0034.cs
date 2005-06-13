// cs0034.cs: Operator `>=' is ambiguous on operands of type `ulong' and `sbyte'
// Line: 7
class X {

	bool ret (ulong u, sbyte s)
	{
		return (u >= s);
	}

	bool ret (ulong u, short s)
	{
		return (u >= s);
	}

}
