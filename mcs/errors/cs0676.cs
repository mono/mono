unsafe class X {
	static volatile int j;
	
	static void Main ()
	{
		fixed (int *p = &j){
			
		}
	}
}
