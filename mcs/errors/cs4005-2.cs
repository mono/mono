// CS4005: Async methods cannot have unsafe parameters
// Line: 11
// Compiler options: -unsafe

class C
{
	unsafe delegate void D (int* i);
	
	public static void Main ()
	{
		D d = async delegate { };
	}
}
