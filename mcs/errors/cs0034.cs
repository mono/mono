// cs0034: operator >= is ambiguous on types ulong and sbyte
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
