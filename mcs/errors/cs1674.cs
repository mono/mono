// cs1674.cs: `int': type used in a using statement must be implicitly convertible to `System.IDisposable'
// Line: 8

class C
{
    void Method (int arg)
    {
	using (arg)
	{
	}
    }
}