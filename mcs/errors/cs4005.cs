// CS4005: Async methods cannot have unsafe parameters
// Line: 7
// Compiler options: -langversion:future -unsafe

class C
{
	public unsafe async void Test (int* arg)
	{
	}
}
