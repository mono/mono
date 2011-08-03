// CS1066: The default value specified for optional parameter `x' will never be used
// Line: 12
// Compiler options: -warnaserror

interface I
{
	void Method (int i);
}

class C : I
{
	void I.Method (int x = 9)
	{
	}
}
