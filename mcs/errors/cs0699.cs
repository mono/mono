// CS0699: `I.Test<T>()': A constraint references nonexistent type parameter `U'
// Line: 6

interface I
{
	void Test<T>() where U : class;
}
