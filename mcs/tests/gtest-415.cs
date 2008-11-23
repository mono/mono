// Compiler options: -unsafe

unsafe struct S
{
	public short nData;
	public fixed int Data [1];
}

unsafe struct S2
{
	public uint Header;
	public fixed byte Data [5];

	public void Test ()
	{
		fixed (byte* bP = Data) {
			S* p = (S*) bP;
			p = (S*) (p->Data + p->nData);
		}
	}

	public static void Main ()
	{
		new S2 ().Test ();
	}
}

