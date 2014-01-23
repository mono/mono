// CS0177: The out parameter `arg' must be assigned to before control leaves the current method
// Line: 12

class C
{
	delegate void D (string s, out int arg);

	public static void Main ()
	{
		D d = delegate (string s, out int arg)
		{
			return;
		};
	}
}
