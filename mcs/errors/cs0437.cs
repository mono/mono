// CS0437: The type `System' conflicts with the imported namespace `System'. Using the definition found in the source file
// Line: 9
// Compiler options: -warnaserror

enum System { A }

class X
{
	void Method (System arg)
	{
	}
}
