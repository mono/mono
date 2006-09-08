// CS0699: `Test<I.T>': A constraint references nonexistent type parameter `U'
// Line: 8

interface I
{
	void Test<T>() where U : class;
}
