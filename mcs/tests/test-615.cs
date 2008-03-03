class C
{
	public static void Main ()
	{
		const bool b = int.MinValue == 0x80000000;
		
		ulong res = 0;
		char c = 'c';
		res = res * 16 + c;		
	}
}
