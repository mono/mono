public class Test
{
	static void Foo (ushort p)
	{
		p = 0x0000;
		p |= 0x0000;
		p &= 0x0000;

		const ushort c = 0x0000;
		p &= c;
	}

	public static void Main ()
	{
		Foo (1);
	}
}