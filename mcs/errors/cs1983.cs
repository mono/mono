// CS1983: The return type of an async method `C.Test()' must be void, Task, or Task<T>
// Line: 6
// Compiler options: -langversion:future

class C
{
	public async object Test ()
	{
	}
}
