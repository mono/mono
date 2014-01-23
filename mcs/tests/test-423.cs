// Compiler options: -unsafe

unsafe class Test
{ 
	static void lowLevelCall (int *pv) { }

	static void Func (out int i)
	{ 
		fixed(int *pi = &i)
			lowLevelCall (pi);
	}

	public static void Main ()
	{
		int i = 0;
		Func (out i);
	}
}

