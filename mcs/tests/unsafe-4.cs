// Compiler options: -unsafe

unsafe class X {
	static int v;
	static int v_calls;
	
	static int* get_v ()
	{
		v_calls++;
		return &v;
	}
	
	static int Main ()
	{
		if ((*get_v ())++ != 0)
			return 1;
		if (v != 1)
			return 2;
		if (v_calls != 1)
			return 3;
		return 0;
	}
}
