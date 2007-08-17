// CS1729: The type `C.S' does not contain a constructor that takes `3' arguments
// Line: 15

class C
{
	struct S
	{
		public S (int i)
		{
		}
	}
	
	static void Main ()
	{
		S i = new S (1,1,1);
	}
}

