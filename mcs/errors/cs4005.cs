// CS4005: Async methods cannot have unsafe parameters
// Line: 7
// Compiler options: -unsafe

class C
{
	public unsafe async void Test (int* arg)
	{
	}
}
